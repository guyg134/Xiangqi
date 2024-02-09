
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;



public class Board
{

    private Dictionary<PieceType, List<Piece>> board;
    //bitboard
    private BitBoard bitBoard;
    private Stack<Move> movesSave;
    private Stack<Fen> fenPositionsSave;

    private GameState gameState;

    public Board(Fen fen)
    {
        board = new Dictionary<PieceType, List<Piece>>();

        movesSave = new Stack<Move>();
        fenPositionsSave = new Stack<Fen>();
        fenPositionsSave.Push(fen);

        bitBoard = null;
        gameState = GameState.Opening;
    }

    public Board(Board otherBoard)
    {
        //create clone of the board with different dictionary, list but the same pieces to save time
        board = CloneBoard(otherBoard.GetDictionary());

        movesSave = new Stack<Move>(otherBoard.movesSave);
        fenPositionsSave = new Stack<Fen>(otherBoard.fenPositionsSave);

        bitBoard = new BitBoard(otherBoard.bitBoard);
        gameState = otherBoard.gameState;
    }

    public void SetBitBoard()
    {
        bitBoard = new BitBoard(this);
    }

    public Fen GetCurrentFen()
    {
        return fenPositionsSave.Peek();
    }

    public int GetFenCount()
    {
        return fenPositionsSave.Count;
    }

    public Dictionary<PieceType, List<Piece>> GetDictionary()
    {
        return board;
    }

    public void AddPiece(Piece piece)
    {
        PieceType pieceType = piece.GetPieceType();

        if(!board.ContainsKey(pieceType))
        {
            List<Piece> piecesList = new List<Piece>();
            board.Add(pieceType, piecesList);
        }

        board[pieceType].Add(piece);
    }

    public Piece FindPiece(int x, int y)
    {
        foreach(PieceType pieceType in board.Keys)
        {
            foreach(Piece piece in board[pieceType])
            {
                if(piece.GetX() == x && piece.GetY() == y)
                {
                    return piece;
                }
            }
        }
        return null;
    }

    public List<Piece> GetPiecesInType(PieceType pieceType)
    {
        return board[pieceType];
    }

    public Piece FindPiece(int x, int y, PieceType pieceType)
    {
        //another find piece func that gets x,y but this one gets piece type also to search faster
        
        foreach(Piece piece in board[pieceType])
        {
            if(piece.GetX() == x && piece.GetY() == y)
            {
                return piece;
            }
        }
        return null;
    }

    private void RemovePiece(Piece removePiece)
    {
        board[removePiece.GetPieceType()].Remove(removePiece);
        Fen lastFen = fenPositionsSave.Pop();
        fenPositionsSave.Clear();
        fenPositionsSave.Push(lastFen);
    }

    public int GetPieceCount(PieceType pieceType)
    {
        return board[pieceType].Count;
    }

    public Piece FindKing(GameColor gameColor)
    {
        List<Piece> kings = board[PieceType.King];
        foreach (Piece king in kings)
        {
            if (king.GetPieceColor() == gameColor)
            {
                return king;
            }
        }
        
        return null;
    }
    
    public void MovePieceOnBoard(Move move)
    {

        int endX = move.EndX;
        int endY = move.EndY;

        //find the pieces
        Piece movingPiece = move.MovingPiece;
        Piece eatenPiece = move.EatenPiece;
        
        //moving the piece
        movingPiece.SetPos(endX, endY);

        if(eatenPiece)
        {
            RemovePiece(eatenPiece);
        }

        bitBoard.UpdateBitBoard(move, movingPiece.GetPieceColor());

        movesSave.Push(move);
        fenPositionsSave.Push(fenPositionsSave.Peek().FenAfterMove(move));
    }

    public void UndoLastMove()
    {
        Move lastMove = movesSave.Pop();
        fenPositionsSave.Pop();

        int x = lastMove.StartX;
        int y = lastMove.StartY;

        //find the piece
        Piece movingPiece = lastMove.MovingPiece;
        Piece eatenPiece = lastMove.EatenPiece;
        //moving the piece
        movingPiece.SetPos(x, y);

        if(eatenPiece)
        {
            AddPiece(eatenPiece);
        }

        bitBoard.UndoMoveBitboard(lastMove, movingPiece.GetPieceColor());
    }

    public List<string> GetMovesList(bool isRedPlayerDown)
    {
        List<string> movesList = new List<string>();

        // Convert the Stack to a List for reverse iteration
        Move[] movesArray = movesSave.ToArray();

        // Iterate through movesReversed
        for (int i = movesArray.Length - 1; i >= 0; i--)
        {
            Move move = new Move(movesArray[i]);

            if (!isRedPlayerDown)
                move.ChangeSide();

            // Add the move name to movesList
            movesList.Add(move.Name());
        }

        return movesList;
        
    }

    public List<Position> GetValidMoves(BigInteger dotsBitboard, Piece piece)
    {
        //delete all the positions that have piece with the same color of this piece
        dotsBitboard = bitBoard.BitboardMovesWithoutDefence(dotsBitboard, piece.GetPieceColor());

        //change the bitboard moves to position positions
        List<Position> dotsPos = bitBoard.BitboardToPosition(dotsBitboard);

        //save the valids moves
        List<Position> validMoves = new List<Position>();
        //create dots for every position
        foreach(Position dotPos in dotsPos)
        {
            Move move = new Move(piece.GetX(), piece.GetY(), dotPos.x, dotPos.y, piece, FindPiece(dotPos.x, dotPos.y));
            bool isCheckAfterThisMove = IsKingUnderAttackAfterMove(move, piece.GetPieceColor().OppositeColor());
            
            //if there is no check after the move add the move to the valids moves list
            if(!isCheckAfterThisMove)
            {
                validMoves.Add(dotPos);
            }
            
        }
        return validMoves;  
    }

    public bool IsRepetitiveMove()
    {
        int repetitions = 0;
        Stack<Fen> fenPositionsCopy = new Stack<Fen>(fenPositionsSave);
        Fen currentFen = fenPositionsCopy.Pop();
        
        foreach(Fen fen in fenPositionsCopy)
        {
            if(fen.GetFenString() == (currentFen.GetFenString()))
            {
                repetitions++;
            }
            if(repetitions > 2)
            {
                return true;
            }
        }
        
        return false;
    }

    public GameState GetGameState()
    {
        //check if the game state is opening and if it is, check if it is middle game
        if(gameState == GameState.Opening && IsMiddle())
        {
            gameState = GameState.MiddleGame;
        }
        //check if the game state is middle game and if it is, check if it is end game
        else if(gameState == GameState.MiddleGame && IsEndgame())
        {
            gameState = GameState.EndGame;
        }

        return gameState;
    }

    private bool IsMiddle()
    {
        bool knightMove = false;
        bool soldierMove = false;
        bool cannonMove = false;

        for(int i = 0; i < board[PieceType.Knight].Count && !knightMove; i++)
        {
            knightMove |= PieceType.Knight.PieceTypeStartingPos(board[PieceType.Knight][i].GetPos());
        }
        for(int i = 0; i < board[PieceType.Soldier].Count && !soldierMove; i++)
        {
            soldierMove |= PieceType.Soldier.PieceTypeStartingPos(board[PieceType.Soldier][i].GetPos());
        }
        for(int i = 0; i < board[PieceType.Cannon].Count && !cannonMove; i++)
        {
            cannonMove |= PieceType.Cannon.PieceTypeStartingPos(board[PieceType.Cannon][i].GetPos());
        }
        return movesSave.Count > 16 && knightMove && soldierMove && cannonMove;
    }

    private bool IsEndgame()
    {
        int redPieces = 0;
        int blackPieces = 0;

        //O(32) - n is the number of pieces = 32 max, possibly less because this check is only called when the game is in the middle game
        foreach(PieceType pieceType in board.Keys)
        {
            foreach(Piece piece in board[pieceType])
            {
                if(piece.GetPieceColor() == GameColor.Red)
                {
                    redPieces++;
                }
                else
                {
                    blackPieces++;
                }
            }
        }
        //if there is less than 6 pieces of one color and there is more than 60 moves, it is endgame
        return (redPieces <= 6 || blackPieces <= 6) && movesSave.Count > 60;
    }

    private bool IsKingUnderAttackAfterMove(Move move, GameColor playerColor)
    {
        //create clone to save the board before
        Board boardAfterMove = new Board(this);
        //do the move
        boardAfterMove.MovePieceOnBoard(move);
    
        bool isKingUnderAttackAfterMove = bitBoard.IsCheck(boardAfterMove, playerColor);
        boardAfterMove.UndoLastMove();

        SearchMove.o++;

        return isKingUnderAttackAfterMove;
    }

    public bool IsCheck(GameColor turnColor)
    {
        return bitBoard.IsCheck(this, turnColor);
    }

    public bool IsDraw()
    {
        return !PiecesForCheckmate() || IsRepetitiveMove();
    }

    private bool PiecesForCheckmate()
    {
        return board[PieceType.Soldier].Count > 1 || board[PieceType.Knight].Count != 0 || board[PieceType.Cannon].Count > 1  || board[PieceType.Rook].Count != 0;
    }

    public Dictionary<PieceType, List<Piece>> CloneBoard(Dictionary<PieceType, List<Piece>> cloneBoard)
    {
        // Create a new instance of the Board
        Dictionary<PieceType, List<Piece>> newBoard = new Dictionary<PieceType, List<Piece>>();

        // Deep clone the dictionary
        foreach (var entry in cloneBoard)
        {
            PieceType pieceType = entry.Key;
            List<Piece> pieceList = entry.Value;

            // Create a shallow copy of the list
            List<Piece> clonedPieceList = new List<Piece>(pieceList);

            // Add the cloned list to the cloned dictionary
            newBoard.Add(pieceType, clonedPieceList);
        }

        return newBoard;
    }

    public bool PlayerHaveMoves(GameColor playerColor)
    {

        foreach(PieceType pieceType in board.Keys)
        {
            foreach(Piece piece in board[pieceType])
            {
                if(piece && (int)piece.GetPieceColor() == (int)playerColor && piece.GetValidMoves(this).Count != 0)
                {
                    return true;
                }
            }
        }
        return false;
    }
}

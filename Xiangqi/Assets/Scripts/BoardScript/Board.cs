
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Linq;



public class Board
{
    //private Dictionary<PieceType, List<Piece>> board;
    private Dictionary<Position, Piece> board;
    private Dictionary<PieceType, int> pieceCount;
    //bitboard
    private BitBoard bitBoard;
    private Stack<Move> movesSave;
    private Stack<Fen> fenPositionsSave;

    private GameState gameState;

    public Board(Fen fen)
    {
        //board = new Dictionary<PieceType, List<Piece>>();
        board = new Dictionary<Position, Piece>();
        pieceCount = new Dictionary<PieceType, int>{{PieceType.King, 0},{PieceType.Soldier, 0}, {PieceType.Knight, 0}, {PieceType.Elephant, 0}, {PieceType.Cannon, 0}, {PieceType.Advisor, 0}, {PieceType.Rook, 0}};

        movesSave = new Stack<Move>();
        fenPositionsSave = new Stack<Fen>();
        fenPositionsSave.Push(fen);

        bitBoard = null;
        gameState = GameState.Opening;
    }

    public Board(Board otherBoard)
    {
        //create clone of the board with different dictionary, list but the same pieces to save time
        board = CloneBoard(otherBoard.board);
        pieceCount = new Dictionary<PieceType, int>(otherBoard.pieceCount);

        movesSave = new Stack<Move>(otherBoard.movesSave);
        fenPositionsSave = new Stack<Fen>(otherBoard.fenPositionsSave);

        bitBoard = new BitBoard(otherBoard.bitBoard);
        gameState = otherBoard.gameState;
    }

    public void SetBitBoard()
    {
        bitBoard = new BitBoard(this);
    }

    public List<Piece> GetPiecesList()
    {
        return board.Values.ToList();
    }

    public Fen GetCurrentFen()
    {
        return fenPositionsSave.Peek();
    }

    public int GetFenCount()
    {
        return fenPositionsSave.Count;
    }

    public GameState GetGameState()
    {
        int moveCount = movesSave.Count;
        gameState = GameStateFactory.GetGameState(moveCount);
        return gameState;
    }

    public void AddPiece(Piece piece)
    {
        try{
            board.Add(piece.GetPos(), piece);
        }
        catch (ArgumentException)
        {
            //if there is piece, eat it and add the new piece
            EatPiece(board[piece.GetPos()]);
            board.Add(piece.GetPos(), piece);
        }
        pieceCount[piece.GetPieceType()]++;
        /*PieceType pieceType = piece.GetPieceType();

        if(!board.ContainsKey(pieceType))
        {
            List<Piece> piecesList = new List<Piece>();
            board.Add(pieceType, piecesList);
        }

        board[pieceType].Add(piece);*/
    }

    private void EatPiece(Piece removePiece)
    {
        //remove
        board.Remove(removePiece.GetPos());
        pieceCount[removePiece.GetPieceType()]--;

        //board[removePiece.GetPieceType()].Remove(removePiece);
        Fen lastFen = fenPositionsSave.Pop();
        fenPositionsSave.Clear();
        fenPositionsSave.Push(lastFen);
    }

    public Piece FindPiece(Position pos)
    {
        try
        {
            return board[pos];
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }
    public Piece FindPiece(int x, int y)
    {
        try
        {
            return board[new Position(x, y)];
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    
        /*foreach(PieceType pieceType in board.Keys)
        {
            foreach(Piece piece in board[pieceType])
            {
                if(piece.GetX() == x && piece.GetY() == y)
                {
                    return piece;
                }
            }
        }
        return null;*/
    }

    public int GetPieceCount(PieceType pieceType)
    {
        return pieceCount[pieceType];
    }

    public Piece FindKing(GameColor gameColor)
    {
        return FindPiece(bitBoard.GetKingBitPos(gameColor));

        /*List<Piece> kings = board[PieceType.King];
        foreach (Piece king in kings)
        {
            if (king.GetPieceColor() == gameColor)
            {
                return king;
            }
        }
        
        return null;*/
    }
    
    public void MovePieceOnBoard(Move move)
    {

        //find the pieces
        Piece movingPiece = move.MovingPiece;
        Piece eatenPiece = move.EatenPiece;
        
        //moving the piece
        board.Remove(move.startPosition);
        movingPiece.SetPos(move.endPosition);

        if(eatenPiece)
        {
            EatPiece(eatenPiece);
        }

        AddPiece(movingPiece);

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
        board.Remove(movingPiece.GetPos());
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

    private bool IsKingUnderAttackAfterMove(Move move, GameColor attackingColor)
    {
        //create clone to save the board before
        Board boardAfterMove = new Board(this);
        //do the move
        boardAfterMove.MovePieceOnBoard(move);
    
        bool isKingUnderAttackAfterMove = bitBoard.IsCheck(boardAfterMove, attackingColor);
        boardAfterMove.UndoLastMove();

        SearchMove.o++;

        return isKingUnderAttackAfterMove;
    }

    public bool IsCheck(GameColor attackingColor)
    {
        return bitBoard.IsCheck(this, attackingColor);
    }

    public bool IsDraw()
    {
        return !PiecesForCheckmate() || IsRepetitiveMove();
    }

    private bool PiecesForCheckmate()
    {
        return pieceCount[PieceType.Soldier] > 1 || pieceCount[PieceType.Knight] != 0 || pieceCount[PieceType.Cannon] > 1  || pieceCount[PieceType.Rook] != 0;
    }

    public Dictionary<Position, Piece> CloneBoard(Dictionary<Position, Piece> cloneBoard)
    {
        // Create a new instance of the Board
        Dictionary<Position, Piece> newBoard = new Dictionary<Position, Piece>();

        // Deep clone the dictionary
        foreach (var entry in cloneBoard)
        {
            Position pos = entry.Key;
            Piece piece = entry.Value;

            // Add the cloned list to the cloned dictionary
            newBoard.Add(pos, piece);
        }

        return newBoard;
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

        foreach(Piece piece in board.Values)
        {
            if(piece && (int)piece.GetPieceColor() == (int)playerColor && piece.GetValidMoves(this).Count != 0)
            {
                return true;
            }
        }
        
        return false;
    }
}

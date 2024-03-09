
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using UnityEngine;



public class Board : MonoBehaviour
{
    //private Dictionary<PieceType, List<Piece>> board;
    private Dictionary<string, Piece> board;
    private Dictionary<PieceType, int> pieceCount;
    //bitboard
    private BitBoard bitBoard;
    private Stack<Move> movesSave;
    private Stack<Fen> fenPositionsSave;

    private GameState gameState;

    public Board(Fen fen)
    {
        //board = new Dictionary<PieceType, List<Piece>>();
        board = new Dictionary<string, Piece>();
        pieceCount = new Dictionary<PieceType, int>{{PieceType.King, 0},{PieceType.Soldier, 0},
         {PieceType.Knight, 0}, {PieceType.Elephant, 0}, {PieceType.Cannon, 0},
          {PieceType.Advisor, 0}, {PieceType.Rook, 0}};

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

    public Piece FindPiece(Position pos)
    {
        try
        {
            return board[pos.Name];
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }
    public Piece FindPiece(int x, int y)
    {
        return FindPiece(new Position(x, y));
    }

    public int GetPieceCount(PieceType pieceType)
    {
        return pieceCount[pieceType];
    }


    public Piece FindKing(GameColor gameColor)
    {
        return FindPiece(bitBoard.GetKingBitPos(gameColor));
    }

    //add new piece to the board
    public void AddPiece(Piece piece)
    {
        try{
            board.Add(piece.GetPos().Name, piece);
            pieceCount[piece.GetPieceType()]++;
        }
        catch (ArgumentException)
        {
            throw new Exception("There is already a piece in position: " + piece.GetPos().x + " " + piece.GetPos().y);
        }
    }

    private void RemovePiece(Piece piece)
    {
        try{
            board.Remove(piece.GetPos().Name);
            pieceCount[piece.GetPieceType()]--;
        }
        catch (KeyNotFoundException)
        {
            throw new Exception("There is no piece in position: " + piece.GetPos().x + " " + piece.GetPos().y);
        }
    }

    private void EatPiece(Piece removePiece)
    {
        //remove
        RemovePiece(removePiece);

        //clear the all positions save because the board cant be the same
        Fen lastFen = fenPositionsSave.Pop();
        fenPositionsSave.Clear();
        fenPositionsSave.Push(lastFen);
    }


    public void MovePieceOnBoard(Move move)
    {
        //O(1)
        //find the pieces
        Piece movingPiece = move.MovingPiece;
        Piece eatenPiece = move.EatenPiece;

        bitBoard.UpdateBitBoard(move, movingPiece.GetPieceColor());
        
        //remove the piece from the board
        RemovePiece(movingPiece);
        //moving the piece
        movingPiece.SetPos(move.endPosition);

        //remove the eaten piece from the board, if there is
        if(eatenPiece != null)
        {
            EatPiece(eatenPiece);
        }

        //add the piece to the new position
        AddPiece(movingPiece);

        movesSave.Push(move);
        //save the fen after the move
        fenPositionsSave.Push(fenPositionsSave.Peek().FenAfterMove(move));
    }


    public void UndoLastMove()
    {
        //O(1)
        //get the last move, and remove the move and the fen from the stacks
        Move lastMove = movesSave.Pop();
        fenPositionsSave.Pop();

        //find the piece
        Piece movingPiece = lastMove.MovingPiece;
        Piece eatenPiece = lastMove.EatenPiece;

        bitBoard.UndoMoveBitboard(lastMove, movingPiece.GetPieceColor());

        //remove the piece from the board
        RemovePiece(movingPiece);
        //moving the piece
        movingPiece.SetPos(lastMove.startPosition);

        //if there is eaten piece add it to board
        if(eatenPiece != null)
        {
            AddPiece(eatenPiece);
        }

        AddPiece(movingPiece);
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
            movesList.Add(move.Name);
        }

        return movesList;
    }

    public List<Position> GetValidMoves(BigInteger dotsBitboard, Piece piece)
    {
        //O(n* m) complexity, n is the number of positions, m is the number of times iskingunderattackaftermove is called max m = 9, n = 18, m*n = 162 
        //delete all the positions that have piece with the same color of this piece
        dotsBitboard = bitBoard.BitboardMovesWithoutDefence(dotsBitboard, piece.GetPieceColor());

        //change the bitboard moves to position positions
        List<Position> dotsPos = BitBoard.BitboardToPositions(dotsBitboard);

        //save the valids moves
        List<Position> validMoves = new List<Position>();
        //create dots for every position
        foreach(Position dotPos in dotsPos)
        {
            Move move = new Move(piece.GetPos(), dotPos, piece, FindPiece(dotPos.x, dotPos.y));
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
        //O(n) complexity, n is the number of moves
        int repetitions = 0;
        Stack<Fen> fenPositionsCopy = new Stack<Fen>(fenPositionsSave);
        Fen currentFen = fenPositionsCopy.Pop();
        
        foreach(Fen fen in fenPositionsCopy)
        {
            if(fen.GetFenString() == currentFen.GetFenString())
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
        //O(9),  complexity
        //create clone to save the board before
        Board boardAfterMove = new Board(this);
        //do the move
        boardAfterMove.MovePieceOnBoard(move);
    
        bool isKingUnderAttackAfterMove = boardAfterMove.IsCheck(attackingColor);
        boardAfterMove.UndoLastMove();

        SearchMove.o++;

        return isKingUnderAttackAfterMove;
    }

    //check if the king is under attack by the attacking color
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

    public bool PlayerHaveMoves(GameColor playerColor)
    {
        //O(n * m) complexity, n is the number of pieces, n = 16, m is the number of valid moves, m = 17
        // Iterate through all the pieces
        foreach(Piece piece in board.Values)
        {
            if(piece.GetPieceColor() == playerColor && piece.GetValidMoves(this).Count != 0)
            {
                return true;
            }
        }
        
        return false;
    }

    public List<Position> GetValidMoves(Piece piece)
    {
        //O(1)
        BigInteger moves = piece.GetPieceBitboardMove(this);
        return GetValidMoves(moves, piece);
    }

    public List<Position> PiecesUnderAttackByPiece(Piece piece)
    {
        //O(n)
        return bitBoard.PiecesUnderAttackByPiece(this, piece);
    }

    public Dictionary<string, Piece> CloneBoard(Dictionary<string, Piece> board)
    {
        Dictionary<string, Piece> newBoard = new Dictionary<string, Piece>();
        foreach(var entry in board)
        {
            newBoard.Add(entry.Key, entry.Value);
        }
        return newBoard;
    }
}

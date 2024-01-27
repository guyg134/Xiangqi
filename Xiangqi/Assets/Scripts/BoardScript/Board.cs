
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine.Android;


public class Board
{

    Dictionary<PieceType, List<Piece>> board;
    Stack<Move> movesSave = new Stack<Move>();

    public Board()
    {
        board = new Dictionary<PieceType, List<Piece>>();
    }

    public Board(Board otherBoard)
    {
        //create clone of the board with different dictionary, list but the same pieces to save time
        this.board = CloneBoard(otherBoard.GetDictionary());
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
    }

    public int GetPieceCount(PieceType pieceType)
    {
        return board[pieceType].Count;
    }

    public Piece FindKing(GameColor gameColor)
    {
        Piece king = board[PieceType.King][0];
        if(king && king.GetPieceColor() == gameColor)
            return king;

        king = board[PieceType.King][1];
        if(king)
            return king; 
        
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

        movesSave.Push(move);
    }

    public void UndoLastMove()
    {
        Move lastMove = movesSave.Pop();

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

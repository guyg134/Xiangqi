
using System;
using System.Collections.Generic;
using System.Numerics;



public class Board
{

    private Dictionary<PieceType, List<Piece>> board;
    //bitboard
    private BitBoard bitBoard;
    private Stack<Move> movesSave;

    public Board()
    {
        board = new Dictionary<PieceType, List<Piece>>();
        movesSave = new Stack<Move>();
        bitBoard = null;
    }

    public Board(Board otherBoard)
    {
        //create clone of the board with different dictionary, list but the same pieces to save time
        this.board = CloneBoard(otherBoard.GetDictionary());
        this.movesSave = new Stack<Move>(otherBoard.movesSave);
        this.bitBoard = new BitBoard(otherBoard.bitBoard);
    }

    public void SetBitBoard()
    {
        this.bitBoard = new BitBoard(this);
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

    private bool IsKingUnderAttackAfterMove(Move move, GameColor playerColor)
    {
        //create clone to save the board before
        Board boardAfterMove = new Board(this);
        //do the move
        boardAfterMove.MovePieceOnBoard(move);

        bool isKingUnderAttackAfterMove = bitBoard.IsCheck(boardAfterMove, playerColor);

        boardAfterMove.UndoLastMove();

    

        return isKingUnderAttackAfterMove;
    }

    public bool IsCheck(GameColor turnColor)
    {
        return bitBoard.IsCheck(this, turnColor);
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

using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class Piece : MonoBehaviour
{
    private GameBoard gameBoard;
    
    private PieceType pieceType;
    private GameColor pieceColor;
    private int x;
    private int y;
    private PieceMovement pieceMovement;

    public void SetPiece(PieceType pieceType, GameColor pieceColor, int x, int y)
    {
        this.pieceType = pieceType;
        this.pieceColor = pieceColor;
        pieceMovement = PieceFactory.GetPiece(pieceType, pieceColor);
        this.x = x;
        this.y = y;
        
        gameBoard = FindObjectOfType<GameBoard>();
    }

    public int GetX()
    {
        return x;
    }

    public int GetY()
    {
        return y;
    }

    public Position GetPos()
    {
        return new Position(x, y);
    }

    public void SetPos(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public PieceType GetPieceType()
    {
        return pieceType;
    }

    public BigInteger GetPieceBitboardMove(Board board)
    {
        return pieceMovement.GetBitboardMoves(x, y, board);
    }

    public void MovePiece(Position newPos)
    {
        //create new move with null on the eaten piece that will be add on the gameboard
        Move move = new Move(x, y, newPos.x, newPos.y, this);
        //update the piece in board
        gameBoard.UpdatePieceInBoard(move);
    }

    public GameColor GetPieceColor()
    {
        return pieceColor;
    }

    

    public void GetDots()
    {
        gameBoard.CreatePieceDots(gameObject, GetValidMoves(gameBoard.GetBoard()));
    }

    public List<Position> GetValidMoves(Board board)
    {
        BigInteger moves = GetPieceBitboardMove(board);
        return board.GetValidMoves(moves, this);
    }

}

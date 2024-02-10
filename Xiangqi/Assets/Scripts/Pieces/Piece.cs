using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class Piece : MonoBehaviour
{
    private GameBoard gameBoard;
    
    private PieceType pieceType;
    private GameColor pieceColor;
    private Position pos;
    private PieceMovement pieceMovement;

    public void SetPiece(PieceType pieceType, GameColor pieceColor, int x, int y)
    {
        this.pieceType = pieceType;
        this.pieceColor = pieceColor;
        pieceMovement = PieceFactory.GetPiece(pieceType, pieceColor);
        pos = new Position(x, y);
        
        gameBoard = FindObjectOfType<GameBoard>();
    }

    public int GetX()
    {
        return pos.x;
    }

    public int GetY()
    {
        return pos.y;
    }

    public Position GetPos()
    {
        return pos;
    }

    public void SetPos(int x, int y)
    {
        pos.x = x;
        pos.y = y;
    }

    public void SetPos(Position pos)
    {
        this.pos = pos;
    }

    public PieceType GetPieceType()
    {
        return pieceType;
    }

    public BigInteger GetPieceBitboardMove(Board board)
    {
        return pieceMovement.GetBitboardMoves(pos.x, pos.y, board);
    }

    public void MovePiece(Position newPos)
    {
        //create new move with null on the eaten piece that will be add on the gameboard
        Move move = new Move(pos.x, pos.y, newPos.x, newPos.y, this);
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

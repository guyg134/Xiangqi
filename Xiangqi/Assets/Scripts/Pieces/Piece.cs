using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class Piece : MonoBehaviour
{
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
        
    }

    public int GetX()
    {
        return x;
    }

    public int GetY()
    {
        return y;
    }

    public Vector2 GetPos()
    {
        return new Vector2(x, y);
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

    public void MovePiece(Vector2 newPos)
    {
        //create new move with null on the eaten piece that will be add on the gameboard
        Move move = new Move(x, y, (int)newPos.x, (int)newPos.y, this);
        //update the piece in board
        GameObject.FindGameObjectWithTag("Board").GetComponent<GameBoard>().UpdatePieceInBoard(move);
    }

    public GameColor GetPieceColor()
    {
        return pieceColor;
    }

    

    public void GetDots()
    {
        GameBoard gameBoard = GameObject.FindGameObjectWithTag("Board").GetComponent<GameBoard>();
        gameBoard.CreatePieceDots(gameObject, GetValidMoves(gameBoard.GetBoard()));
    }

    public List<Vector2> GetValidMoves(Board board)
    {
        BigInteger moves = GetPieceBitboardMove(board);
        return GameObject.FindGameObjectWithTag("Board").GetComponent<GameBoard>().GetValidMoves(moves, this, x, y);
    }

}

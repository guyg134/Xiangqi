using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move
{
    private int startX;
    private int startY;
    private int endX;
    private int endY;

    private Piece movingPiece;
    private Piece eatenPiece;

    public Move(int startX, int startY, int endX, int endY, Piece movingPiece, Piece eatenPiece)
    {
        this.startX = startX;
        this.startY = startY;
        this.endX = endX;
        this.endY = endY;
        this.movingPiece = movingPiece;
        this.eatenPiece = eatenPiece;
    }

    public Move(int startX, int startY, int endX, int endY, Piece movingPiece)
    {
        this.startX = startX;
        this.startY = startY;
        this.endX = endX;
        this.endY = endY;
        this.movingPiece = movingPiece;
        this.eatenPiece = null;
    }

     public Move(int startX, int startY, int endX, int endY)
    {
        this.startX = startX;
        this.startY = startY;
        this.endX = endX;
        this.endY = endY;
        this.movingPiece = null;
        this.eatenPiece = null;
    }

    public int getStartX()
    {
        return startX;
    }
    public int getStartY()
    {
        return startY;
    }
    public int getEndX()
    {
        return endX;
    }
    public int getEndY()
    {
        return endY;
    }

    public Piece GetPiece()
    {
        return movingPiece;
    }

    public Piece EatenPiece()
    {
        return eatenPiece;
    }

     public void SetPiece(Piece movingPiece)
    {
        this.movingPiece = movingPiece;
    }

    public void SetEatenPiece(Piece eatenPiece)
    {
        this.eatenPiece = eatenPiece;
    }

    public bool Equal(Move move)
    {
        return startX == move.startX && startY == move.startY && endX == move.endX && endY == move.endY;
    }
}

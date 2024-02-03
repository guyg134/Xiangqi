using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PieceType
{
    King=1, 
    Soldier=2, 
    Knight=3, 
    Elephant=4, 
    Cannon=5, 
    Advisor=6, 
    Rook=7
}

public static class PieceTypeMethods
{

    public static bool PieceCanAttackKing(this PieceType pieceType)
    {
        return pieceType == PieceType.Soldier || pieceType == PieceType.Knight || pieceType == PieceType.Cannon || pieceType == PieceType.Rook;
    }

    // Returns if piece is on starting position
    public static bool PieceTypeStartingPos(this PieceType pieceType, Position pos)
    {
        if(pieceType == PieceType.King)
        {
            return pos.x == 4 && (pos.y == 0 || pos.y == 9);
        }
        else if(pieceType == PieceType.Soldier)
        {
            return pos.y == 3 || pos.y == 6;
        }
        else if(pieceType == PieceType.Knight)
        {
            return (pos.x == 1 || pos.x == 7) && (pos.y == 0 || pos.y == 9);
        }
        else if(pieceType == PieceType.Elephant)
        {
            return (pos.x == 2 || pos.x == 6) && (pos.y == 0 || pos.y == 9);
        }
        else if(pieceType == PieceType.Cannon)
        {
            return (pos.x == 1 || pos.x == 7) && (pos.y == 2 || pos.y == 7);
        }
        else if(pieceType == PieceType.Advisor)
        {
            return (pos.x == 3 || pos.x == 5) && (pos.y == 0 || pos.y == 9);
        }
        else if(pieceType == PieceType.Rook)
        {
            return (pos.x == 0 || pos.x == 8) && (pos.y == 0 || pos.y == 9);
        }
        else
        {
            return false;
        }
    }
}

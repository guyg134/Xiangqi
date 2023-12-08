using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Evaluate : MonoBehaviour
{/*
    const int soliderValue = 100;
    const int advisorValue = 200;
    const int elephantValue = 250;
    const int knightValue = 400;
    const int cannonValue = 450;
    const int rookValue = 900;

    const int red = 1;
    const int black = 2;

    public static int GetEvaluate()
    {
        GameObject[] pieces = Board.getPiecesArray();

        int redMaterial = CountMaterial(pieces, red);
        int blackMaterial = CountMaterial(pieces, black);

        int evaluate = redMaterial - blackMaterial;

        int turn = (GameManager.getTurnColor() == "red") ? 1 : -1;
        return evaluate * turn;
    }

    //return every piece of color multiple by value
    private static int CountMaterial(GameObject[] pieces, int color)
    {
        int material = 0;
        //add to materialcounter every piece of color multiple by value
        material += soliderValue*Board.GetPieceCount(color * 10 + (int)Piece.PieceType.Soldier);
        material += advisorValue*Board.GetPieceCount(color * 10 + (int)Piece.PieceType.Advisor);
        material += elephantValue*Board.GetPieceCount(color * 10 + (int)Piece.PieceType.Elephant);
        material += knightValue*Board.GetPieceCount(color * 10 + (int)Piece.PieceType.Knight);
        material += cannonValue*Board.GetPieceCount(color * 10 + (int)Piece.PieceType.Cannon);
        material += rookValue*Board.GetPieceCount(color * 10 + (int)Piece.PieceType.Rook);
         
        return material;
    }*/
}

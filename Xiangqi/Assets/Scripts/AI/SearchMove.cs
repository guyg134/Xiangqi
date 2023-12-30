using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;

public class SearchMove : MonoBehaviour
{
    Board board;
    PlayerColor playerColor;
    public void SetSearchMove(PlayerColor playerColor)
    {
        board = GameObject.FindWithTag("Board").GetComponent<Board>();
        this.playerColor = playerColor;
    }
    public void FindMove()
    {
        int maxSearches = 1500;
        Piece piece;
        
        do{
            if(maxSearches == 0) return;
            maxSearches--;
            int x = Random.Range(0, 9);
            int y = Random.Range(0, 10);

            piece = board.GetPieceAtPosition(x, y);
        }while(!piece || (int)piece.GetPieceColor() != (int)playerColor || piece.GetDots().Count == 0);
        StartCoroutine(DoMove(piece));
    }   

    IEnumerator DoMove(Piece piece)
    {
        yield return new WaitForSeconds(1.5f);
        List<Vector2> validMoves = piece.GetDots();

        int i = Random.Range(0, validMoves.Count);

        piece.MovePiece(validMoves[i]);
    }
}

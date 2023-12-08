using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class SearchMove : MonoBehaviour
{/*
    private static string color;
    private Boolean didMove = false;
    public static void startAi(string c)
    {
        
       color = c;
    }

    public static string getPlayerColor()
    {
        return color;
    }

    void Update()
    {
        if(GameManager.getTurnColor() == color && !didMove)
        {
            didMove = true;
            searchBestMove(3, Board.getPiecesArray());
        }
    }

    private int searchBestMove(int depth, GameObject[] pieces)
    {
        if(depth == 0)
            return Evaluate.GetEvaluate();

        //list of specific piece moves
        List<int> pieceMoves = new List<int>();

        int bestEvaluation = -100000000;
        int bestPiece = 0;
        int bestMove = 1;
        for(int i =0; i < pieces.Length; i++){
            if(pieces[i])
            {
                PieceController pieceController = pieces[i].GetComponent<PieceController>();
                    pieceMoves = pieceController.getMoveOptions();
                    for(int j = 0; j < pieceMoves.Count; j++)
                    {
                        //create new board array after the current move
                        GameObject[] boardAfterMove = (GameObject[])pieces.Clone();
                        boardAfterMove[pieceMoves[j]] = boardAfterMove[i];
                        boardAfterMove[i] = null;
                        //call to searchbestmove after the current move to get evaluation
                        int evaluation = -searchBestMove(depth-1, boardAfterMove);
                        //if the current move better than the last best move
                        if(depth == 3 && bestEvaluation > evaluation && j != i)
                        {
                            bestEvaluation = evaluation;
                            bestPiece = i;
                            bestMove = j;
                        }
                    
                }
            }
        }
        if(depth == 3){
            print(bestPiece + ", best move " + bestMove);
            doMove(pieces[bestPiece].GetComponent<PieceController>(), bestMove);
        }
        return bestEvaluation;
    }

    

    private void doMove(PieceController pieceIndex, int move)
    {
        pieceIndex.movePiece(move);
    }*/
}

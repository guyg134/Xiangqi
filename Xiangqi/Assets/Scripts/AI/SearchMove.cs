using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;

public class SearchMove : MonoBehaviour
{
    Board board;
    GameColor playerColor;
    public void SetSearchMove(GameColor playerColor)
    {
        board = GameObject.FindWithTag("Board").GetComponent<Board>();
        this.playerColor = playerColor;
    }

    public void DoTurn()
    {
        //DoRandomMove();
        GenerateMove();
    }
    private void DoRandomMove()
    {
        Piece piece;
        int x;
        int y;
        do{
            
            x = Random.Range(0, 9);
            y = Random.Range(0, 10);

            piece = board.GetPiece(x, y);
        }while(!piece || (int)piece.GetPieceColor() != (int)playerColor || piece.GetValidMoves().Count == 0);
        //piece.GetDots();

        List<Vector2> validMoves = piece.GetValidMoves();
        int i = Random.Range(0, validMoves.Count);

        StartCoroutine(DoMove(piece, validMoves[i]));
    }  

    private void GenerateMove()
    {
        float bestEval;
        Piece[,] pieces = board.GetBoard();
        if(playerColor == GameColor.Red)
            bestEval   = -1000000000;       
        else
            bestEval = 1000000000;     
        
        Move bestMove = new Move(0,0,0,0);

        for (int y = 0; y < 10; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                Piece piece = pieces[y, x];
                if(!piece || piece.GetPieceColor() != playerColor) continue;

                

                foreach(Vector2 pos in piece.GetValidMoves())
                {
                    //create clone to save the board before
                    Piece[,] saveBoard = board.CloneBoard(pieces);
                    //do the move
                    Piece pieceToMove = pieces[y, x];
                    pieces[y, x] = null;
                    pieces[(int)pos.y, (int)pos.x] = pieceToMove;
                    
                    float evalMove = Evaluate.EvaluateFunc(pieces, playerColor);
                    
                    if(playerColor == GameColor.Red)
                    {
                        if(evalMove > bestEval)
                        {
                            bestEval = evalMove;
                            bestMove = new Move(x, y, (int)pos.x, (int)pos.y);
                            print("the  eval is " + bestEval);
                            print(" Move is start x" + bestMove.getStartX() + " and y" + bestMove.getStartY() + " end x" +bestMove.getEndX() +" y" + bestMove.getEndY());
                        }
                    }
                    else
                    {
                        if(evalMove < bestEval)
                        {
                            bestEval = evalMove;
                            bestMove = new Move(x, y, (int)pos.x, (int)pos.y);
                            print("the  eval is " + bestEval);
                            print(" Move is start x" + bestMove.getStartX() + " and y" + bestMove.getStartY() + " end x" +bestMove.getEndX() +" y" + bestMove.getEndY());
                        }
                    }

                    pieces = saveBoard;
                }
            }
        }
        //print("the best eval is " + bestEval);
        //print("best Move is start x" + bestMove.getStartX() + " and y" + bestMove.getStartY() + " end x" +bestMove.getEndX() +" y" + bestMove.getEndY());
        
        StartCoroutine(DoMove(board.GetBoard()[bestMove.getStartY(), bestMove.getStartX()], new Vector2(bestMove.getEndX(), bestMove.getEndY())));
    } 

    IEnumerator DoMove(Piece piece, Vector2 pos)
    {
        yield return new WaitForSeconds(1.5f);

        piece.MovePiece(pos);
    }
}

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
    GameBoard gameBoard;
    GameColor playerColor;
    OpeningBook openingBook;

    public float timeForMove = 0;

    public void SetSearchMove(GameColor playerColor)
    {
        gameBoard = GameObject.FindWithTag("Board").GetComponent<GameBoard>();
        this.playerColor = playerColor;
        openingBook = new OpeningBook();

    }

    public void DoTurn()
    {
        //DoRandomMove();
        int moves = GameObject.FindWithTag("GameManager").GetComponent<GameManager>().GetMovesCounter();
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
            
            piece = gameBoard.GetBoard().FindPiece(x, y);
        }while(!piece || (int)piece.GetPieceColor() != (int)playerColor || piece.GetValidMoves(gameBoard.GetBoard()).Count == 0);
        //piece.GetDots();

        List<Vector2> validMoves = piece.GetValidMoves(gameBoard.GetBoard());
        int i = Random.Range(0, validMoves.Count);

        StartCoroutine(DoMove(piece, validMoves[i]));
    }  
    
    private void GenerateMove()
    {
        double bestEval;
        
        if(playerColor == GameColor.Red)
            bestEval   = -1000000000;       
        else
            bestEval = 1000000000;     
        
        Move bestMove = new Move(0,0,0,0, null, null);

        Board board = gameBoard.GetBoard();

        for(int i = 1; i < 8; i++)
        {
            foreach(Piece piece in board.GetPiecesInType((PieceType)i))
            {
                if(!piece || piece.GetPieceColor() != playerColor) continue;

                

                foreach(Vector2 pos in piece.GetValidMoves(board))
                {
                    //create clone to save the board before
                    Board boardAfterMove = new Board(board);
                    Move move = new Move(piece.GetX(), piece.GetY(), (int)pos.x, (int)pos.y, piece, board.FindPiece((int)pos.x, (int)pos.y));
                    //do the move
                    boardAfterMove.MovePieceOnBoard(move);
                    
                    double evalMove = Evaluate.EvaluateFunc(boardAfterMove, playerColor);
                    
                    if(playerColor == GameColor.Red)
                    {
                        if(evalMove > bestEval)
                        {
                            bestEval = evalMove;
                            bestMove = move;
                        }
                    }
                    else
                    {
                        if(evalMove < bestEval)
                        {
                            bestEval = evalMove;
                            bestMove = move;
                        }
                    }

                    boardAfterMove.UndoLastMove();
                }
            } 
        }
        
        StartCoroutine(DoMove(bestMove.GetPiece(), new Vector2(bestMove.getEndX(), bestMove.getEndY())));
    } 

    /*public Move GetRandomOpeningMove(Board board)
    {

    }*/

    IEnumerator DoMove(Piece piece, Vector2 pos)
    {
        yield return new WaitForSeconds(timeForMove);

        piece.MovePiece(pos);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using Random = UnityEngine.Random;

public class SearchMove : MonoBehaviour
{
    [SerializeField] private GameBoard gameBoard;
    [SerializeField] private GameManager gameManager;
    Player Player;
    private static OpeningBook openingBook;

    [Range(0f, 1f)]
    [SerializeField] private float timeForMove = 0;
    [SerializeField] private bool doRandomMove = false;

    public static int o = 0;
    public static double sumTimeToMove = 0;
    public static double worthTime;

    private const double tolerance = 0.1;



    public void SetSearchMove(Player player)
    {
        this.Player = player;
        openingBook = new OpeningBook();
    }

    public void DoTurn()
    {
        DateTime start = DateTime.Now;
        o = 0;

        int movesPlayed = gameManager.GetMovesCounter();
        //find the move to do
        Move move = GetMove(movesPlayed);

        //calculate the time it took to find the move
        double timeToMove = (DateTime.Now - start).TotalSeconds;
        if( timeToMove > worthTime)
        {
            worthTime = timeToMove;
        }
        sumTimeToMove += timeToMove;
        print("time took to move: " + timeToMove + "worth time to move: " + worthTime + " function calls: " + o);

        //do the move
        StartCoroutine(DoMove(move.MovingPiece, move.PositionEnd));
    }

    public Move GetMove(int movesPlayed)
    {
        //do random move
        if(doRandomMove)
            return DoRandomMove();

        //if moves played are less than 40, try to do an opening move
        if(false)//(movesPlayed < 40)
        {
            Move openingMove = OpeningMove();
            if(openingMove != null)
            {
                return openingMove;
            }
        }
            
        //if there is no opening move, generate move
        return GenerateMove();
    }

    private Move DoRandomMove()
    {
        Piece piece;
        int x;
        int y;
        List<Position> validMoves = new List<Position>();
        do{
            
            x = Random.Range(0, 9);
            y = Random.Range(0, 10);
            
            piece = gameBoard.GetBoard().FindPiece(x, y);
            if(piece && piece.GetPieceColor() == Player.playerColor)
            {
                validMoves = piece.GetValidMoves(gameBoard.GetBoard());
            }
        }while(validMoves.Count == 0);

        int i = Random.Range(0, validMoves.Count);

        return new Move(x, y, validMoves[i].x, validMoves[i].y, piece);
    }  
    
    private Move GenerateMove()
    {
        double bestEval;
        GameColor playerColor = Player.playerColor;
        //set the best evaluation to the lowest possible value if the player is red, and to the highest possible value if the player is black
        if(playerColor == GameColor.Red)
            bestEval   = -EvaluateConstants.Infinity;       
        else
            bestEval = EvaluateConstants.Infinity;     
        
        List<Move> bestMoves = new List<Move>();
        Board copyBoard = gameBoard.GetBoardCopy();

        //iterate through all the pieces
        
        foreach(Piece piece in copyBoard.GetPiecesList())
        {
            //if the piece is an enemy piece, continue
            if(piece.GetPieceColor() == playerColor)
            {
                //get all the valid moves of the piece
                foreach(Position pos in piece.GetValidMoves(copyBoard))
                {
                    //create clone to save the board before
                    Move move = new Move(piece.GetPos(), pos, piece, copyBoard.FindPiece(pos));

                    //do the move on the clone
                    copyBoard.MovePieceOnBoard(move);
                    Evaluate evaluate = new Evaluate();
                    //evaluate the move
                    double evalMove = evaluate.EvaluatePosition(copyBoard, Player);

                    // Determine if the current move is the best based on player color
                    if (isBetterMove(evalMove, bestEval))
                    {
                        GameColor enemyColor = playerColor.OppositeColor();
                        //if the move is cause checkmate on 1 move for the enemy, set the evaluation to the checkmate value
                        if(evaluate.CheckMateNextMove(copyBoard, enemyColor))
                        {
                            evalMove = playerColor == GameColor.Red ? -EvaluateConstants.CheckMateValue : EvaluateConstants.CheckMateValue;
                        }
                        //if the move is a draw
                        else if(copyBoard.IsDraw())
                        {
                            evalMove = 0;
                        }

                        // Update the best move and evaluation if the current move is better
                        if (!bestMoves.Any() || isBetterMove(evalMove, bestEval))
                        {
                            bestMoves.Clear(); // Clear the previous best moves list
                            bestEval = evalMove;
                        }
                    }
                    //if the evaluation of the move is really close to the best evaluation, add it to the best moves list
                    if(Math.Abs(evalMove - bestEval) < tolerance)
                    {
                        bestMoves.Add(move);
                    }

                    copyBoard.UndoLastMove();
                }
            } 
            
        }
        
        int randomIndex = Random.Range(0, bestMoves.Count);
        return bestMoves[randomIndex];
    } 

    private bool isBetterMove(double evalMove, double bestEval)
    {
        bool isBetterMove = false;
        GameColor playerColor = Player.playerColor;

        if((playerColor == GameColor.Red && evalMove > bestEval) ||
            (playerColor == GameColor.Black && evalMove < bestEval))
        {
            isBetterMove = true;
        }

        return isBetterMove;
    }

    private Move OpeningMove()
    {
        //get the moves in the game from the red down perspective
        List<string> movesInGame = gameBoard.GetBoard().GetMovesList(Player.playOnDownSide == (Player.playerColor == GameColor.Red));
        string moveString = openingBook.GetRandomOpeningMove(movesInGame);

        //if there is no opening move, return null
        if(moveString == null)
        {
            return null;
        }

        Move move = Move.NameToMove(moveString);
        //if the player is red so the bot is on top, the move is from the other side
        if(Player.playOnDownSide != (Player.playerColor == GameColor.Red))
            move.ChangeSide();
        
        move.MovingPiece = gameBoard.GetBoard().FindPiece(move.StartX, move.StartY);

        return move;
    }

    IEnumerator DoMove(Piece piece, Position pos)
    {
        yield return new WaitForSeconds(timeForMove);

        piece.MovePiece(pos);
    }
}

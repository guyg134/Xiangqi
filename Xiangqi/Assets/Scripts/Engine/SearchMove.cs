using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;
using Random = UnityEngine.Random;

public class SearchMove : MonoBehaviour
{
    [SerializeField] private GameBoard gameBoard;
    [SerializeField] private GameManager gameManager;
    Player Player;
    private static OpeningBook openingBook;

    [SerializeField] private float timeForMove = 0;
    [SerializeField] private bool doRandomMove = false;

    public static int o = 0;

    private const double tolerance = 0.0001;



    public void SetSearchMove(Player player)
    {
        this.Player = player;
        openingBook = new OpeningBook();
    }

    public void DoTurn()
    {

        int movesPlayed = gameManager.GetMovesCounter();
        //find the move to do
        Move move = GetMove(movesPlayed);
        //do the move
        StartCoroutine(DoMove(move.MovingPiece, move.PositionEnd));
    }

    public Move GetMove(int movesPlayed)
    {
        //do random move
        if(doRandomMove)
            return DoRandomMove();

        //if moves played are less than 40, try to do an opening move
        if(movesPlayed < 40)
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
        do{
            
            x = Random.Range(0, 9);
            y = Random.Range(0, 10);
            
            piece = gameBoard.GetBoard().FindPiece(x, y);
        }while(!piece || piece.GetPieceColor() != Player.playerColor || piece.GetValidMoves(gameBoard.GetBoard()).Count == 0);
        //piece.GetDots();

        List<Position> validMoves = piece.GetValidMoves(gameBoard.GetBoard());
        int i = Random.Range(0, validMoves.Count);

        return new Move(x, y, validMoves[i].x, validMoves[i].y, piece);
    }  
    
    private Move GenerateMove()
    {
        DateTime start = DateTime.Now;

        double bestEval;
        GameColor playerColor = Player.playerColor;
        
        if(playerColor == GameColor.Red)
            bestEval   = -1000000000;       
        else
            bestEval = 1000000000;     
        
        List<Move> bestMoves = new List<Move>();

        Board board = gameBoard.GetBoard();
        o = 0;
        for(int i = 1; i < 8; i++)
        {
            Board copyBoard = new Board(board);
            foreach(Piece piece in board.GetPiecesInType((PieceType)i))
            {
                //if the piece is an enemy piece, continue
                if(piece.GetPieceColor() == playerColor)
                {
                    //get all the valid moves of the piece
                    foreach(Position pos in piece.GetValidMoves(board))
                    {
                        //create clone to save the board before
                        Move move = new Move(piece.GetX(), piece.GetY(), pos.x, pos.y, piece, board.FindPiece(pos.x, pos.y));

                        //do the move on the clone
                        copyBoard.MovePieceOnBoard(move);
                        
                        //evaluate the move
                        double evalMove = Evaluate.EvaluateMove(copyBoard, Player, bestEval, copyBoard.GetGameState());
                        //if the player is red, so the best move is the one with the highest evaluation
                        //if the player is black, so the best move is the one with the lowest evaluation
                        if ((playerColor == GameColor.Red && evalMove > bestEval) || (playerColor == GameColor.Black && evalMove < bestEval))
                        {
                            bestMoves.Clear();
                            bestEval = evalMove;
                        }
                        //if the evaluation of the move is really close to the best evaluation, add it to the best moves list
                        if(evalMove == bestEval) //(Math.Abs(evalMove - bestEval) < tolerance)
                        {
                            bestMoves.Add(move);
                        }

                        copyBoard.UndoLastMove();
                    }
                } 
            }
        }
        print("time took to move: " + (DateTime.Now - start).TotalSeconds + " function calls: " + o);
        
        int randomIndex = Random.Range(0, bestMoves.Count);
        return bestMoves[randomIndex];
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

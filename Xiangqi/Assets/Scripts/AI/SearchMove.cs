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



    public void SetSearchMove(Player player)
    {
        this.Player = player;
        openingBook = new OpeningBook();
    }

    public void DoTurn()
    {
        int movesPlayed = gameManager.GetMovesCounter();

        Move move = GetMove(movesPlayed);
        //print("x: " + move.StartX + " y: " + move.StartY + " x: " + move.EndX + " y: " + move.EndY);
        StartCoroutine(DoMove(move.MovingPiece, move.PositionEnd));
    }

    public Move GetMove(int movesPlayed)
    {
        //do random move
        if(doRandomMove)
            return DoRandomMove();

        if(movesPlayed < 37)
        {
            Move openingMove = OpeningMove();
            if(openingMove != null)
            {
                print("move is from opening");
                return openingMove;
            }
        }
            
        //if there is no opening move, generate move
        print("move is generated");
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
        double bestEval;
        GameColor playerColor = Player.playerColor;
        
        if(playerColor == GameColor.Red)
            bestEval   = -1000000000;       
        else
            bestEval = 1000000000;     
        
        List<Move> bestMoves = new List<Move>();

        Board board = gameBoard.GetBoard();

        for(int i = 1; i < 8; i++)
        {
            foreach(Piece piece in board.GetPiecesInType((PieceType)i))
            {
                if(!piece || piece.GetPieceColor() != playerColor) continue;

                

                foreach(Position pos in piece.GetValidMoves(board))
                {
                    //create clone to save the board before
                    Board boardAfterMove = new Board(board);
                    Move move = new Move(piece.GetX(), piece.GetY(), (int)pos.x, (int)pos.y, piece, board.FindPiece((int)pos.x, (int)pos.y));
                    //do the move
                    boardAfterMove.MovePieceOnBoard(move);
                    
                    double evalMove = Evaluate.EvaluatePosition(boardAfterMove, Player);
                    
                    if(playerColor == GameColor.Red)
                    {
                        if(evalMove == bestEval)
                        {
                            bestMoves.Add(move);
                        }
                        else if(evalMove > bestEval)
                        {
                            bestEval = evalMove;

                            //clear the list and add the new best move
                            bestMoves.Clear();
                            bestMoves.Add(move);
                        }
                    }
                    else
                    {
                        if(evalMove == bestEval)
                        {
                            bestMoves.Add(move);
                        }
                        else if(evalMove < bestEval)
                        {
                            bestEval = evalMove;
                            
                            //clear the list and add the new best move
                            bestMoves.Clear();
                            bestMoves.Add(move);
                        }
                    }

                    boardAfterMove.UndoLastMove();
                }
            } 
        }
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

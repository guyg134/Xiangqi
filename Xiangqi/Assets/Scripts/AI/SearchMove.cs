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


    private bool HaveOpening;

    public void SetSearchMove(Player player)
    {
        this.Player = player;
        openingBook = new OpeningBook();
        HaveOpening = true;
    }

    public void DoTurn()
    {
        int movesPlayed = gameManager.GetMovesCounter();

        Move move = GetMove();
        //print("x: " + move.StartX + " y: " + move.StartY + " x: " + move.EndX + " y: " + move.EndY);
        StartCoroutine(DoMove(move.MovingPiece, move.PositionEnd));
    }

    public Move GetMove()
    {
        //do random move
        if(doRandomMove)
            return DoRandomMove();

        Move openingMove = OpeningMove();
            
        //if there is opening move, do it
        if(openingMove != null)
        {
            print("move is from opening");
            return openingMove;
        }
        //if there is no opening move, generate move
        print("move is generated");
        HaveOpening = false;
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
        
        Move bestMove = new Move(0,0,0,0, null, null);

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
        
        return bestMove;
    } 

    private Move OpeningMove()
    {
        //get the moves in the game from the red down perspective
        List<string> movesInGame = gameBoard.GetBoard().GetMovesList(Player.playOnDownSide == (Player.playerColor == GameColor.Red));
        string moveString = openingBook.GetRandomOpeningMove(movesInGame);

        //if there is no opening move, return null
        if(!HaveOpening || moveString == null)
        {
            HaveOpening = false;
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

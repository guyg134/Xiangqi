using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using UnityEngine;

public class Evaluate 
{
    public static readonly int checkMateValue = 25000;


    const GameColor red = GameColor.Red;
    const GameColor black = GameColor.Black;

    //evaluate the current position and return the evaluate value (for the eval bar)
    public static double EvaluateCurrentPosition(Board board, Player currentPlayer)
    {
        if(CheckMateNextMove(board, currentPlayer.playerColor.OppositeColor()))
        {
            return EvaluateNumberByColor(checkMateValue, currentPlayer.playerColor.OppositeColor());
        }

        return EvaluatePosition(board, currentPlayer);
       
    }

    public static bool CheckMateNextMove(Board board, GameColor enemyColor)
    {
        Board copyBoard = new Board(board);
        
        foreach (Piece piece in copyBoard.GetPiecesList())
        {
            if(piece.GetPieceColor() == enemyColor){
                List<Position> validMoves = piece.GetValidMoves(copyBoard);

                foreach(Position pos in validMoves)
                {
                    Move move = new Move(piece.GetX(), piece.GetY(), pos.x, pos.y, piece, board.FindPiece(pos.x, pos.y));

                    copyBoard.MovePieceOnBoard(move);

                    // If after the move, the current player has no legal moves, it's checkmate
                    if (!copyBoard.PlayerHaveMoves(piece.GetPieceColor().OppositeColor()))
                    {
                        copyBoard.UndoLastMove();
                        return true;
                    }

                    copyBoard.UndoLastMove();
                }
            }
        }

        return false;

    }

    public static double EvaluateNumberByColor(double eval, GameColor turnColor)
    {
        return turnColor == red ? eval : -eval;
    }
    
    public static double EvaluatePosition(Board board, Player currentPlayer)
    {
        GameState gameState = board.GetGameState();
        GameColor turnColor = currentPlayer.playerColor;

        // If there is no moves for the player or the enemy return checkmate value
        if (!board.PlayerHaveMoves(turnColor))
        {
            return EvaluateNumberByColor(checkMateValue, turnColor.OppositeColor());
        }
        if(!board.PlayerHaveMoves(turnColor.OppositeColor()))
        {
            return EvaluateNumberByColor(checkMateValue, turnColor);
        }
        
        // Get the evaluation state for the current game state
        EvaluationState evaluateState = EvaluationState.GetEvaluationState(gameState);

        // Calculate the evaluation
        return evaluateState.EvaluateState(board, gameState, currentPlayer);
    }
    

}
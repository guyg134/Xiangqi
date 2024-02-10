using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    Opening,
    MiddleGame,
    EndGame
}

public static class GameStateFactory
{
    public static GameState GetGameState(int moveCount)
    {
        if (moveCount < Constants.OpeningMinMoves)
        {
            return GameState.Opening;
        }
        else if (moveCount < 40)
        {
            return GameState.MiddleGame;
        }
        else
        {
            return GameState.EndGame;
        }
    }

    /*private static bool IsMiddle()
    {
        bool knightMove = false;
        bool soldierMove = false;
        bool cannonMove = false;

        for(int i = 0; i < board[PieceType.Knight].Count && !knightMove; i++)
        {
            knightMove |= PieceType.Knight.PieceTypeStartingPos(board[PieceType.Knight][i].GetPos());
        }
        for(int i = 0; i < board[PieceType.Soldier].Count && !soldierMove; i++)
        {
            soldierMove |= PieceType.Soldier.PieceTypeStartingPos(board[PieceType.Soldier][i].GetPos());
        }
        for(int i = 0; i < board[PieceType.Cannon].Count && !cannonMove; i++)
        {
            cannonMove |= PieceType.Cannon.PieceTypeStartingPos(board[PieceType.Cannon][i].GetPos());
        }
        return movesSave.Count > 16 && knightMove && soldierMove && cannonMove;
    }

    private bool IsEndgame()
    {
        int redPieces = 0;
        int blackPieces = 0;

        //O(32) - n is the number of pieces = 32 max, possibly less because this check is only called when the game is in the middle game
        foreach(PieceType pieceType in board.Keys)
        {
            foreach(Piece piece in board[pieceType])
            {
                if(piece.GetPieceColor() == GameColor.Red)
                {
                    redPieces++;
                }
                else
                {
                    blackPieces++;
                }
            }
        }
        //if there is less than 6 pieces of one color and there is more than 60 moves, it is endgame
        return (redPieces <= 6 || blackPieces <= 6) && movesSave.Count > 60;
    }*/
}

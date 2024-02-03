using System.Numerics;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class MidgameEvaluation : EvaluationState
{
    
    // Weights
    protected const double checkWeight = 1.5f; 
    protected const double pieceValueDiffWeight = 2; 
    protected const double playerIntersectionWeight = 0.22f; 
    protected const double enemyIntersectionWeight = 0.22f; 
    protected const double maxUndefendedPieceValueWeight = 1.5f; 
    protected const double maxAttackingPieceValueWeight = 0.2f;

        //(18700, 400, 500, 600, 800, 600, 300) 


    // Piece values
    protected const int kingValue = 18700;
    protected const int soliderValue = 300;
    protected const int advisorValue = 400;
    protected const int elephantValue = 450;
    protected const int knightValue = 600;
    protected const int cannonValue = 650;
    protected const int rookValue = 800;

    public MidgameEvaluation() : base()
    {
       PieceValues[(int)PieceType.King] = kingValue;
        PieceValues[(int)PieceType.Soldier] = soliderValue;
        PieceValues[(int)PieceType.Advisor] = advisorValue;
        PieceValues[(int)PieceType.Elephant] = elephantValue;
        PieceValues[(int)PieceType.Knight] = knightValue;
        PieceValues[(int)PieceType.Cannon] = cannonValue;
        PieceValues[(int)PieceType.Rook] = rookValue;

        checkingBonus = 150;
        checkingWithFewPiecesBonus = 350;
        kingMovesBonus = 50;
    }

    protected override double CalculateEvaluation(GameColor turnColor)
    {
        double eval = 0;

        //add check for the conclusion
        eval += checkWeight * CheckBonus;

        PiecesValueCounterPlayer -= (int)(MaxUnDefendedPieceValue * maxUndefendedPieceValueWeight);
        PiecesValueCounterEnemy -= (int)(MaxAttackingPieceValue * maxAttackingPieceValueWeight); 
        
        //add the pieces differences of the players
         // Adjust the weight as needed
        eval += pieceValueDiffWeight * (PiecesValueCounterPlayer - PiecesValueCounterEnemy);

        //add the pieces positions 
        eval += playerIntersectionWeight * (PlayerPiecesIntersectionEvaluateSum / PiecesCounterPlayer);
        eval -= enemyIntersectionWeight * (EnemyPiecesIntersectionEvaluateSum / PiecesCounterEnemy);
        
        return turnColor == GameColor.Red ? eval : -eval;
    }
}
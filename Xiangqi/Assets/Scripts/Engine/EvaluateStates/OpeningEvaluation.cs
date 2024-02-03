using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using UnityEngine;

public class OpeningEvaluation : EvaluationState
{
    
    // Weights
    private const double checkWeight = 1f; 
    private const double pieceValueDiffWeight = 1.5f; 
    private const double playerIntersectionWeight = 0.3f; 
    private const double enemyIntersectionWeight = 0.3f; 
    private const double maxUndefendedPieceValueWeight = 1f; 
    private const double maxAttackingPieceValueWeight = 0.1f;

    // Piece values
    private const int kingValue = 18700;
    private const int soliderValue = 100;
    private const int advisorValue = 400;
    private const int elephantValue = 450;
    private const int knightValue = 650;
    private const int cannonValue = 750;
    private const int rookValue = 900;

    public OpeningEvaluation() : base()
    {
        PieceValues[(int)PieceType.King] = kingValue;
        PieceValues[(int)PieceType.Soldier] = soliderValue;
        PieceValues[(int)PieceType.Advisor] = advisorValue;
        PieceValues[(int)PieceType.Elephant] = elephantValue;
        PieceValues[(int)PieceType.Knight] = knightValue;
        PieceValues[(int)PieceType.Cannon] = cannonValue;
        PieceValues[(int)PieceType.Rook] = rookValue;

        checkingBonus = 100;
        checkingWithFewPiecesBonus = 300;
        kingMovesBonus = 50;
    }

    protected override double CalculateEvaluation(GameColor turnColor)
    {
        Debug.Log("opening evaluation");
        double eval = 0;

        //add check for the conclusion
        eval += checkWeight * CheckBonus;

        PiecesValueCounterPlayer -= (int)(MaxUnDefendedPieceValue * maxUndefendedPieceValueWeight);
        PiecesValueCounterEnemy -= (int)(MaxAttackingPieceValue * maxAttackingPieceValueWeight); 

        //add the pieces differences of the players
         // Adjust the weight as needed
        eval += pieceValueDiffWeight * (PiecesValueCounterPlayer - PiecesValueCounterEnemy);
        //Debug.Log("PiecesValueCounterPlayer: " + PiecesValueCounterPlayer + " PiecesValueCounterEnemy: " + PiecesValueCounterEnemy + " eval: " + eval);

        //add the pieces positions 
        eval += playerIntersectionWeight * (PlayerPiecesIntersectionEvaluateSum / PiecesCounterPlayer);
        eval -= enemyIntersectionWeight * (EnemyPiecesIntersectionEvaluateSum / PiecesCounterEnemy);
        //Debug.Log("PlayerPiecesIntersectionEvaluateSum: " + PlayerPiecesIntersectionEvaluateSum + " EnemyPiecesIntersectionEvaluateSum: " + EnemyPiecesIntersectionEvaluateSum + " eval: " + eval);

        return turnColor == GameColor.Red ? eval : -eval;
    }
}
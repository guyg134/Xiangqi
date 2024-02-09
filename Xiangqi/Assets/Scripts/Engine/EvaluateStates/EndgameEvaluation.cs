using System.Numerics;

public class EndgameEvaluation : EvaluationState
{

    // Weights
    protected const double checkWeight = 2f; 
    protected const double pieceValueDiffWeight = 2; 
    protected const double playerIntersectionWeight = 0.5f; 
    protected const double enemyIntersectionWeight = 0.5f; 
    protected const double maxUndefendedPieceValueWeight = 1.2f; 
    protected const double maxAttackingPieceValueWeight = 0.2f;

    // Piece values
    protected const int kingValue = 18700;
    protected const int soliderValue = 350;
    protected const int advisorValue = 450;
    protected const int elephantValue = 400;
    protected const int knightValue = 600;
    protected const int cannonValue = 700;
    protected const int rookValue = 850;

    public EndgameEvaluation() : base()
    {
       PieceValues[(int)PieceType.King] = kingValue;
        PieceValues[(int)PieceType.Soldier] = soliderValue;
        PieceValues[(int)PieceType.Advisor] = advisorValue;
        PieceValues[(int)PieceType.Elephant] = elephantValue;
        PieceValues[(int)PieceType.Knight] = knightValue;
        PieceValues[(int)PieceType.Cannon] = cannonValue;
        PieceValues[(int)PieceType.Rook] = rookValue;

        checkingBonus = 300;
        checkingWithFewPiecesBonus = 500;
        kingMovesBonus = 80;
    }

    protected override double CalculateEvaluation(GameColor turnColor)
    {
        double eval = 0;
        PiecesValueCounterPlayer -= (int)(MaxUnDefendedPieceValue * maxUndefendedPieceValueWeight);
        PiecesValueCounterEnemy -= (int)(MaxAttackingPieceValue * maxAttackingPieceValueWeight); 
        
        //add check for the conclusion
        eval += checkWeight * CheckBonus;

        //add the pieces differences of the players
         // Adjust the weight as needed
        eval += pieceValueDiffWeight * (PiecesValueCounterPlayer - PiecesValueCounterEnemy);

        //add the pieces positions 
        eval += playerIntersectionWeight * (PlayerPiecesIntersectionEvaluateSum / PiecesCounterPlayer);
        eval -= enemyIntersectionWeight * (EnemyPiecesIntersectionEvaluateSum / PiecesCounterEnemy);

        return turnColor == GameColor.Red ? eval : -eval;
    }
}

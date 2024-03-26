
public class Constants 
{
    //board sizes constants
    public const int BOARD_SIZE = 90;
    public const int BOARD_WIDTH = 9;
    public const int BOARD_HEIGHT = 10;
    public const int CastleSize = 3;
    public const int CastleLeftX = 3;
    public const int CastleRightX = 5;

    //when player playing red pieces
    public const string StartFenRed = "rneakaenr/9/1c5c1/p1p1p1p1p/9/9/P1P1P1P1P/1C5C1/9/RNEAKAENR";
    //when player playing black pieces
    public const string StartFenBlack = "RNEAKAENR/9/1C5C1/P1P1P1P1P/9/9/p1p1p1p1p/1c5c1/9/rneakaenr";

    //game manager constants 
    public const int DownPlayerIndex = 0;
    public const int UpPlayerIndex = 1;

    public const int MaxRepiteMoves = 2;
    public const int OpeningMinMoves = 16;
    public const int MiddleGameMinMoves = 60;


}

public class EvaluateConstants
{
    public const int kingValue = 18700;
    public const int soliderValue = 300;
    public const int advisorValue = 400;
    public const int elephantValue = 450;
    public const int knightValue = 600;
    public const int cannonValue = 650;
    public const int rookValue = 800;

    public static readonly int[] PieceValues = new int[8] {0, kingValue, soliderValue, knightValue, elephantValue, cannonValue, advisorValue, rookValue};

    public const float PieceValueDiffWeight = 2.5f;
    public const float PiecesUnderAttackWeight = 0.5f;
    public const float PiecesIntersectionsWeight = 1f;
    public const float PiecesMobilityWeight = 10f;
    public const float KingSafetyWeight = 10f;
    public const float CastleSafetyWeight = 20f;

    public const int CheckValue = 200;
    public const int CheckMateValue = 20000;

    public const int Infinity = 9999999;
}

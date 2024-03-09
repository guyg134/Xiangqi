
public class Constants 
{
    //board sizes constants
    public const int BOARD_SIZE = 90;
    public const int BOARD_WIDTH = 9;
    public const int BOARD_HEIGHT = 10;
    public const int CastleSize = 3;

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

using System.Collections.Generic;
using System.Numerics;


public class Move
{

    public Position startPosition{ get; private set;}
    public Position endPosition{ get; private set;}
    private Piece movingPiece;
    private Piece eatenPiece;

    public Move(Move move)
    {
        startPosition = new Position(move.startPosition);
        endPosition = new Position(move.endPosition);
        movingPiece = move.movingPiece;
        eatenPiece = move.eatenPiece;
    }

    public Move(Position startPosition, Position endPosition, Piece movingPiece, Piece eatenPiece)
    {
        this.startPosition = startPosition;
        this.endPosition = endPosition;
        this.movingPiece = movingPiece;
        this.eatenPiece = eatenPiece;
    }

    public Move(int startX, int startY, int endX, int endY, Piece movingPiece, Piece eatenPiece) : this(new Position(startX, startY), new Position(endX, endY), movingPiece, eatenPiece)
    {
    }
    

    public Move(int startX, int startY, int endX, int endY, Piece movingPiece) : this(new Position(startX, startY), new Position(endX, endY), movingPiece, null)
    {
    }
   

     public Move(int startX, int startY, int endX, int endY) : this(new Position(startX, startY), new Position(endX, endY), null, null)
     {
     }
    

    public string Name => "" + startPosition.Name + endPosition.Name;

    public Position PositionStart
    {
        get { return PositionStart; }
    }

    public Position PositionEnd
    {
        get { return endPosition; }
    }

    public int StartX
    {
        get { return startPosition.x; }
    }

    public int StartY
    {
        get { return startPosition.y; }
    }

    public int EndX
    {
        get { return endPosition.x; }
    }

    public int EndY
    {
        get { return endPosition.y; }
    }

    public Piece MovingPiece
    {
        get { return movingPiece; }
        set { movingPiece = value; }
    }

    public Piece EatenPiece
    {
        get { return eatenPiece; }
        set { eatenPiece = value; }
    }

    public void ChangeSide()
    {
        startPosition.ChangeSidePosition();
        endPosition.ChangeSidePosition();
    }

    public static Move NameToMove(string name)
    {
        Position startPosition = new Position(name.Substring(0, 2));
        Position endPosition = new Position(name.Substring(2, 2));

        return new Move(startPosition, endPosition, null, null);
    }

    public void ReverseMove()
    {
        Position temp = startPosition;
        startPosition = endPosition;
        endPosition = temp;
    }

    public bool Equal(Move move)
    {
        return StartX == move.StartX && StartY == move.StartY && EndX == move.EndX && EndY == move.EndY;
    }

}


using System.Numerics;
using UnityEngine;


public class PieceFactory
{
    public static PieceMovement GetPiece(PieceType pieceInt, GameColor pieceColor)
    {
        switch((int)pieceInt)
        {
            case(1):
                return new King(pieceColor);
            case(2):
                return new Solider(pieceColor);
            case(3):
                return new Knight(pieceColor);
            case(4):
                return new Elephant(pieceColor);
            case(5):
                return new Cannon(pieceColor); 
            case(6):
                return new Advisor(pieceColor); 
            case(7):
                return new Rook(pieceColor);     
            default:
                return null;           
        }
    
    }
}

public abstract class PieceMovement
{
    private GameColor color;
    protected GameBoard gameBoard;
    private GameManager gameManager;
    

    public PieceMovement(GameColor color)
    {
        this.color = color;
        gameBoard = GameObject.FindGameObjectWithTag("Board").GetComponent<GameBoard>();
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }
    //each piece have implemention of getting the posssible moves
    //public abstract List<Vector2> getMovesOptions(int x, int y);

    public abstract BigInteger GetBitboardMoves(int x, int y, Board board);

    protected bool IsInPalace(int x, int y)
    {
        //return if piece is in palace, 5>x>3 and 2>y>0 or 9>y>7
        return x >= 3 && x <= 5 && ((y >= 0 && y <= 2) || (y >= 7 && y <= 9));
    }

    //return if the move is in board borders and if after the move there is check so its ilegal
    protected bool InBorders(int x, int y)
    {
        return GameBoard.CheckIfInBorders(x, y);
    }

    protected bool ThereIsEnemyKingInPos(Position pos, Board board)
    {
        Piece enemyKing = board.FindPiece(pos);
        //return if there is enemy king in the position
        return enemyKing && enemyKing.GetPieceType() == PieceType.King 
        && enemyKing.GetPieceColor() != color;
    }

    protected bool IsColorOnDownSide()
    {
        return gameManager.IsColorOnDownSide(color);
    }

}


    public class King : PieceMovement 
    {
        public King(GameColor color) : base(color)
        {
        }

        public override BigInteger GetBitboardMoves(int x, int y, Board board)
        {
            BigInteger movesBitboard = 0;
            //add all regular moves of king, up down left right if in palace
            if(IsInPalace(x+1, y))
                movesBitboard |= BitBoard.PosToBit(x+1, y);

            if(IsInPalace(x-1, y))
                movesBitboard |= BitBoard.PosToBit(x-1, y);

            if(IsInPalace(x, y-1))
                movesBitboard |= BitBoard.PosToBit(x, y-1);

            if(IsInPalace(x, y+1))
                movesBitboard |= BitBoard.PosToBit(x, y+1);

            //add eat king if there is no pieces in the way
            //if up king
            if(y > 5)
            {   
                int i = y-1;
                //go up until it find a piece
                while(i > 0 && !board.FindPiece(x, i))
                    i--;
                //if it find king add it to the moves than break
                if(ThereIsEnemyKingInPos(new Position(x, i), board))
                    movesBitboard |= BitBoard.PosToBit(x, i);
            }
            //if down king
            else
            {
                int i = y+1;
                //go up until it find a piece
                while(i < 9 && !board.FindPiece(x, i))
                    i++;
                //if it find king add it to the moves than break
                if(ThereIsEnemyKingInPos(new Position(x, i), board))
                    movesBitboard |= BitBoard.PosToBit(x, i);
            }
            return movesBitboard;
        }

    }


    public class Solider : PieceMovement
    {
        public Solider(GameColor color) : base(color)
        {
        }

        public override BigInteger GetBitboardMoves(int x, int y, Board board)
        {
            BigInteger movesBitboard = 0;            
            //if the solider is in the second half of the board he can move to the sides and back too
            if(IsColorOnDownSide())
            {
                //if the solider is in the second half of the board he can move to the sides and back too
                if(y>4){
                    if(InBorders(x+1, y))
                        movesBitboard |= BitBoard.PosToBit(x+1, y);
                    if(InBorders(x-1, y))
                        movesBitboard |= BitBoard.PosToBit(x-1, y); 
                }
                //if not it can move just forward
                if(InBorders(x, y+1))
                    movesBitboard |= BitBoard.PosToBit(x, y+1);
            }
            else
            {
                //if the solider is in the second half of the board he can move to the sides and back too
                if(y<5){
                    if(InBorders(x+1, y))
                        movesBitboard |= BitBoard.PosToBit(x+1, y);
                    if(InBorders(x-1, y))
                        movesBitboard |= BitBoard.PosToBit(x-1, y);
                }
                //if not it can move just forward
                if(InBorders(x, y-1))
                    movesBitboard |= BitBoard.PosToBit(x, y-1);
            }
            return movesBitboard;
        }

    }


    public class Knight : PieceMovement
    {
        public Knight(GameColor color) : base(color)
        {
        }

        public override BigInteger GetBitboardMoves(int x, int y, Board board)
        {
            BigInteger movesBitboard = 0;

            if(!board.FindPiece(x+1, y))
            {
                if(InBorders(x+2, y+1))
                    movesBitboard |= BitBoard.PosToBit(x+2, y+1);
                if(InBorders(x+2, y-1))
                    movesBitboard |= BitBoard.PosToBit(x+2, y-1);
                
            }
            if(!board.FindPiece(x-1, y))
            {
                if(InBorders(x-2, y+1))
                    movesBitboard |= BitBoard.PosToBit(x-2, y+1);
                if(InBorders(x-2, y-1))
                    movesBitboard |= BitBoard.PosToBit(x-2, y-1);
            }
            if(!board.FindPiece(x, y+1))
            {
                if(InBorders(x+1, y+2))
                    movesBitboard |= BitBoard.PosToBit(x+1, y+2);
                if(InBorders(x-1, y+2))
                    movesBitboard |= BitBoard.PosToBit(x-1, y+2);
            }
            if(!board.FindPiece(x, y-1))
            {
                if(InBorders(x+1, y-2))
                    movesBitboard |= BitBoard.PosToBit(x+1, y-2);
                if(InBorders(x-1, y-2))
                    movesBitboard |= BitBoard.PosToBit(x-1, y-2);
            }
            
            return movesBitboard;
        }

    }


    public class Elephant : PieceMovement
    {
        public Elephant(GameColor color) : base(color)
        {
        }

        public override BigInteger GetBitboardMoves(int x, int y, Board board)
        {
            BigInteger movesBitboard = 0;
            //if in borders, its down side and the move is y<5 or its up side and the move is y>4 and there is no piece in the way
            if(InBorders(x+2, y+2) && ((IsColorOnDownSide()&& y+2 < 5)||(!IsColorOnDownSide()&& y+2 > 4)) && !board.FindPiece(x+1,y+1))
                movesBitboard |= BitBoard.PosToBit(x+2, y+2);

            if(InBorders(x-2, y+2) && ((IsColorOnDownSide()&&y+2 < 5)||(!IsColorOnDownSide()&& y+2 > 4)) && !board.FindPiece(x-1,y+1))
                movesBitboard |= BitBoard.PosToBit(x-2, y+2);

            if(InBorders(x+2, y-2) && ((IsColorOnDownSide()&& y-2 < 5)||(!IsColorOnDownSide()&& y-2 > 4)) && !board.FindPiece(x+1,y-1))
                movesBitboard |= BitBoard.PosToBit(x+2, y-2);

            if(InBorders(x-2, y-2) && ((IsColorOnDownSide()&& y-2 < 5)||(!IsColorOnDownSide()&& y-2 > 4)) && !board.FindPiece(x-1,y-1))
                movesBitboard |= BitBoard.PosToBit(x-2, y-2);

            return movesBitboard;
        }

    }


    public class Cannon : PieceMovement
    {
        public Cannon(GameColor color) : base(color)
        {
        }

        public override BigInteger GetBitboardMoves(int x, int y, Board board)
        {
            BigInteger movesBitboard = 0;
            bool thereWasAPiece = false;

            int i;
            //y + i
            //this loop go throught until the y + i gets to the end of the board or it already find one piece and now it find another piece
            for (i = 1; InBorders(x, y + i) && (!thereWasAPiece || !board.FindPiece(x, y + i)); i++)
            {
                if (!thereWasAPiece && !board.FindPiece(x, y + i))
                {
                    movesBitboard |= BitBoard.PosToBit(x, y+i);
                }
                thereWasAPiece |= board.FindPiece(x, y + i);
            }
            //add the move if there is a piece on this position and there already was a piece
            if(InBorders(x, y + i) && thereWasAPiece && board.FindPiece(x, y + i))
                movesBitboard |= BitBoard.PosToBit(x, y + i);
            
            //init therewasapiece boolean
            thereWasAPiece = false;
            //x + i
            //this loop go throught until the y + i gets to the end of the board or it already find one piece and now it find another piece
            for (i = 1; InBorders(x + i, y) && (!thereWasAPiece || !board.FindPiece(x + i, y)); i++)
            {
                if (!thereWasAPiece && !board.FindPiece(x + i, y))
                {
                    movesBitboard |= BitBoard.PosToBit(x + i, y);
                }
                thereWasAPiece |= board.FindPiece(x + i, y);
            }
            //add the move if there is a piece on this position and there already was a piece
            if(InBorders(x + i, y) && thereWasAPiece && board.FindPiece(x + i, y))
                movesBitboard |= BitBoard.PosToBit(x + i, y);
            

            //init therewasapiece boolean
            thereWasAPiece = false;
            //x - i
            //this loop go throught until the y + i gets to the end of the board or it already find one piece and now it find another piece
            for (i = 1; InBorders(x - i, y) && (!thereWasAPiece || !board.FindPiece(x - i, y)); i++)
            {
                if (!thereWasAPiece && !board.FindPiece(x - i, y))
                {
                    movesBitboard |= BitBoard.PosToBit(x - i, y);
                }
                thereWasAPiece |= board.FindPiece(x - i, y);
            }
            //add the move if there is a piece on this position and there already was a piece
            if(InBorders(x - i, y) && thereWasAPiece && board.FindPiece(x - i, y))
                movesBitboard |= BitBoard.PosToBit(x - i, y);


            //init therewasapiece boolean
            thereWasAPiece = false;
            //y - i
            //this loop go throught until the y + i gets to the end of the board or it already find one piece and now it find another piece
            for (i = 1; InBorders(x, y - i) && (!thereWasAPiece || !board.FindPiece(x, y - i)); i++)
            {
                if (!thereWasAPiece && !board.FindPiece(x, y - i))
                {
                    movesBitboard |= BitBoard.PosToBit(x, y - i);
                }
                thereWasAPiece |= board.FindPiece(x, y - i);
            }
            //add the move if there is a piece on this position and there already was a piece
            if(InBorders(x, y - i) && thereWasAPiece && board.FindPiece(x, y - i))
                movesBitboard |= BitBoard.PosToBit(x, y - i);

            return movesBitboard;
        }

    }


    public class Advisor : PieceMovement
    {
        public Advisor(GameColor color) : base(color)
        {
        }

        public override BigInteger GetBitboardMoves(int x, int y, Board board)
        {
            BigInteger movesBitboard = 0;

            if(IsInPalace(x+1, y+1))
                movesBitboard |= BitBoard.PosToBit(x+1, y+1);

            if(IsInPalace(x-1, y+1))
                movesBitboard |= BitBoard.PosToBit(x-1, y+1);

            if(IsInPalace(x+1, y-1))
                movesBitboard |= BitBoard.PosToBit(x+1, y-1);

            if(IsInPalace(x-1, y-1))
                movesBitboard |= BitBoard.PosToBit(x-1, y-1);

            return movesBitboard;
        }

    }


    public class Rook : PieceMovement
    {
        public Rook(GameColor color) : base(color)
        {
        }

        public override BigInteger GetBitboardMoves(int x, int y, Board board)
        {
            BigInteger movesBitboard = 0;
            bool thereWasAPiece = false;

            //this loop go throught until the x + i, x - i, y + i, y - i gets to the end of the board or it find a piece after it already added the move
            for(int i = 1;InBorders(x+i,y) && !thereWasAPiece; i++){
                //add the move
                movesBitboard |= BitBoard.PosToBit(x+i, y);
                //if there is piece on the way break
                if(board.FindPiece(x+i, y))
                    thereWasAPiece = true;
            }
            
            thereWasAPiece = false;
            for(int i = 1;InBorders(x,y+i) && !thereWasAPiece; i++){
                //if the move is not valid continue(king under check after the move)
                movesBitboard |= BitBoard.PosToBit(x, y+i);
                //if there is piece on the way break
                if(board.FindPiece(x, y+i))
                    thereWasAPiece = true;
            }

            thereWasAPiece = false;
            for(int i = 1;InBorders(x-i,y) && !thereWasAPiece; i++){
                //if the move is not valid continue(king under check after the move)
                movesBitboard |= BitBoard.PosToBit(x-i, y);
                //if there is piece on the way break
                if(board.FindPiece(x-i, y))
                    thereWasAPiece = true;
            }

            thereWasAPiece = false;
            for(int i = 1;InBorders(x,y-i) && !thereWasAPiece; i++){
                //if the move is not valid continue(king under check after the move)
                movesBitboard |= BitBoard.PosToBit(x, y-i);
                //if there is piece on the way break
                if(board.FindPiece(x, y-i))
                    thereWasAPiece = true;
            }
            return movesBitboard;
    }

}





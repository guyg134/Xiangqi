using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography.X509Certificates;
using Unity.Mathematics;
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
    

    public PieceMovement(GameColor color)
    {
        this.color = color;
        gameBoard = GameObject.FindGameObjectWithTag("Board").GetComponent<GameBoard>();
    }
    //each piece have implemention of getting the posssible moves
    //public abstract List<Vector2> getMovesOptions(int x, int y);

    public abstract BigInteger GetBitboardMoves(int x, int y, Board board);

    protected bool IsInPalace(int x, int y)
    {
        //return if piece is in palace
        return x >= 3 && x <= 5 && ((y >= 0 && y <= 2) || (y >= 7 && y <= 9));
    }

    //return if the move is in board borders and if after the move there is check so its ilegal
    protected bool InBorders(int x, int y)
    {
        return GameBoard.CheckIfInBorders(x, y);
    }

    protected bool ThereIsKingInPos(int x, int y, Board board)
    {
        return board.FindPiece(x, y, PieceType.King) != null;
    }

    protected bool IsColorOnDownSide()
    {
        return GameObject.FindWithTag("GameManager").GetComponent<GameManager>().IsColorOnDownSide(color);
        //return (!currentPlayer.PlayOnDownSide() && (color != currentPlayer.GetPlayerColor())) || (currentPlayer.PlayOnDownSide() && (int)color == (int)currentPlayer.GetPlayerColor());
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

            if(IsInPalace(x+1, y) && InBorders(x+1, y))
                movesBitboard |= BitBoard.PosToBitInteger(x+1, y);

            if(IsInPalace(x-1, y) && InBorders(x-1, y))
                movesBitboard |= BitBoard.PosToBitInteger(x-1, y);

            if(IsInPalace(x, y-1) && InBorders(x, y-1))
                movesBitboard |= BitBoard.PosToBitInteger(x, y-1);

            if(IsInPalace(x, y+1) && InBorders(x, y+1))
                movesBitboard |= BitBoard.PosToBitInteger(x, y+1);

            //add eat king if there is no pieces in the way
            //if up king
            if(y > 5)
            {
                for(int i = y-1; i >= 0; i--)
                {
                    //if there is piece on the way
                    if(board.FindPiece(x, i))
                    {
                        //if it find king add it to the int than break
                        if(board.FindPiece(x, i, PieceType.King))
                            movesBitboard |= BitBoard.PosToBitInteger(x, i);
                        break;
                    }
                }
            }
            //if down king
            else
            {
                for(int i = y+1; i < 10; i++)
                {
                    //if there is piece on the way
                    if(board.FindPiece(x, i))
                    {
                        //if it find king add it to the int than break
                        if(board.FindPiece(x, i, PieceType.King))
                            movesBitboard |= BitBoard.PosToBitInteger(x, i);
                        break;
                    }
                }
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
                        movesBitboard |= BitBoard.PosToBitInteger(x+1, y);
                    if(InBorders(x-1, y))
                        movesBitboard |= BitBoard.PosToBitInteger(x-1, y); 
                }
                //if not it can move just forward
                if(InBorders(x, y+1))
                    movesBitboard |= BitBoard.PosToBitInteger(x, y+1);
            }
            else
            {
                //if the solider is in the second half of the board he can move to the sides and back too
                if(y<5){
                    if(InBorders(x+1, y))
                        movesBitboard |= BitBoard.PosToBitInteger(x+1, y);
                    if(InBorders(x-1, y))
                        movesBitboard |= BitBoard.PosToBitInteger(x-1, y);
                }
                //if not it can move just forward
                if(InBorders(x, y-1))
                    movesBitboard |= BitBoard.PosToBitInteger(x, y-1);
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
                    movesBitboard |= BitBoard.PosToBitInteger(x+2, y+1);
                if(InBorders(x+2, y-1))
                    movesBitboard |= BitBoard.PosToBitInteger(x+2, y-1);
                
            }
            if(!board.FindPiece(x-1, y))
            {
                if(InBorders(x-2, y+1))
                    movesBitboard |= BitBoard.PosToBitInteger(x-2, y+1);
                if(InBorders(x-2, y-1))
                    movesBitboard |= BitBoard.PosToBitInteger(x-2, y-1);
            }
            if(!board.FindPiece(x, y+1))
            {
                if(InBorders(x+1, y+2))
                    movesBitboard |= BitBoard.PosToBitInteger(x+1, y+2);
                if(InBorders(x-1, y+2))
                    movesBitboard |= BitBoard.PosToBitInteger(x-1, y+2);
            }
            if(!board.FindPiece(x, y-1))
            {
                if(InBorders(x+1, y-2))
                    movesBitboard |= BitBoard.PosToBitInteger(x+1, y-2);
                if(InBorders(x-1, y-2))
                    movesBitboard |= BitBoard.PosToBitInteger(x-1, y-2);
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

            if(InBorders(x+2, y+2) && ((IsColorOnDownSide()&& y+2 < 5)||(!IsColorOnDownSide()&& y+2 > 4)) && !board.FindPiece(x+1,y+1))
                movesBitboard |= BitBoard.PosToBitInteger(x+2, y+2);

            if(InBorders(x-2, y+2) && ((IsColorOnDownSide()&&y+2 < 5)||(!IsColorOnDownSide()&& y+2 > 4)) && !board.FindPiece(x-1,y+1))
                movesBitboard |= BitBoard.PosToBitInteger(x-2, y+2);

            if(InBorders(x+2, y-2) && ((IsColorOnDownSide()&& y-2 < 5)||(!IsColorOnDownSide()&& y-2 > 4)) && !board.FindPiece(x+1,y-1))
                movesBitboard |= BitBoard.PosToBitInteger(x+2, y-2);

            if(InBorders(x-2, y-2) && ((IsColorOnDownSide()&& y-2 < 5)||(!IsColorOnDownSide()&& y-2 > 4)) && !board.FindPiece(x-1,y-1))
                movesBitboard |= BitBoard.PosToBitInteger(x-2, y-2);

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
            //this loop go throught until the y + i gets to the end of the board or it find a piece after it already find a first piec in the way like cannons in xianqi
            for(int i = 1; GameBoard.CheckIfInBorders(x, y+i); i++){
                //if the move is not valid continue(king under check after the move)
                if(!InBorders(x, y+i))
                    continue;

                if(!thereWasAPiece){
                    if(board.FindPiece(x, y+i))
                        thereWasAPiece = true;
                    else
                        movesBitboard |= BitBoard.PosToBitInteger(x, y+i);
                }
                else{
                    if(board.FindPiece(x, y+i)){
                        movesBitboard |= BitBoard.PosToBitInteger(x, y+i);
                        break;
                    }
                }
            }
            //init therewasapiece boolean
            thereWasAPiece = false;
            //this loop go throught until the y + i gets to the end of the board or it find a piece after it already find a first piec in the way like cannons in xianqi
            for(int i = 1;GameBoard.CheckIfInBorders(x+i,i); i++){
                //if the move is not valid continue(king under check after the move)
                if(!InBorders(x+i, y))
                    continue;

                if(!thereWasAPiece){
                    if(board.FindPiece(x+i,y))
                        thereWasAPiece = true;
                    else
                        movesBitboard |= BitBoard.PosToBitInteger(x+i, y);
                }
                else{
                    if(board.FindPiece(x+i,y)){
                        movesBitboard |= BitBoard.PosToBitInteger(x+i, y);
                        break;
                    }
                }
            }
            //init therewasapiece boolean
            thereWasAPiece = false;
            for(int i = 1;GameBoard.CheckIfInBorders(x-i,y); i++){
                //if the move is not valid continue(king under check after the move)
                if(!InBorders(x-i, y))
                    continue;

                if(!thereWasAPiece){
                    if(board.FindPiece(x-i,y))
                        thereWasAPiece = true;
                    else
                        movesBitboard |= BitBoard.PosToBitInteger(x-i, y);
                }
                else{
                    if(board.FindPiece(x-i,y)){
                        movesBitboard |= BitBoard.PosToBitInteger(x-i, y);
                        break;
                    }
                }
            }
            //init therewasapiece boolean
            thereWasAPiece = false;
            for(int i = 1; GameBoard.CheckIfInBorders(x, y-i); i++){
                //if the move is not valid continue(king under check after the move)
                if(!InBorders(x, y-i))
                    continue;

                if(!thereWasAPiece){
                    if(board.FindPiece(x, y-i))
                        thereWasAPiece = true;
                    else
                        movesBitboard |= BitBoard.PosToBitInteger(x, y-i);
                }
                else{
                    if(board.FindPiece(x, y-i)){
                        movesBitboard |= BitBoard.PosToBitInteger(x, y-i);
                        break;
                    }
                }
            }

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

            if(IsInPalace(x+1, y+1) && InBorders(x+1, y+1))
                movesBitboard |= BitBoard.PosToBitInteger(x+1, y+1);

            if(IsInPalace(x-1, y+1) && InBorders(x-1, y+1))
                movesBitboard |= BitBoard.PosToBitInteger(x-1, y+1);

            if(IsInPalace(x+1, y-1) && InBorders(x+1, y-1))
                movesBitboard |= BitBoard.PosToBitInteger(x+1, y-1);

            if(IsInPalace(x-1, y-1) && InBorders(x-1, y-1))
                movesBitboard |= BitBoard.PosToBitInteger(x-1, y-1);

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

            for(int i = 1;GameBoard.CheckIfInBorders(x+i,y); i++){
                //if the move is not valid continue(king under check after the move)
                if(!InBorders(x+i, y))
                    continue;

                movesBitboard |= BitBoard.PosToBitInteger(x+i, y);
                if(board.FindPiece(x+i, y))
                    break;
            }

            for(int i = 1;GameBoard.CheckIfInBorders(x,y+i); i++){
                //if the move is not valid continue(king under check after the move)
                if(!InBorders(x, y+i))
                    continue;

                movesBitboard |= BitBoard.PosToBitInteger(x, y+i);
                if(board.FindPiece(x, y+i))
                    break;
            }

            for(int i = 1;GameBoard.CheckIfInBorders(x-i,y); i++){
                //if the move is not valid continue(king under check after the move)
                if(!InBorders(x-i, y))
                    continue;

                movesBitboard |= BitBoard.PosToBitInteger(x-i, y);
                if(board.FindPiece(x-i, y))
                    break;
            }

            for(int i = 1;GameBoard.CheckIfInBorders(x,y-i); i++){
                //if the move is not valid continue(king under check after the move)
                if(!InBorders(x, y-i))
                    continue;
                    
                movesBitboard |= BitBoard.PosToBitInteger(x, y-i);
                if(board.FindPiece(x, y-i))
                    break;
            }
            return movesBitboard;
    }

}






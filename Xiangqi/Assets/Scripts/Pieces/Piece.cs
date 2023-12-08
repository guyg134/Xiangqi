using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class Piece : MonoBehaviour
{
    public enum PieceType {King=1, Soldier=2, Knight=3, Elephant=4, Cannon=5, Advisor=6, Rook=7};
    public enum PieceColor{Red = 0, Black = 1};
    private PieceType pieceType;
    private PieceColor pieceColor;
    private int x;
    private int y;
    private PieceMovement pieceMovement;

    public void SetPiece(PieceType pieceType, PieceColor pieceColor, int x, int y)
    {
        this.pieceType = pieceType;
        this.pieceColor = pieceColor;
        pieceMovement = PieceFactory.GetPiece(pieceType, pieceColor);
        this.x = x;
        this.y = y;
        //print(pieceMovement == null);
        //GameObject.FindGameObjectWithTag("Board").GetComponent<Board>().drawPiece(x, y, pieceType, pieceColor);
    }

    public PieceType GetPieceType()
    {
        return pieceType;
    }

    public BigInteger GetPieceBitboardMove()
    {
        return pieceMovement.GetBitboardMoves(x, y);
    }

    public void MovePiece(Vector2 newPos)
    {
        Move move = new Move(x, y, (int)newPos.x, (int)newPos.y);
        //update the x and y axis of the piece
        x = (int)newPos.x;
        y = (int)newPos.y;
        //update the piece in board
        GameObject.FindGameObjectWithTag("Board").GetComponent<Board>().updatePieceInBoard(move);
    }

    public PieceColor GetPieceColor()
    {
        return pieceColor;
    }

    public BigInteger GetMoves()
    {
        //print(pieceMovement == null);
        return this.pieceMovement.GetBitboardMoves(x, y);
    }

    public void GetDots()
    {
        GameObject.FindGameObjectWithTag("Board").GetComponent<Board>().drawDots(GetMoves(), this.gameObject, pieceMovement.GetBitboardMoves(x, y));
    }


public class PieceFactory
{
    public static PieceMovement GetPiece(PieceType pieceInt, PieceColor pieceColor)
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
                print("null");
                return null;           
        }
    
    }
}


public abstract class PieceMovement
{
    private PieceColor color;
    protected Board board;
    

    public PieceMovement(PieceColor color)
    {
        this.color = color;
        board = GameObject.FindGameObjectWithTag("Board").GetComponent<Board>();
    }
    //each piece have implemention of getting the posssible moves
    //public abstract List<Vector2> getMovesOptions(int x, int y);

    public abstract BigInteger GetBitboardMoves(int x, int y);

    protected bool isInPalace(int x, int y)
    {
        //return if piece is in palace
        return x >= 3 && x <= 5 && ((y >= 0 && y <= 2) || (y >= 7 && y <= 9));
    }

    //for vector2 moves 
    protected bool checkIfThereIsPieceSameColor(int x, int y)
    {
        Piece piece = board.GetPieceAtPosition(x, y);
        if(!piece)
            return false;
        return color == piece.GetPieceColor();
    }

    //return if the move is in board borders and if after the move there is check so its ilegal
    protected bool ValidMove(int startX, int startY, int endX, int endY)
    {
        Move move = new Move(startX, startY, endX, endY);
        return Board.checkIfInBorders(endX, endY) && !board.IsKingUnderAttackAfterMove(move);
    }

    protected bool isPlayOnDownSide()
    {
        return GameObject.FindWithTag("GameManager").GetComponent<GameManager>().getTurnPlayer().playOnDownSide();
    }

    protected PieceColor getColor()
    {
        return color;
    }
}


    public class King : PieceMovement 
    {
        public King(PieceColor color) : base(color)
        {
        }

        public override BigInteger GetBitboardMoves(int x, int y)
        {
            BigInteger movesBitboard = 0;

            if(isInPalace(x+1, y) && ValidMove(x, y, x+1, y))
                movesBitboard |= BitBoard.PosToBitInteger(x+1, y);

            if(isInPalace(x-1, y) && ValidMove(x, y, x-1, y))
                movesBitboard |= BitBoard.PosToBitInteger(x-1, y);

            if(isInPalace(x, y-1) && ValidMove(x, y, x, y-1))
                movesBitboard |= BitBoard.PosToBitInteger(x, y-1);

            if(isInPalace(x, y+1) && ValidMove(x, y, x, y+1))
                movesBitboard |= BitBoard.PosToBitInteger(x, y+1);

            return movesBitboard;
        }

        /*public override List<Vector2> getMovesOptions(int x, int y)
        {
            List<Vector2> transforms = new List<Vector2>();

            if(isInPalace(x+1, y) && !checkIfThereIsPieceSameColor(x+1, y))
                transforms.Add(new Vector2(x+1, y));

            if(isInPalace(x-1, y) && !checkIfThereIsPieceSameColor(x-1, y))
                transforms.Add(new Vector2(x-1, y));

            if(isInPalace(x, y-1) && !checkIfThereIsPieceSameColor(x, y-1))
                transforms.Add(new Vector2(x, y-1));

            if(isInPalace(x, y+1) && !checkIfThereIsPieceSameColor(x, y+1))
                transforms.Add(new Vector2(x, y+1));

            return transforms;
        }*/

       /* public bool isKingUnderAttack(int[] piecesPositions, int x, int y)
        {

            //check if rook, king and cannon check
            bool therewasapiece = false;
            for(int i = 1;Board.checkIfInBorders(x,y-i); i++){
                //there is piece in this pos
                if(piecesPositions[(y-i)*10 + x] != 0){
                    //if the piece is not in the same color check what piece is it
                    if(piecesPositions[(y-i)*10 + x] / 10 != (int)getColor()){
                        if(!therewasapiece){
                            //if rook or king return true
                            if(piecesPositions[((y-i)*10) + x]%10 == 7 || piecesPositions[((y-i)*10) + x]%10 == 1)
                                return true;
                        }
                        else{
                            if(piecesPositions[((y-i)*10) + x]%10 == 5)
                                return true;
                            break;
                        }
                    }
                    if(therewasapiece)
                        break;
                    therewasapiece = true;
                }
            }

            therewasapiece = false;
            for(int i = 1;Board.checkIfInBorders(x,y+i); i++){
                //there is piece in this pos
                if(piecesPositions[(y+i)*10 + x] != 0){
                    //if the piece is not in the same color check what piece is it
                    if(piecesPositions[(y+i)*10 + x] / 10 != (int)getColor()){
                        if(!therewasapiece){
                            //if rook or king return true
                            if(piecesPositions[(y+i)*10 + x]%10 == 7 || piecesPositions[(y+i)*10 + x]%10 == 1)
                                return true;
                        }
                        else{
                            if(piecesPositions[(y+i)*10 + x]%10 == 5)
                                return true;
                            break;
                        }
                    }
                    if(therewasapiece)
                        break;
                    therewasapiece = true;
                }
            }

            therewasapiece = false;
            for(int i = 1;Board.checkIfInBorders(x-i,y); i++){
                //there is piece in this pos
                if(piecesPositions[y*10 + (x-i)] != 0){
                    //if the piece is not in the same color check what piece is it
                    if(piecesPositions[y*10 + (x-i)] / 10 != (int)getColor()){
                        if(!therewasapiece){
                            //if rook return true
                            if(piecesPositions[y*10 + (x-i)]%10 == 7)
                                return true;
                        }
                        else{
                            if(piecesPositions[y*10 + (x-i)]%10 == 5)
                                return true;
                            break;
                        }
                    }
                    if(therewasapiece)
                        break;
                    therewasapiece = true;
                }
            }

            therewasapiece = false;
            for(int i = 1;Board.checkIfInBorders(x+i,y); i++){
                //there is piece in this pos
                if(piecesPositions[y*10 + (x+i)] != 0){
                    //if the piece is not in the same color check what piece is it
                    if(piecesPositions[y*10 + (x+i)] / 10 != (int)getColor()){
                        if(!therewasapiece){
                            //if rook return true
                            if(piecesPositions[y*10 + (x+i)]%10 == 7)
                                return true;
                        }
                        else{
                            if(piecesPositions[y*10 + (x+i)]%10 == 5)
                                return true;
                            break;
                        }
                    }
                    if(therewasapiece)
                        break;
                    therewasapiece = true;
                }
            }
            //check if there is solider that checks the king
            //check the sides
            if((Board.checkIfInBorders(x+1,y)&&piecesPositions[y*10 + (x+1)] / 10 != (int)getColor()&&piecesPositions[y*10 + (x+1)]%10 == 2)
            ||(Board.checkIfInBorders(x-1,y)&&piecesPositions[y*10 + (x-1)] / 10 != (int)getColor()&&piecesPositions[y*10 + (x-1)]%10 ==2))
                return true;
            //check the up and down if the king is up or down of the board
            if(y>5)
            {
                if(Board.checkIfInBorders(x,y-1)&&piecesPositions[(y-1)*10 + x] / 10 != (int)getColor() && piecesPositions[(y-1)*10 + x]%10 == 2)
                    return true;
            }
            else
            {
                if(Board.checkIfInBorders(x,y+1)&&piecesPositions[(y+1)*10 + x] / 10 != (int)getColor() && piecesPositions[(y+1)*10 + x]%10 == 2)
                    return true;
            }

            //check if there is knight that checks the king
            if(Board.checkIfInBorders(x+1, y+1))
            {
                if(Board.checkIfInBorders(x+2, y+1) && piecesPositions[(y+1)*10 + (x+2)]/10 != (int)getColor() && piecesPositions[(y+1)*10 + (x+2)]%10 == 3)
                    return true;
                if(Board.checkIfInBorders(x+1, y+2) && piecesPositions[(y+2)*10 + (x+1)]/10 != (int)getColor() && piecesPositions[(y+2)*10 + (x+1)]%10 == 3)
                    return true;
            }
            if(Board.checkIfInBorders(x-1, y+1))
            {
                if(Board.checkIfInBorders(x-2, y+1) && piecesPositions[(y+1)*10 + (x-2)]/10 != (int)getColor() && piecesPositions[(y+1)*10 + (x-2)]%10 == 3)
                    return true;
                if(Board.checkIfInBorders(x-1, y+2) && piecesPositions[(y+2)*10 + (x-1)]/10 != (int)getColor() && piecesPositions[(y+2)*10 + (x-1)]%10 == 3)
                    return true;
            }
            if(Board.checkIfInBorders(x+1, y-1))
            {
                if(Board.checkIfInBorders(x+2, y-1) && piecesPositions[(y-1)*10 + (x+2)]/10 != (int)getColor() && piecesPositions[(y-1)*10 + (x+2)]%10 == 3)
                    return true;
                if(Board.checkIfInBorders(x+1, y-2) && piecesPositions[(y-2)*10 + (x+1)]/10 != (int)getColor() && piecesPositions[(y-2)*10 + (x+1)]%10 == 3)
                    return true;
            }
            if(Board.checkIfInBorders(x-1, y-1))
            {
                if(Board.checkIfInBorders(x-2, y-1) && piecesPositions[(y-1)*10 + (x-2)]/10 != (int)getColor() && piecesPositions[(y-1)*10 + (x-2)]%10 == 3)
                    return true;
                if(Board.checkIfInBorders(x-1, y-2) && piecesPositions[(y-2)*10 + (x-1)]/10 != (int)getColor() && piecesPositions[(y-2)*10 + (x-1)]%10 == 3)
                    return true;
            }
            return false;
        }*/
    }


    public class Solider : PieceMovement
    {
        public Solider(PieceColor color) : base(color)
        {
        }

        public override BigInteger GetBitboardMoves(int x, int y)
        {
            BigInteger movesBitboard = 0;            
            //if the solider is in the second half of the board he can move to the sides and back too
            if(isPlayOnDownSide())
            {
                //if the solider is in the second half of the board he can move to the sides and back too
                if(y>4){
                    if(ValidMove(x, y, x+1, y))
                        movesBitboard |= BitBoard.PosToBitInteger(x+1, y);
                    if(ValidMove(x, y, x-1, y))
                        movesBitboard |= BitBoard.PosToBitInteger(x-1, y); 
                }
                //if not it can move just forward
                if(ValidMove(x, y, x, y+1))
                    movesBitboard |= BitBoard.PosToBitInteger(x, y+1);
            }
            else
            {
                //if the solider is in the second half of the board he can move to the sides and back too
                if(y<5){
                    if(ValidMove(x, y, x+1, y))
                        movesBitboard |= BitBoard.PosToBitInteger(x+1, y);
                    if(ValidMove(x, y, x-1, y))
                        movesBitboard |= BitBoard.PosToBitInteger(x-1, y);
                }
                //if not it can move just forward
                if(ValidMove(x, y, x, y-1))
                    movesBitboard |= BitBoard.PosToBitInteger(x, y-1);
            }
            return movesBitboard;
        }

        /*public override List<Vector2> getMovesOptions(int x, int y)
        {
            List<Vector2> transforms = new List<Vector2>();
            //if the solider is in the second half of the board he can move to the sides and back too
            if(isPlayOnDownSide())
            {
                //if the solider is in the second half of the board he can move to the sides and back too
                if(y>4){
                    if(Board.checkIfInBorders(x+1, y) && !checkIfThereIsPieceSameColor(x+1, y))
                        transforms.Add(new Vector2(x+1, y));
                    if(Board.checkIfInBorders(x-1, y) && !checkIfThereIsPieceSameColor(x-1, y))
                        transforms.Add(new Vector2(x-1, y)); 
                }
                //if not it can move just forward
                if(Board.checkIfInBorders(x, y+1) && !checkIfThereIsPieceSameColor(x, y+1))
                    transforms.Add(new Vector2(x, y+1));
            }
            else
            {
                //if the solider is in the second half of the board he can move to the sides and back too
                if(y<5){
                    if(Board.checkIfInBorders(x+1, y) && !checkIfThereIsPieceSameColor(x+1, y))
                        transforms.Add(new Vector2(x+1, y));
                    if(Board.checkIfInBorders(x-1, y) && !checkIfThereIsPieceSameColor(x-1, y))
                        transforms.Add(new Vector2(x-1, y));
                }
                //if not it can move just forward
                if(Board.checkIfInBorders(x, y-1) && !checkIfThereIsPieceSameColor(x, y-1))
                    transforms.Add(new Vector2(x, y-1));
            }
            return transforms;
        }*/
    }


    public class Knight : PieceMovement
    {
        public Knight(PieceColor color) : base(color)
        {
        }

        public override BigInteger GetBitboardMoves(int x, int y)
        {
            BigInteger movesBitboard = 0;

            if(!board.checkIfThereIsPiece(x+1, y))
            {
                if(ValidMove(x, y, x+2, y+1))
                    movesBitboard |= BitBoard.PosToBitInteger(x+2, y+1);
                if(ValidMove(x, y, x+2, y-1))
                    movesBitboard |= BitBoard.PosToBitInteger(x+2, y-1);
                
            }
            if(!board.checkIfThereIsPiece(x-1, y))
            {
                if(ValidMove(x, y, x-2, y+1))
                    movesBitboard |= BitBoard.PosToBitInteger(x-2, y+1);
                if(ValidMove(x, y, x-2, y-1))
                    movesBitboard |= BitBoard.PosToBitInteger(x-2, y-1);
            }
            if(!board.checkIfThereIsPiece(x, y+1))
            {
                if(ValidMove(x, y, x+1, y+2))
                    movesBitboard |= BitBoard.PosToBitInteger(x+1, y+2);
                if(ValidMove(x, y, x-1, y+2))
                    movesBitboard |= BitBoard.PosToBitInteger(x-1, y+2);
            }
            if(!board.checkIfThereIsPiece(x, y-1))
            {
                if(ValidMove(x, y, x+1, y-2))
                    movesBitboard |= BitBoard.PosToBitInteger(x+1, y-2);
                if(ValidMove(x, y, x-1, y-2))
                    movesBitboard |= BitBoard.PosToBitInteger(x-1, y-2);
            }
            
            return movesBitboard;
        }

        /*public override List<Vector2> getMovesOptions(int x, int y)
        {
            List<Vector2> transforms = new List<Vector2>();

            if(!board.checkIfThereIsPiece(x+1, y))
            {
                if(Board.checkIfInBorders(x+2, y+1) && !checkIfThereIsPieceSameColor(x+2, y+1))
                    transforms.Add(new Vector2(x+2, y+1));
                if(Board.checkIfInBorders(x+2, y-1) && !checkIfThereIsPieceSameColor(x+2, y-1))
                    transforms.Add(new Vector2(x+2, y-1));
                
            }
            if(!board.checkIfThereIsPiece(x-1, y))
            {
                if(Board.checkIfInBorders(x-2, y+1) && !checkIfThereIsPieceSameColor(x-2, y+1))
                    transforms.Add(new Vector2(x-2, y+1));
                if(Board.checkIfInBorders(x-2, y-1) && !checkIfThereIsPieceSameColor(x-2, y-1))
                    transforms.Add(new Vector2(x-2, y-1));
            }
            if(!board.checkIfThereIsPiece(x, y+1))
            {
                if(Board.checkIfInBorders(x+1, y+2) && !checkIfThereIsPieceSameColor(x+1, y+2))
                    transforms.Add(new Vector2(x+1, y+2));
                if(Board.checkIfInBorders(x-1, y+2) && !checkIfThereIsPieceSameColor(x-1, y+2))
                    transforms.Add(new Vector2(x-1, y+2));
            }
            if(!board.checkIfThereIsPiece(x, y-1))
            {
                if(Board.checkIfInBorders(x+1, y-2) && !checkIfThereIsPieceSameColor(x+1, y-2))
                    transforms.Add(new Vector2(x+1, y-2));
                if(Board.checkIfInBorders(x-1, y-2) && !checkIfThereIsPieceSameColor(x-1, y-2))
                    transforms.Add(new Vector2(x-1, y-2));
            }
            
            return transforms;
        }*/
    }


    public class Elephant : PieceMovement
    {
        public Elephant(PieceColor color) : base(color)
        {
        }

        public override BigInteger GetBitboardMoves(int x, int y)
        {
            BigInteger movesBitboard = 0;

            if(((isPlayOnDownSide()&& y+2 < 5)||(!isPlayOnDownSide()&& y+2 > 4)) && !board.checkIfThereIsPiece(x+1,y+1) && ValidMove(x, y, x+2, y+2))
                movesBitboard |= BitBoard.PosToBitInteger(x+2, y+2);

            if(((isPlayOnDownSide()&&y+2 < 5)||(!isPlayOnDownSide()&& y+2 > 4)) && !board.checkIfThereIsPiece(x-1,y+1) && ValidMove(x, y, x-2, y+2))
                movesBitboard |= BitBoard.PosToBitInteger(x-2, y+2);

            if(((isPlayOnDownSide()&& y-2 < 5)||(!isPlayOnDownSide()&& y-2 > 4)) && !board.checkIfThereIsPiece(x+1,y-1) && ValidMove(x, y, x+2, y-2))
                movesBitboard |= BitBoard.PosToBitInteger(x+2, y-2);

            if(((isPlayOnDownSide()&& y-2 < 5)||(!isPlayOnDownSide()&& y-2 > 4)) && !board.checkIfThereIsPiece(x-1,y-1) && ValidMove(x, y, x-2, y-2))
                movesBitboard |= BitBoard.PosToBitInteger(x-2, y-2);

            return movesBitboard;
        }

        /*public override List<Vector2> getMovesOptions(int x, int y)
        {
            List<Vector2> transforms = new List<Vector2>();
            if(Board.checkIfInBorders(x+2,y+2) && ((isPlayOnDownSide()&& y+2 < 5)||(!isPlayOnDownSide()&& y+2 > 4)) && !board.checkIfThereIsPiece(x+1,y+1) && !checkIfThereIsPieceSameColor(x+2, y+2))
                transforms.Add(new Vector2(x+2, y+2));

            if(Board.checkIfInBorders(x-2,y+2) && ((isPlayOnDownSide()&&y+2 < 5)||(!isPlayOnDownSide()&& y+2 > 4)) && !board.checkIfThereIsPiece(x-1,y+1) && !checkIfThereIsPieceSameColor(x-2, y+2))
                transforms.Add(new Vector2(x-2, y+2));

            if(Board.checkIfInBorders(x+2,y-2) && ((isPlayOnDownSide()&& y-2 < 5)||(!isPlayOnDownSide()&& y-2 > 4)) && !board.checkIfThereIsPiece(x+1,y-1) && !checkIfThereIsPieceSameColor(x+2, y-2))
                transforms.Add(new Vector2(x+2, y-2));

            if(Board.checkIfInBorders(x-2,y-2) && ((isPlayOnDownSide()&& y-2 < 5)||(!isPlayOnDownSide()&& y-2 > 4)) && !board.checkIfThereIsPiece(x-1,y-1) && !checkIfThereIsPieceSameColor(x-2, y-2))
                transforms.Add(new Vector2(x-2, y-2));
            return transforms;
        }*/
    }


    public class Cannon : PieceMovement
    {
        public Cannon(PieceColor color) : base(color)
        {
        }

        public override BigInteger GetBitboardMoves(int x, int y)
        {
            BigInteger movesBitboard = 0;
            bool thereWasAPiece = false;
            //this loop go throught until the y + i gets to the end of the board or it find a piece after it already find a first piec in the way like cannons in xianqi
            for(int i = 1; Board.checkIfInBorders(x, y+i); i++){
                //if the move is not valid continue(king under check after the move)
                if(!ValidMove(x, y, x, y+i))
                    continue;

                if(!thereWasAPiece){
                    if(board.checkIfThereIsPiece(x, y+i))
                        thereWasAPiece = true;
                    else
                        movesBitboard |= BitBoard.PosToBitInteger(x, y+i);
                }
                else{
                    if(board.checkIfThereIsPiece(x, y+i)){
                        movesBitboard |= BitBoard.PosToBitInteger(x, y+i);
                        break;
                    }
                }
            }
            //init therewasapiece boolean
            thereWasAPiece = false;
            //this loop go throught until the y + i gets to the end of the board or it find a piece after it already find a first piec in the way like cannons in xianqi
            for(int i = 1;Board.checkIfInBorders(x+i,i); i++){
                //if the move is not valid continue(king under check after the move)
                if(!ValidMove(x, y, x+i, y))
                    continue;

                if(!thereWasAPiece){
                    if(board.checkIfThereIsPiece(x+i,y))
                        thereWasAPiece = true;
                    else
                        movesBitboard |= BitBoard.PosToBitInteger(x+i, y);
                }
                else{
                    if(board.checkIfThereIsPiece(x+i,y)){
                        movesBitboard |= BitBoard.PosToBitInteger(x+i, y);
                        break;
                    }
                }
            }
            //init therewasapiece boolean
            thereWasAPiece = false;
            for(int i = 1;Board.checkIfInBorders(x-i,y); i++){
                //if the move is not valid continue(king under check after the move)
                if(!ValidMove(x, y, x-i, y))
                    continue;

                if(!thereWasAPiece){
                    if(board.checkIfThereIsPiece(x-i,y))
                        thereWasAPiece = true;
                    else
                        movesBitboard |= BitBoard.PosToBitInteger(x-i, y);
                }
                else{
                    if(board.checkIfThereIsPiece(x-i,y)){
                        movesBitboard |= BitBoard.PosToBitInteger(x-i, y);
                        break;
                    }
                }
            }
            //init therewasapiece boolean
            thereWasAPiece = false;
            for(int i = 1; Board.checkIfInBorders(x, y-i); i++){
                //if the move is not valid continue(king under check after the move)
                if(!ValidMove(x, y, x, y-i))
                    continue;

                if(!thereWasAPiece){
                    if(board.checkIfThereIsPiece(x, y-i))
                        thereWasAPiece = true;
                    else
                        movesBitboard |= BitBoard.PosToBitInteger(x, y-i);
                }
                else{
                    if(board.checkIfThereIsPiece(x, y-i)){
                        movesBitboard |= BitBoard.PosToBitInteger(x, y-i);
                        break;
                    }
                }
            }

            return movesBitboard;
        }

        /*public override List<Vector2> getMovesOptions(int x, int y)
        {
            List<Vector2> transforms = new List<Vector2>();
            bool thereWasAPiece = false;
            //this loop go throught until the y + i gets to the end of the board or it find a piece after it already find a first piec in the way like cannons in xianqi
            for(int i = 1; Board.checkIfInBorders(x, y+i); i++){
                
                if(!thereWasAPiece){
                    if(board.checkIfThereIsPiece(x, y+i))
                        thereWasAPiece = true;
                    else
                        transforms.Add(new Vector2(x, y+i));
                }
                else{
                    if(board.checkIfThereIsPiece(x, y+i)){
                        if(!checkIfThereIsPieceSameColor(x, y+i))
                            transforms.Add(new Vector2(x, y+i));
                        break;
                    }
                }
            }
            //init therewasapiece boolean
            thereWasAPiece = false;
            //this loop go throught until the y + i gets to the end of the board or it find a piece after it already find a first piec in the way like cannons in xianqi
            for(int i = 1;Board.checkIfInBorders(x+i,i); i++){
                if(!thereWasAPiece){
                    if(board.checkIfThereIsPiece(x+i,y))
                        thereWasAPiece = true;
                    else
                        transforms.Add(new Vector2(x+i, y));
                }
                else{
                    if(board.checkIfThereIsPiece(x+i,y)){
                        if(!checkIfThereIsPieceSameColor(x+i,y))
                            transforms.Add(new Vector2(x+i, y));
                        break;
                    }
                }
            }
            //init therewasapiece boolean
            thereWasAPiece = false;
            for(int i = 1;Board.checkIfInBorders(x-i,y); i++){
                if(!thereWasAPiece){
                    if(board.checkIfThereIsPiece(x-i,y))
                        thereWasAPiece = true;
                    else
                        transforms.Add(new Vector2(x-i, y));
                }
                else{
                    if(board.checkIfThereIsPiece(x-i,y)){
                        if(!checkIfThereIsPieceSameColor(x-i,y))
                            transforms.Add(new Vector2(x-i, y));
                        break;
                    }
                }
            }
            //init therewasapiece boolean
            thereWasAPiece = false;
            for(int i = 1; Board.checkIfInBorders(x, y-i); i++){
                
                if(!thereWasAPiece){
                    if(board.checkIfThereIsPiece(x, y-i))
                        thereWasAPiece = true;
                    else
                        transforms.Add(new Vector2(x, y-i));
                }
                else{
                    if(board.checkIfThereIsPiece(x, y-i)){
                        if(!checkIfThereIsPieceSameColor(x, y-i))
                            transforms.Add(new Vector2(x, y-i));
                        break;
                    }
                }
            }

            return transforms;
        }*/
    }


    public class Advisor : PieceMovement
    {
        public Advisor(PieceColor color) : base(color)
        {
        }

        public override BigInteger GetBitboardMoves(int x, int y)
        {
            BigInteger movesBitboard = 0;

            if(isInPalace(x+1, y+1) && ValidMove(x, y, x+1, y+1))
                movesBitboard |= BitBoard.PosToBitInteger(x+1, y+1);

            if(isInPalace(x-1, y+1) && ValidMove(x, y, x-1, y+1))
                movesBitboard |= BitBoard.PosToBitInteger(x-1, y+1);

            if(isInPalace(x+1, y-1) && ValidMove(x, y, x+1, y-1))
                movesBitboard |= BitBoard.PosToBitInteger(x+1, y-1);

            if(isInPalace(x-1, y-1) && ValidMove(x, y, x-1, y-1))
                movesBitboard |= BitBoard.PosToBitInteger(x-1, y-1);

            return movesBitboard;
        }

        /*public override List<Vector2> getMovesOptions(int x, int y)
        {
            List<Vector2> transforms = new List<Vector2>();

            if(isInPalace(x+1, y+1) && !checkIfThereIsPieceSameColor(x+1,y+1))
                transforms.Add(new Vector2(x+1, y+1));

            if(isInPalace(x-1, y+1) && !checkIfThereIsPieceSameColor(x-1,y+1))
                transforms.Add(new Vector2(x-1, y+1));

            if(isInPalace(x+1, y-1) && !checkIfThereIsPieceSameColor(x+1,y-1))
                transforms.Add(new Vector2(x+1, y-1));

            if(isInPalace(x-1, y-1) && !checkIfThereIsPieceSameColor(x-1,y-1))
                transforms.Add(new Vector2(x-1, y-1));

            return transforms;
        }*/

    }


    public class Rook : PieceMovement
    {
        public Rook(PieceColor color) : base(color)
        {
        }

        public override BigInteger GetBitboardMoves(int x, int y)
        {
            BigInteger movesBitboard = 0;

            for(int i = 1;Board.checkIfInBorders(x+i,y); i++){
                //if the move is not valid continue(king under check after the move)
                if(!ValidMove(x, y, x+i, y))
                    continue;

                movesBitboard |= BitBoard.PosToBitInteger(x+i, y);
                if(board.checkIfThereIsPiece(x+i, y))
                    break;
            }

            for(int i = 1;Board.checkIfInBorders(x,y+i); i++){
                //if the move is not valid continue(king under check after the move)
                if(!ValidMove(x, y, x, y+i))
                    continue;

                movesBitboard |= BitBoard.PosToBitInteger(x, y+i);
                if(board.checkIfThereIsPiece(x, y+i))
                    break;
            }

            for(int i = 1;Board.checkIfInBorders(x-i,y); i++){
                //if the move is not valid continue(king under check after the move)
                if(!ValidMove(x, y, x-i, y))
                    continue;

                movesBitboard |= BitBoard.PosToBitInteger(x-i, y);
                if(board.checkIfThereIsPiece(x-i, y))
                    break;
            }

            for(int i = 1;Board.checkIfInBorders(x,y-i); i++){
                //if the move is not valid continue(king under check after the move)
                if(!ValidMove(x, y, x, y-i))
                    continue;
                    
                movesBitboard |= BitBoard.PosToBitInteger(x, y-i);
                if(board.checkIfThereIsPiece(x, y-i))
                    break;
            }
            return movesBitboard;
        }

        /*public override List<Vector2> getMovesOptions(int x, int y)
        {
            List<Vector2> transforms = new List<Vector2>();

            for(int i = 1;Board.checkIfInBorders(x+i,y) && !checkIfThereIsPieceSameColor(x+i,y); i++){
                transforms.Add(new Vector2(x+i, y));
                if(board.checkIfThereIsPiece(x+i, y))
                    break;
            }

            for(int i = 1;Board.checkIfInBorders(x,y+i) && !checkIfThereIsPieceSameColor(x,y+i); i++){
                transforms.Add(new Vector2(x, y+i));
                if(board.checkIfThereIsPiece(x, y+i))
                    break;
            }

            for(int i = 1;Board.checkIfInBorders(x-i,y) && !checkIfThereIsPieceSameColor(x-i,y); i++){
                transforms.Add(new Vector2(x-i, y));
                if(board.checkIfThereIsPiece(x-i, y))
                    break;
            }

            for(int i = 1;Board.checkIfInBorders(x,y-i) && !checkIfThereIsPieceSameColor(x,y-i); i++){
                transforms.Add(new Vector2(x, y-i));
                if(board.checkIfThereIsPiece(x, y-i))
                    break;
            }
            return transforms;
        }*/
    }
}

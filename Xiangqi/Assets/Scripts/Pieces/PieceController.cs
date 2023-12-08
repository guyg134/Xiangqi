using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography.X509Certificates;
using Unity.Mathematics;
using UnityEngine;

public class PieceController : MonoBehaviour
{/*
    private int pieceInt; //  /10 = color %10 = type
    private int position; //  /10 = y,  %10 = x
    private PieceMovement pieceMovement;
    [SerializeField] private GameObject optionDot;
    [SerializeField] private Color eatPieceDotColor;
    [SerializeField] private GameObject checkCircle;
    
    //create new piece controller
    public void createPieceController(int pieceInt, int position)
    {
        this.pieceInt = pieceInt;
        this.position = position;
        //create new piecemovemnt with the type and the color of the piece
        pieceMovement = PieceFactory.GetPiece(pieceInt%10, GetColor());
    }

    public PieceMovement GetPieceMovement()
    {
        return this.pieceMovement;
    }

    //show the dots where the piece can move to
    public void pieceMoveOptions()
    {
        //x and y of the piece from the position
        int x = position % 10;
        int y = position / 10;

        List<int> transforms = pieceMovement.getVectorsOptions(x, y);

        if(transforms!=null){
            foreach(int pos in transforms)
            {
                if(!Board.kingUnderAttackAfterMove(position, pos))
                {
                    //spawn dot where the piece can move
                    GameObject dot = Instantiate(optionDot, Board.positionToVector3(pos%10, pos/10), Quaternion.identity);
                    dot.transform.parent = gameObject.transform;
                    dot.GetComponent<MoveInput>().createDot(pos);
                    //if there is piece change the color of the dot
                    if(Board.checkIfThereIsPiece(pos%10, pos/10)){
                        dot.GetComponent<SpriteRenderer>().color = eatPieceDotColor;
                    }
                }
            }
        }
        else{
            print("no moves");
        }
    }

    public List<int> getMoveOptions()
    {
        //x and y of the piece from the position
        int x = position % 10;
        int y = position / 10;

        List<int> transforms = pieceMovement.getVectorsOptions(x, y);
        List<int> possibleMoves = new List<int>();

        if(transforms!=null){
            foreach(int pos in transforms)
            {
                //if after this move king in check remove the move
                if(!Board.kingUnderAttackAfterMove(position, pos))
                {
                    possibleMoves.Add(pos);
                }
            }
        }
        //return possible moves by this piece
        return possibleMoves;
    }

    

    //move the piece
    public void movePiece(int newPosition)
    {
        Board.updatePieceInBoard(position, newPosition);
        GameManager.changeTurn();
        GameManager.checkIfCurrentKingIsUnderAttack();
        this.position = newPosition;
        transform.position = Board.positionToVector3(position%10, position/10);
    }

    //return the color of the piece
    public string GetColor()
    {
        int colorInt = pieceInt/10;

        switch(colorInt)
        {
            case(1):
                return "red";
            case(2):
                return "black";
        }
        return "null";
    }

}

public class PieceFactory
{
    public static PieceMovement GetPiece(int pieceInt, string pieceColor)
    {
        switch(pieceInt)
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
    private string color;
    

    public PieceMovement(string color)
    {
        this.color = color;
    }

    public abstract List<int> getVectorsOptions(int x, int y);

    protected int positionToInt(int x, int y){
        return y * 10 + x;
    }

    protected Boolean isInPalace(int x, int y)
    {
        return (x >= 3 && x <= 5) && ((y >= 0 && y <= 2) || ((y >= 7 && y <= 9)));
    }

    protected Boolean checkIfThereIsPieceSameColor(int x, int y)
    {
        return color == Board.colorOfPieceInXY(x, y);
    }

    protected Boolean isPlayerPiece()
    {
        return PlayerScript.getPlayerColor() == color;
    }

    protected int getColorInt()
    {
        if(this.color == "red")
            return 1;
        else
            return 2;
    }
}


public class King : PieceMovement 
{
    public King(string color) : base(color)
    {
    }

    public override List<int> getVectorsOptions(int x, int y)
    {
        List<int> transforms = new List<int>();

        if(isInPalace(x+1, y) && !checkIfThereIsPieceSameColor(x+1, y))
            transforms.Add(positionToInt(x+1, y));

        if(isInPalace(x-1, y) && !checkIfThereIsPieceSameColor(x-1, y))
            transforms.Add(positionToInt(x-1, y));

        if(isInPalace(x, y-1) && !checkIfThereIsPieceSameColor(x, y-1))
            transforms.Add(positionToInt(x, y-1));

        if(isInPalace(x, y+1) && !checkIfThereIsPieceSameColor(x, y+1))
            transforms.Add(positionToInt(x, y+1));

        return transforms;
    }

    public Boolean isKingUnderAttack(int[] piecesPositions, int x, int y)
    {

        //check if rook, king and cannon check
        Boolean therewasapiece = false;
        for(int i = 1;Board.checkIfInBorders(x,y-i); i++){
            //there is piece in this pos
            if(piecesPositions[(y-i)*10 + x] != 0){
                //if the piece is not in the same color check what piece is it
                if(piecesPositions[(y-i)*10 + x] / 10 != getColorInt()){
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
                if(piecesPositions[(y+i)*10 + x] / 10 != getColorInt()){
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
                if(piecesPositions[y*10 + (x-i)] / 10 != getColorInt()){
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
                if(piecesPositions[y*10 + (x+i)] / 10 != getColorInt()){
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
        if((Board.checkIfInBorders(x+1,y)&&piecesPositions[y*10 + (x+1)] / 10 != getColorInt()&&piecesPositions[y*10 + (x+1)]%10 == 2)
         ||(Board.checkIfInBorders(x-1,y)&&piecesPositions[y*10 + (x-1)] / 10 != getColorInt()&&piecesPositions[y*10 + (x-1)]%10 ==2))
            return true;
        //check the up and down if the king is up or down of the board
        if(y>5)
        {
            if(Board.checkIfInBorders(x,y-1)&&piecesPositions[(y-1)*10 + x] / 10 != getColorInt()&&piecesPositions[(y-1)*10 + x]%10 == 2)
                return true;
        }
        else
        {
            if(Board.checkIfInBorders(x,y+1)&&piecesPositions[(y+1)*10 + x] / 10 != getColorInt()&&piecesPositions[(y+1)*10 + x]%10 == 2)
                return true;
        }

        //check if there is knight that checks the king
        if(Board.checkIfInBorders(x+1, y+1))
        {
            if(Board.checkIfInBorders(x+2, y+1) && piecesPositions[(y+1)*10 + (x+2)]/10 != getColorInt() && piecesPositions[(y+1)*10 + (x+2)]%10 == 3)
                return true;
            if(Board.checkIfInBorders(x+1, y+2) && piecesPositions[(y+2)*10 + (x+1)]/10 != getColorInt() && piecesPositions[(y+2)*10 + (x+1)]%10 == 3)
                return true;
        }
        if(Board.checkIfInBorders(x-1, y+1))
        {
            if(Board.checkIfInBorders(x-2, y+1) && piecesPositions[(y+1)*10 + (x-2)]/10 != getColorInt() && piecesPositions[(y+1)*10 + (x-2)]%10 == 3)
                return true;
            if(Board.checkIfInBorders(x-1, y+2) && piecesPositions[(y+2)*10 + (x-1)]/10 != getColorInt() && piecesPositions[(y+2)*10 + (x-1)]%10 == 3)
                return true;
        }
        if(Board.checkIfInBorders(x+1, y-1))
        {
            if(Board.checkIfInBorders(x+2, y-1) && piecesPositions[(y-1)*10 + (x+2)]/10 != getColorInt() && piecesPositions[(y-1)*10 + (x+2)]%10 == 3)
                return true;
            if(Board.checkIfInBorders(x+1, y-2) && piecesPositions[(y-2)*10 + (x+1)]/10 != getColorInt() && piecesPositions[(y-2)*10 + (x+1)]%10 == 3)
                return true;
        }
        if(Board.checkIfInBorders(x-1, y-1))
        {
            if(Board.checkIfInBorders(x-2, y-1) && piecesPositions[(y-1)*10 + (x-2)]/10 != getColorInt() && piecesPositions[(y-1)*10 + (x-2)]%10 == 3)
                return true;
            if(Board.checkIfInBorders(x-1, y-2) && piecesPositions[(y-2)*10 + (x-1)]/10 != getColorInt() && piecesPositions[(y-2)*10 + (x-1)]%10 == 3)
                return true;
        }
        return false;
    }
}
public class Solider : PieceMovement
{
    public Solider(string color) : base(color)
    {
    }

    public override List<int> getVectorsOptions(int x, int y)
    {
        List<int> transforms = new List<int>();
        //if the solider is in the second half of the board he can move to the sides and back too
        if(isPlayerPiece())
        {
            //if the solider is in the second half of the board he can move to the sides and back too
            if(y>4){
                if(Board.checkIfInBorders(x+1, y) && !checkIfThereIsPieceSameColor(x+1, y))
                    transforms.Add(positionToInt(x+1, y));
                if(Board.checkIfInBorders(x-1, y) && !checkIfThereIsPieceSameColor(x-1, y))
                    transforms.Add(positionToInt(x-1, y)); 
            }
            //if not it can move just forward
            if(Board.checkIfInBorders(x, y+1) && !checkIfThereIsPieceSameColor(x, y+1))
                transforms.Add(positionToInt(x, y+1));
        }
        else
        {
            //if the solider is in the second half of the board he can move to the sides and back too
            if(y<5){
                if(Board.checkIfInBorders(x+1, y) && !checkIfThereIsPieceSameColor(x+1, y))
                    transforms.Add(positionToInt(x+1, y));
                if(Board.checkIfInBorders(x-1, y) && !checkIfThereIsPieceSameColor(x-1, y))
                    transforms.Add(positionToInt(x-1, y));
            }
            //if not it can move just forward
            if(Board.checkIfInBorders(x, y-1) && !checkIfThereIsPieceSameColor(x, y-1))
                transforms.Add(positionToInt(x, y-1));
        }
        return transforms;
    }
}
public class Knight : PieceMovement
{
    public Knight(string color) : base(color)
    {
    }

    public override List<int> getVectorsOptions(int x, int y)
    {
        List<int> transforms = new List<int>();

        if(!Board.checkIfThereIsPiece(x+1, y))
        {
            if(Board.checkIfInBorders(x+2, y+1) && !checkIfThereIsPieceSameColor(x+2, y+1))
                transforms.Add(positionToInt(x+2, y+1));
            if(Board.checkIfInBorders(x+2, y-1) && !checkIfThereIsPieceSameColor(x+2, y-1))
                transforms.Add(positionToInt(x+2, y-1));
            
        }
        if(!Board.checkIfThereIsPiece(x-1, y))
        {
            if(Board.checkIfInBorders(x-2, y+1) && !checkIfThereIsPieceSameColor(x-2, y+1))
                transforms.Add(positionToInt(x-2, y+1));
            if(Board.checkIfInBorders(x-2, y-1) && !checkIfThereIsPieceSameColor(x-2, y-1))
                transforms.Add(positionToInt(x-2, y-1));
        }
        if(!Board.checkIfThereIsPiece(x, y+1))
        {
            if(Board.checkIfInBorders(x+1, y+2) && !checkIfThereIsPieceSameColor(x+1, y+2))
                transforms.Add(positionToInt(x+1, y+2));
            if(Board.checkIfInBorders(x-1, y+2) && !checkIfThereIsPieceSameColor(x-1, y+2))
                transforms.Add(positionToInt(x-1, y+2));
        }
        if(!Board.checkIfThereIsPiece(x, y-1))
        {
            if(Board.checkIfInBorders(x+1, y-2) && !checkIfThereIsPieceSameColor(x+1, y-2))
                transforms.Add(positionToInt(x+1, y-2));
            if(Board.checkIfInBorders(x-1, y-2) && !checkIfThereIsPieceSameColor(x-1, y-2))
                transforms.Add(positionToInt(x-1, y-2));
        }
        
        return transforms;
    }
}
public class Elephant : PieceMovement
{
    public Elephant(string color) : base(color)
    {
    }

    public override List<int> getVectorsOptions(int x, int y)
    {
        List<int> transforms = new List<int>();
        if(Board.checkIfInBorders(x+2,y+2) && ((isPlayerPiece()&& y+2 < 5)||(!isPlayerPiece()&& y+2 > 4)) && !Board.checkIfThereIsPiece(x+1,y+1) && !checkIfThereIsPieceSameColor(x+2, y+2))
            transforms.Add(positionToInt(x+2, y+2));

        if(Board.checkIfInBorders(x-2,y+2) && ((isPlayerPiece()&&y+2 < 5)||(!isPlayerPiece()&& y+2 > 4)) && !Board.checkIfThereIsPiece(x-1,y+1) && !checkIfThereIsPieceSameColor(x-2, y+2))
            transforms.Add(positionToInt(x-2, y+2));

        if(Board.checkIfInBorders(x+2,y-2) && ((isPlayerPiece()&& y-2 < 5)||(!isPlayerPiece()&& y-2 > 4)) && !Board.checkIfThereIsPiece(x+1,y-1) && !checkIfThereIsPieceSameColor(x+2, y-2))
            transforms.Add(positionToInt(x+2, y-2));

        if(Board.checkIfInBorders(x-2,y-2) && ((isPlayerPiece()&& y-2 < 5)||(!isPlayerPiece()&& y-2 > 4)) && !Board.checkIfThereIsPiece(x-1,y-1) && !checkIfThereIsPieceSameColor(x-2, y-2))
            transforms.Add(positionToInt(x-2, y-2));
        return transforms;
    }
}
public class Cannon : PieceMovement
{
    public Cannon(string color) : base(color)
    {
    }

    public override List<int> getVectorsOptions(int x, int y)
    {
        List<int> transforms = new List<int>();
        Boolean thereWasAPiece = false;
        //this loop go throught until the y + i gets to the end of the board or it find a piece after it already find a first piec in the way like cannons in xianqi
        for(int i = 1; Board.checkIfInBorders(x, y+i); i++){
            
            if(!thereWasAPiece){
                if(Board.checkIfThereIsPiece(x, y+i))
                    thereWasAPiece = true;
                else
                    transforms.Add(positionToInt(x, y+i));
            }
            else{
                if(Board.checkIfThereIsPiece(x, y+i)){
                    if(!checkIfThereIsPieceSameColor(x, y+i))
                        transforms.Add(positionToInt(x, y+i));
                    break;
                }
            }
        }
        //init therewasapiece boolean
        thereWasAPiece = false;
        //this loop go throught until the y + i gets to the end of the board or it find a piece after it already find a first piec in the way like cannons in xianqi
        for(int i = 1;Board.checkIfInBorders(x+i,i); i++){
            if(!thereWasAPiece){
                if(Board.checkIfThereIsPiece(x+i,y))
                    thereWasAPiece = true;
                else
                    transforms.Add(positionToInt(x+i, y));
            }
            else{
                if(Board.checkIfThereIsPiece(x+i,y)){
                    if(!checkIfThereIsPieceSameColor(x+i,y))
                        transforms.Add(positionToInt(x+i, y));
                    break;
                }
            }
        }
        //init therewasapiece boolean
        thereWasAPiece = false;
        for(int i = 1;Board.checkIfInBorders(x-i,y); i++){
            if(!thereWasAPiece){
                if(Board.checkIfThereIsPiece(x-i,y))
                    thereWasAPiece = true;
                else
                    transforms.Add(positionToInt(x-i, y));
            }
            else{
                if(Board.checkIfThereIsPiece(x-i,y)){
                    if(!checkIfThereIsPieceSameColor(x-i,y))
                        transforms.Add(positionToInt(x-i, y));
                    break;
                }
            }
        }
        //init therewasapiece boolean
        thereWasAPiece = false;
        for(int i = 1; Board.checkIfInBorders(x, y-i); i++){
            
            if(!thereWasAPiece){
                if(Board.checkIfThereIsPiece(x, y-i))
                    thereWasAPiece = true;
                else
                    transforms.Add(positionToInt(x, y-i));
            }
            else{
                if(Board.checkIfThereIsPiece(x, y-i)){
                    if(!checkIfThereIsPieceSameColor(x, y-i))
                        transforms.Add(positionToInt(x, y-i));
                    break;
                }
            }
        }

        return transforms;
    }
}
public class Advisor : PieceMovement
{
    public Advisor(string color) : base(color)
    {
    }

    public override List<int> getVectorsOptions(int x, int y)
    {
        List<int> transforms = new List<int>();

        if(isInPalace(x+1, y+1) && !checkIfThereIsPieceSameColor(x+1,y+1))
            transforms.Add(positionToInt(x+1, y+1));

        if(isInPalace(x-1, y+1) && !checkIfThereIsPieceSameColor(x-1,y+1))
            transforms.Add(positionToInt(x-1, y+1));

        if(isInPalace(x+1, y-1) && !checkIfThereIsPieceSameColor(x+1,y-1))
            transforms.Add(positionToInt(x+1, y-1));

        if(isInPalace(x-1, y-1) && !checkIfThereIsPieceSameColor(x-1,y-1))
            transforms.Add(positionToInt(x-1, y-1));

        return transforms;
    }

}
public class Rook : PieceMovement
{
    public Rook(string color) : base(color)
    {
    }

    public override List<int> getVectorsOptions(int x, int y)
    {
        List<int> transforms = new List<int>();

        for(int i = 1;Board.checkIfInBorders(x+i,y) && !checkIfThereIsPieceSameColor(x+i,y); i++){
            transforms.Add(positionToInt(x+i, y));
            if(Board.checkIfThereIsPiece(x+i, y))
                break;
        }

        for(int i = 1;Board.checkIfInBorders(x,y+i) && !checkIfThereIsPieceSameColor(x,y+i); i++){
            transforms.Add(positionToInt(x, y+i));
            if(Board.checkIfThereIsPiece(x, y+i))
                break;
        }

        for(int i = 1;Board.checkIfInBorders(x-i,y) && !checkIfThereIsPieceSameColor(x-i,y); i++){
            transforms.Add(positionToInt(x-i, y));
            if(Board.checkIfThereIsPiece(x-i, y))
                break;
        }

        for(int i = 1;Board.checkIfInBorders(x,y-i) && !checkIfThereIsPieceSameColor(x,y-i); i++){
            transforms.Add(positionToInt(x, y-i));
            if(Board.checkIfThereIsPiece(x, y-i))
                break;
        }
        return transforms;
    }*/
}



using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;

public class Board : MonoBehaviour
{
    public static float BOARD_SQUARE_LENGTH = 0.8625f;
    public static float BOARD_SQUARE_HEIGHT = 3.88f;

    //pieces sprite asset of black and redd pieces and the prefab of the piece
    [SerializeField] private Sprite[] redPiecesSprites;
    [SerializeField] private Sprite[] blackPiecesSprites;
    [SerializeField] private GameObject chessPiecePrefab;
    [SerializeField] private GameObject moveDotPrefab;

    
    //array that keeps all the positions with int that represent the piece and the color
    private Piece[,] pieces = new Piece[10, 9];
    //bitboard
    private BitBoard bitBoard;
    
    private GameManager gameManager;



    public void CreateBoard(PlayerColor playerColor, GameManager gameManager)
    {
        this.gameManager = gameManager;

        //when player playing red pieces
        string startFenRed = "rneakaenr/9/1c5c1/p1p1p1p1p/9/9/P1P1P1P1P/1C5C1/9/RNEAKAENR";
        //when player playing black pieces
        string startFenBlack = "RNEAKAENR/9/1C5C1/P1P1P1P1P/9/9/p1p1p1p1p/1c5c1/9/rneakaenr";

        string gameFen = "";

        gameFen = playerColor==PlayerColor.Red ? startFenRed : startFenBlack;
        LoadPositionFromFen(gameFen);

        bitBoard = GetComponent<BitBoard>();
        bitBoard.SetBitBoards(pieces);
        bitBoard.PrintCurrentBitBoard();
    }



    //get a fen string and set the position of every piece in the positions array
    private void LoadPositionFromFen(string fen) 
    {
        //change char to the int of specific piece
        var pieceTypeFromSymbol = new Dictionary<char, Piece.PieceType> (){
            ['k'] = Piece.PieceType.King, ['p'] = Piece.PieceType.Soldier, ['n'] = Piece.PieceType.Knight,
            ['e'] = Piece.PieceType.Elephant, ['c'] = Piece.PieceType.Cannon, ['r'] = Piece.PieceType.Rook,
            ['a'] = Piece.PieceType.Advisor
        };

        string fenBoard = fen;
        int file = 0, rank = 9;
        

        foreach(char symbol in fenBoard)
        {
            //if the symbol is / so it goes to lower rank
            if(symbol == '/'){
                file = 0;
                rank--;
            }
            else{
                //if its digit so it need to get forward in the file
                if(char.IsDigit(symbol)){
                    file += (int) char.GetNumericValue(symbol);
                }
                //else its digit so its piece
                else{
                    //find the piece color int by checking if its upper or lower letter
                    Piece.PieceColor pieceColor = (char.IsUpper(symbol)) ? Piece.PieceColor.Red : Piece.PieceColor.Black;
                    //get the piece int by sent the letter to the piecetypefromsymbol 
                    Piece.PieceType pieceType = pieceTypeFromSymbol[char.ToLower(symbol)];
                    //the position index is  rank, file and the piece is color + type
                    
                    //Piece currentPiece = new Piece(pieceType, pieceColor, file, rank);
                    //pieces[rank, file] = currentPiece;
                    DrawPiece(file, rank, pieceType, pieceColor);
                    
                    file++;
                }
            }
        }
    }


    //go through the the position array and instatntiate the pieces on the board
    public void DrawPiece(int x, int y, Piece.PieceType pieceType, Piece.PieceColor pieceColor)
    {
        //instantiate the peice by calculate the position of the piece by multiple the *index* and the board square length and height
        GameObject obj = Instantiate(chessPiecePrefab, positionToVector2(x, y), Quaternion.identity);
        //add the new piece to the pieces array
        obj.transform.parent = gameObject.transform;
        
        //change the name of the piece for the specific piece and color
        obj.name = pieceType + " " + pieceColor;

        obj.GetComponent<Piece>().SetPiece(pieceType, pieceColor, x, y);
        pieces[y, x] = obj.GetComponent<Piece>();


        //set the piece sprite 
        //if the position / 10 is 1 so the piece its red and the piece sprite is in the red pieces sprites array
        //red
        if(pieceColor == Piece.PieceColor.Red){
            //change the piece sprite
            obj.GetComponent<SpriteRenderer>().sprite = redPiecesSprites[(int)pieceType - 1];
        }
        //if the position / 10 is 2 so the piece its black and the piece sprite is in the black pieces sprites array
        //black
        else{
            //change the piece sprite 
            obj.GetComponent<SpriteRenderer>().sprite = blackPiecesSprites[(int)pieceType - 1];
        }
    }

    public void drawDots(BigInteger dotsBitboard, GameObject piece, BigInteger moves)
    {
        deleteDots();

        List<Vector2> dotsPos = bitBoard.BitboardToVector2s(dotsBitboard);
        //create dots for the new piece
        foreach(Vector2 dotPos in dotsPos)
        {
            GameObject dotObject = Instantiate(moveDotPrefab, positionToVector2((int)dotPos.x, (int)dotPos.y), Quaternion.identity);
            dotObject.transform.parent = piece.transform;
            dotObject.GetComponent<MoveInput>().SetPos(dotPos);
        }

        gameManager.bitboardText.text = BitBoard.BigIntegerToBinaryString(moves);
    }

    private void deleteDots()
    {
        //remove the dots of the last piece
        GameObject[] dots = GameObject.FindGameObjectsWithTag("Dot");
        foreach(GameObject dot in dots)
        {
            Destroy(dot.gameObject);
        }
    }

    public Piece GetPieceAtPosition(int x, int y)
    {
        return pieces[y, x];
    }

    //check there is piece in the input position
    public bool checkIfThereIsPiece(int x, int y)
    {
        if(checkIfInBorders(x, y)){
            return pieces[y, x]!=null;
        }
        return false;
    }

    //return if position is in the board borders
    public static bool checkIfInBorders(int x, int y)
    {
        return x>=0 && y>=0 && x<9 && y < 10;
    }

    //translate position in int to vector3
    private  Vector2 positionToVector2(int x, int y)
    {
        return new Vector2((x * Board.BOARD_SQUARE_LENGTH) - (Board.BOARD_SQUARE_LENGTH*4), (y * Board.BOARD_SQUARE_LENGTH)-Board.BOARD_SQUARE_HEIGHT);
    }

    public void  updatePieceInBoard(Move move)
    {
        deleteDots();

        Piece piece = pieces[move.getStartY(), move.getStartX()];
        //check if there is piece and take the piece if true
        if(pieces[move.getEndY(), move.getEndX()])
            eatPiece(move.getEndX(), move.getEndY());
        //update piece in board array
        pieces[move.getStartY(), move.getStartX()] = null;
        pieces[move.getEndY(), move.getEndX()] = piece;
        //update piece axis
        piece.gameObject.transform.position = positionToVector2(move.getEndX(), move.getEndY());
        //update the piece in the bitboard
        bitBoard.UpdateBitBoard(move, piece.GetPieceColor());

        //check if there is check on the king now
        IsKingUnderAttack();

        gameManager.changeTurn();
    }

    private void IsKingUnderAttack()
    {
        if(bitBoard.IsCheck(pieces, gameManager.getTurnColor()))
        {
            print("check");
            IsCheckMate();
        }
    }

    public bool IsKingUnderAttackAfterMove(Move move)
    {
        //create clone of the board
        Piece[,] piecesAfterMove = (Piece[,])pieces.Clone();

        //do the move
        Piece pieceToMove = piecesAfterMove[move.getStartY(), move.getStartX()];
        piecesAfterMove[move.getStartY(), move.getStartX()] = null;
        piecesAfterMove[move.getEndY(), move.getEndX()] = pieceToMove;

        return bitBoard.IsCheck(piecesAfterMove, (PlayerColor)((int)gameManager.getTurnColor()^1));
    }

    private void IsCheckMate()
    {
        if(bitBoard.IsCheckMate())
        {
            print("check mate GG");
        }
    }

    private void eatPiece(int x, int y)
    {
        Destroy(pieces[y, x].gameObject);
    }
/*
    public static Boolean thereIsCheckMate()
    {
        List<int> movesPossible = new List<int>();

        PieceController pc;
        //go through all pieces and add their moves to the list
        for(int i =0; i< pieces.Length; i++)
        {
        
            if(pieces[i]!=null){
                pc = pieces[i].GetComponent<PieceController>();
                if(pc.GetColor() == GameManager.getTurnColor())
                {
                    movesPossible.AddRange(pc.getMoveOptions());
                }
            }
        }
    
        return movesPossible.Count ==0;
    }

    //return if king is under attack after move
    public static Boolean kingUnderAttackAfterMove(int lastPos, int newPos)
    {
        //initial the new arrays for the move
        int[] newPositions = (int[])positions.Clone();
        GameObject[] newPieces = (GameObject[])pieces.Clone();

        //update the new move in the positions
        newPositions[newPos] = newPositions[lastPos];
        newPositions[lastPos] = 0;
        
        //update the new move in the pieces
        newPieces[newPos] = newPieces[lastPos];
        newPieces[lastPos] = null;
        //
        
        int currentKingPos = getCurrentTurnKingPosition(newPositions);
        
        
        King kingPieceMovement = newPieces[currentKingPos].GetComponent<PieceController>().GetPieceMovement() as King;
 
        Boolean isCheck = kingPieceMovement.isKingUnderAttack(newPositions, currentKingPos%10, currentKingPos/10);

        //print("the position is y: " + newPos/10 + " x: " + newPos %10 + " and if check is " + isCheck);
        return isCheck;
        
    }*/
}

using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class BitBoard : MonoBehaviour
{

    //save the black pieces in bitboard
    private BigInteger blackBitboard = 0;
    //save the red pieces in bitboard
    private BigInteger redBitboard = 0;


    public void SetBitBoards(Piece[,] pieces)
    {
        BigInteger redBoard = 0;
        BigInteger blackBoard = 0;

        for (int row = 0; row < 10; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                Piece piece = pieces[row, col];
                if(!piece) continue;

                // Set the corresponding bit position based on the piece type
                BigInteger bitPosition = row * 9 + col;
                BigInteger bitValue = piece ? 1 : 0;
                if(piece.GetPieceColor() == Piece.PieceColor.Red)
                    redBoard |= bitValue << (int)bitPosition;
                else
                    blackBoard |= bitValue << (int)bitPosition;
            }
        }
        
        this.redBitboard = redBoard;
        this.blackBitboard = blackBoard;
        
    }

    public void UpdateBitBoard(Move move, Piece.PieceColor color)
    {
        //if its red piece update red Bitboard
        if(color == Piece.PieceColor.Red){
            //remove the current position of the piece in the board
            BigInteger bitPosition = move.getStartY() * 9 + move.getStartX();
            BigInteger bitValue = 1;
            redBitboard ^= bitValue << (int)bitPosition;

            //update the new position in the board
            bitPosition = move.getEndY() * 9 + move.getEndX();
    
            //check if there is black piece and remove it from blackbitboard
            if((blackBitboard & bitValue << (int)bitPosition) != 0)
                blackBitboard ^= bitValue << (int)bitPosition;

            redBitboard |= bitValue << (int)bitPosition;
        }
        //if its black update black Bitboard
        else{
            //remove the current position of the piece in the board
            BigInteger bitPosition = move.getStartY() * 9 + move.getStartX();
            BigInteger bitValue = 1;
            blackBitboard ^= bitValue << (int)bitPosition;

            //update the new position in the board
            bitPosition = move.getEndY() * 9 + move.getEndX();
            
            //check if there is black piece and remove it from blackbitboard
            if((redBitboard & bitValue << (int)bitPosition) != 0)
                redBitboard ^= bitValue << (int)bitPosition;

            blackBitboard |= bitValue << (int)bitPosition;
        }
    }

    public bool IsCheck(Piece[,] pieces, PlayerColor currentTurnColor)
    {   
        BigInteger kingPos;
        BigInteger attackPos = 0;

        for (int y = 0; y < 10; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                Piece currentPiece = pieces[y, x];
                if(!currentPiece)
                    continue;
                //check if the piece is king
                if(currentPiece.GetPieceType() == Piece.PieceType.King && (int)currentPiece.GetPieceColor() == ((int)currentTurnColor ^ 1))
                {
                    kingPos = PosToBitInteger(x, y);
                    continue;
                }
                if((int)currentPiece.GetPieceColor() == (int)currentTurnColor)
                    attackPos |= currentPiece.GetPieceBitboardMove();
            }
        }
        
        if((attackPos & kingPos) != 0)
        {
            GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().bitboardText.text = BigIntegerToBinaryString(attackPos);
            return true;
        }
       
        return false;
    }

    public bool IsCheckMate()
    {
        return false;
    }

    public BigInteger PrintCurrentBitBoard()
    {
        //print the decimal value of the board
        print(redBitboard | blackBitboard);
        //print the bit board
        PrintBitBoard(redBitboard);

        return redBitboard | blackBitboard;
    }

    public static void PrintBitBoard(BigInteger bitboardInt)
    {
        string bitboardString = BigIntegerToBinaryString(bitboardInt);
        /*string[] rows = bitboard.Split(' ');
        foreach(var row in rows)
        {
            print(row);
        }*/
        print(bitboardString);
    }

    public static string BigIntegerToBinaryString(BigInteger value)
    {
        int rowLength = 9;
        int totalBits = 90; // Assuming a 9x10 board
        string binaryString = "";

        for (int i = 0; i < totalBits; i++)
        {
            binaryString += ((value & (BigInteger.One << i)) == 0) ? "0" : "1";

            // Insert newline character after each row
            if ((i + 1) % rowLength == 0 && i != totalBits - 1)
            {
                binaryString += "\n";
            }
        }

        // Split the string into rows
        string[] rows = binaryString.Split('\n');

        // Manually reverse the array of rows
        int start = 0;
        int end = rows.Length - 1;

        while (start < end)
        {
            string temp = rows[start];
            rows[start] = rows[end];
            rows[end] = temp;

            start++;
            end--;
        }

        // Join the rows back into a single string
        return string.Join("\n", rows);

    }

    public List<Vector2> BitboardToVector2s(BigInteger bigboard)
    {
        int rowLength = 9;
        int totalBits = 90; // Assuming a 9x10 board
        List<Vector2> positions = new List<Vector2>();

        for (int i = 0; i < totalBits; i++)
        {
            if((bigboard & (BigInteger.One << i)) != 0)
                positions.Add(new Vector2(i%rowLength, (int)i/rowLength));
        }

        return positions;
    }

    public static BigInteger PosToBitInteger(int x , int y)
    {
        BigInteger bitPos = 0;
        BigInteger bitPosition = y * 9 + x;
        BigInteger value = 1;
        
        
        bitPos |= value << (int)bitPosition;
        

        return bitPos;
    }

}

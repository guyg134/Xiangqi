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
                if(piece.GetPieceColor() == GameColor.Red)
                    redBoard |= bitValue << (int)bitPosition;
                else
                    blackBoard |= bitValue << (int)bitPosition;
            }
        }
        
        this.redBitboard = redBoard;
        this.blackBitboard = blackBoard;
    }

    public BigInteger GetRedBitboard()
    {
        return redBitboard;
    }

    public BigInteger GetBlackBitboard()
    {
        return blackBitboard;
    }

    public void UpdateBitBoard(Move move, GameColor color)
    {
        //if its red piece update red Bitboard
        if(color == GameColor.Red){
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

    public bool IsCheck(Piece[,] pieces, GameColor currentTurnColor)
    {   
        BigInteger kingPos = 0;
        BigInteger attackPos;

        for (int y = 0; y < 10; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                Piece currentPiece = pieces[y, x];
                
                if(currentPiece == null)
                    continue;
                //check if the piece is king and its the color of the enemy
                if(currentPiece.GetPieceType() == PieceType.King && (int)currentPiece.GetPieceColor() == ((int)currentTurnColor ^ 1))
                {
                    kingPos = PosToBitInteger(x, y);
                    continue;
                }
            }
        }

        attackPos = AttackingSquaresBitboard(pieces, currentTurnColor);
        
        if((attackPos & kingPos) != 0)
        {
            return true;
        }
       
        return false;
    }

    public static BigInteger AttackingSquaresBitboard(Piece[,] pieces, GameColor pieceColor)
    {
        BigInteger attackPos = 0;

        for (int y = 0; y < 10; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                Piece currentPiece = pieces[y, x];
                
                if(currentPiece == null)
                    continue;
                
                if((int)currentPiece.GetPieceColor() == (int)pieceColor){
                    attackPos |= currentPiece.GetPieceBitboardMove();

                }
            }
        }
        return attackPos;
    }

    public BigInteger PrintCurrentBitBoard()
    {
        //print the decimal value of the board
        //print(redBitboard | blackBitboard);
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

    public List<Vector2> BitboardToVector2s(BigInteger bitboard)
    {
        int totalBits = 90; // Assuming a 9x10 board
        List<Vector2> positions = new List<Vector2>();

        for (int i = 0; i < totalBits; i++)
        {
            if((bitboard & (BigInteger.One << i)) != 0)
                positions.Add(new Vector2(i%9, i/9));
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

    public BigInteger BitboardMovesWithoutDefence(BigInteger bitboardMoves, GameColor playerColor)
    {
        //red turn
        if(playerColor == GameColor.Red)
            return (bitboardMoves|redBitboard) ^  redBitboard;
        //black turn
        return (bitboardMoves|blackBitboard) ^  blackBitboard;
    }

    public static BigInteger BoardToBitboardByColor(Piece[,] pieces, GameColor color)
    {
        BigInteger colorBitboard = 0;
        for (int row = 0; row < 10; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                Piece piece = pieces[row, col];
                if(!piece) continue;

                // Set the corresponding bit position based on the piece type
                BigInteger bitPosition = row * 9 + col;
                BigInteger bitValue = piece ? 1 : 0;
                if(piece.GetPieceColor() == color)
                    colorBitboard |= bitValue << (int)bitPosition;
            }
        }
        return colorBitboard;
    }

}

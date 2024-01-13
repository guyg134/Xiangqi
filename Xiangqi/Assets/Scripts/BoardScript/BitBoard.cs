using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class BitBoard : MonoBehaviour
{

    //save the black pieces in bitboard
    private BigInteger blackBitboard = 0;
    //save the red pieces in bitboard
    private BigInteger redBitboard = 0;


    public void SetBitBoards(Board board)
    {
        redBitboard = BoardToBitboardByColor(board, GameColor.Red);
        blackBitboard = BoardToBitboardByColor(board, GameColor.Black);
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

    public bool IsCheck(Board board, GameColor currentTurnColor)
    {   
        BigInteger kingPos = PosToBitInteger(board.FindKing(currentTurnColor.OppositeColor()).GetPos());
        BigInteger attackPos;

        attackPos = AttackingSquaresBitboard(board, currentTurnColor);
        
        if((attackPos & kingPos) != 0)
        {
            return true;
        }
       
        return false;
    }

    public static BigInteger BoardToBitboardByColor(Board board, GameColor pieceColor)
    {
        BigInteger bitboard = 0;

        for(int i = 1; i < 8; i++)
        {
            foreach(Piece piece in board.GetPiecesInType((PieceType)i))
            {
                // Set the corresponding bit position based on the piece type
                BigInteger bitPosition = piece.GetY() * 9 + piece.GetX();
                BigInteger bitValue = piece ? 1 : 0;
                if(piece.GetPieceColor() == pieceColor)
                    bitboard |= bitValue << (int)bitPosition;
            }
        }
                
        return bitboard;
    }

    public static BigInteger AttackingSquaresBitboard(Board board, GameColor pieceColor)
    {
        BigInteger attackPos = 0;

        for(int i = 1; i < 8; i++)
        {
            foreach(Piece piece in board.GetPiecesInType((PieceType)i))
            {
                if(piece.GetPieceColor() == pieceColor)
                {
                    attackPos |= piece.GetPieceBitboardMove(board);
                }
            }
        }
                
        return attackPos;
    }

    //return the piece type of the piece that attacking specific position
    public static Piece PieceOfAttackingPieceOnPos(Board board, BigInteger pos, GameColor attackingColor)
    {
        for(int i = 1; i < 8; i++)
        {
            foreach(Piece piece in board.GetPiecesInType((PieceType)i))
            {
                if(piece.GetPieceColor() == attackingColor)
                {
                    if((pos & piece.GetPieceBitboardMove(board)) != 0)
                        return piece;
                }
            }
        }
                
        return null;
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

    public static BigInteger PosToBitInteger(Vector2 pos)
    {
        BigInteger bitPos = 0;
        BigInteger bitPosition = (int)pos.y * 9 + (int)pos.x;
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

}

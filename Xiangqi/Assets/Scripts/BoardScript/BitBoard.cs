using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class BitBoard 
{

    //save the black pieces in bitboard
    private BigInteger blackBitboard = 0;
    //save the red pieces in bitboard
    private BigInteger redBitboard = 0;


    public BitBoard(BitBoard bitBoard)
    {
        redBitboard = bitBoard.GetRedBitboard();
        blackBitboard = bitBoard.GetBlackBitboard();
    }

    public BitBoard(Board board)
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
        BigInteger bitPosition = move.StartY * 9 + move.StartX;
        BigInteger bitValue = 1;

        //if its red piece update red Bitboard
        if(color == GameColor.Red){
            //remove the current position of the piece in the board
            redBitboard ^= bitValue << (int)bitPosition;

            //update the new position in the board
            bitPosition = move.EndY * 9 + move.EndX;
    
            //check if there is black piece and remove it from blackbitboard
            if((blackBitboard & bitValue << (int)bitPosition) != 0)
                blackBitboard ^= bitValue << (int)bitPosition;

            redBitboard |= bitValue << (int)bitPosition;
        }
        //if its black update black Bitboard
        else{
            //remove the current position of the piece in the board
            blackBitboard ^= bitValue << (int)bitPosition;

            //update the new position in the board
            bitPosition = move.EndY * 9 + move.EndX;
            
            //check if there is black piece and remove it from blackbitboard
            if((redBitboard & bitValue << (int)bitPosition) != 0)
                redBitboard ^= bitValue << (int)bitPosition;

            blackBitboard |= bitValue << (int)bitPosition;
        }
    }

    public void UndoMoveBitboard(Move move, GameColor turnColor)
    {
        BigInteger bitPosition = move.EndY * 9 + move.EndX;
        BigInteger bitValue = 1;

        //if its red piece update red Bitboard
        if(turnColor == GameColor.Red){
            //remove the current position of the piece in the board
            redBitboard ^= bitValue << (int)bitPosition;

            //add the eaten piece to the enemy board(black)
            if(move.EatenPiece)
                blackBitboard |= bitValue << (int)bitPosition;

            //update the new position in the board
            bitPosition = move.StartY * 9 + move.StartX;

            redBitboard |= bitValue << (int)bitPosition;
        }
        //if its black update black Bitboard
        else{
            //remove the current position of the piece in the board
            blackBitboard ^= bitValue << (int)bitPosition;

            //add the eaten piece to the enemy board(red)
            if(move.EatenPiece)
                redBitboard |= bitValue << (int)bitPosition;

            //update the new position in the board
            bitPosition = move.StartY * 9 + move.StartX;

            blackBitboard |= bitValue << (int)bitPosition;
        }
    }

    public bool IsCheck(Board board, GameColor currentTurnColor)
    {   
        BigInteger kingPos = PosToBitInteger(board.FindKing(currentTurnColor.OppositeColor()).GetPos());
        
        //go over all the pieces and check if they can attack the king, stop when you find one
        for(int i = 1; i < 8; i++)
        {
            foreach(Piece piece in board.GetPiecesInType((PieceType)i))
            {
                if(piece.GetPieceColor() == currentTurnColor && (piece.GetPieceBitboardMove(board) & kingPos) != 0)
                {
                    return true;
                }
            }
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

    public static BigInteger IntersectionsUnderAttackByColor(Board board, GameColor pieceColor)
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

    //return the piece that attacking the piece on the specific pos
    public static List<Piece> PiecesThatAttackingPos(Board board, BigInteger pos, GameColor attackingColor)
    {
        List<Piece> pieces = new List<Piece>();
        //check all the pieces that can attack the pos
        //O(16 * n) n- number of moves of the piece
        for(int i = 1; i < 8; i++)
        {
            foreach(Piece piece in board.GetPiecesInType((PieceType)i))
            {
                if(piece.GetPieceColor() == attackingColor)
                {
                    if((pos & piece.GetPieceBitboardMove(board)) != 0)
                        pieces.Add(piece);
                }
            }
        }
        
        return pieces;
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

    }

    public static string BigIntegerToBinaryString(BigInteger value)
    {
        //O(1)

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

    public List<Position> BitboardToPosition(BigInteger bitboard)
    {
        //O(1)
        int totalBits = 90; // Assuming a 9x10 board
        List<Position> positions = new List<Position>();

        for (int i = 0; i < totalBits; i++)
        {
            if((bitboard & (BigInteger.One << i)) != 0)
                positions.Add(new Position(i%9, i/9));
        }

        return positions;
    }

    public static BigInteger PosToBitInteger(int x , int y)
    {
        //O(1)
        BigInteger bitPos = 0;
        BigInteger bitPosition = y * 9 + x;
        BigInteger value = 1;
        
        
        bitPos |= value << (int)bitPosition;
        

        return bitPos;
    }

    public static BigInteger PosToBitInteger(Position pos)
    {
        //O(1)
        BigInteger bitPos = 0;
        BigInteger bitPosition = pos.y * 9 + pos.x;
        BigInteger value = 1;
        
        
        bitPos |= value << (int)bitPosition;
        

        return bitPos;
    }

    public BigInteger BitboardMovesWithoutDefence(BigInteger bitboardMoves, GameColor playerColor)
    {
        //O(1)
        //red turn
        if(playerColor == GameColor.Red)
            return (bitboardMoves|redBitboard) ^  redBitboard;
        //black turn
        return (bitboardMoves|blackBitboard) ^  blackBitboard;
    }

}

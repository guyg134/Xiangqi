using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class BitBoard 
{

    //save the black pieces in bitboard
    private BigInteger blackBitboard = 0;
    //save the red pieces in bitboard
    private BigInteger redBitboard = 0;

    //save the attacking squares by red and black for the current position
    private BigInteger attackingSquaresByRed;
    private BigInteger attackingSquaresByBlack;

    //save the kings position in bitboard
    private BigInteger KingsBitboard = 0;


    public BitBoard(BitBoard bitBoard)
    {
        redBitboard = bitBoard.redBitboard;
        blackBitboard = bitBoard.blackBitboard;

        attackingSquaresByRed = bitBoard.attackingSquaresByRed;
        attackingSquaresByBlack = bitBoard.attackingSquaresByBlack;

        KingsBitboard = bitBoard.KingsBitboard;
    }

    public BitBoard(Board board)
    {
        //save the red and black pieces in bitboard
        redBitboard = BoardToBitboardByColor(board, GameColor.Red);
        blackBitboard = BoardToBitboardByColor(board, GameColor.Black);

        //save the attacking squares by red and black for the current position
        UpdateAttackingSquares(board);
    }

    public BigInteger GetBitboardByColor(GameColor color)
    {
        return color == GameColor.Red ? redBitboard : blackBitboard;
    }

    public BigInteger GetAttackingSquaresByColor(GameColor color)
    {
        return color == GameColor.Red ? attackingSquaresByRed : attackingSquaresByBlack;
    }

    public Position GetKingBitPos(GameColor color)
    {
        List<Position> positions = BitboardToPosition(color == GameColor.Red ? KingsBitboard & redBitboard : 
        KingsBitboard & blackBitboard);
        
        if(positions.Count != 1)
            throw new System.Exception("There is "+positions.Count+" king in color: " + color + 
            "in the board");

        return positions[0];
    }

    public void UpdateBitBoard(Board board, Move move, GameColor color)
    {
        BigInteger bitStartPosition = PosToBitInteger(move.startPosition);
        BigInteger bitEndPosition = PosToBitInteger(move.endPosition);

        //if its red piece update red Bitboard
        if(color == GameColor.Red)
        {
            //remove the current position of the piece in the board
            if((bitStartPosition & redBitboard) == 0)
                throw new System.Exception("The piece is not in the start position");
            redBitboard ^= bitStartPosition;
    
            //remove black piece from blackbitboard, if eaten
            if(move.EatenPiece != null)
                blackBitboard ^= bitEndPosition;

            //update the new position in the board
            redBitboard |= bitEndPosition;

        }
        //if its black update black Bitboard
        else
        {
            if((bitStartPosition & blackBitboard) == 0)
                throw new System.Exception("The piece is not in the start position");
            //remove the current position of the piece in the board
            blackBitboard ^= bitStartPosition;
    
            //remove red piece from blackbitboard
            if(move.EatenPiece != null)
                redBitboard ^= bitEndPosition;

            //update the new position in the board
            blackBitboard |= bitEndPosition;
        }

        //if its king update king Bitboard
        if(move.MovingPiece.GetPieceType() == PieceType.King)
        {
            if((KingsBitboard & bitStartPosition) == 0)
                throw new System.Exception("The king is not in the start position"
                + "start position: " + move.startPosition + " end position: " + move.endPosition + " color: " + color + " piece: " + move.MovingPiece.GetPieceType() + " " + move.MovingPiece.GetPieceColor() + " " + move.MovingPiece.GetPos() + " " + move.MovingPiece.GetPieceBitboardMove(null) + " " + move.MovingPiece.GetPieceBitboardMove(null));
            KingsBitboard ^= bitStartPosition;
            KingsBitboard |= bitEndPosition;
        }

        //update attacking squares
        UpdateAttackingSquares(board);
    }

    public void UndoMoveBitboard(Move move, GameColor turnColor)
    {
        BigInteger bitStartPosition = PosToBitInteger(move.startPosition);
        BigInteger bitEndPosition = PosToBitInteger(move.endPosition);

        //if its red piece update red Bitboard
        if(turnColor == GameColor.Red){
            //remove the current position of the piece in the board
            redBitboard ^= bitEndPosition;

            //add the eaten piece to the enemy board(black)
            if(move.EatenPiece)
                blackBitboard |= bitEndPosition;

            redBitboard |= bitStartPosition;
        }
        //if its black update black Bitboard
        else{
            //remove the current position of the piece in the board
            blackBitboard ^= bitEndPosition;

            //add the eaten piece to the enemy board(red)
            if(move.EatenPiece)
                redBitboard |= bitEndPosition;

            blackBitboard |= bitStartPosition;
        }

        //if its king update king Bitboard
        if(move.MovingPiece.GetPieceType() == PieceType.King)
        {
            KingsBitboard ^= bitEndPosition;
            KingsBitboard |= bitStartPosition;
        }
    }

    public void UpdateAttackingSquares(Board board)
    {
        attackingSquaresByRed = SquaresUnderAttackByColor(board, GameColor.Red);
        attackingSquaresByBlack = SquaresUnderAttackByColor(board, GameColor.Black);
    }

    public bool IsCheck(Board board, GameColor attackingColor)
    {   
        //get the bitboard of the defending player(the bitboard of pieces with the king)
        BigInteger defendingBitboard = attackingColor == GameColor.Red ? blackBitboard : redBitboard;
        //get the position of the king by & the bitboard of the king and the bitboard color of the king
        BigInteger kingPos = KingsBitboard & defendingBitboard;

        //if the king is under attack return true
        return ((attackingColor == GameColor.Red ? attackingSquaresByRed : attackingSquaresByBlack) & kingPos) != 0;
    }

    private BigInteger BoardToBitboardByColor(Board board, GameColor pieceColor)
    {
        BigInteger bitboard = 0;

        foreach (Piece piece in board.GetPiecesList())
        {
            if(piece.GetPieceColor() == pieceColor)
            {
                // Set the corresponding bit position based on the piece type
                BigInteger bitPosition = PosToBitInteger(piece.GetPos());
                bitboard |= bitPosition;

                if(piece.GetPieceType() == PieceType.King)
                {
                    BigInteger mask = bitPosition;
                    KingsBitboard |= mask;
                }
            }
        }
                
        return bitboard;
    }

    public BigInteger SquaresUnderAttackByColor(Board board, GameColor pieceColor)
    {
        BigInteger attackPos = 0;

        foreach (Piece piece in board.GetPiecesList())
        {
            if(piece.GetPieceColor() == pieceColor)
            {
                //here it not checking vaildmoves because it make it a lot slower
                attackPos |= piece.GetPieceBitboardMove(board);
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
        foreach (Piece piece in board.GetPiecesList())
        {
            if(piece.GetPieceColor() == attackingColor)
            {
                if((pos & piece.GetPieceBitboardMove(board)) != 0)
                    pieces.Add(piece);
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

    public static List<Position> BitboardToPosition(BigInteger bitboard)
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
        return PosToBitInteger(pos.x, pos.y);
    }

    public static BigInteger PosToBitInteger(List<Position> positions)
    {
        BigInteger bitboardOfPositions = 0;
        foreach(Position pos in positions)
        {
            bitboardOfPositions |= PosToBitInteger(pos);
        }

        return bitboardOfPositions;
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

    public static BigInteger GetCastleBitboard(bool isDownSide)
    {
        int startY = isDownSide ? 0 : 7;
        BigInteger castleBitboard = 0;

        // Iterate over the 3x3 grid of the castle
        for (int y = startY; y < startY + 3; y++)
        {
            for (int x = 3; x < 6; x++) // Castle is at columns 3, 4, and 5
            {
                castleBitboard |= PosToBitInteger(x, y);
            }
        }

        return castleBitboard;
    }
}

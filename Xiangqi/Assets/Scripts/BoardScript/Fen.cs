using System.Collections;
using System.Collections.Generic;

public class Fen 
{
    private string fenString;


    public Fen()
    {
        fenString = "";
    }
    public Fen(string fenString)
    {
        this.fenString = fenString;
    }

    // Implicit conversion from string to Fen
    public static implicit operator Fen(string fenString)
    {
        return new Fen(fenString);
    }

    // Implicit conversion from Fen to string
    public static implicit operator string(Fen fen)
    {
        return fen.fenString;
    }

    public string GetFenString()
    {
        return fenString;
    }

    public string FenAfterMove(Move move)
    {
        //split the fen string to rows
        string[] fenRows = fenString.Split('/');
        //remove the piece from the start position
        fenRows[move.StartY] = RemovePieceInFen(fenRows[move.StartY], move.StartX);
        //add the piece to the end position
        fenRows[move.EndY] = AddPieceInFen(fenRows[move.EndY], move.EndX, move.MovingPiece.GetPieceType().PieceTypeToChar(move.MovingPiece.GetPieceColor()));
        
        //join the rows to one string
        string newFen = string.Join("/", fenRows);
        return newFen;
    }

    private string RemovePieceInFen(string fenRow, int x)
    {
        char[] newRow = new char[9];
        int currentX = 0;
        //row to array of chars
        for(int i = 0; i < fenRow.Length; i++)
        {
            if(char.IsDigit(fenRow[i]))
            {
                int num = int.Parse(fenRow[i].ToString());
                //add nulls to the array
                for(int j = 0; j < num; j++)
                {
                    newRow[currentX + j] = '\0';
                }
                currentX += num;
            }
            else
            {
                newRow[currentX] = fenRow[i];
                currentX++;
            }
        }

        //remove the piece from the array
        newRow[x] = '\0';

        //array to string
        string newFenRow = FenRowToString(newRow);

        return newFenRow;
    }

    private string AddPieceInFen(string fenRow, int x, char piece)
    {
        char[] newRow = new char[9];
        int currentX = 0;
        //row to array of chars
        for(int i = 0; i < fenRow.Length; i++)
        {
            if(char.IsDigit(fenRow[i]))
            {
                int num = int.Parse(fenRow[i].ToString());
                //add nulls to the array
                for(int j = 0; j < num; j++)
                {
                    newRow[currentX + j] = '\0';
                }
                currentX += num;
            }
            else
            {
                newRow[currentX] = fenRow[i];
                currentX++;
            }
        }

        //add the piece to the array
        newRow[x] = piece;

        //array to string
        string newFenRow = FenRowToString(newRow);

        return newFenRow;
    }

    private string FenRowToString(char[] fenRow)
    {
        string newFen = "";
        int range = 0;
        for(int i = 0; i < 9; i++)
        {
            if(fenRow[i] == '\0')
            {
                range++;
            }
            else
            {
                if(range != 0)
                {
                    newFen += range.ToString();
                    range = 0;
                }
                newFen += fenRow[i];
            }
        }
        if(range != 0)
        {
            newFen += range.ToString();
        }
        return newFen;
    }
}

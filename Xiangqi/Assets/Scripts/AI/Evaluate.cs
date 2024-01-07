using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class Evaluate : MonoBehaviour
{

    //private BitBoard bitBoard;

    const int soliderValue = 100;
    const int advisorValue = 200;
    const int elephantValue = 250;
    const int knightValue = 400;
    const int cannonValue = 450;
    const int rookValue = 900;

    const int red = 1;
    const int black = 2;

    private static Dictionary<PieceType, int> pieceValueFromType = new Dictionary<PieceType, int> (){
           
            [PieceType.Soldier] = soliderValue, [PieceType.Knight] = knightValue,
            [PieceType.Elephant] = elephantValue, [PieceType.Cannon] = cannonValue, [PieceType.Rook] = rookValue,
            [PieceType.Advisor] = advisorValue
        };



    public static float EvaluateFunc(Piece[,] piecesArray, GameColor color)
    {

        float eval = 0;


        eval += (float)((float)PiecesValueCounterByColor(piecesArray, GameColor.Red)/PiecesValueCounterByColor(piecesArray, GameColor.Black)-1);
        return eval ;
    }

    private static int PiecesValueCounterByColor(Piece[,] piecesArray, GameColor piecesColor)
    {
        int piecesCounter = 0;
        

        for (int y = 0; y < 10; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                Piece piece = piecesArray[y, x];
                if(!piece || piece.GetPieceType() == PieceType.King)
                    continue;
                if(piece.GetPieceColor() == piecesColor)
                {
                    piecesCounter += pieceValueFromType[piece.GetPieceType()];
                }
            }
        }
        return piecesCounter;
    }

    private static int UnProtectedPiecesSum(Piece[,] piecesArray, GameColor pieceColor)
    {
        BigInteger colorBitboard = BitBoard.BoardToBitboardByColor(piecesArray, pieceColor); 
        int unDefendedPiecesValues = 0;

        for (int y = 0; y < 10; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                Piece currentPiece = piecesArray[y, x];
                BigInteger attackedPieces;
                
                if(currentPiece == null)
                    continue;
                
                if((int)currentPiece.GetPieceColor() == ((int)pieceColor^1)){
                    attackedPieces = colorBitboard&currentPiece.GetPieceBitboardMove();
                    if(attackedPieces  != 0)
                    {
                        BigInteger piecesDefending = BitBoard.AttackingSquaresBitboard(piecesArray, pieceColor);
                        if((piecesDefending&attackedPieces) == 0)
                        {
                            unDefendedPiecesValues += pieceValueFromType[currentPiece.GetPieceType()];
                        }
                        else
                        {
                            continue;
                            //here check if the piece that underAttack is more value than the attacking piece
                        }
                    }
                }
            }
        }

        return unDefendedPiecesValues;
    }
}

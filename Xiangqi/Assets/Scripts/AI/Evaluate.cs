using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class Evaluate : MonoBehaviour
{
    
    //private BitBoard bitBoard;

    const int kingValue = 18700;
    const int soliderValue = 300;
    const int advisorValue = 400;
    const int elephantValue = 450;
    const int knightValue = 600;
    const int cannonValue = 650;
    const int rookValue = 800;

    //(18700, 400, 500, 600, 800, 600, 300)

    const int red = 1;
    const int black = 2;

    private static Dictionary<PieceType, int> pieceValueFromType = new Dictionary<PieceType, int> (){
           
            [PieceType.King] = kingValue, [PieceType.Soldier] = soliderValue, [PieceType.Knight] = knightValue,
            [PieceType.Elephant] = elephantValue, [PieceType.Cannon] = cannonValue, [PieceType.Rook] = rookValue,
            [PieceType.Advisor] = advisorValue
        };



    public static double EvaluateFunc(Board board, GameColor turnColor)
    {

        double eval = 0;

        //check bonus will have bonus if the player attacking enemy king or minus if the player king under attack
        double checkBonus = 0;

        //save the value of the max undefended piece of the current turn
        int maxUnDefendedPieceValue = 0;
        //save the value of the max attacking piece by the player
        int maxAttackingPieceValue = 0;

        //sum the evaluate of each piece in their intersection
        double playerPiecesIntersectionEvaluateSum = 0;
        double enemyPiecesIntersectionEvaluateSum = 0;

        //sum the value of pieces for each color
        int piecesValueCounterPlayer = 0;
        int piecesValueCounterEnemy = 0;

        //count how many pieces each color
        int piecesCounterPlayer = 0;
        int piecesCounterEnemy = 0;


        for(int i = 1; i < 8; i++)
        {
            foreach(Piece piece in board.GetPiecesInType((PieceType)i))
            {
                Vector2 piecePos = piece.GetPos();
                BigInteger bitPiecePos = BitBoard.PosToBitInteger(piecePos);
                GameColor pieceColor = piece.GetPieceColor();
                PieceType pieceType = piece.GetPieceType();

                
                //the piece is of the current turn
                if(pieceColor == turnColor)
                {
                    piecesCounterPlayer++;

                    //add to the sum values of pieces of red
                    piecesValueCounterPlayer += pieceValueFromType[pieceType];

                    //find max unprotected piece
                    //if this piece is under attack by the enemy pieces                                                                 
                    if((bitPiecePos & BitBoard.AttackingSquaresBitboard(board, turnColor.OppositeColor())) != 0)
                    {
                        //if attacked piece is player king
                        if(pieceType == PieceType.King)
                        {
                            Piece pieceAttackingKing = BitBoard.PieceThatAttackingPieceOnPos(board, bitPiecePos, turnColor.OppositeColor());
                            //check if checkmate and if it is down the evaluate to the max
                            if(!board.PlayerHaveMoves(turnColor))
                            {
                                return -10000;
                            }
                            //if not check mate check if the piece that attacking the king is on safe intersection if yes add king to the max attaking piece value if not keep going
                            else if((BitBoard.PosToBitInteger(pieceAttackingKing.GetPos()) & BitBoard.AttackingSquaresBitboard(board, turnColor)) == 0)
                                checkBonus -= 100;
                        }

                        //if the piece is undefended by the player pieces
                        else if((bitPiecePos & BitBoard.AttackingSquaresBitboard(board, turnColor)) == 0)
                        {
                            if(maxUnDefendedPieceValue < pieceValueFromType[pieceType])
                            {
                                maxUnDefendedPieceValue = pieceValueFromType[pieceType];
                            }
                        }
                    }


                    //add the piece intersection value for the sum of the player
                    //if the player is on down side of the board use x and y, if its up on the board use 8-x and 9-y
                    if(GameObject.FindWithTag("GameManager").GetComponent<GameManager>().IsColorOnDownSide(turnColor))
                        playerPiecesIntersectionEvaluateSum += EvaluatePieceIntersectionsTables.GetPieceSquareValue(piece.GetPieceType(), piece.GetX(), piece.GetY());
                    else
                        playerPiecesIntersectionEvaluateSum += EvaluatePieceIntersectionsTables.GetPieceSquareValue(piece.GetPieceType(), 8 - piece.GetX(), 9 -piece.GetY());
                    

                }
                //the piece is an enemy piece
                else
                {
                    piecesCounterEnemy++;

                    //add to the sum values of pieces of red
                    piecesValueCounterEnemy += pieceValueFromType[pieceType];
                    //find max under attack piece
                    //if this piece is under attack by the player pieces
                    if((bitPiecePos & BitBoard.AttackingSquaresBitboard(board, turnColor)) != 0)
                    {
                        //save the piece that attacking the enemy piece
                        Piece pieceAttacking = BitBoard.PieceThatAttackingPieceOnPos(board, bitPiecePos, turnColor);

                        //check if the piece that attacking the enemy piece is on safe intersection
                        if((BitBoard.PosToBitInteger(pieceAttacking.GetPos()) & BitBoard.AttackingSquaresBitboard(board, turnColor.OppositeColor())) == 0)
                        {
                            //if attacked piece is enemy king
                            if(pieceType == PieceType.King)
                            {
                                //check if checkmate and if it is up the evaluate to the max
                                if(!board.PlayerHaveMoves(turnColor.OppositeColor()))
                                {
                                    return 10000;
                                }
                                checkBonus += 100;
                            }
                            //if the piece is undefended by the enemy pieces
                            else if((bitPiecePos & BitBoard.AttackingSquaresBitboard(board, turnColor.OppositeColor())) == 0)
                            {
                                if(maxAttackingPieceValue < pieceValueFromType[pieceType])
                                {
                                    maxAttackingPieceValue = pieceValueFromType[pieceType];
                                }
                            }
                        }
                    }


                    //add the piece intersection value for the sum of the enemy
                    //if the enemy is on down side of the board use x and y, if its up on the board use 8-x and 9-y
                    if(GameObject.FindWithTag("GameManager").GetComponent<GameManager>().IsColorOnDownSide(turnColor.OppositeColor()))
                        enemyPiecesIntersectionEvaluateSum += EvaluatePieceIntersectionsTables.GetPieceSquareValue(piece.GetPieceType(), piece.GetX(), piece.GetY());
                    else
                        enemyPiecesIntersectionEvaluateSum += EvaluatePieceIntersectionsTables.GetPieceSquareValue(piece.GetPieceType(), 8 - piece.GetX(), 9 -piece.GetY());
                    
                }
            }
        }

        
        piecesValueCounterPlayer -= maxUnDefendedPieceValue;
        piecesValueCounterEnemy -= (int)(maxAttackingPieceValue * 0.5f); 
        
        //add check for the conclusion
        double checkWeight = 1.5f;
        eval += checkWeight * checkBonus;

        //add the pieces differences of the players
        double pieceValueDiffWeight = 2f;  // Adjust the weight as needed
        eval += pieceValueDiffWeight * (piecesValueCounterPlayer - piecesValueCounterEnemy);

        //add the pieces positions 
        double playerIntersectionWeight = 0.2f;  // Adjust the weight as needed
        double enemyIntersectionWeight = 0.2f;   // Adjust the weight as needed
        eval += playerIntersectionWeight * (playerPiecesIntersectionEvaluateSum / piecesCounterPlayer);
        eval -= enemyIntersectionWeight * (enemyPiecesIntersectionEvaluateSum / piecesCounterPlayer);
        
        if(turnColor == GameColor.Red)
            return eval;
        else
            return eval * (-1);
    }
 
}

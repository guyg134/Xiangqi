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
                            Piece pieceAttackingKing = BitBoard.PieceOfAttackingPieceOnPos(board, bitPiecePos, turnColor.OppositeColor());
                            //check if checkmate and if it is down the evaluate to the max
                            if(!board.PlayerHaveMoves(turnColor))
                            {
                                checkBonus = -10000;
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
                        //if attacked piece is enemy king
                        if(pieceType == PieceType.King)
                        {
                            Piece pieceAttackingKing = BitBoard.PieceOfAttackingPieceOnPos(board, bitPiecePos, turnColor);
                            //check if checkmate and if it is up the evaluate to the max
                            if(!board.PlayerHaveMoves(turnColor.OppositeColor()))
                            {
                                checkBonus = 10000;
                            }
                            //check if the piece that attacking the king is on safe intersection if yes add king to the max attaking piece value if not keep going
                            if((BitBoard.PosToBitInteger(pieceAttackingKing.GetPos()) & BitBoard.AttackingSquaresBitboard(board, turnColor.OppositeColor())) == 0)
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
        piecesValueCounterEnemy -= (int)(maxAttackingPieceValue); 
        
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


    /*
    //evaluate the pieces differences and unprotected pieces on the board between the two colors
    private static float EvaluatePieces(Board board, GameColor turnColor)
    {
        int piecesValueCounterRed = PiecesValueCounterByColor(board, GameColor.Red);
        int piecesValueCounterBlack = PiecesValueCounterByColor(board, GameColor.Black);

        int maxUnprotectedPieceOfEnemy = UnProtectedPieceMax(board, turnColor);

        if(turnColor == GameColor.Red)
        {
            piecesValueCounterRed -= maxUnprotectedPieceOfEnemy;
        }
        else
        {
            piecesValueCounterBlack -= maxUnprotectedPieceOfEnemy;
        }

        return (float)((float)piecesValueCounterRed/piecesValueCounterBlack-1);
    }

    private static float EvaluatePiecesIntersections(Board board)
    {
        float redPiecesIntersectionsSum = PieceIntersectionsValuesSum(board, GameColor.Red);
        float blackPiecesIntersectionsSum = PieceIntersectionsValuesSum(board, GameColor.Black);
        return (float)((float)redPiecesIntersectionsSum/blackPiecesIntersectionsSum - 1);
    }

    //
    private static int PiecesValueCounterByColor(Board board, GameColor piecesColor)
    {
        int piecesCounter = 0;
        
        for(int i = 1; i < 8; i++)
        {
            foreach(Piece piece in board.GetPiecesInType((PieceType)i))
            {
                if(piece.GetPieceColor() == piecesColor)
                {
                    piecesCounter += pieceValueFromType[piece.GetPieceType()];
                }
            }
        }
        
        return piecesCounter;
    }

    
    //return the max value unprotected piece
    private static int UnProtectedPieceMax(Board board, GameColor piecesColor)
    {
        BigInteger colorBitboard = BitBoard.BoardToBitboardByColor(board, piecesColor); 
        int unDefendedPiecesValueMax = 0;

        for(int i = 1; i < 8; i++)
        {
            foreach(Piece currentPiece in board.GetPiecesInType((PieceType)i))
            {
                BigInteger attackedPieces;
                
                if(currentPiece.GetPieceColor() == piecesColor.OppositeColor() && currentPiece.GetPieceType() != PieceType.King){
                    //the board with just the player color & the bitboard of attacking squares by the enemy
                    attackedPieces = colorBitboard & currentPiece.GetPieceBitboardMove(board);
                    //if its equal to 0 so there is no pieces that under attack else check if its defended
                    if(attackedPieces  != 0)
                    {
                        //get the squares that defended
                        BigInteger piecesDefending = BitBoard.AttackingSquaresBitboard(board, piecesColor);
                        //check if the pieces under attack and the defended squares are the same
                        if((piecesDefending&attackedPieces) == 0 )
                        {
                            if(unDefendedPiecesValueMax < pieceValueFromType[currentPiece.GetPieceType()])
                                unDefendedPiecesValueMax = pieceValueFromType[currentPiece.GetPieceType()];
                        }
                        else 
                        {
                            if(unDefendedPiecesValueMax < pieceValueFromType[currentPiece.GetPieceType()])
                                continue;
                                //i want to check if the protectinc 
                        }
                    }
                }
            }
        }

        return unDefendedPiecesValueMax;
    }

    //return the sum of the piece intersections evaluate values
    private static float PieceIntersectionsValuesSum(Board board, GameColor gameColor)
    {
        // Implement piece-square tables evaluation here
        float pstScore = 0;
        //scan without kings
        for(int i = 2; i < 8; i++)
            {
                foreach(Piece piece in board.GetPiecesInType((PieceType)i))
                {
                    //if the piece is the color of the function add the piece position value
                    if(piece.GetPieceColor() == gameColor)
                    {
                        //if the player is on the down side i send the regular x and y if not i send its alternate
                        if(GameObject.FindWithTag("GameManager").GetComponent<GameManager>().IsColorOnDownSide(gameColor))
                            pstScore += EvaluatePieceIntersectionsTables.GetPieceSquareValue(piece.GetPieceType(), piece.GetX(), piece.GetY());
                        else
                            pstScore += EvaluatePieceIntersectionsTables.GetPieceSquareValue(piece.GetPieceType(), 8 - piece.GetX(), 9 -piece.GetY());
                    }
                }
            }

        return pstScore;
    }*/
}

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using UnityEngine;

public class Evaluate 
{
    
    private const int kingValue = 18700;
    private const int soliderValue = 300;
    private const int advisorValue = 400;
    private const int elephantValue = 450;
    private const int knightValue = 600;
    private const int cannonValue = 650;
    private const int rookValue = 800;

    //(18700, 400, 500, 600, 800, 600, 300)

    private const double checkWeight = 1.5f; 
    private const double pieceValueDiffWeight = 2; 
    private const double playerIntersectionWeight = 0.25f; 
    private const double enemyIntersectionWeight = 0.25f; 
    private const double maxUndefendedPieceValueWeight = 1.2f; 
    private const double maxAttackingPieceValueWeight = 0.2f; 

    private const int checkMateValue = 10000;

    // Counters for the number of pieces
    private static int piecesCounterPlayer = 0;
    private static int piecesCounterEnemy = 0;
    // Counters for the value of the pieces
    private static int piecesValueCounterPlayer = 0;
    private static int piecesValueCounterEnemy = 0;
    // Max value of undefended piece(or defended but the exchange not worth it, the value will be undefended piece value-attackingpiece value) and attacking piece
    private static int maxUnDefendedPieceValue = 0;
    private static int maxAttackingPieceValue = 0;
    // Save the check bonus for the player
    private static double checkBonus = 0;
    // Sum of the intersection values of the pieces for each player 
    private static double playerPiecesIntersectionEvaluateSum = 0;
    private static double enemyPiecesIntersectionEvaluateSum = 0;


    const GameColor red = GameColor.Red;
    const GameColor black = GameColor.Black;

    private static Dictionary<PieceType, int> pieceValueFromType = new Dictionary<PieceType, int> (){
           
            [PieceType.King] = kingValue, [PieceType.Soldier] = soliderValue, [PieceType.Knight] = knightValue,
            [PieceType.Elephant] = elephantValue, [PieceType.Cannon] = cannonValue, [PieceType.Rook] = rookValue,
            [PieceType.Advisor] = advisorValue
        };


    
        //calculate the position and return the evaluate value
        
            // Evaluate the position and return the evaluate value

            public static double EvaluatePosition(Board board, Player currentPlayer)
            {
                //reset the values of the variables
                piecesCounterPlayer = 0;
                piecesCounterEnemy = 0;
                // Counters for the value of the pieces
                piecesValueCounterPlayer = 0;
                piecesValueCounterEnemy = 0;
                // Max value of undefended piece and attacking piece
                maxUnDefendedPieceValue = 0;
                maxAttackingPieceValue = 0;
                // Save the check bonus for the player
                checkBonus = 0;
                // Sum of the intersection values of the pieces for each player 
                playerPiecesIntersectionEvaluateSum = 0;
                enemyPiecesIntersectionEvaluateSum = 0;

                GameColor turnColor = currentPlayer.playerColor;
                
                // Get the bitboards of the attacking squares for each player
                BigInteger PlayerAttackingBitboard = BitBoard.AttackingSquaresBitboard(board, turnColor);
                BigInteger EnemyAttackingBitboard = BitBoard.AttackingSquaresBitboard(board, turnColor.OppositeColor());

                for (int i = 1; i < 8; i++)
                {
                    foreach (Piece piece in board.GetPiecesInType((PieceType)i))
                    {
                        GameColor pieceColor = piece.GetPieceColor();
                        // Get the position of the piece as bit integer
                        BigInteger bitPiecePos = BitBoard.PosToBitInteger(piece.GetPos());

                        // If the piece is player piece
                        if (pieceColor == turnColor)
                        {
                            //call to update player variables
                            UpdatePlayerVariables(  board, turnColor, piece, bitPiecePos, PlayerAttackingBitboard, EnemyAttackingBitboard);
                            UpdatePlayerPiecesIntersectionEvaluateSum(currentPlayer, piece);
                        }
                        // If the piece is enemy piece
                        else
                        {
                            // Go through the enemy pieces moves that can attack king and check if there is checkmate, if yes return the checkmate value
                            /*if(GameBoard.PieceCanAttackKing(piece.GetPieceType()) && CheckMateNextMove(board, piece))
                            {
                                return EvaluateNumberByColor(checkMateValue, turnColor);
                            }*/

                            // call to update enemy variables
                            UpdateEnemyVariables( board, turnColor, piece, bitPiecePos, PlayerAttackingBitboard, EnemyAttackingBitboard);
                            UpdateEnemyPiecesIntersectionEvaluateSum(currentPlayer, piece);
                        }
                    }
                }
                //return the evaluate number after the calculation
                return EvaluateNumber(piecesValueCounterPlayer, piecesValueCounterEnemy, maxUnDefendedPieceValue, maxAttackingPieceValue,
                    checkBonus, playerPiecesIntersectionEvaluateSum, enemyPiecesIntersectionEvaluateSum, piecesCounterPlayer, piecesCounterEnemy, turnColor);
            }

            // Update variables for player pieces
            private static void UpdatePlayerVariables(Board board, GameColor turnColor, Piece piece, 
                BigInteger bitPiecePos, BigInteger PlayerAttackingBitboard, BigInteger EnemyAttackingBitboard)
            {
                // Counters for the number of pieces
                piecesCounterPlayer++;
                piecesValueCounterPlayer += pieceValueFromType[piece.GetPieceType()];
                // If the piece under attack by the enemy pieces
                if ((bitPiecePos & EnemyAttackingBitboard) != 0)
                {
                    // Get list of enemy pieces that attacking the piece 
                    List<Piece> enemyPiecesAttackingMyPiece = BitBoard.PiecesThatAttackingPos(board, bitPiecePos, turnColor.OppositeColor());
                    
                    // If the piece is king (cant happened to player but in the evaluation of the eval bar, because there is no move that player can do that put his king under attack)
                    if (piece.GetPieceType() == PieceType.King)
                    {
                        UpdatePlayerKingVariables(board, bitPiecePos, turnColor, enemyPiecesAttackingMyPiece);
                    }
                    // If the piece is not king
                    else 
                    {
                        // If the piece is not defended
                        if ((bitPiecePos & PlayerAttackingBitboard) == 0)
                            UpdateMaxUnDefendedPieceValue(pieceValueFromType[piece.GetPieceType()]);
                        // If the piece is defended by the player pieces
                        else
                        {
                            if(enemyPiecesAttackingMyPiece.Count > 1)
                            {
                                //if there is more than 1 enemy piece attacking the piece, update the max undefended piece value with the piece value because it doesn't matter the player piece defended 
                                UpdateMaxUnDefendedPieceValue(pieceValueFromType[piece.GetPieceType()]);
                            }
                            else
                            {
                                // if just one enemy piece attacking my piece update the max undefended piece value with the difference between the piece value and the value attacking piece 
                                UpdateMaxUnDefendedPieceValue(pieceValueFromType[piece.GetPieceType()] - pieceValueFromType[enemyPiecesAttackingMyPiece[0].GetPieceType()]);
                            }
                        }
                    }
                }
            }

            // Update variables for enemy pieces
            private static void UpdateEnemyVariables(Board board, GameColor turnColor, Piece piece, 
                BigInteger bitPiecePos, BigInteger PlayerAttackingBitboard, BigInteger EnemyAttackingBitboard)
            {
                piecesCounterEnemy++;
                piecesValueCounterEnemy += pieceValueFromType[piece.GetPieceType()];
                // If the enemy piece under attack by the player pieces
                if ((bitPiecePos & PlayerAttackingBitboard) != 0)
                {
                    // Get list of enemy pieces that attacking the piece 
                    List<Piece> playerPiecesAttackingEnemyPiece = BitBoard.PiecesThatAttackingPos(board, bitPiecePos, turnColor);

                    if (piece.GetPieceType() == PieceType.King)
                    {
                        UpdateEnemyKingVariables(board, bitPiecePos, turnColor, playerPiecesAttackingEnemyPiece);
                    }
                    // If the piece is not king
                    else
                    {
                        // If the enemy piece is not defended
                        if((bitPiecePos & EnemyAttackingBitboard) == 0)
                            UpdateMaxAttackingPieceValue(pieceValueFromType[piece.GetPieceType()]);
                        // If the piece is defended by the enemy pieces
                        else
                        {
                            int pieceValue = pieceValueFromType[piece.GetPieceType()];
                            int maxAttackingPieceValue = pieceValueFromType[GetMostWorthPieceInList(playerPiecesAttackingEnemyPiece).GetPieceType()];
                            // If the attacking piece is worth more than the enemy piece
                            UpdateMaxAttackingPieceValue(pieceValue - maxAttackingPieceValue);
                        }
                    }
                }
            }

            // Update variables for player king
            private static void UpdatePlayerKingVariables(Board board, BigInteger bitPiecePos, GameColor turnColor, List<Piece> enemyPiecesAttackingKing)
            {
                
                // Check if checkmate and if it is down the evaluate to the max
                if (!board.PlayerHaveMoves(turnColor))
                {
                    checkBonus -= checkMateValue;
                }
                // If not check mate, check if the pieces that attacking the king is more than one if yes down the check bonus
                else if(enemyPiecesAttackingKing.Count > 1)
                {
                    checkBonus -= 150;
                }
                // If not check mate, check if the piece that attacking the king is on safe intersection if yes down the check bonus
                else if ((BitBoard.PosToBitInteger(enemyPiecesAttackingKing[0].GetPos()) & BitBoard.AttackingSquaresBitboard(board, turnColor)) == 0)
                {
                    checkBonus -= 100;
                }
            }

            // Update variables for enemy king
            private static void UpdateEnemyKingVariables(Board board, BigInteger bitPiecePos, GameColor turnColor, List<Piece> playerPiecesAttackingKing)
            {
                // Check if checkmate and if it is up the evaluate to the max
                if (!board.PlayerHaveMoves(turnColor.OppositeColor()))
                {
                    checkBonus += checkMateValue;
                }
                // If not check mate, check if the pieces that attacking the king is more than one if yes up the check bonus
                else if(playerPiecesAttackingKing.Count > 1)
                {
                    checkBonus -= 150;
                }
                // If not check mate, check if the piece that attacking the king is on safe intersection if yes add to the check bonus
                else if ((BitBoard.PosToBitInteger(playerPiecesAttackingKing[0].GetPos()) & BitBoard.AttackingSquaresBitboard(board, turnColor.OppositeColor())) == 0)
                {
                    checkBonus += 100;
                }
            }

            // Update max undefended piece value
            private static void UpdateMaxUnDefendedPieceValue(int pieceValue)
            {
                if (maxUnDefendedPieceValue < pieceValue)
                {
                    maxUnDefendedPieceValue = pieceValue;
                }
                // If the piece value is negative set it to zero, because you dont want bonus for piece under attack even if the piece that attacking it is worth MORE
                if(maxUnDefendedPieceValue < 0)
                {
                    maxUnDefendedPieceValue = 0;
                }
            }

            // Update max attacking piece value
            private static void UpdateMaxAttackingPieceValue(int pieceValue)
            {
                if (maxAttackingPieceValue < pieceValue)
                {
                    maxAttackingPieceValue = pieceValue;
                }
            }

            // Update player pieces intersection evaluate sum
            private static void UpdatePlayerPiecesIntersectionEvaluateSum(Player turnPlayer, Piece piece)
            {
                if (turnPlayer.playOnDownSide)
                {
                    playerPiecesIntersectionEvaluateSum += EvaluatePieceIntersectionsTables.GetPieceSquareValue(piece.GetPieceType(), piece.GetX(), piece.GetY());
                }
                else
                {
                    playerPiecesIntersectionEvaluateSum += EvaluatePieceIntersectionsTables.GetPieceSquareValue(piece.GetPieceType(), 8 - piece.GetX(), 9 - piece.GetY());
                }
            }

            // Update enemy pieces intersection evaluate sum
            private static void UpdateEnemyPiecesIntersectionEvaluateSum(Player turnPlayer, Piece piece)
            {
                if (!turnPlayer.playOnDownSide)
                {
                    enemyPiecesIntersectionEvaluateSum += EvaluatePieceIntersectionsTables.GetPieceSquareValue(piece.GetPieceType(), piece.GetX(), piece.GetY());
                }
                else
                {
                    enemyPiecesIntersectionEvaluateSum += EvaluatePieceIntersectionsTables.GetPieceSquareValue(piece.GetPieceType(), 8 - piece.GetX(), 9 - piece.GetY());
                }
            }

    


    //calculate the evaluate number with the all evaluate paremters
    private static double EvaluateNumber(int piecesValueCounterPlayer, int piecesValueCounterEnemy, int maxUnDefendedPieceValue, int maxAttackingPieceValue,
      double checkBonus, double playerPiecesIntersectionEvaluateSum, double enemyPiecesIntersectionEvaluateSum, int piecesCounterPlayer, int piecesCounterEnemy, GameColor turnColor)
    {
        
        double eval = 0;
        piecesValueCounterPlayer -= (int)(maxUnDefendedPieceValue * maxUndefendedPieceValueWeight);
        piecesValueCounterEnemy -= (int)(maxAttackingPieceValue * maxAttackingPieceValueWeight); 
        
        //add check for the conclusion
        eval += checkWeight * checkBonus;

        //add the pieces differences of the players
         // Adjust the weight as needed
        eval += pieceValueDiffWeight * (piecesValueCounterPlayer - piecesValueCounterEnemy);

        //add the pieces positions 
        eval += playerIntersectionWeight * (playerPiecesIntersectionEvaluateSum / piecesCounterPlayer);
        eval -= enemyIntersectionWeight * (enemyPiecesIntersectionEvaluateSum / piecesCounterEnemy);

        return EvaluateNumberByColor(eval, turnColor);
    }


    //return the evaluate number of checkmate by the color of the current turn
    private static double EvaluateNumberByColor(double eval, GameColor turnColor)
    {
        return turnColor == red ? eval : -eval;
    }

    private static Piece GetMostWorthPieceInList(List<Piece> pieces)
    {
        Piece mostWorthPiece = pieces[0];
        foreach (Piece piece in pieces)
        {
            if (pieceValueFromType[piece.GetPieceType()] > pieceValueFromType[mostWorthPiece.GetPieceType()])
            {
                mostWorthPiece = piece;
            }
        }

        return mostWorthPiece;
    }

    private static Piece GetLeastPieceInList(List<Piece> pieces)
    {
        Piece leastPiece = pieces[0];
        foreach (Piece piece in pieces)
        {
            if (pieceValueFromType[piece.GetPieceType()] < pieceValueFromType[leastPiece.GetPieceType()])
            {
                leastPiece = piece;
            }
        }

        return leastPiece;
    }

    private static bool CheckMateNextMove(Board board, Piece enemyPiece)
    {
        List<Position> validMoves = enemyPiece.GetValidMoves(board);
        foreach(Position pos in validMoves)
        {
            Board copyBoard = new Board(board);

            Move move = new Move(enemyPiece.GetX(), enemyPiece.GetY(), pos.x, pos.y, enemyPiece, board.FindPiece(pos.x, pos.y));
            Console.WriteLine($"Checking move: {move.Name()}");

            copyBoard.MovePieceOnBoard(move);

            // If after the move, the current player has no legal moves, it's checkmate
            if (!copyBoard.PlayerHaveMoves(enemyPiece.GetPieceColor().OppositeColor()))
            {
                return true;
            }

            copyBoard.UndoLastMove();
        }

        return false;

    }

}
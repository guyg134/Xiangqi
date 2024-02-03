using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using UnityEngine;

public class Evaluate 
{
    public static readonly int checkMateValue = 25000;


    const GameColor red = GameColor.Red;
    const GameColor black = GameColor.Black;


    public static double EvaluatePosition(Board board, Player currentPlayer)
    {
        GameState gameState = board.GetGameState();
        GameColor turnColor = currentPlayer.playerColor;

        // If there is no moves for the player or the enemy return checkmate value
        if (!board.PlayerHaveMoves(turnColor))
        {
            return EvaluateNumberByColor(checkMateValue, turnColor.OppositeColor());
        }
        if(!board.PlayerHaveMoves(turnColor.OppositeColor()))
        {
            return EvaluateNumberByColor(checkMateValue, turnColor);
        }
        
        // Get the evaluation state for the current game state
        EvaluationState evaluateState = EvaluationState.GetEvaluationState(gameState);

        // Calculate the evaluation
        return evaluateState.EvaluateState(board, currentPlayer);
    }

    //evaluate the current position and return the evaluate value (for the eval bar)
    public static double EvaluateCurrentPosition(Board board, Player currentPlayer)
    {
        if(CheckMateNextMove(board, currentPlayer.playerColor.OppositeColor()))
        {
            return EvaluateNumberByColor(checkMateValue, currentPlayer.playerColor.OppositeColor());
        }

        return EvaluatePosition(board, currentPlayer);
       
    }

    public static bool CheckMateNextMove(Board board, GameColor enemyColor)
    {
        Board copyBoard = new Board(board);
        
        for (int i = 1; i < 8; i++)
        {
            foreach (Piece piece in board.GetPiecesInType((PieceType)i))
            {
                if(piece.GetPieceColor() == enemyColor){
                    List<Position> validMoves = piece.GetValidMoves(copyBoard);

                    foreach(Position pos in validMoves)
                    {
                        Move move = new Move(piece.GetX(), piece.GetY(), pos.x, pos.y, piece, board.FindPiece(pos.x, pos.y));

                        copyBoard.MovePieceOnBoard(move);

                        // If after the move, the current player has no legal moves, it's checkmate
                        if (!copyBoard.PlayerHaveMoves(piece.GetPieceColor().OppositeColor()))
                        {
                            copyBoard.UndoLastMove();
                            return true;
                        }

                        copyBoard.UndoLastMove();
                    }
                }
            }
        }

        return false;

    }

    public static double EvaluateNumberByColor(double eval, GameColor turnColor)
    {
        return turnColor == red ? eval : -eval;
    }
    
    /*private static double EvaluatePosition(Board board, Player currentPlayer)
    {
        EvaluationState evaluateState = EvaluationState.GetEvaluationState(board, currentPlayer);
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
    private static double EvaluateNumber(EvaluationState evaluationState, GameColor turnColor)
    {
        int piecesCounterPlayer = evaluationState.PiecesCounterPlayer;
        int piecesCounterEnemy = evaluationState.PiecesCounterEnemy;
        int piecesValueCounterPlayer = evaluationState.PiecesValueCounterPlayer;
        int piecesValueCounterEnemy = evaluationState.PiecesValueCounterEnemy;
        int maxUnDefendedPieceValue = evaluationState.MaxUnDefendedPieceValue;
        int maxAttackingPieceValue = evaluationState.MaxAttackingPieceValue;
        double checkBonus = evaluationState.CheckBonus;
        double playerPiecesIntersectionEvaluateSum = evaluationState.PlayerPiecesIntersectionEvaluateSum;
        double enemyPiecesIntersectionEvaluateSum = evaluationState.EnemyPiecesIntersectionEvaluateSum;
        
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
    }*/

}
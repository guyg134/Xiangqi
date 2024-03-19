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

    private Player CurrentPlayer;
    private BigInteger playerAttackingBitboard;
    private BigInteger enemyAttackingBitboard;
    
    
    private int ValueDiff;

    //pieces counter and pieces value sum for player and enemy
    private int PiecesCounterPlayer;
    private int PiecesCounterEnemy;

    //under attack pieces value sum of player and enemy
    private double UnderAttackPlayer;
    private double UnderAttackEnemy;

    
    public Evaluate()
    {
        //initialzed parameters
        ValueDiff = 0;

        PiecesCounterPlayer = 0;
        PiecesCounterEnemy = 0;

        UnderAttackPlayer = 0;
        UnderAttackEnemy = 0;
    }

    //evaluate the current position and return the evaluate value (for the eval bar)
    public double EvaluateCurrentPosition(Board board, Player currentPlayer)
    {
        if(CheckMateNextMove(board, currentPlayer.playerColor.OppositeColor()))
        {
            
            return EvaluateNumberByColor(checkMateValue, currentPlayer.playerColor.OppositeColor());
        }

        return EvaluatePosition(board, currentPlayer);
       
    }

    public bool CheckMateNextMove(Board board, GameColor enemyColor)
    {
        Board copyBoard = new Board(board);
        
        foreach (Piece piece in copyBoard.GetPiecesList())
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

        return false;

    }

    public static double EvaluateNumberByColor(double eval, GameColor turnColor)
    {
        return turnColor == red ? eval : -eval;
    }
    
    public double EvaluatePosition(Board board, Player currentPlayer)
    {
        CurrentPlayer = currentPlayer;
        GameColor turnColor = CurrentPlayer.playerColor;

        // Get the bitboards of the attacking squares for each player
        playerAttackingBitboard = BitBoard.IntersectionsUnderAttackByColor(board, turnColor);
        enemyAttackingBitboard = BitBoard.IntersectionsUnderAttackByColor(board, turnColor.OppositeColor());

        foreach (Piece piece in board.GetPiecesList())
        {
            PieceType pieceType = piece.GetPieceType();
            bool isPlayerPiece = piece.GetPieceColor() == currentPlayer.playerColor;

            //add the piece value of the piece 
            CountersValues(isPlayerPiece, pieceType);
            
            if(pieceType == PieceType.King)
            {
                //KingSafety();
            }
            else
            {
                IsPieceUnderAttack(board, piece);
            }
        }

        return CalculateEvaluation(turnColor);
    
    }

    // Update the variables of piece
    private void IsPieceUnderAttack(Board board, Piece piece)
    {
        //init turn color and isPlayerPiece
        GameColor turnColor = CurrentPlayer.playerColor;
        bool isPlayerPiece = piece.GetPieceColor() == turnColor;
        //init the piece bitpos
        BigInteger bitPiecePos = BitBoard.PosToBitInteger(piece.GetPos());
        
        // If its player's piece and under attack by the enemy pieces, or if its enemy piece and under attack by the player pieces
        if ((bitPiecePos & (isPlayerPiece ? enemyAttackingBitboard : playerAttackingBitboard)) != 0)
        {
            // Get list of pieces that attacking this piece
            List<Piece> attackingPieces = BitBoard.PiecesThatAttackingPos(board, bitPiecePos, isPlayerPiece ? turnColor.OppositeColor() : turnColor);

            if(piece.GetPieceType() != PieceType.King)
            {
                // If its player piece
                if(isPlayerPiece)
                {
                    int underAttack = EvaluateConstants.PieceValues[(int)piece.GetPieceType()];
                    // If the piece is not defended or there is more than 1 enemy piece attacking it, update the max undefended piece value with the piece value
                    if ((bitPiecePos & playerAttackingBitboard) == 0 || attackingPieces.Count > 1)
                    {
                        UnderAttackPlayer += underAttack;
                    }
                    else
                    {
                        int attackingPiece = EvaluateConstants.PieceValues[(int)attackingPieces[0].GetPieceType()];
                        // If the piece is defended by the player pieces but it more variable than the attacking piece, 
                        //update the max undefended piece value with the difference between the piece value and the value attacking piece
                        if((bitPiecePos & playerAttackingBitboard) != 0 && underAttack > attackingPiece)
                            UnderAttackPlayer += underAttack - attackingPiece;
                    }
                }
                // If its enemy piece
                else
                {
                    // If there is more than 1 player piece attacking the enemy piece or its just one and it not defended
                    if(attackingPieces.Count > 1 || (BitBoard.PosToBitInteger(attackingPieces[0].GetPos()) & enemyAttackingBitboard) == 0)
                    {
                        // If the enemy piece is not defended by the enemy pieces
                        if ((bitPiecePos & enemyAttackingBitboard) == 0)
                        {
                            UpdateMaxAttackingPieceValue(PieceValues[(int)piece.GetPieceType()]);
                        }
                        // If the enemy piece is defended by the enemy pieces
                        else
                        {
                            //update the max attacking piece value with the difference between the enemy piece value and the value of most variable attacking piece
                            UpdateMaxAttackingPieceValue(PieceValues[(int)piece.GetPieceType()] -
                                PieceValues[(int)attackingPiece.GetPieceType()]);
                        }
                    }
                }
            }
        }
    }

    private void CountersValues(bool isPlayerPiece, PieceType pieceType)
    {
        if (isPlayerPiece)
        {
            PiecesCounterPlayer++;
            ValueDiff += EvaluateConstants.PieceValues[(int)pieceType];
        }
        else
        {
            PiecesCounterEnemy++;
            ValueDiff -= EvaluateConstants.PieceValues[(int)pieceType];
        }
    }
    
    private double CalculateEvaluation(GameColor turnColor)
    {
        double eval = 0;

        //add check for the conclusion
        eval += checkWeight * CheckBonus;

        PiecesValueCounterPlayer -= (int)(MaxUnDefendedPieceValue * maxUndefendedPieceValueWeight);
        PiecesValueCounterEnemy -= (int)(MaxAttackingPieceValue * maxAttackingPieceValueWeight); 
        
        //add the pieces differences of the players
         // Adjust the weight as needed
        eval += pieceValueDiffWeight * (PiecesValueCounterPlayer - PiecesValueCounterEnemy);

        //add the pieces positions 
        eval += playerIntersectionWeight * (PlayerPiecesIntersectionEvaluateSum / PiecesCounterPlayer);
        eval -= enemyIntersectionWeight * (EnemyPiecesIntersectionEvaluateSum / PiecesCounterEnemy);
        
        return turnColor == GameColor.Red ? eval : -eval;
    }
}
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UIElements;

public class Evaluate 
{
    public static readonly int checkMateValue = 25000;

    private Player CurrentPlayer;
    private Board board;

    // The attacking squares bitboards of the player and the enemy
    private BigInteger playerAttackingBitboard;
    private BigInteger enemyAttackingBitboard;
    
    // Save the difference between the pieces value sum of the player and the enemy
    private int ValueDiff;

    //pieces counter and pieces value sum for player and enemy
    private int SafePiecesCounterPlayer;
    private int SafePiecesCounterEnemy;

    //under attack pieces value sum of player and enemy
    private double UnderAttackPlayer;
    private double UnderAttackEnemy;

    //pieces intersection evaluate sum of player and enemy
    private float PlayerPiecesIntersectionEvaluateSum;
    private float EnemyPiecesIntersectionEvaluateSum;

    //knight moves counter of player and enemy
    private int KnightMovesCounterPlayer;
    private int KnightMovesCounterEnemy;

    private int kingSafetyPlayer;
    private int kingSafetyEnemy;
    
    public Evaluate()
    {
        //initialzed parameters
        ValueDiff = 0;

        SafePiecesCounterPlayer = 0;
        SafePiecesCounterEnemy = 0;

        UnderAttackPlayer = 0;
        UnderAttackEnemy = 0;

        PlayerPiecesIntersectionEvaluateSum = 0;
        EnemyPiecesIntersectionEvaluateSum = 0;

        KnightMovesCounterPlayer = 0;
        KnightMovesCounterEnemy = 0;

        kingSafetyPlayer = 0;
        kingSafetyEnemy = 0;
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
        return turnColor == GameColor.Red ? eval : -eval;
    }
    
    public double EvaluatePosition(Board board, Player currentPlayer)
    {
        CurrentPlayer = currentPlayer;
        GameColor turnColor = CurrentPlayer.playerColor;

        // Get the bitboards of the attacking squares for each player
        playerAttackingBitboard = board.GetAttackingSquaresBitboard(turnColor);
        enemyAttackingBitboard = board.GetAttackingSquaresBitboard(turnColor.OppositeColor());

        foreach (Piece piece in board.GetPiecesList())
        {
            PieceType pieceType = piece.GetPieceType();
            bool isPlayerPiece = piece.GetPieceColor() == currentPlayer.playerColor;

            //add the piece value of the piece 
            CountersValues(isPlayerPiece, pieceType);
            
            if(pieceType == PieceType.King)
            {
                KingSafety(piece, isPlayerPiece);
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

            int underAttack = EvaluateConstants.PieceValues[(int)piece.GetPieceType()];

            // If its player piece
            if(isPlayerPiece)
            {
                // If there is more than 1 enemy piece attacking it
                if (attackingPieces.Count > 1)
                {
                    UnderAttackPlayer += underAttack;
                }
                else
                {
                    //If there is only 1 piece attacking it
                    int attackingPiece = EvaluateConstants.PieceValues[(int)attackingPieces[0].GetPieceType()];

                    //if the piece is not defended by the player pieces
                    if((bitPiecePos & playerAttackingBitboard) == 0)
                        UnderAttackPlayer += underAttack;
                    // If the piece is defended by the player pieces but it more variable than the attacking piece
                    else if(underAttack > attackingPiece)
                        UnderAttackPlayer += underAttack - attackingPiece;
                    // If the piece is defended by the player pieces and the attacking piece is more valuable than the piece, than it's defended
                    else
                        SafePiece(piece, isPlayerPiece);
                }
            }
            // If its enemy piece
            else
            {
                // If the piece is not defended or there is more than 1 enemy piece attacking it, update the undefended pieces value with the piece value
                if(attackingPieces.Count > 1)
                {
                    UnderAttackEnemy += underAttack;
                }
                else
                {
                    //If there is only 1 piece attacking it
                    int attackingPiece = EvaluateConstants.PieceValues[(int)attackingPieces[0].GetPieceType()];

                    //If the piece is not defended by the enemy pieces
                    if((bitPiecePos & enemyAttackingBitboard) == 0)
                        UnderAttackEnemy += underAttack;
                    // If the piece is defended by the enemy pieces but it more variable than the attacking piece,
                    else if(underAttack > attackingPiece)
                        UnderAttackEnemy += underAttack - attackingPiece;
                    // If the piece is defended by the player pieces and the attacking piece is more valuable than the piece, than it's defended
                    else
                        SafePiece(piece, isPlayerPiece);
                }
                
            }
        }
        else
        {
            // If the piece is not under attack, add the intersection rating of the piece
            SafePiece(piece, isPlayerPiece);
        }
    }

    private void SafePiece(Piece piece, bool isPlayerPiece)
    {
        // If the piece is not under attack, add the intersection rating of the piece, and add +1 to the safe pieces counter
        if(isPlayerPiece)
        {
            SafePiecesCounterPlayer++;
            PlayerPiecesIntersectionEvaluateSum += 
            EvaluatePieceIntersectionsTables.GetPieceIntersectionValue(GameState.MiddleGame, piece.GetPieceType(), piece.GetPos(), CurrentPlayer.playOnDownSide);
        }
        else
        {
            SafePiecesCounterEnemy++;
            EnemyPiecesIntersectionEvaluateSum += 
            EvaluatePieceIntersectionsTables.GetPieceIntersectionValue(GameState.MiddleGame, piece.GetPieceType(), piece.GetPos(), !CurrentPlayer.playOnDownSide);
        }

        // If the piece is a knight, add +1 to the knight moves counter
        if(piece.GetPieceType() == PieceType.Knight)
        {
            if(isPlayerPiece)
                KnightMovesCounterPlayer += BitBoard.BitboardToPosition(piece.GetPieceBitboardMove(board)).Count;
            else
                KnightMovesCounterEnemy += BitBoard.BitboardToPosition(piece.GetPieceBitboardMove(board)).Count;
        }
    }

    private void CountersValues(bool isPlayerPiece, PieceType pieceType)
    {
        if (isPlayerPiece)
        {
            ValueDiff += EvaluateConstants.PieceValues[(int)pieceType];
        }
        else
        {
            ValueDiff -= EvaluateConstants.PieceValues[(int)pieceType];
        }
    }
    
    private void KingSafety(Piece piece, bool isPlayerPiece)
    {
        BigInteger bitPiecePos = BitBoard.PosToBitInteger(piece.GetPos());
        bool kingOnDownSide = isPlayerPiece ? CurrentPlayer.playOnDownSide : !CurrentPlayer.playOnDownSide;
        int KingSafety = 0;

        // If the king is under attack, add the check weight to the king safety
        if((bitPiecePos & (isPlayerPiece ? enemyAttackingBitboard : playerAttackingBitboard)) != 0)
        {
            KingSafety -= EvaluateConstants.CheckWeight;
        }

        // Add the Safety of the castle
        // calculate the castle squares that under attack bitboard
        BigInteger castleUnderAttack = BitBoard.GetCastleBitboard(kingOnDownSide) & (isPlayerPiece ? enemyAttackingBitboard : playerAttackingBitboard);
        // Add the castle safety * weight to the king safety
        KingSafety -= (int)(BitBoard.BitboardToPosition(castleUnderAttack).Count * EvaluateConstants.CastleSafetyWeight);
    }

    private double CalculateEvaluation(GameColor turnColor)
    {
        double score = 0;

        // Calcualte the score for the position
        // Add the value difference between the player and the enemy
        score += ValueDiff * EvaluateConstants.PieceValueDiffWeight;

        // Add the value difference of the pieces that are under attack
        score += (UnderAttackPlayer - UnderAttackEnemy) * EvaluateConstants.PiecesUnderAttackWeight;

        // Add the value difference of the avg pieces Intersections rating of pieces that are safe(Dont want to consider pieces differences here, so i use avg)
        score += ((PlayerPiecesIntersectionEvaluateSum/SafePiecesCounterPlayer) - (EnemyPiecesIntersectionEvaluateSum/SafePiecesCounterEnemy)) 
        * EvaluateConstants.PiecesIntersectionsWeight;

        // Add the value difference of the knight moves counter
        score += (KnightMovesCounterPlayer - KnightMovesCounterEnemy) * EvaluateConstants.PiecesMobilityWeight;

        // Add the value difference of the king safety
        score += (kingSafetyPlayer - kingSafetyEnemy) * EvaluateConstants.KingSafetyWeight;

        return EvaluateNumberByColor(score, turnColor);
    }
}
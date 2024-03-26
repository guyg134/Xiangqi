using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UIElements;

public class Evaluate 
{
    private Player CurrentPlayer;
    private Board CurrentBoard;

    // The attacking squares bitboards of the player and the enemy
    private BigInteger playerAttackingBitboard;
    private BigInteger enemyAttackingBitboard;
    
    // Save the difference between the pieces value sum of the player and the enemy
    private int ValueDiff;

    //pieces counter and pieces value sum for player and enemy
    private int PiecesCounterPlayer;
    private int PiecesCounterEnemy;

    //The max value Piece of player that is under attack by the enemy pieces
    private double MaxUnderAttackPlayer;

    //Player pieces that are under attack by the enemy pieces
    private double UnderAttackOfPlayer;
    //Enemy pieces that are under attack by the player pieces
    private double UnderAttackOfEnemy;

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

        PiecesCounterPlayer = 0;
        PiecesCounterEnemy = 0;

        MaxUnderAttackPlayer = 0;

        UnderAttackOfPlayer = 0;
        UnderAttackOfEnemy = 0;

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
        //first check if the there is checkmate in 1 move
        if(CheckMateNextMove(board, currentPlayer.playerColor.OppositeColor()))
        {
            //if there is checkmate in 1 move return the checkmate value
            return EvaluateNumberByColor(EvaluateConstants.CheckMateValue, currentPlayer.playerColor.OppositeColor());
        }

        //if there is no checkmate in 1 move, evaluate the current position
        return EvaluatePosition(board, currentPlayer);
       
    }

    public bool CheckMateNextMove(Board board, GameColor enemyColor)
    {
        Board copyBoard = new Board(board);
        SearchMove.o++;
        foreach (Piece piece in copyBoard.GetPiecesList())
        {
            // If the piece is an enemy piece and can attack the king, or if the piece is an enemy piece and can't attack the king but is in the castle x,
            // check if the piece has a valid move that can cause checkmate
            if(piece.GetPieceColor() == enemyColor && (PieceTypeMethods.PieceCanAttackKing(piece.GetPieceType()) 
            || (!PieceTypeMethods.PieceCanAttackKing(piece.GetPieceType()) && piece.GetX() > Constants.CastleLeftX && piece.GetX() < Constants.CastleRightX))){
                List<Position> validMoves = piece.GetValidMoves(copyBoard);

                foreach(Position pos in validMoves)
                {
                    Move move = new Move(piece.GetX(), piece.GetY(), pos.x, pos.y, piece, board.FindPiece(pos.x, pos.y));

                    copyBoard.MovePieceOnBoard(move);

                    // If after the move, the current player has no legal moves, it's checkmate
                    if (copyBoard.IsCheck(piece.GetPieceColor()) && !copyBoard.PlayerHaveMoves(piece.GetPieceColor().OppositeColor()))
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
        CurrentBoard = board;
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
                KingSafety(piece, isPlayerPiece);
            }
            else
            {
                IsPieceUnderAttack(board, piece);
                PositionsValue(piece, isPlayerPiece);
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
                //If there is only 1 piece attacking it, if not, there is more than 1 attacking piece so its under attack
                if (attackingPieces.Count == 1)
                {
                    //If there is only 1 piece attacking it
                    int attackingPiece = EvaluateConstants.PieceValues[(int)attackingPieces[0].GetPieceType()];

                    // If the piece is defended by the player pieces
                    if((bitPiecePos & playerAttackingBitboard) != 0)
                    {
                        //if the piece is more valuable than the attacking piece
                        if(underAttack > attackingPiece)
                        {
                            underAttack -= attackingPiece;
                        }
                        //if the piece is less valuable than the attacking piece, the piece is not under attack
                        else
                        {
                            underAttack = 0;
                        }
                    }
                    
                }
                UnderAttackOfPlayer += underAttack;
                MaxUnderAttackPlayer = Math.Max(MaxUnderAttackPlayer, underAttack);
            }
            // If its enemy piece
            else
            {
                //If there is only 1 piece attacking it, if not, there is more than 1 attacking piece so its under attack
                if(attackingPieces.Count == 1)
                {
                    //If there is only 1 piece attacking it
                    int attackingPiece = EvaluateConstants.PieceValues[(int)attackingPieces[0].GetPieceType()];

                    // If the piece is defended by the enemy pieces
                    if((bitPiecePos & enemyAttackingBitboard) != 0)
                    {
                        //if the piece is more valuable than the attacking piece
                        if(underAttack > attackingPiece)
                        {
                            underAttack -= attackingPiece;
                        }
                        //if the piece is less valuable than the attacking piece, the piece is not under attack
                        else
                        {
                            underAttack = 0;
                        }
                    }
                }
                UnderAttackOfEnemy += underAttack;
            }
        }
    }

    private void PositionsValue(Piece piece, bool isPlayerPiece)
    {
        // If the piece is not under attack, add the intersection rating of the piece, and add +1 to the safe pieces counter
        if(isPlayerPiece)
        {
            PiecesCounterPlayer++;
            PlayerPiecesIntersectionEvaluateSum += 
            EvaluatePieceIntersectionsTables.GetPieceIntersectionValue(GameState.MiddleGame, piece.GetPieceType(), piece.GetPos(), CurrentPlayer.playOnDownSide);
        }
        else
        {
            PiecesCounterEnemy++;
            EnemyPiecesIntersectionEvaluateSum += 
            EvaluatePieceIntersectionsTables.GetPieceIntersectionValue(GameState.MiddleGame, piece.GetPieceType(), piece.GetPos(), !CurrentPlayer.playOnDownSide);
        }

        // If the piece is a knight, add +1 to the knight moves counter
        if(piece.GetPieceType() == PieceType.Knight)
        {
            if(isPlayerPiece)
                KnightMovesCounterPlayer += BitBoard.BitboardToPosition(piece.GetPieceBitboardMove(CurrentBoard)).Count;
            else
                KnightMovesCounterEnemy += BitBoard.BitboardToPosition(piece.GetPieceBitboardMove(CurrentBoard)).Count;
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
    
    private void KingSafety(Piece king, bool isPlayerKing)
    {
        BigInteger bitKingPos = BitBoard.PosToBitInteger(king.GetPos());
        bool kingOnDownSide = isPlayerKing ? CurrentPlayer.playOnDownSide : !CurrentPlayer.playOnDownSide;
        int KingSafety = 0;

        // If the king is under attack, add the check weight to the king safety
        if((bitKingPos & (isPlayerKing ? enemyAttackingBitboard : playerAttackingBitboard)) != 0)
        {
            if(!CurrentBoard.PlayerHaveMoves(king.GetPieceColor()))
            {
                KingSafety += EvaluateConstants.CheckMateValue;
            }
            KingSafety -= EvaluateConstants.CheckValue;
        }

        // Add the Safety of the castle
        // calculate the castle squares that under attack bitboard
        BigInteger castleUnderAttack = BitBoard.GetCastleBitboard(kingOnDownSide) & (isPlayerKing ? enemyAttackingBitboard : playerAttackingBitboard);
        // Add the castle safety * weight to the king safety
        KingSafety -= (int)(BitBoard.BitboardToPosition(castleUnderAttack).Count * EvaluateConstants.CastleSafetyWeight);

        if(isPlayerKing)
        {
            kingSafetyPlayer = KingSafety;
        }
        else
        {
            kingSafetyEnemy = KingSafety;
        }
    }

    private double CalculateEvaluation(GameColor turnColor)
    {
        double score = 0;

        // Calcualte the score for the position
        // Add the value difference between the player and the enemy
        score += (ValueDiff - MaxUnderAttackPlayer) * EvaluateConstants.PieceValueDiffWeight;

        
        // Add the value difference of the pieces that are under attack
        score += UnderAttackOfEnemy * EvaluateConstants.PiecesUnderAttackWeight;

        // Add the value difference of the avg pieces Intersections rating of pieces that are safe(Dont want to consider pieces differences here, so i use avg)
        score += ((PlayerPiecesIntersectionEvaluateSum/PiecesCounterPlayer) - (EnemyPiecesIntersectionEvaluateSum/PiecesCounterEnemy)) 
        * EvaluateConstants.PiecesIntersectionsWeight;

        // Add the value difference of the knight moves counter
        score += (KnightMovesCounterPlayer - KnightMovesCounterEnemy) * EvaluateConstants.PiecesMobilityWeight;

        // Add the value difference of the king safety
        score += (kingSafetyPlayer - kingSafetyEnemy) * EvaluateConstants.KingSafetyWeight;

        return EvaluateNumberByColor(score, turnColor);
    }
}
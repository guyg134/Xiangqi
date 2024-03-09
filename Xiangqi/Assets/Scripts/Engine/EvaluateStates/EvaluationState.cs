using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;


public abstract class EvaluationState
{
    protected int PiecesCounterPlayer { get; set; }
    protected int PiecesCounterEnemy { get; set; }
    protected int PiecesValueCounterPlayer { get; set; }
    protected int PiecesValueCounterEnemy { get; set; }
    protected int MaxUnDefendedPieceValue { get; set; }
    protected int MaxAttackingPieceValue { get; set; }
    protected double CheckBonus { get; set; }
    protected double PlayerPiecesIntersectionEvaluateSum { get; set; }
    protected double EnemyPiecesIntersectionEvaluateSum { get; set; }

    protected int[] PieceValues = new int[8];

    protected int checkingBonus;
    protected int checkingWithFewPiecesBonus;
    protected int kingMovesBonus;

    protected EvaluationState()
    {
        PiecesCounterPlayer = 0;
        PiecesCounterEnemy = 0;
        PiecesValueCounterPlayer = 0;
        PiecesValueCounterEnemy = 0;
        MaxUnDefendedPieceValue = 0;
        MaxAttackingPieceValue = 0;
        CheckBonus = 0;
        PlayerPiecesIntersectionEvaluateSum = 0;
        EnemyPiecesIntersectionEvaluateSum = 0;
    }

    public static EvaluationState GetEvaluationState(GameState gameState)
    {
        switch (gameState)
        {
            case GameState.Opening:
                return new OpeningEvaluation();

            case GameState.MiddleGame:
                return new MidgameEvaluation();

            case GameState.EndGame:
                return new EndgameEvaluation();
            default:
                return null;
        }
    }

    protected abstract double CalculateEvaluation(GameColor turnColor);

    public double EvaluateState(Board board, GameState gameState, Player player)
    {
        GameColor turnColor = player.playerColor;

        // Get the bitboards of the attacking squares for each player
        BigInteger playerAttackingBitboard = BitBoard.IntersectionsUnderAttackByColor(board, turnColor);
        BigInteger enemyAttackingBitboard = BitBoard.IntersectionsUnderAttackByColor(board, turnColor.OppositeColor());

        foreach (Piece piece in board.GetPiecesList())
        {
            UpdateVariables(board, turnColor, piece, playerAttackingBitboard, enemyAttackingBitboard);
            UpdatePieceIntersectionEvaluateSum(gameState, player, piece);
        }

        return CalculateEvaluation(turnColor);
    }
    
    // Update the variables of piece
    private void UpdateVariables(Board board, GameColor turnColor, Piece piece, BigInteger playerAttackingBitboard, BigInteger enemyAttackingBitboard)
    {
        bool isPlayerPiece = piece.GetPieceColor() == turnColor;
        BigInteger bitPiecePos = BitBoard.PosToBit(piece.GetPos());

        CountersValues(isPlayerPiece, piece.GetPieceType());


        // If its player's piece and under attack by the enemy pieces, or if its enemy piece and under attack by the player pieces
        if ((bitPiecePos & (isPlayerPiece ? enemyAttackingBitboard : playerAttackingBitboard)) != 0)
        {
            // Get list of pieces that attacking this piece
            List<Piece> attackingPieces = BitBoard.PiecesThatAttackingPos(board, bitPiecePos, isPlayerPiece ? turnColor.OppositeColor() : turnColor);

            // If the piece is king
            if (piece.GetPieceType() == PieceType.King)
            {
                UpdateKingVariables(board, piece, attackingPieces, isPlayerPiece ? playerAttackingBitboard : enemyAttackingBitboard, isPlayerPiece);
            }
            // If its player piece
            else if(isPlayerPiece)
            {
                // If the piece is not defended or there is more than 1 enemy piece attacking it, update the max undefended piece value with the piece value
                if ((bitPiecePos & playerAttackingBitboard) == 0 || attackingPieces.Count > 1)
                {
                    UpdateMaxUnDefendedPieceValue(PieceValues[(int)piece.GetPieceType()]);
                }
                else
                {
                    // If the piece is defended by the player pieces, update the max undefended piece value with the difference between the piece value and the value attacking piece
                    UpdateMaxUnDefendedPieceValue(PieceValues[(int)piece.GetPieceType()] -
                        PieceValues[(int)attackingPieces[0].GetPieceType()]);
                }
            }
            // If its enemy piece
            else
            {
                Piece attackingPiece = MostValuablePiece(attackingPieces);
                BigInteger attackingPiecePos = BitBoard.PosToBit(attackingPiece.GetPos());
                
                // If the attacking piece is not under attack by the enemy pieces
                if((attackingPiecePos & enemyAttackingBitboard) == 0)
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

    private void CountersValues(bool isPlayerPiece, PieceType pieceType)
    {
        if(pieceType == PieceType.King)
        {
            return;
        }
        
        if (isPlayerPiece)
        {
            PiecesCounterPlayer++;
            PiecesValueCounterPlayer += PieceValues[(int)pieceType];
        }
        else
        {
            PiecesCounterEnemy++;
            PiecesValueCounterEnemy += PieceValues[(int)pieceType];
        }
    }

    private void UpdateKingVariables(Board board, Piece king, List<Piece> piecesAttackingKing, BigInteger kingColorAttackingIntersections, bool isPlayerKing)
    {
        //if its player king you want to multiply the bonus by -1 to get the negative bonus, if its enemy king you want to multiply the bonus by 1
        int colorMultiplier = isPlayerKing ? -1 : 1;
        BigInteger attackingPiecePos = BitBoard.PosToBit(piecesAttackingKing[0].GetPos());

        //check if the pieces that attacking the king is more than one and all the pieces are on safe intersections if yes change the check with few pieces bonus
        if(piecesAttackingKing.Count > 1 && PiecesAreOnSafeIntersection(piecesAttackingKing, kingColorAttackingIntersections))
        {
            CheckBonus = checkingWithFewPiecesBonus * colorMultiplier;
        }
        //check if the piece that attacking the king is on safe intersection if yes down the check bonus 
        else if ((attackingPiecePos & kingColorAttackingIntersections) == 0)
        {
            CheckBonus = checkingBonus * colorMultiplier;
        }
        // Add the king moves bonus to the check bonus (more moves to enemy king is better for the player, and less moves to player king is better for the enemy)
        int maxKingMoves = 4;
        CheckBonus += (maxKingMoves - king.GetValidMoves(board).Count) * kingMovesBonus * colorMultiplier;
    }

    private void UpdatePieceIntersectionEvaluateSum(GameState gameState, Player turnPlayer, Piece piece)
    {
        // If its player piece
        bool isPlayerPiece = piece.GetPieceColor() == turnPlayer.playerColor;
        // If the piece is player piece add the piece intersection value to the player pieces intersection evaluate sum, else add it to the enemy pieces intersection evaluate sum
        if(isPlayerPiece)
            PlayerPiecesIntersectionEvaluateSum += EvaluatePieceIntersectionsTables.GetPieceIntersectionValue(gameState, piece.GetPieceType(), piece.GetX(), piece.GetY());
        else
            EnemyPiecesIntersectionEvaluateSum += EvaluatePieceIntersectionsTables.GetPieceIntersectionValue(gameState, piece.GetPieceType(), piece.GetX(), piece.GetY());
    }


    // Update max undefended piece value
    private void UpdateMaxUnDefendedPieceValue(int pieceValue)
    {
        if (MaxUnDefendedPieceValue < pieceValue)
        {
            MaxUnDefendedPieceValue = pieceValue;
        }
        // If the piece value is negative set it to zero, because you dont want bonus for piece under attack even if the piece that attacking it is worth MORE
        if(MaxUnDefendedPieceValue < 0)
        {
            MaxUnDefendedPieceValue = 0;
        }
    }

    // Update max attacking piece value
    private void UpdateMaxAttackingPieceValue(int pieceValue)
    {
        if (MaxAttackingPieceValue < pieceValue)
        {
            MaxAttackingPieceValue = pieceValue;
        }
        if(MaxAttackingPieceValue < 0)
        {
            MaxAttackingPieceValue = 0;
        }
    }

    private Piece MostValuablePiece(List<Piece> pieces)
    {
        Piece mostValuablePiece = pieces[0];
        foreach (Piece piece in pieces)
        {
            if (PieceValues[(int)piece.GetPieceType()] > PieceValues[(int)mostValuablePiece.GetPieceType()])
            {
                mostValuablePiece = piece;
            }
        }
        return mostValuablePiece;
    }

    private Piece LeastValuablePiece(List<Piece> pieces)
    {
        Piece leastValuablePiece = pieces[0];
        foreach (Piece piece in pieces)
        {
            if (PieceValues[(int)piece.GetPieceType()] < PieceValues[(int)leastValuablePiece.GetPieceType()])
            {
                leastValuablePiece = piece;
            }
        }
        return leastValuablePiece;
    }

    private bool PiecesAreOnSafeIntersection(List<Piece> pieces, BigInteger attackingBitboard)
    {
        foreach (Piece piece in pieces)
        {
            if ((BitBoard.PosToBit(piece.GetPos()) & attackingBitboard) == 0)
            {
                return false;
            }
        }
        return true;
    }

}
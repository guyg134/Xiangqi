using System.Collections.Generic;


public class Evaluate 
{
    public static readonly int checkMateValue = 20000;


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
        return evaluateState.EvaluateState(board, gameState, currentPlayer);
    }

    //evaluate the current position and return the evaluate value (for the eval bar)
    public static double EvaluateCurrentPosition(Board board, Player currentPlayer)
    {
        if(CheckMateNextMove(board, currentPlayer.playerColor.OppositeColor()))
        {
            return EvaluateNumberByColor(checkMateValue, currentPlayer.playerColor.OppositeColor());
        }
        if(board.IsDraw())
        {
            return 0;
        }

        return EvaluatePosition(board, currentPlayer);
    }

    public static bool CheckMateNextMove(Board board, GameColor enemyColor)
    {
        //O(11 * 10)
        Board copyBoard = new Board(board);
        
        // Iterate through all the pieces
        foreach (Piece piece in copyBoard.GetPiecesList())
        {
            // If the piece is an enemy piece, do all the valid moves of the piece and check if the player has no legal moves
            if(piece.GetPieceColor() == enemyColor && PieceTypeMethods.PieceCanAttackKing(piece.GetPieceType())){
                List<Position> validMoves = piece.GetValidMoves(copyBoard);

                foreach(Position pos in validMoves)
                {
                    Move move = new Move(piece.GetX(), piece.GetY(), pos.x, pos.y, piece, copyBoard.FindPiece(pos.x, pos.y));

                    copyBoard.MovePieceOnBoard(move);

                    // If after the move, the current player has no legal moves, it's checkmate
                    if (copyBoard.IsCheck(enemyColor) && !copyBoard.PlayerHaveMoves(enemyColor.OppositeColor()))
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
    
}
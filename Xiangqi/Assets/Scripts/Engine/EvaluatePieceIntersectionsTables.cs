using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class EvaluatePieceIntersectionsTables
{
    private static readonly Dictionary<PieceType, float[,]> openingPieceIntersectionTable;
    private static readonly Dictionary<PieceType, float[,]> midPieceIntersectionTable;
    private static readonly Dictionary<PieceType, float[,]> endPieceIntersectionTable;

    
    static EvaluatePieceIntersectionsTables()
    {
        // Initialize piece-square tables for each piece type
        openingPieceIntersectionTable = new Dictionary<PieceType, float[,]>
         {
            // Initialize piece-intersection tables for each piece type in opening game
            [PieceType.King] = new float[,]
            {
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  1,  1,  1,  0,  0,  0},
                { 0,  0,  0,  2,  2,  2,  0,  0,  0},
                { 0,  0,  0,  4,  8,  4,  0,  0,  0}

            },
            [PieceType.Soldier] = new float[,]
            {
                {  0, 3, 6, 9, 12, 9, 6, 3, 0},
                { 18, 36, 56, 80,120, 80, 56, 36, 18},
                { 14, 26, 42, 60, 80, 60, 42, 26, 14},
                { 10, 20, 30, 34, 40, 34, 30, 20, 10},
                {  6, 12, 18, 18, 20, 18, 18, 12, 6},
                {  2, 0, 10, 0, 8, 0, 10, 0, 2},
                {  0, 0, -4, 0, 4, 0, -4, 0, 0},
                {  0, 0, 0, 0,  0, 0, 0, 0, 0},
                {  0, 0, 0, 0,  0, 0, 0, 0, 0},
                {  0, 0, 0, 0,  0, 0, 0, 0, 0}

            },
            [PieceType.Knight] = new float[,]
            {
                {0, 0, 0, 1, 0, 1, 0, 0, 0},
                {0, 1, 3, 6, 6, 6, 3, 1, 0},
                {0, 3, 4, 8, 10, 8, 4, 3, 0},
                {1, 6, 8, 12, 10, 12, 8, 6, 1},
                {0, 6, 10, 16, 20, 16, 10, 6, 0},
                {0, 6, 10, 16, 20, 16, 10, 6, 0},
                {1, 6, 8, 12, 10, 12, 8, 6, 1},
                {8, 3, 10, 8, 10, 8, 10, 3, 8},
                {0, 1, 3, 6, 6, 6, 3, 1, 0},
                {0, -4, 0, 1, 0, 1, 0, -4, 0}
            },
            [PieceType.Elephant] = new float[,]
            {
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  6,  0,  0,  0,  6,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 4,  0,  0,  0,  14,  0,  0,  0,  4},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  2,  0,  0,  0,  2,  0,  0}

            },
            [PieceType.Cannon] = new float[,]
            {
                {6, 4, 0, -10, -12, -10, 0, 4, 6},
                {0, 0, 0, -4, -14, -4, 0, 0, 0},
                {0, 0, 0, -10, -8, -10, 0, 0, 0},
                {0, 0, -4, 0, 0, 0, -4, 0, 0},
                {0, 0, -2, 0, 0, 0, -2, 0, 0},
                {-2, 0, 4, 2, 6, 2, 4, 0, -2},
                {0, 0, 0, 2, 4, 2, 0, 0, 0},
                {4, -2, 2, 6, 12, 6, 2, -2, 4},
                {0, 2, 4, 6, 6, 6, 4, 2, 0},
                {0, 0, 2, 6, 6, 6, 2, 0, 0}
            },
            [PieceType.Advisor] = new float[,]
            {
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  2,  0,  2,  0,  0,  0},
                { 0,  0,  0,  0,  8,  0,  0,  0,  0},
                { 0,  0,  0,  4,  0,  4,  0,  0,  0}


            },
            [PieceType.Rook] = new float[,]
            {
                { 14, 14, 12, 18, 16, 18, 12, 14, 14},
                { 16, 20, 18, 24, 26, 24, 18, 20, 16},
                { 12, 12, 12, 18, 18, 18, 12, 12, 12},
                { 12, 18, 16, 22, 22, 22, 16, 18, 12},
                { 12, 14, 12, 18, 18, 18, 12, 14, 12},
                { 12, 16, 14, 20, 20, 20, 14, 16, 12},
                { 6, 10, 8, 14, 14, 14, 8, 10, 6},
                { 4, 8, 6, 14, 12, 14, 6, 8, 4 },
                { 8, 4, 8, 16, 8, 16, 8, 4, 8},
                { -4, 12, 6, 14, 12, 14, 6, 12, -4}
            },
        };

        // Initialize piece-intersection tables for each piece type in mid game
        midPieceIntersectionTable = new Dictionary<PieceType, float[,]>
         {
            // Initialize square tables for each piece type
            [PieceType.King] = new float[,]
            {
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  1,  1,  1,  0,  0,  0},
                { 0,  0,  0,  2,  2,  2,  0,  0,  0},
                { 0,  0,  0,  4,  8,  4,  0,  0,  0}

            },
            [PieceType.Soldier] = new float[,]
            {
                {  0, 3, 6, 9, 12, 9, 6, 3, 0},
                { 18, 36, 56, 80,120, 80, 56, 36, 18},
                { 14, 26, 42, 60, 80, 60, 42, 26, 14},
                { 10, 20, 30, 34, 40, 34, 30, 20, 10},
                {  6, 12, 18, 18, 20, 18, 18, 12, 6},
                {  2, 0, 10, 0, 8, 0, 10, 0, 2},
                {  0, 0, -2, 0, 4, 0, -2, 0, 0},
                {  0, 0, 0, 0,  0, 0, 0, 0, 0},
                {  0, 0, 0, 0,  0, 0, 0, 0, 0},
                {  0, 0, 0, 0,  0, 0, 0, 0, 0}

            },
            [PieceType.Knight] = new float[,]
            {
                {0, 0, 0, 1, 0, 1, 0, 0, 0},
                {0, 1, 3, 6, 6, 6, 3, 1, 0},
                {0, 3, 4, 8, 10, 8, 4, 3, 0},
                {1, 6, 8, 12, 10, 12, 8, 6, 1},
                {0, 6, 10, 16, 20, 16, 10, 6, 0},
                {0, 6, 10, 16, 20, 16, 10, 6, 0},
                {1, 6, 8, 12, 10, 12, 8, 6, 1},
                {0, 3, 6, 8, 10, 8, 6, 3, 0},
                {0, 1, 3, 6, 6, 6, 3, 1, 0},
                {0, 0, 0, 1, 0, 1, 0, 0, 0}
            },
            [PieceType.Elephant] = new float[,]
            {
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  6,  0,  0,  0,  6,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 2,  0,  0,  0,  14,  0,  0,  0,  2},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  6,  0,  0,  0,  6,  0,  0}

            },
            [PieceType.Cannon] = new float[,]
            {
                {6, 4, 0, -10, -12, -10, 0, 4, 6},
                {2, 2, 0, -4, -14, -4, 0, 2, 2},
                {2, 2, 0, -10, -8, -10, 0, 2, 2},
                {0, 0, -2, 4, 10, 4, -2, 0, 0},
                {0, 0, 0, 2, 8, 2, 0, 0, 0},
                {-2, 0, 4, 2, 6, 2, 4, 0, -2},
                {0, 0, 0, 2, 4, 2, 0, 0, 0},
                {4, 0, 4, 6, 14, 6, 4, 0, 4},
                {0, 2, 4, 6, 6, 6, 4, 2, 0},
                {0, 0, 2, 6, 6, 6, 2, 0, 0}
            },
            [PieceType.Advisor] = new float[,]
            {
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  2,  0,  2,  0,  0,  0},
                { 0,  0,  0,  0,  8,  0,  0,  0,  0},
                { 0,  0,  0,  4,  0,  4,  0,  0,  0}


            },
            [PieceType.Rook] = new float[,]
            {
                { 14, 14, 12, 18, 16, 18, 12, 14, 14},
                { 16, 20, 18, 24, 26, 24, 18, 20, 16},
                { 12, 12, 12, 18, 18, 18, 12, 12, 12},
                { 12, 18, 16, 22, 22, 22, 16, 18, 12},
                { 12, 14, 12, 18, 18, 18, 12, 14, 12},
                { 12, 16, 14, 20, 20, 20, 14, 16, 12},
                { 6, 10, 8, 14, 14, 14, 8, 10, 6},
                { 4, 8, 6, 14, 12, 14, 6, 8, 4 },
                { 6, 4, 8, 16, 8, 16, 8, 4, 6},
                { -2, 10, 6, 14, 12, 14, 6, 10, -2}
            },
        };

        // Initialize piece-intersection tables for each piece type in end game
        endPieceIntersectionTable = new Dictionary<PieceType, float[,]>
         {
            // Initialize square tables for each piece type
            [PieceType.King] = new float[,]
            {
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  1,  1,  1,  0,  0,  0},
                { 0,  0,  0,  2,  2,  2,  0,  0,  0},
                { 0,  0,  0,  4,  8,  4,  0,  0,  0}

            },
            [PieceType.Soldier] = new float[,]
            {
                {  0, 3, 6, 9, 12, 9, 6, 3, 0},
                { 18, 36, 56, 80,120, 80, 56, 36, 18},
                { 14, 26, 42, 60, 80, 60, 42, 26, 14},
                { 10, 20, 30, 34, 40, 34, 30, 20, 10},
                {  6, 12, 20, 18, 22, 18, 20, 12, 6},
                {  2, 0, 10, 0, 8, 0, 10, 0, 2},
                {  0, 0, 0, 0, 4, 0, 0, 0, 0},
                {  0, 0, 0, 0,  0, 0, 0, 0, 0},
                {  0, 0, 0, 0,  0, 0, 0, 0, 0},
                {  0, 0, 0, 0,  0, 0, 0, 0, 0}

            },
            [PieceType.Knight] = new float[,]
            {
                {0, 0, 0, 1, 0, 1, 0, 0, 0},
                {0, 8, 12, 6, 6, 6, 12, 8, 0},
                {0, 3, 10, 8, 10, 8, 10, 3, 0},
                {1, 6, 8, 12, 10, 12, 8, 6, 1},
                {0, 6, 10, 16, 20, 16, 10, 6, 0},
                {0, 6, 10, 16, 20, 16, 10, 6, 0},
                {1, 6, 8, 12, 10, 12, 8, 6, 1},
                {0, 3, 6, 8, 10, 8, 6, 3, 0},
                {0, 1, 3, 6, 6, 6, 3, 1, 0},
                {0, 0, 0, 1, 0, 1, 0, 0, 0}
            },
            [PieceType.Elephant] = new float[,]
            {
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  6,  0,  0,  0,  6,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 2,  0,  0,  0,  14,  0,  0,  0,  2},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  6,  0,  0,  0,  6,  0,  0}

            },
            [PieceType.Cannon] = new float[,]
            {
                {6, 4, 0, -10, -12, -10, 0, 4, 6},
                {2, 2, 0, -4, -14, -4, 0, 2, 2},
                {2, 2, 0, -10, -8, -10, 0, 2, 2},
                {0, 0, -2, 4, 10, 4, -2, 0, 0},
                {0, 0, 0, 2, 8, 2, 0, 0, 0},
                {-2, 0, 4, 2, 6, 2, 4, 0, -2},
                {0, 0, 0, 2, 4, 2, 0, 0, 0},
                {4, 0, 4, 6, 12, 6, 4, 0, 4},
                {0, 2, 4, 6, 6, 6, 4, 2, 0},
                {0, 0, 2, 6, 6, 6, 2, 0, 0}
            },
            [PieceType.Advisor] = new float[,]
            {
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,  2,  0,  2,  0,  0,  0},
                { 0,  0,  0,  0,  10,  0,  0,  0,  0},
                { 0,  0,  0,  4,  0,  4,  0,  0,  0}


            },
            [PieceType.Rook] = new float[,]
            {
                { 14, 14, 12, 18, 16, 18, 12, 14, 14},
                { 16, 20, 18, 24, 26, 24, 18, 20, 16},
                { 12, 12, 12, 18, 18, 18, 12, 12, 12},
                { 12, 18, 16, 22, 22, 22, 16, 18, 12},
                { 12, 14, 12, 18, 18, 18, 12, 14, 12},
                { 12, 16, 14, 20, 20, 20, 14, 16, 12},
                { 6, 10, 8, 14, 14, 14, 8, 10, 6},
                { 4, 8, 6, 14, 12, 14, 6, 8, 4 },
                { 6, 4, 8, 16, 8, 16, 8, 4, 6},
                { -2, 10, 6, 14, 12, 14, 6, 10, -2}
            },
        };
    }

    public static float GetPieceIntersectionValue(GameState gameState, PieceType pieceType, Position pos, bool isDownSide)
    {   
        // Get the piece square value based on the piece type and the position on the board (9 - y because array's y is 0 on top and 9 on bottom)
        int xTable = pos.x;
        int yTable = 9 - pos.y;
        if(!isDownSide)
        {
            xTable = 8 - pos.x;
            yTable = 9 - yTable;
        }

        switch(gameState)
        {
            case GameState.Opening:
                return openingPieceIntersectionTable[pieceType][yTable, xTable];
            case GameState.MiddleGame:
                return midPieceIntersectionTable[pieceType][yTable, xTable];
            case GameState.EndGame:
                return endPieceIntersectionTable[pieceType][yTable, xTable];
            default:
                throw new System.Exception("Invalid game phase");
        }
    }
}

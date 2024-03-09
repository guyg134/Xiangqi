using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvaluateState2 : MonoBehaviour
{
    private static Dictionary<GameState, List<double>> gameWeights = new Dictionary<GameState, List<double>>()
    {
        {GameState.Opening, new List<double>(){0, 0, 0, 0, 0, 0, 0, 0}},
        {GameState.MiddleGame, new List<double>(){0, 0, 0, 0, 0, 0, 0, 0}},
        {GameState.EndGame, new List<double>(){0, 0, 0, 0, 0, 0, 0, 0}}
    };

    private static Dictionary<GameState, int[]> pieceValues = new Dictionary<GameState, int[]>()
    {
        {GameState.Opening, new int[]{200, 250, 450, 550, 550, 100, -200, -250, -450, -550, -550, -100}},
        {GameState.MiddleGame, new int[]{200, 250, 450, 550, 550, 100, -200, -250, -450, -550, -550, -100}},
        {GameState.EndGame, new int[]{200, 250, 450, 550, 550, 100, -200, -250, -450, -550, -550, -100}}
    };

    public static double GetStatWeight(GameState gameState, EvaluateStats evaluateStats)
    {
        return gameWeights[gameState][(int)evaluateStats];
    }

    public static int GetPieceValue(GameState gameState, int pieceIndex)
    {
        return pieceValues[gameState][pieceIndex];
    }
}

public enum EvaluateStats
{
    Mobility = 0,
    Material = 1,
    Position = 2,
    KingSafety = 3,
    PawnStructure = 4,
    PieceSquareTable = 5,
    GamePhase = 6,
    Tempo = 7
}

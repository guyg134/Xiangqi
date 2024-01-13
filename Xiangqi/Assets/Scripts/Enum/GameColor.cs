using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameColor
{
    Red = 0,
    Black = 1
}

public static class GameColorExtensions
{
    public static GameColor OppositeColor(this GameColor color)
    {
        // Toggle between Red and Black
        return (color == GameColor.Red) ? GameColor.Black : GameColor.Red;
    }
}

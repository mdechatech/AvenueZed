using System;
using UnityEngine;
using System.Collections;

[Serializable]
public class LevelData
{
    public int CarGoal = 50;
    public float TimeGoal = 20;
    public int StartMoney = 10;
    public int Rows = 1;
    public int Cols = 1;
    public int Turns = 1;

    [Space]
    public int WaveAmtStart = 10;
    public int WaveAmtGrowth = 10;
    public float WaveFreqStart = 1;
    public float WaveFreqGrowth = 0.3f;
    public float WaveSpeedStart = 0.4f;
    public float WaveSpeedGrowth = 0.05f;
    public float WaveDelay = 10;
}

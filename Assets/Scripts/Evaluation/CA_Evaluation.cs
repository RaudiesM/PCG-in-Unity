using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CA_Evaluation : EvaluationBase
{
    public int FillPercentage;
    public int Iterations;
    public string Neighbourhood;
    public bool RemoveNonReachableTiles;
    public float[] GenerationTimes;

    public CA_Evaluation(int fieldSize, int fillPercent, int iterations, string neighbourhood, bool removeTiles) { 
        this.AlgorithmName = "Cellula Automata Algorithm";
        this.FillPercentage = fillPercent;
        this.FieldSize = fieldSize;
        this.Iterations = iterations;
        this.Neighbourhood = neighbourhood;
        this.RemoveNonReachableTiles = removeTiles;
    }

    public override void SetUpTimetable(float[] timetable)
    {
        GenerationTimes = new float[timetable.Length];
        GenerationTimes = timetable;
    }
}

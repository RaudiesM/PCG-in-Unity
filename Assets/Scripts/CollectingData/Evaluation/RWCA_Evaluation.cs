using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RWCA_Evaluation : EvaluationBase
{
    public int FillPercentage;
    public int Iterations;
    public string Neighbourhood;
    public float[] GenerationTimes;
    public int[] GeneratedTiles;
    public RWCA_Evaluation(CA_Evaluation ca_data, RW_Evaluation rw_data) { 
        this.AlgorithmName = "RW/CA Hybrid Algorithm";
        this.FillPercentage = rw_data.FillPercentage;
        this.FieldSize = rw_data.FieldSize;
        this.Iterations = ca_data.Iterations;
        this.Neighbourhood = ca_data.Neighbourhood;
    }

    public override void SetUpTimetable(float[] timetable)
    {
        GenerationTimes = new float[timetable.Length];
        GenerationTimes = timetable;
    }
    public override void SetUpFilledTiles(int[] filledTilesList)
    {
        GeneratedTiles = new int[filledTilesList.Length];
        GeneratedTiles = filledTilesList;
    }
}

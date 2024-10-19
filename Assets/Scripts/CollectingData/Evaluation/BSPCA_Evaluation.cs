using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BSPCA_Evaluation : EvaluationBase
{
    public int NumberRooms;
    public int RoomSize;
    public int FillPercentage;
    public int Iterations;
    public string Neighbourhood;
    public bool RemoveNonReachableTiles;
    public float[] GenerationTimes;
    public int[] GeneratedTiles;

    public BSPCA_Evaluation(CA_Evaluation ca_data, BSP_Evaluation bsp_data) { 
        this.AlgorithmName = "Cellula Automata Algorithm";
        this.NumberRooms = bsp_data.NumberRooms;
        this.RoomSize = bsp_data.SkelettonRoomSize;
        this.FillPercentage = ca_data.FillPercentage;
        this.FieldSize = bsp_data.FieldSize;
        this.Iterations = ca_data.Iterations;
        this.Neighbourhood = ca_data.Neighbourhood;
        this.RemoveNonReachableTiles = ca_data.RemoveNonReachableTiles;
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

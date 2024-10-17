using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BSP_Evaluation : EvaluationBase
{
    public int NumberRooms;
    public int RoomMinSize;
    public float[] GenerationTimes;
    public BSP_Evaluation(int fieldSize, int numbRooms, int roomMinSize)
    {
        this.AlgorithmName = "Binary Space Partitioning Algorithm";
        this.FieldSize = fieldSize;
        this.NumberRooms = numbRooms;
        this.RoomMinSize = roomMinSize;
    }
    public override void SetUpTimetable(float[] timetable)
    {
        GenerationTimes = new float[timetable.Length];
        GenerationTimes = timetable;
    }
}

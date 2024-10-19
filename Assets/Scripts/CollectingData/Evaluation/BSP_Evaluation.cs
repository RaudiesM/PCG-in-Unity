using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BSP_Evaluation : EvaluationBase
{
    public int NumberRooms;
    public int RoomMinSize;
    public int SkelettonRoomSize;
    public float[] GenerationTimes;
    public int[] GeneratedTiles;
    public BSP_Evaluation(int fieldSize, int numbRooms, int roomMinSize, int skelettonRoomSize)
    {
        this.AlgorithmName = "Binary Space Partitioning Algorithm";
        this.FieldSize = fieldSize;
        this.NumberRooms = numbRooms;
        this.RoomMinSize = roomMinSize;
        this.SkelettonRoomSize = skelettonRoomSize;
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

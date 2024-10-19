using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RW_Evaluation : EvaluationBase
{
    public int FillPercentage;
    public DungeonType CurrentDungeonType;
    public int RoomSpawnRate;
    public bool CombiningRooms;
    public float[] GenerationTimes;
    public int[] GeneratedTiles;
    public RW_Evaluation(int fieldSize, int fillPercentage, DungeonType type, int roomSpawnRate = 0, bool combiningRooms = false)
    {
        this.AlgorithmName = "Random Walker Algorithm";
        this.FieldSize = fieldSize;
        this.FillPercentage = fillPercentage;
        this.CurrentDungeonType = type;
        this.RoomSpawnRate = roomSpawnRate;
        this.CombiningRooms = combiningRooms;
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

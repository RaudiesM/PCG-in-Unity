using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EvaluationBase
{
    public string AlgorithmName;
    public int FieldSize;
    public abstract void SetUpTimetable(float[] timetable);

    public abstract void SetUpFilledTiles(int[] filledTilesList);
}

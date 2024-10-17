using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DungeonAlgorithmBase : MonoBehaviour
{
    public virtual DungeonTiles GenerateDungeonTiles() { throw new NotImplementedException(); }
    public virtual DungeonTiles SetUpGeneration() { throw new NotImplementedException(); }
    public virtual DungeonTiles ContinueIterating() { throw new NotImplementedException(); }
    public virtual EvaluationBase GetAlgorithmData() { throw new NotImplementedException(); }
}

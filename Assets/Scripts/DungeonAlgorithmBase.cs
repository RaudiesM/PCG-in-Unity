using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DungeonAlgorithmBase : MonoBehaviour
{
    public abstract DungeonTiles GenerateDungeonTiles();
    public abstract DungeonTiles SetUpGeneration();
    public abstract DungeonTiles ContinueIterating();
}

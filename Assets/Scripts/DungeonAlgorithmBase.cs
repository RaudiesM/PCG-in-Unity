using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DungeonAlgorithmBase : MonoBehaviour
{
    public abstract bool IsReady();
    public abstract HashSet<Vector2Int> GenerateDungeonTiles();
}

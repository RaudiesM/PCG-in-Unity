using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public struct CellState
{
    //used in CellulaAutomataAlgorithm_old to have unchangeable Tiles that form the foundation of the dungeon
    public bool IsFloor { get; private set; }
    public bool IsChangeable { get; private set; }
    public CellState(bool isFloor = false, bool isChangeable = true)
    {
        IsFloor = isFloor;
        IsChangeable = isChangeable;
    }
}

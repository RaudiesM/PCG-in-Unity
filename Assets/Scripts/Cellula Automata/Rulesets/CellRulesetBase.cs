using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CellRulesetBase : ScriptableObject
{
    public abstract bool ApplyRulesToCell(bool isFloor, int numNeighbours);
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CellRulesets/ReverseConwayRuleset", fileName = "ReverseConwayRuleset")]
public class ReverseConwayRuleset : CellRulesetBase
{
    public override bool ApplyRulesToCell(bool isFloor, int numNeighbours)
    {
        bool newState = false;
        if(!isFloor)
        {
            if(numNeighbours <= 1 || numNeighbours >= 4)
            {
                newState = false;
            }
            else
            {
                newState = true;
            }
        }
        else
        {
            if(numNeighbours == 3) {
                newState = true;
            }
            else
            {
                newState = false;
            }
        }
        return newState;
    }
}

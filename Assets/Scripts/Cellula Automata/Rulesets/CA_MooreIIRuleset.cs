using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CellRulesets/CA_MooreIIRuleset", fileName = "CA_MooreIIRuleset")]
public class CA_MooreIIRuleset : CellRulesetBase
{
    //Moore: 0-8 neighbours
    public override bool ApplyRulesToCell(bool isFloor, int numNeighbours)
    {
        bool newState = false;
        if (isFloor)
        {
            if (numNeighbours <= 2)
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
            if (numNeighbours >= 5)
            {
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

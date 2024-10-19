using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CellRulesets/CA_NeumannRuleset", fileName = "CA_NeumannRuleset")]
public class CA_NeumannRuleset : CellRulesetBase
{
    //Neumann: 0-4 neighbours
    public override bool ApplyRulesToCell(bool isFloor, int numNeighbours)
    {
        bool newState = false;
        if (isFloor)
        {
            if (numNeighbours <= 1)
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
            if (numNeighbours >= 3)
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

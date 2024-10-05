using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CellRulesets/CA_NeumannRuleset", fileName = "CA_NeumannRuleset")]
public class CA_NeumannRuleset : CellRulesetBase
{
    //Neumann kann zw. 0-4 Nachbarn haben
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

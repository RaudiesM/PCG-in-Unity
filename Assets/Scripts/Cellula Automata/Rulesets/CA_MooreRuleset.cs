using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CellRulesets/CA_MooreRuleset", fileName = "CA_MooreRuleset")]
public class CA_MooreRuleset : CellRulesetBase
{
    //Neumann kann zw. 0-8 Nachbarn haben

    public override bool ApplyRulesToCell(bool isFloor, int numNeighbours)
    {
        bool newState = false;
        if (isFloor)
        {
            if (numNeighbours <= 3)
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
            if (numNeighbours >= 4)
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

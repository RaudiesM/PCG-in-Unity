using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public struct CellState
{
    public bool IsFloor { get; private set; }
    public bool IsChangeable { get; private set; }

    public void SwitchState()
    {
        if(IsChangeable) 
        { 
            this.IsFloor = !this.IsFloor;
        }
    }

    public void SetChangeability(bool newChangeability)
    {
        IsChangeable = newChangeability;
    }

    public CellState(bool isFloor = false, bool isChangeable = true)
    {
        IsFloor = isFloor;
        IsChangeable = isChangeable;
    }

}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DungeonAlgorithm : DungeonAlgorithmBase
{
    public override DungeonTiles GenerateDungeonTiles() { throw new NotImplementedException(); }
    public override DungeonTiles SetUpGeneration() { throw new NotImplementedException(); }
    public override DungeonTiles ContinueIterating() { throw new NotImplementedException(); }
    public override string GetAlgorithmData() { throw new NotImplementedException(); }
}

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



public class CellulaAutomataAlgorithm : DungeonAlgorithmBase
{
    [SerializeField] protected int numIterations = 3;
    [SerializeField] protected BoundsInt fieldSize = new BoundsInt(new Vector3Int(0, 0, 0), new Vector3Int(10, 10, 0));
    [SerializeField][Range(0, 1)] protected float fillPercentage = 0.5f;
    [SerializeField] protected bool isReducingTiles = true;
    [SerializeField] protected bool showRedundantSpace = false;

    [Header("Neighbourhood Parameter")]
    [SerializeField] protected NeighbourType currentNeighbour = NeighbourType.Moore;
    [SerializeField] protected CellRulesetBase currentMooreRuleset;
    [SerializeField] protected CellRulesetBase currentNeumannRuleset;

    protected int currentIteration = 0;
    protected int neighbourDistance = 1;
    private HashSet<Vector2Int> cellDistribution = new HashSet<Vector2Int>();

    public override EvaluationBase GetAlgorithmData()
    {
        EvaluationBase evaluationData = new CA_Evaluation(
            fieldSize.size.x * fieldSize.size.y,
            Mathf.RoundToInt(fillPercentage * 100),
            numIterations,
            currentNeighbour.ToString(),
            isReducingTiles
            );
            
        return evaluationData;
    }
    #region Generation Methods
    public override DungeonTiles GenerateDungeonTiles()
    {
        FirstGeneration();
        for (int i = 0; i < numIterations; i++)
        {
            ApplyCellulaAutomata();
        }
        DungeonTiles tiles = GetCellDistribution();
        if (isReducingTiles)
            tiles = ReduceTiles(tiles);
        return tiles;
    }
    public override DungeonTiles FirstGeneration()
    {
        Reset();

        DistributeCells();
        DungeonTiles tiles = GetCellDistribution();
        return tiles;
    }
    public override DungeonTiles ContinueIterating()
    {
        DungeonTiles tiles = new DungeonTiles(AlgorithmType.CellulaAutomata);
        
        if(currentIteration < numIterations)
        {
            ApplyCellulaAutomata();
            tiles = GetCellDistribution();
        }
        else
        {
            tiles = GetCellDistribution();
            
            if(isReducingTiles)
                tiles = ReduceTiles(tiles);
        }
        return tiles;
    }
    #region hybrid only
    public DungeonTiles Hybrid_GenerateDungeonTiles(DungeonTiles inputTiles)
    {
        Hybrid_PrepareGeneration(inputTiles);

        for(int i = 0; i < numIterations; i++)
        {
            ApplyCellulaAutomata();
        }

        DungeonTiles tiles = GetCellDistribution();
        if (isReducingTiles)
            tiles = ReduceTiles(tiles);
        return tiles;
    }
    public void Hybrid_PrepareGeneration(DungeonTiles inputTiles)
    {
        currentIteration = 0;
        cellDistribution = inputTiles.GetCorridors();
    }
    #endregion
    #endregion
    private int CheckNeighbourCells(Vector2Int currentCell)
    {
        int numFloorNeighbour = 0;
        Vector2Int cellToCheck = new Vector2Int();

        if (currentNeighbour == NeighbourType.Moore)
        {
            for(int i = -neighbourDistance; i<=neighbourDistance; i++)
            {
                for(int j = -neighbourDistance; j<=neighbourDistance; j++)
                {
                    cellToCheck = new Vector2Int(currentCell.x+i, currentCell.y+j);
                    if(currentCell != cellToCheck && cellDistribution.Contains(cellToCheck))
                    {
                        numFloorNeighbour++;
                    }
                }
            }
        }
        else if(currentNeighbour == NeighbourType.Neumann)
        {
            for (int i = -neighbourDistance; i <= neighbourDistance; i++)
            {
                //ignore self
                if (i == 0)
                    continue;

                //add to x & check
                cellToCheck = new Vector2Int(currentCell.x + i, currentCell.y);
                if (cellDistribution.Contains(cellToCheck))
                {
                    numFloorNeighbour++;
                }

                //add to y & check
                cellToCheck = new Vector2Int(currentCell.x, currentCell.y+i);
                if (cellDistribution.Contains(cellToCheck))
                {
                    numFloorNeighbour++;
                }
            }
        }
        return numFloorNeighbour;
    }
    protected DungeonTiles ReduceTiles(DungeonTiles tiles)
    {
        Debug.Log("Reducing!");
        DungeonTiles newDungeonTiles = new DungeonTiles(AlgorithmType.CellulaAutomata);
        
        HashSet<Vector2Int> tilesToCheck = new HashSet<Vector2Int>();
        HashSet<Vector2Int> checkedTiles = new HashSet<Vector2Int>();

        tiles.TryGetCorridors(out tilesToCheck);

        foreach (var tile in tilesToCheck)
        {
            if (checkedTiles.Contains(tile))
                continue;

            HashSet<Vector2Int> currentTiles = new HashSet<Vector2Int>();
            Vector2Int currentPosition = tile;
            
            currentTiles = UtilityFunctions.GetConnectedTiles(tilesToCheck, currentTiles, ref currentPosition);
            checkedTiles.UnionWith(currentTiles);

            if (showRedundantSpace)
            {
                newDungeonTiles.AddRoom(currentTiles);
            }
            else
            {
                if (newDungeonTiles.Count() < currentTiles.Count)
                {
                    newDungeonTiles.SetCorridor(currentTiles);
                }
            }
        }
        return newDungeonTiles;
    }

    private DungeonTiles GetCellDistribution()
    {
        DungeonTiles tiles = new DungeonTiles(AlgorithmType.CellulaAutomata);
        foreach (var cell in cellDistribution)
        {
            tiles.AddCorridorTile(cell);
        }
        return tiles;
    }
    private void DistributeCells()
    {
        int numMaxCells = fieldSize.yMax * fieldSize.xMax;
        int cellPercent = Mathf.FloorToInt(numMaxCells * fillPercentage);
        int convertedCells = 0;
        int maxCounter = 0;
        
        while(convertedCells < cellPercent) 
        {
            Vector2Int randomPosition = new Vector2Int(
                                            Random.Range(fieldSize.xMin, fieldSize.xMax), 
                                            Random.Range(fieldSize.yMin, fieldSize.yMax)
                                            );
            if (cellDistribution.Contains(randomPosition))
            {
                continue;
            }
            
            cellDistribution.Add(randomPosition);
            convertedCells++;
            
            maxCounter++;
            if(maxCounter == 1000000)
            {
                Debug.LogError("To many iterations during while");
                break;
            }
        }
    }
    private void ApplyCellulaAutomata()
    {
        currentIteration++;

        Dictionary<Vector2Int, bool> newCellStates = new Dictionary<Vector2Int, bool>();
        for (int i = fieldSize.xMin; i <= fieldSize.xMax; i++)
        {
            for (int j = fieldSize.yMin; j <= fieldSize.yMax; j++)
            {
                Vector2Int newPosition = new Vector2Int(i, j);
                bool isFloor = cellDistribution.Contains(newPosition);
                int numNeighbours = CheckNeighbourCells(newPosition);
                
                bool newStateIsFloor = false;
                if (currentNeighbour == NeighbourType.Moore)
                {
                    newStateIsFloor = currentMooreRuleset.ApplyRulesToCell(isFloor, numNeighbours);
                    //Debug.Log($"Position {newPosition} / Neighbours {numNeighbours}");
                }
                else if(currentNeighbour == NeighbourType.Neumann)
                {
                    //Debug.Log($"Position {newPosition} / Neighbours {numNeighbours}");
                    newStateIsFloor = currentNeumannRuleset.ApplyRulesToCell(isFloor, numNeighbours);
                }
                
                //nur hinzufügen, wenn sich was ändert
                if (newStateIsFloor != isFloor) 
                { 
                    newCellStates.Add(newPosition, newStateIsFloor);
                }
            }
        }

        foreach(var cell in newCellStates)
        {
            if (cell.Value && cellDistribution.Contains(cell.Key) == false)
            {
                cellDistribution.Add(cell.Key);
            }
            else if(cell.Value == false && cellDistribution.Contains(cell.Key))
            {
                cellDistribution.Remove(cell.Key);
            }
        }
    }
    private void Reset()
    {
        cellDistribution.Clear();
        currentIteration = 0;
    }
}

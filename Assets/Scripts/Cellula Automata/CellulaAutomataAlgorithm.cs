using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class CellulaAutomataAlgorithm : MonoBehaviour
{
    [SerializeField] private int numIterations = 3;
    [SerializeField] private BoundsInt fieldSize = new BoundsInt(new Vector3Int(0, 0, 0), new Vector3Int(10, 10, 0));
    [SerializeField][Range(0, 1)] private float fillPercentage = 0.5f;

    [SerializeField] private NeighbourType currentNeighbour = NeighbourType.Moore;
    [SerializeField] private bool showWalls = false;

    [SerializeField] private CellRulesetBase currentMooreRuleset;
    [SerializeField] private CellRulesetBase currentNeumannRuleset;

    private int neighbourDistance = 1;
    private Dictionary<Vector2Int, CellState> cellDistribution = new Dictionary<Vector2Int, CellState>();

    private void Start()
    {
        Debug.Log($"fieldSize: Position[{fieldSize.position}], Zentrum[{fieldSize.center}], Min[{fieldSize.min}], Max[{fieldSize.max}]");
    }

    public HashSet<Vector2Int> GetDungeonTiles()
    {
        cellDistribution.Clear();
        DistributeCells();
        /*foreach(var item in cellDistribution)
        {
            if(item.Value.IsFloor)
            {
                Debug.Log("Cell at "+item.Key+" has "+CheckNeighbourCells(item.Key)+" neighbours");
            }
        }*/
        //ApplyCellulaAutomata();
        
        HashSet<Vector2Int> tiles = new HashSet<Vector2Int>();
        foreach(var cell in cellDistribution)
        {
            if(cell.Value.IsFloor) 
            {
                tiles.Add(cell.Key);
            }
        }
        return tiles;
    }

    public HashSet<Vector2Int> ContinueIterating()
    {
        ApplyCellulaAutomata();
        
        HashSet<Vector2Int> tiles = new HashSet<Vector2Int>();
        foreach (var cell in cellDistribution)
        {
            if (cell.Value.IsFloor)
            {
                tiles.Add(cell.Key);
            }
        }
        return tiles;
    }

    public HashSet<Vector2Int> GetBoundry()
    {
        HashSet<Vector2Int> tiles = new HashSet<Vector2Int>();
        if (showWalls)
        {
            Debug.Log("Umrandung = "+cellDistribution.Count);
            foreach (var cell in cellDistribution)
            {
                if (cell.Value.IsChangeable == false)
                {
                    tiles.Add(cell.Key);
                }
            }
        }
        return tiles;
    }

    private void DistributeCells()
    {
        SurroundFieldWithWall();
        int numMaxCells = fieldSize.yMax * fieldSize.xMax;
        int cellPercent = Mathf.FloorToInt(numMaxCells * fillPercentage);
        int convertedCells = 0;
        int maxCounter = 0;
        
        while(convertedCells < cellPercent  || maxCounter == 10000) 
        {
            Vector2Int randomPosition = new Vector2Int(
                                            Random.Range(fieldSize.xMin, fieldSize.xMax), 
                                            Random.Range(fieldSize.yMin, fieldSize.yMax)
                                            );
            if (cellDistribution.ContainsKey(randomPosition) == false)
            {
                CellState cellState = new CellState(true, true);
                cellDistribution.Add(randomPosition, cellState);
                convertedCells++;
            }
            maxCounter++;
            if(maxCounter == 1000000)
            {
                Debug.LogError("To many iterations during while");
            }
        }
    }

    private void SurroundFieldWithWall()
    {
        CellState boundryState = new CellState(false, false);
        HashSet<Vector3Int> boundryTiles = new HashSet<Vector3Int>();
        for (int i = fieldSize.xMin-1; i <= fieldSize.xMax+1; i++) 
        { 
            Vector3Int yMinPos = new Vector3Int(i, fieldSize.yMin-1);
            Vector3Int yMaxPos = new Vector3Int(i, fieldSize.yMax+1);
            boundryTiles.Add(yMinPos);
            boundryTiles.Add(yMaxPos);
        }
        for (int i = fieldSize.yMin - 1; i <= fieldSize.yMax + 1; i++)
        {
            Vector3Int xMinPos = new Vector3Int(fieldSize.xMin-1, i);
            Vector3Int xMaxPos = new Vector3Int(fieldSize.xMax+1, i);
            boundryTiles.Add(xMinPos);
            boundryTiles.Add(xMaxPos);
        }

        foreach (Vector2Int position in boundryTiles) 
        { 
            if(cellDistribution.ContainsKey(position) == false) 
            cellDistribution.Add(position, boundryState);
        }
    }


    private void ApplyCellulaAutomata()
    {
        Dictionary<Vector2Int, bool> newCellStates = new Dictionary<Vector2Int, bool>();
        for (int i = fieldSize.xMin; i <= fieldSize.xMax; i++)
        {
            for (int j = fieldSize.yMin; j <= fieldSize.yMax; j++)
            {
                Vector2Int newPosition = new Vector2Int(i, j);
                bool isFloor = cellDistribution.ContainsKey(newPosition);
                int numNeighbours = CheckNeighbourCells(newPosition);
                
                bool newStateIsFloor = false;
                if (currentNeighbour == NeighbourType.Moore)
                {
                    newStateIsFloor = currentMooreRuleset.ApplyRulesToCell(isFloor, numNeighbours);
                }
                else if(currentNeighbour == NeighbourType.Neumann)
                {
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
            if (cell.Value && cellDistribution.ContainsKey(cell.Key) == false)
            {
                cellDistribution.Add(cell.Key, new CellState(true));
            }
            else if(cell.Value == false && cellDistribution.ContainsKey(cell.Key))
            {
                cellDistribution.Remove(cell.Key);
            }
        }
    }

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
                    if(currentCell != cellToCheck && cellDistribution.ContainsKey(cellToCheck))
                    {
                        if (cellDistribution[cellToCheck].IsFloor)
                        {
                            numFloorNeighbour++;
                        }
                    }
                }
            }
        }
        else if(currentNeighbour == NeighbourType.Neumann)
        {
            for (int i = -neighbourDistance; i <= neighbourDistance; i++)
            {

                //add to x & check
                cellToCheck = new Vector2Int(currentCell.x + i, currentCell.y);
                if (currentCell != cellToCheck && cellDistribution.ContainsKey(cellToCheck))
                {
                    if (cellDistribution[cellToCheck].IsFloor)
                    {
                        numFloorNeighbour++;
                    }
                }

                //add to y & check
                cellToCheck = new Vector2Int(currentCell.x, currentCell.y+i);
                if (currentCell != cellToCheck && cellDistribution.ContainsKey(cellToCheck))
                {
                    if (cellDistribution[cellToCheck].IsFloor)
                    {
                        numFloorNeighbour++;
                    }
                }

            }
        }
        return numFloorNeighbour;
    }
}

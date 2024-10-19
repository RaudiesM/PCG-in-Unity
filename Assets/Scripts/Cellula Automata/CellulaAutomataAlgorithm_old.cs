using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;



public class CellulaAutomataAlgorithm_old : DungeonAlgorithmBase
{
    [SerializeField] private int numIterations = 3;
    [SerializeField] private BoundsInt fieldSize = new BoundsInt(new Vector3Int(0, 0, 0), new Vector3Int(10, 10, 0));
    [SerializeField][Range(0, 1)] private float fillPercentage = 0.5f;

    [SerializeField] private NeighbourType currentNeighbour = NeighbourType.Moore;
    [SerializeField] private bool showWalls = false;
    [SerializeField] private bool isReducingTiles = true;
    [SerializeField] private bool showRedundantSpace = false;

    [SerializeField] private CellRulesetBase currentMooreRuleset;
    [SerializeField] private CellRulesetBase currentNeumannRuleset;

    private int currentIteration = 0;
    private int neighbourDistance = 1;
    private Dictionary<Vector2Int, CellState> cellDistribution = new Dictionary<Vector2Int, CellState>();
    private HashSet<BoundsInt> cellRooms;
    private DungeonRoom roomBones;

    public override DungeonTiles GenerateDungeonTiles()
    {
        cellDistribution.Clear();
        DistributeCells();
        currentIteration = 0;
        for(int i = 0; i < numIterations; i++)
        {
            ApplyCellulaAutomata();
        }
        DungeonTiles tiles = GetCellDistribution();
        tiles = ReduceTiles(tiles);
        return tiles;
    }
    public DungeonTiles GenerateDungeonTiles(DungeonTiles inputTiles, HashSet<BoundsInt> rooms, HashSet<Vector3Int> centers)
    {
        FirstGeneration(inputTiles, rooms, centers);
        currentIteration = 0;
        for (int i = 0; i < numIterations; i++)
        {
            ApplyCellulaAutomata();
        }
        DungeonTiles tiles = GetCellDistribution();
        if (isReducingTiles)
            tiles = ReduceTiles(tiles);
        tiles.AddRoom(roomBones);
        return tiles;
    }


    public DungeonTiles FirstGeneration(DungeonTiles tiles, HashSet<BoundsInt> rooms, HashSet<Vector3Int> center)
    {
        cellDistribution.Clear();
        currentIteration = 0;
        IncorporateTiles(tiles);
        HashSet<DungeonRoom> dungeonRooms = new HashSet<DungeonRoom>();
        tiles.TryGetRooms(out dungeonRooms);
        List<BoundsInt> roomList = rooms.ToList();
        List<Vector3Int> centerList = center.ToList();
        for(int i = 0; i<roomList.Count; i++)
        {
            DistributeCells(roomList[i], centerList[i]);
        }
        return GetCellDistribution();
    }

    private void IncorporateTiles(DungeonTiles tiles)
    {
        HashSet<Vector2Int> newTiles = new HashSet<Vector2Int>();
        HashSet<DungeonRoom> newRoomTiles = new HashSet<DungeonRoom>();
        tiles.TryGetCorridors(out newTiles);
        tiles.TryGetRooms(out newRoomTiles);
        roomBones = new DungeonRoom(newTiles);
        foreach(DungeonRoom room in newRoomTiles)
        {
            newTiles.UnionWith(room.GetRoomTiles());
        }
        foreach(Vector2Int tile in newTiles)
        {
            if (cellDistribution.ContainsKey(tile) == false)
            {
                CellState cellState = new CellState(true, false);
                cellDistribution.Add(tile, cellState);
            }
        }

    }

    public override DungeonTiles ContinueIterating()
    {
        DungeonTiles tiles = new DungeonTiles(AlgorithmType.CellulaAutomata);
        if(currentIteration < numIterations)
        {
            ApplyCellulaAutomata();
            tiles = GetCellDistribution();
            tiles.AddRoom(roomBones);
        }
        else
        {
            tiles = GetCellDistribution();
            tiles = ReduceTiles(tiles);
            tiles.AddRoom(roomBones);
        }
        return tiles;
    }

    private DungeonTiles GetCellDistribution()
    {
        DungeonTiles tiles = new DungeonTiles(AlgorithmType.CellulaAutomata);
        foreach (var cell in cellDistribution)
        {
            if (cell.Value.IsFloor)
            {
                tiles.AddCorridorTile(cell.Key);
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

    private void DistributeCells(BoundsInt room, Vector3Int newCenter)
    {
        //SurroundFieldWithWall();
        Vector2Int refPosition = new Vector2Int(newCenter.x, newCenter.y);
        
        int numMaxCells = (room.size.x -1) * (room.size.y -1);
        int cellPercent = Mathf.FloorToInt(numMaxCells * fillPercentage);
        int convertedCells = 0;
        int maxCounter = 0;
       
        int newXMin = room.position.x - (room.size.x / 2);
        int newYMin = room.position.y - (room.size.y / 2);
        int newXMax = room.position.x + (room.size.x / 2);
        int newYMax = room.position.y + (room.size.y / 2);

        /*Debug.DrawLine(new Vector3(newXMin, newYMin), new Vector3(newXMin, newYMax), Color.red, 10);
        Debug.DrawLine(new Vector3(newXMin, newYMin), new Vector3(newXMax, newYMin), Color.red, 10);
        Debug.DrawLine(new Vector3(newXMax, newYMax), new Vector3(newXMin, newYMax), Color.blue, 10);
        Debug.DrawLine(new Vector3(newXMax, newYMax), new Vector3(newXMax, newYMin), Color.blue, 10);
        */

        while (convertedCells < cellPercent)
        {
            Vector2Int randomPositionA = new Vector2Int(
                                            Random.Range(newXMin +2, newXMax -2),
                                            Random.Range(newYMin +2, newYMax-2)
                                            );
            Vector2Int randomPositionB = new Vector2Int(
                                            Random.Range(newXMin + 2, newXMax - 2),
                                            Random.Range(newYMin + 2, newYMax - 2)
                                            );
            Vector2Int randomPosition = Vector2Int.Distance(randomPositionA, refPosition) < Vector2Int.Distance(randomPositionB, refPosition) ? randomPositionA : randomPositionB;
            if (cellDistribution.ContainsKey(randomPosition) == false)
            {
                CellState cellState = new CellState(true, true);
                cellDistribution.Add(randomPosition, cellState);
                convertedCells++;
            }
            maxCounter++;
            if (maxCounter >= 100000)
            {
                Debug.LogError("To many iterations during while");
                break;
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
        currentIteration++;

        Dictionary<Vector2Int, bool> newCellStates = new Dictionary<Vector2Int, bool>();
        for (int i = fieldSize.xMin; i <= fieldSize.xMax; i++)
        {
            for (int j = fieldSize.yMin; j <= fieldSize.yMax; j++)
            {
                Vector2Int newPosition = new Vector2Int(i, j);
                bool isFloor = false;
                if (newCellStates.ContainsKey(newPosition))
                {
                    isFloor = true;
                    if (cellDistribution[newPosition].IsChangeable == false) 
                    {
                        continue;
                    }
                }
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
                if (i == 0)
                    continue;

                cellToCheck = new Vector2Int(currentCell.x + i, currentCell.y);
                if (cellDistribution.ContainsKey(cellToCheck))
                {
                    if (cellDistribution[cellToCheck].IsFloor)
                    {
                        numFloorNeighbour++;
                        //Debug.Log($"currentPosition {currentCell} / currentNeighbour {cellToCheck}");
                    }
                }

                //add to y & check
                cellToCheck = new Vector2Int(currentCell.x, currentCell.y+i);
                if (cellDistribution.ContainsKey(cellToCheck))
                {
                    if (cellDistribution[cellToCheck].IsFloor)
                    {
                        numFloorNeighbour++;
                        //Debug.Log($"currentPosition {currentCell} / currentNeighbour {cellToCheck}");
                    }
                }

            }
        }
        return numFloorNeighbour;
    }
    public HashSet<Vector2Int> GetBoundry()
    {
        HashSet<Vector2Int> tiles = new HashSet<Vector2Int>();
        if (showWalls)
        {
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

    private DungeonTiles ReduceTiles(DungeonTiles tiles)
    {
        DungeonTiles newDungeonTiles = new DungeonTiles(AlgorithmType.CellulaAutomata);
        
        HashSet<Vector2Int> allTiles = new HashSet<Vector2Int>();
        HashSet<Vector2Int> checkedTiles = new HashSet<Vector2Int>();

        tiles.TryGetCorridors(out allTiles);

        foreach (var tile in allTiles) 
        {
            if (checkedTiles.Contains(tile))
                continue;

            HashSet<Vector2Int> currentTiles = new HashSet<Vector2Int>();
            Vector2Int currentPosition = tile;
            Queue<Vector2Int> lastSafePoints = new Queue<Vector2Int>();
            bool isChecking = true;
            int safetyCheck = 0;
            while (isChecking && safetyCheck <= 1000000)
            {
                safetyCheck++;
                bool neighbourIsSet = false;
                int possibleNeighbours = 0;
                Vector2Int lastSafePoint = currentPosition;
                foreach(var neighbour in GetNeighbour(currentPosition))
                {
                    if(allTiles.Contains(neighbour) && currentTiles.Contains(neighbour) == false)
                    {
                        possibleNeighbours++;
                        if(neighbourIsSet == false)
                        {
                            currentPosition = neighbour;
                            neighbourIsSet = true;
                        }
                    }
                }
                if (possibleNeighbours > 1)
                {
                    lastSafePoints.Enqueue(lastSafePoint);
                }
                if(currentPosition == lastSafePoint && lastSafePoints.Count > 0)
                {
                    currentPosition = lastSafePoints.Dequeue();
                }else if(lastSafePoints.Count == 0)
                {
                    isChecking = false;
                }

                currentTiles.Add(currentPosition);
            }
            
            checkedTiles.UnionWith(currentTiles);
            if(showRedundantSpace)
            {
                newDungeonTiles.AddRoom(currentTiles);
            }
            else
            {
                if(newDungeonTiles.Count() < currentTiles.Count ) 
                { 
                    newDungeonTiles.SetCorridor(currentTiles);
                }
            }
        }
        return newDungeonTiles;
    }

    private HashSet<Vector2Int> GetNeighbour(Vector2Int position)
    {
        HashSet<Vector2Int> neighbours = new HashSet<Vector2Int>();
        neighbours.Add(position + Vector2Int.up);
        neighbours.Add(position + Vector2Int.down);
        neighbours.Add(position + Vector2Int.left);
        neighbours.Add(position + Vector2Int.right);
        return neighbours;
    }
}

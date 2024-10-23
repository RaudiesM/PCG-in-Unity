using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;



public class CellulaAutomataAlgorithm_alt : CellulaAutomataAlgorithm 
{

    [SerializeField] private int offset = 2;
    private Dictionary<Vector2Int, Changeability> cellDistribution = new Dictionary<Vector2Int, Changeability>();
    private DungeonRoom roomSkeletton;

    #region Generation Methods
    public DungeonTiles Hybrid_GenerateDungeonTiles(DungeonTiles inputTiles, HashSet<BoundsInt> rooms, HashSet<Vector3Int> centers)
    {
        Hybrid_PrepareGeneration(inputTiles, rooms, centers);

        for (int i = 0; i < numIterations; i++)
        {
            ApplyCellulaAutomata();
        }

        DungeonTiles tiles = GetCellDistribution();
        if (isReducingTiles)
            tiles = ReduceTiles(tiles);

        //to visualize the underlying skeletton of the dungeon
        tiles.AddRoom(roomSkeletton);
        
        return tiles;
    }
    public DungeonTiles Hybrid_PrepareGeneration(DungeonTiles tiles, HashSet<BoundsInt> rooms, HashSet<Vector3Int> center)
    {
        cellDistribution.Clear();
        currentIteration = 0;
        IncorporateTiles(tiles);
        HashSet<DungeonRoom> dungeonRooms = new HashSet<DungeonRoom>();
        tiles.TryGetRooms(out dungeonRooms);
        List<BoundsInt> roomList = rooms.ToList();
        List<Vector3Int> centerList = center.ToList();
        for (int i = 0; i < roomList.Count; i++)
        {
            DistributeCells(roomList[i], centerList[i]);
        }
        return GetCellDistribution();
    }


    public override DungeonTiles ContinueIterating()
    {
        DungeonTiles tiles = new DungeonTiles(AlgorithmType.CellulaAutomata);
        if(currentIteration < numIterations)
        {
            ApplyCellulaAutomata();
            tiles = GetCellDistribution();
            tiles.AddRoom(roomSkeletton);
        }
        else
        {
            tiles = GetCellDistribution();
            tiles = ReduceTiles(tiles);
            tiles.AddRoom(roomSkeletton);
        }
        return tiles;
    }
    #endregion
    private void IncorporateTiles(DungeonTiles tiles)
    {
        HashSet<Vector2Int> newTiles = new HashSet<Vector2Int>();
        HashSet<DungeonRoom> newRoomTiles = new HashSet<DungeonRoom>();

        tiles.TryGetCorridors(out newTiles);
        tiles.TryGetRooms(out newRoomTiles);
        
        roomSkeletton = new DungeonRoom(newTiles);
        
        foreach(DungeonRoom room in newRoomTiles)
        {
            newTiles.UnionWith(room.GetRoomTiles());
        }
        foreach(Vector2Int tile in newTiles)
        {
            if (cellDistribution.ContainsKey(tile) == false)
            {
                cellDistribution.Add(tile, Changeability.unchangeable);
            }
        }
    }

    private DungeonTiles GetCellDistribution()
    {
        DungeonTiles tiles = new DungeonTiles(AlgorithmType.CellulaAutomata);

        foreach (var cell in cellDistribution)
        {
            tiles.AddCorridorTile(cell.Key);
        }
        return tiles;
    }
    private void DistributeCells(BoundsInt room, Vector3Int newCenter)
    {
        Vector2Int refPosition = new Vector2Int(newCenter.x, newCenter.y);
        
        int numMaxCells = (room.size.x -1) * (room.size.y -1);
        int cellPercent = Mathf.FloorToInt(numMaxCells * fillPercentage);
        int convertedCells = 0;
        int maxCounter = 0;
       
        int newXMin = room.position.x - (room.size.x / 2);
        int newYMin = room.position.y - (room.size.y / 2);
        int newXMax = room.position.x + (room.size.x / 2);
        int newYMax = room.position.y + (room.size.y / 2);

        while (convertedCells < cellPercent)
        {
            Vector2Int randomPositionA = UtilityFunctions.GetRandomPointWithinBounds(newXMin, newYMin, newXMax, newYMax, offset);
            Vector2Int randomPositionB = UtilityFunctions.GetRandomPointWithinBounds(newXMin, newYMin, newXMax, newYMax, offset);

            float distanceA = Vector2Int.Distance(randomPositionA, refPosition);
            float distanceB = Vector2Int.Distance(randomPositionB, refPosition);

            //distribute cells closer to new center
            Vector2Int randomPosition = distanceA < distanceB ? randomPositionA : randomPositionB;
            
            if (cellDistribution.ContainsKey(randomPosition))
            {
                continue;
            }

            cellDistribution.Add(randomPosition, Changeability.changeable);
            convertedCells++;

            maxCounter++;
            if (maxCounter >= 1000 * fieldSize.size.x * fieldSize.size.y)
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
                bool isFloor = false;
                if (newCellStates.ContainsKey(newPosition))
                {
                    isFloor = true;
                    if (cellDistribution[newPosition] == Changeability.unchangeable) 
                    {
                        continue;
                    }
                }
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
                cellDistribution.Add(cell.Key, Changeability.changeable);
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
            for (int i = -neighbourDistance; i <= neighbourDistance; i++)
            {
                for (int j = -neighbourDistance; j <= neighbourDistance; j++)
                {
                    cellToCheck = new Vector2Int(currentCell.x + i, currentCell.y + j);
                    if (currentCell != cellToCheck && cellDistribution.ContainsKey(cellToCheck))
                    {
                        numFloorNeighbour++;
                    }
                }
            }
        }
        else if (currentNeighbour == NeighbourType.Neumann)
        {
            for (int i = -neighbourDistance; i <= neighbourDistance; i++)
            {
                //ignore self
                if (i == 0)
                    continue;

                //add to x & check
                cellToCheck = new Vector2Int(currentCell.x + i, currentCell.y);
                if (cellDistribution.ContainsKey(cellToCheck))
                {
                    numFloorNeighbour++;
                }

                //add to y & check
                cellToCheck = new Vector2Int(currentCell.x, currentCell.y + i);
                if (cellDistribution.ContainsKey(cellToCheck))
                {
                    numFloorNeighbour++;
                }
            }
        }
        return numFloorNeighbour;
    }
}

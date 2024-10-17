using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DungeonSpawner : MonoBehaviour
{
    [SerializeField] private AlgorithmType currentAlgorithm = AlgorithmType.CellulaAutomata;
    [SerializeField] private CellulaAutomataAlgorithm CA_Algorithm;
    [SerializeField] private BinarySpacePartitioning_Algorithm BSP_Algorithm;
    [SerializeField] private RandomWalker_Algorithm RW_Algorithm;

    [SerializeField] private TilePlacer tilePlacer;

    private void Start()
    {
        InputManager.OnSetup += SetupGeneration;
        InputManager.OnIterate += IterateMore;
        InputManager.OnWholeDungeon += GenerateDungeon;
    }
    public void SetupGeneration()
    {
        DungeonTiles dungeonTiles = new DungeonTiles(currentAlgorithm);

        if (currentAlgorithm == AlgorithmType.RandomWalker)
        {
            dungeonTiles = RW_Algorithm.SetUpGeneration();

        }else if(currentAlgorithm == AlgorithmType.CellulaAutomata)
        {
            dungeonTiles = CA_Algorithm.SetUpGeneration();
        }
        else if(currentAlgorithm == AlgorithmType.BinarySpacepartitioning)
        {
            tilePlacer.VisualizeRooms(BSP_Algorithm.SetupGeneration());
        }else if(currentAlgorithm == AlgorithmType.RW_CA)
        {
            dungeonTiles = RW_Algorithm.GenerateDungeonTiles();
            CA_Algorithm.SetUpGeneration(dungeonTiles);
        }
        PlaceDungeon(dungeonTiles);
    }

    private void IterateMore()
    {
        DungeonTiles dungeonTiles = new DungeonTiles(currentAlgorithm);

        if (currentAlgorithm == AlgorithmType.RandomWalker)
        {
            dungeonTiles = RW_Algorithm.ContinueIterating();
        }
        else if (currentAlgorithm == AlgorithmType.CellulaAutomata)
        {
            dungeonTiles = CA_Algorithm.ContinueIterating();
        }else if(currentAlgorithm == AlgorithmType.BinarySpacepartitioning)
        {
            HashSet<Bounds> roomBounds = new HashSet<Bounds>();
            dungeonTiles = BSP_Algorithm.ContinueIterating(out roomBounds);
            if (roomBounds.Count > 0)
            {
                tilePlacer.VisualizeRooms(roomBounds);
            }
        }else if( currentAlgorithm == AlgorithmType.RW_CA)
        { 
            dungeonTiles = CA_Algorithm.ContinueIterating() ;
        }

        PlaceDungeon(dungeonTiles);
    }

    private void GenerateDungeon()
    {
        DungeonTiles dungeonTiles = new DungeonTiles(currentAlgorithm);
        if (currentAlgorithm == AlgorithmType.RandomWalker)
        {
            dungeonTiles =RW_Algorithm.GenerateDungeonTiles();
        }
        else if (currentAlgorithm == AlgorithmType.CellulaAutomata)
        {
            dungeonTiles = CA_Algorithm.GenerateDungeonTiles();
        }
        else if (currentAlgorithm == AlgorithmType.BinarySpacepartitioning)
        {
            dungeonTiles = BSP_Algorithm.GenerateDungeonTiles();
        }else if(currentAlgorithm == AlgorithmType.RW_CA)
        {
            dungeonTiles = RW_Algorithm.GenerateDungeonTiles();
            dungeonTiles = CA_Algorithm.GenerateDungeonTiles(dungeonTiles);
        }
        PlaceDungeon(dungeonTiles);
    }

    private void PlaceDungeon(DungeonTiles _tiles) 
    {
        HashSet<DungeonRoom> roomTiles = new HashSet<DungeonRoom>();
        HashSet<Vector2Int> corridorTiles = new HashSet<Vector2Int>();
        tilePlacer.ClearTilemap();
        if (_tiles.TryGetRooms(out roomTiles))
        {
            tilePlacer.PlaceRoomTiles(roomTiles);
        }
        if (_tiles.TryGetCorridors(out corridorTiles))
        {
            tilePlacer.PlaceFloorTiles(corridorTiles);
        }
    }
}

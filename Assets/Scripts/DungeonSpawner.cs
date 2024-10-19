using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DungeonSpawner : MonoBehaviour
{
    [SerializeField] private AlgorithmType currentAlgorithm = AlgorithmType.CellulaAutomata;
    [SerializeField] private CellulaAutomataAlgorithm CA_Algorithm;
    [SerializeField] private CellulaAutomataAlgorithm_old CA_Algorithm_old;
    [SerializeField] private BinarySpacePartitioning_Algorithm BSP_Algorithm;
    [SerializeField] private RandomWalker_Algorithm RW_Algorithm;

    [SerializeField] private TilePlacer tilePlacer;

    private void Start()
    {
        ConnectToInputs();
    }

    private void ConnectToInputs()
    {
        InputManager.OnSetup += SetupGeneration;
        Debug.Log("Press [A] to (re)start the step-by-step generation");
        InputManager.OnIterate += IterateMore;
        Debug.Log("Press [S] to iterate to iterate through the generation process step-by-step");
        InputManager.OnSpawnDungeon += GenerateDungeon;
        Debug.Log("Press [D] to create complete dungeon");
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
            dungeonTiles = BSP_Algorithm.SetUpGeneration();
        }
        else if(currentAlgorithm == AlgorithmType.RW_CA)
        {
            dungeonTiles = RW_Algorithm.GenerateDungeonTiles();
            CA_Algorithm.SetUpGeneration(dungeonTiles);
        }
        else if(currentAlgorithm == AlgorithmType.BSP_CA)
        {
            HashSet<BoundsInt> rooms = new HashSet<BoundsInt>();
            HashSet<Vector3Int> centers = new HashSet<Vector3Int>();
            dungeonTiles = BSP_Algorithm.GenerateDungeonTiles(out rooms, out centers);
            dungeonTiles = CA_Algorithm_old.SetUpGeneration(dungeonTiles, rooms, centers);
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
            dungeonTiles = BSP_Algorithm.ContinueIterating();
        }else if( currentAlgorithm == AlgorithmType.RW_CA)
        { 
            dungeonTiles = CA_Algorithm.ContinueIterating() ;
        }else if(currentAlgorithm == AlgorithmType.BSP_CA)
        {
            dungeonTiles = CA_Algorithm_old.ContinueIterating();
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
        }else if (currentAlgorithm == AlgorithmType.BSP_CA)
        {
            HashSet<BoundsInt> rooms = new HashSet<BoundsInt>();
            HashSet<Vector3Int> centers = new HashSet<Vector3Int>();
            dungeonTiles = BSP_Algorithm.GenerateDungeonTiles(out rooms, out centers);
            dungeonTiles = CA_Algorithm_old.GenerateDungeonTiles(dungeonTiles, rooms, centers);
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
            tilePlacer.PlaceCorridorTiles(corridorTiles);
        }
    }
}

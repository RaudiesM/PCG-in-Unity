using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DungeonSpawner : MonoBehaviour
{
    [SerializeField] private AlgorithmType currentAlgorithmType = AlgorithmType.CellulaAutomata;
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
        InputManager.OnSetup += FirstGeneration;
        Debug.Log("Press [A] to (re)start the step-by-step generation");
        InputManager.OnIterate += IterateMore;
        Debug.Log("Press [S] to iterate to iterate through the generation process step-by-step");
        InputManager.OnSpawnDungeon += GenerateDungeon;
        Debug.Log("Press [D] to create complete dungeon");
    }
    
    #region GetDungeonTiles
    private void FirstGeneration()
    {
        DungeonTiles dungeonTiles = new DungeonTiles(currentAlgorithmType);
        DungeonAlgorithmBase currentAlgorithm = GetAlgorithm(currentAlgorithmType);

        if(currentAlgorithmType == AlgorithmType.RW_CA)
        {
            //get dungeonTiles of RW and then iterate over it like CA
            dungeonTiles = RW_Algorithm.GenerateDungeonTiles();
            CA_Algorithm.FirstGeneration(dungeonTiles);
        }
        else if(currentAlgorithmType == AlgorithmType.BSP_CA)
        {
            //get dungeonTiles, rooms and newRoomCenters of BSP and randomly add cells of CA
            //rooms are added seperatly as Bounds to differenciate between the different rooms
            HashSet<BoundsInt> rooms = new HashSet<BoundsInt>();
            //newRoomCenters are given to spawn cells around the shifted room center
            HashSet<Vector3Int> newRoomCenters = new HashSet<Vector3Int>();
            dungeonTiles = BSP_Algorithm.GenerateDungeonTiles(out rooms, out newRoomCenters);
            dungeonTiles = CA_Algorithm_old.FirstGeneration(dungeonTiles, rooms, newRoomCenters);
        }
        else
        {
            dungeonTiles = currentAlgorithm.FirstGeneration();
        }

        PlaceDungeon(dungeonTiles);
    }
    private void IterateMore()
    {
        DungeonTiles dungeonTiles = new DungeonTiles(currentAlgorithmType);
        DungeonAlgorithmBase currentAlgorithm = GetAlgorithm(currentAlgorithmType);

        if (currentAlgorithmType == AlgorithmType.RW_CA)
        {
            //RWCA uses the base CA algorithm iteration
            currentAlgorithm = GetAlgorithm(AlgorithmType.CellulaAutomata);
        }

        if(currentAlgorithmType == AlgorithmType.BSP_CA)
        {
            //BSPCA uses the old version of CA algorithm
            dungeonTiles = CA_Algorithm_old.ContinueIterating();
        }
        else
        {
            dungeonTiles = currentAlgorithm.ContinueIterating();
        }
        PlaceDungeon(dungeonTiles);
    }
    private void GenerateDungeon()
    {
        DungeonTiles dungeonTiles = new DungeonTiles(currentAlgorithmType);
        DungeonAlgorithmBase currentAlgorithm = GetAlgorithm(currentAlgorithmType);

        if (currentAlgorithmType == AlgorithmType.RW_CA)
        {
            //get dungeonTiles of RW and then iterate over it like CA
            dungeonTiles = RW_Algorithm.GenerateDungeonTiles();
            dungeonTiles = CA_Algorithm.GenerateDungeonTiles(dungeonTiles);
        }
        else if (currentAlgorithmType == AlgorithmType.BSP_CA)
        {
            //get dungeonTiles, rooms and newRoomCenters of BSP and randomly add cells of CA
            //rooms are added seperatly as Bounds to differenciate between the different rooms
            HashSet<BoundsInt> rooms = new HashSet<BoundsInt>();
            //newRoomCenters are given to spawn cells around the shifted room center
            HashSet<Vector3Int> newRoomCenters = new HashSet<Vector3Int>();
            dungeonTiles = BSP_Algorithm.GenerateDungeonTiles(out rooms, out newRoomCenters);
            dungeonTiles = CA_Algorithm_old.GenerateDungeonTiles(dungeonTiles, rooms, newRoomCenters);
        }
        else
        {
            dungeonTiles = currentAlgorithm.GenerateDungeonTiles();
        }

        PlaceDungeon(dungeonTiles);
    }
    #endregion
    private DungeonAlgorithmBase GetAlgorithm(AlgorithmType thisAlgorithmType)
    {
        if (thisAlgorithmType == AlgorithmType.RandomWalker)
        {
            return RW_Algorithm;
        }
        else if (thisAlgorithmType == AlgorithmType.CellulaAutomata)
        {
            return CA_Algorithm;
        }
        else if (thisAlgorithmType == AlgorithmType.BinarySpacepartitioning)
        {
            return BSP_Algorithm;
        }
        return null;
    }
    private void PlaceDungeon(DungeonTiles _tiles) 
    {
        tilePlacer.ClearTilemap();

        HashSet<DungeonRoom> roomTiles = new HashSet<DungeonRoom>();
        HashSet<Vector2Int> corridorTiles = new HashSet<Vector2Int>();

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

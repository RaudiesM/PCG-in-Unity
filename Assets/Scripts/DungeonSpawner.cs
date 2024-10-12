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
        InputManager.OnGenerate += GenerateDungeon;
        InputManager.OnIterate += IterateMore;
    }

    private void IterateMore()
    {
        DungeonTiles dungeonTiles = new DungeonTiles(currentAlgorithm);
        HashSet<DungeonRoom> roomTiles = new HashSet<DungeonRoom>();
        HashSet<Vector2Int> corridorTiles = new HashSet<Vector2Int>();


        if (currentAlgorithm == AlgorithmType.CellulaAutomata)
        {
            dungeonTiles = CA_Algorithm.ContinueIterating();
        }else if(currentAlgorithm == AlgorithmType.BinarySpacepartitioning)
        {
            HashSet<Bounds> roomBounds = new HashSet<Bounds>();
            dungeonTiles = BSP_Algorithm.GetRooms(out roomBounds);
            if (roomBounds.Count > 0)
            {
                tilePlacer.VisualizeRooms(roomBounds);
            }
        }else if(currentAlgorithm == AlgorithmType.RandomWalker)
        {
            dungeonTiles = RW_Algorithm.GetDungeonTiles();
        }

        tilePlacer.ClearTilemap();
        if (dungeonTiles.TryGetRooms(out roomTiles))
        {
            tilePlacer.PlaceRoomTiles(roomTiles);
        }
        if (dungeonTiles.TryGetCorridors(out corridorTiles))
        {
            tilePlacer.PlaceFloorTiles(corridorTiles);
        }
    }

    public void GenerateDungeon()
    {
        DungeonTiles dungeonTiles = new DungeonTiles(currentAlgorithm);
        HashSet<DungeonRoom> roomTiles = new HashSet<DungeonRoom>();
        HashSet<Vector2Int> corridorTiles = new HashSet<Vector2Int>();

        if (currentAlgorithm == AlgorithmType.CellulaAutomata)
        {
            dungeonTiles = CA_Algorithm.GetDungeonTiles();

        }else if(currentAlgorithm == AlgorithmType.BinarySpacepartitioning)
        {
            //dungeonTiles = BSP_Algorithm.GetDungeonTiles();
            tilePlacer.VisualizeRooms(BSP_Algorithm.NewRooms());
        }

        tilePlacer.ClearTilemap();
        if (dungeonTiles.TryGetRooms(out roomTiles))
        {
            tilePlacer.PlaceRoomTiles(roomTiles);
        }
        if(dungeonTiles.TryGetCorridors(out corridorTiles))
        {
            tilePlacer.PlaceFloorTiles(corridorTiles);
        }
    }
}

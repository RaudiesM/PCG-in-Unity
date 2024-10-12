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
    [SerializeField] private TilemapVisiualizer tileVisualizer;

    private void Start()
    {
        InputManager.OnGenerate += GenerateDungeon;
        InputManager.OnIterate += IterateMore;
    }

    private void IterateMore()
    {
        HashSet<Vector2Int> dungeonTiles = new HashSet<Vector2Int>();
        if (currentAlgorithm == AlgorithmType.CellulaAutomata)
        {
            dungeonTiles = CA_Algorithm.ContinueIterating();
            tilePlacer.PlaceFloorTiles(dungeonTiles);
        }else if(currentAlgorithm == AlgorithmType.BinarySpacepartitioning)
        {
            tilePlacer.PlaceRooms(BSP_Algorithm.GetRooms(out HashSet<Vector2Int> dungeonRooms));
            if(dungeonRooms.Count > 0)
            {
                tilePlacer.PlaceFloorTiles(dungeonRooms);
            }
        }else if(currentAlgorithm == AlgorithmType.RandomWalker)
        {
            HashSet<Bounds> roomsAsBounds = new HashSet<Bounds>();
            dungeonTiles.Clear();
            dungeonTiles = RW_Algorithm.GetDungeonTiles(out roomsAsBounds);
            tilePlacer.PlaceFloorTiles(dungeonTiles);
            tilePlacer.PlaceRooms(roomsAsBounds);
            tileVisualizer.PaintFloorTiles(dungeonTiles);
        }
    }

    public void GenerateDungeon()
    {
        HashSet<Vector2Int> dungeonTiles = new HashSet<Vector2Int>();
        if (currentAlgorithm == AlgorithmType.CellulaAutomata)
        {
            dungeonTiles = CA_Algorithm.GetDungeonTiles();
            HashSet<Vector2Int> boundryTiles = CA_Algorithm.GetBoundry();
            if(boundryTiles.Count > 0)
            {
                tilePlacer.PlaceBoundryTiles(boundryTiles);
            }
            tilePlacer.PlaceFloorTiles(dungeonTiles);

        }else if(currentAlgorithm == AlgorithmType.BinarySpacepartitioning)
        {
            //dungeonTiles = BSP_Algorithm.GetDungeonTiles();
            tilePlacer.PlaceRooms(BSP_Algorithm.NewRooms());
        }
    }
}

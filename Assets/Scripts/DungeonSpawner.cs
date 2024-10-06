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

    [SerializeField] private TilePlacer tilePlacer;

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
            tilePlacer.PlaceRooms(BSP_Algorithm.GetRooms());
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

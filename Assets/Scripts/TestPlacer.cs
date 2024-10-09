using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPlacer : MonoBehaviour
{
    [SerializeField] private BoundsInt testBounds;
    [SerializeField] private TilePlacer tilePlacer;

    private void Start()
    {
        HashSet<BoundsInt> bounds = new HashSet<BoundsInt>();
        bounds.Add(testBounds);
        HashSet<Vector2Int> bounds2 = GetBoundsAsTiles();
        tilePlacer.PlaceRooms(bounds);
        tilePlacer.PlaceFloorTiles(bounds2);
    }

    private HashSet<Vector2Int> GetBoundsAsTiles()
    {
        HashSet<Vector2Int> newHV = new HashSet<Vector2Int>();
        Debug.Log($"bounds xMin/Max {testBounds.xMin}/{testBounds.xMax}; yMin/Max {testBounds.yMin}/{testBounds.yMax}");
        for(int i = testBounds.xMin-testBounds.size.x/2; i < testBounds.xMax- testBounds.size.x/2; i++)
        {
            for(int j = testBounds.yMin - testBounds.size.y/2; j < testBounds.yMax - testBounds.size.y/2; j++)
            {
                newHV.Add(new Vector2Int(i, j));
            }
        }
    return newHV;
    }
}

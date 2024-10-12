using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapVisiualizer : MonoBehaviour
{
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private TileBase floorTile;

    public void PaintFloorTiles(IEnumerable<Vector2Int> tiles)
    {
        foreach (Vector2Int tile in tiles)
        {
            PaintTiles(tile);
        }
    }

    private void PaintTiles(Vector2Int tile)
    {
        var tilePosition = floorTilemap.WorldToCell((Vector3Int)tile);
        floorTilemap.SetTile(tilePosition, floorTile);
    }

    private void ClearTilemap()
    {
        floorTilemap.ClearAllTiles();
    }
}

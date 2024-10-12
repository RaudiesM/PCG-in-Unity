using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapVisiualizer : MonoBehaviour
{
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private TileBase floorTile;
    [SerializeField] private TileBase boundryTile;

    public void PaintFloorTiles(HashSet<Vector2Int> tiles)
    {
        PaintTiles(tiles, floorTile);
    }

    public void PaintBoundryTiles(HashSet<Vector2Int> tiles)
    {
        PaintTiles(tiles, boundryTile);
    }


    private void PaintTiles(HashSet<Vector2Int> tiles, TileBase tileType)
    {
        foreach(var tile in tiles)
        {
            var tilePosition = floorTilemap.WorldToCell((Vector3Int)tile);
            floorTilemap.SetTile(tilePosition, tileType);
        }
    }

    private void ClearTilemap()
    {
        floorTilemap.ClearAllTiles();
    }
}

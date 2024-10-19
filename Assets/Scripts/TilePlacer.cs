using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilePlacer : MonoBehaviour
{
    [SerializeField] private Tilemap dungeonTilemap;

    [SerializeField] private TileBase floorTile;
    [SerializeField] private List<TileBase> roomTiles;

    [SerializeField] private GameObject roomPanel;

    [SerializeField] private bool visualizeRooms = false;

    #region PlaceTiles
    public void PlaceCorridorTiles(HashSet<Vector2Int> tilePosition)
    {
        foreach (Vector2Int position in tilePosition)
        {
            PlaceTile(position, floorTile);
        }
    }
    public void PlaceRoomTiles(HashSet<DungeonRoom> rooms)
    {
        TileBase tile = floorTile;
        foreach(DungeonRoom room in rooms)
        {
            if (visualizeRooms)
            {
                //get different color for each room
                int randValue = Random.Range(0, roomTiles.Count);
                tile = roomTiles[randValue];
            }

            foreach(Vector2Int position in room.GetRoomTiles())
            {
                PlaceTile(position, tile);
            }
        }

    }
    private void PlaceTile(Vector2Int tile, TileBase tileType)
    {
        var tilePosition = dungeonTilemap.WorldToCell((Vector3Int)tile);
        dungeonTilemap.SetTile(tilePosition, tileType);
    }
    #endregion
    public void ClearTilemap()
    {
        dungeonTilemap.ClearAllTiles();
    }
}

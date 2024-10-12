using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilePlacer : MonoBehaviour
{
    [SerializeField] private Tilemap floorTilemap;

    [SerializeField] private TileBase floorTile;
    [SerializeField] private TileBase boundryTile;
    [SerializeField] private TileBase roomTile;

    [SerializeField] private GameObject roomPanel;
    [SerializeField] private GameObject tileParent;

    [SerializeField] private bool visualizeRooms = false;

    public void PlaceFloorTiles(HashSet<Vector2Int> tilePosition)
    {
        PlaceTiles(tilePosition, floorTile);
    }

    public void PlaceBoundryTiles(HashSet<Vector2Int> tilePosition)
    {
        PlaceTiles(tilePosition, boundryTile);
    }

    public void PlaceRoomTiles(HashSet<Vector2Int> tilePosition)
    {
        TileBase tile = floorTile;
        if (visualizeRooms)
        {
            tile = roomTile;
        }
        PlaceTiles(tilePosition, tile);
    }

    private void PlaceTiles(HashSet<Vector2Int> tiles, TileBase tileType)
    {
        ClearTilemap();
        foreach (var tile in tiles)
        {
            var tilePosition = floorTilemap.WorldToCell((Vector3Int)tile);
            floorTilemap.SetTile(tilePosition, tileType);
        }
    }
    private void ClearTilemap()
    {
        floorTilemap.ClearAllTiles();
    }

    public void VisualizeRooms(HashSet<Bounds> roomPosition)
    {
        RemoveRooms();
        foreach (var room in roomPosition)
        {

            GameObject newRoomTile = Instantiate(roomPanel);
            newRoomTile.transform.parent = tileParent.transform;
            newRoomTile.transform.position = room.center + new Vector3(0, 0, -1f);
            newRoomTile.transform.localScale = room.size;
            newRoomTile.name = "Room";
            if (newRoomTile.TryGetComponent<SpriteRenderer>(out SpriteRenderer thisRenderer))
            {
                thisRenderer.color = new Color(Random.value, Random.value, Random.value);
            }
        }
    }

    public void RemoveRooms()
    {
        int numChildren = tileParent.transform.childCount;
        for (int i = 0; i < numChildren; i++)
        {
            Destroy(tileParent.transform.GetChild(i).gameObject);
        }
    }
}

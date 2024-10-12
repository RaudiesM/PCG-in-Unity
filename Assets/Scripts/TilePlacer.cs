using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilePlacer : MonoBehaviour
{
    [SerializeField] private Tilemap floorTilemap;

    [SerializeField] private TileBase floorTile;
    [SerializeField] private TileBase boundryTile;
    [SerializeField] private List<TileBase> roomTiles;

    [SerializeField] private GameObject roomPanel;
    [SerializeField] private GameObject tileParent;

    [SerializeField] private bool visualizeRooms = false;

    public void PlaceFloorTiles(HashSet<Vector2Int> tilePosition)
    {
        RemoveRooms();
        foreach (Vector2Int position in tilePosition)
        {
            PlaceTile(position, floorTile);
        }
    }

    public void PlaceBoundryTiles(HashSet<Vector2Int> tilePosition)
    {
        RemoveRooms();
        foreach (Vector2Int position in tilePosition) 
        { 
            PlaceTile(position, boundryTile); 
        }
    }

    public void PlaceRoomTiles(HashSet<DungeonRoom> rooms)
    {
        RemoveRooms();
        TileBase tile = floorTile;
        foreach(DungeonRoom room in rooms)
        {
            if (visualizeRooms)
            {
                int randValue = Random.Range(0, roomTiles.Count);
                tile = roomTiles[randValue];
            }

            foreach(Vector2Int position in room.GetRoomTiles())
            {
                PlaceTile(position, tile);
            }
        }

    }

    private void PlaceRooms(HashSet<DungeonRoom> rooms, TileBase tile)
    {
        throw new System.NotImplementedException();
    }

    private void PlaceTile(Vector2Int tile, TileBase tileType)
    {
        var tilePosition = floorTilemap.WorldToCell((Vector3Int)tile);
        floorTilemap.SetTile(tilePosition, tileType);
    }
    public void ClearTilemap()
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilePlacer : MonoBehaviour
{
    [SerializeField] private GameObject floorTile;
    [SerializeField] private GameObject boundryTile;
    [SerializeField] private GameObject roomTile;
    [SerializeField] private GameObject tileParent;


    public void PlaceFloorTiles(HashSet<Vector2Int> tilePosition)
    {
        PlaceTiles(tilePosition, floorTile);
    }

    public void PlaceBoundryTiles(HashSet<Vector2Int> tilePosition)
    {
        PlaceTiles(tilePosition, boundryTile);
    }

    public void PlaceRooms(HashSet<BoundsInt> roomPosition)
    {
        RemoveTiles();
        foreach (var room in roomPosition) { 
            
            GameObject newRoomTile = Instantiate(roomTile);
            newRoomTile.transform.parent = tileParent.transform;
            newRoomTile.transform.position = new Vector3(room.x, room.y, -0.01f);
            newRoomTile.transform.localScale = room.size;
            newRoomTile.name = "Room";
            if(newRoomTile.TryGetComponent<SpriteRenderer>(out SpriteRenderer thisRenderer))
            {
                thisRenderer.color = new Color(Random.value, Random.value, Random.value);
            }
        }
    }

    private void PlaceTiles(HashSet<Vector2Int> tilePosition, GameObject placeableObject)
    {
        RemoveTiles();
        foreach (var item in tilePosition)
        {
            Vector3 newPosition = new Vector3(item.x, item.y);
            GameObject newFloorTile = Instantiate(placeableObject);
            newFloorTile.transform.position = newPosition;
            newFloorTile.transform.parent = tileParent.transform;
        }
    }

    public void RemoveTiles()
    {
        int numChildren = tileParent.transform.childCount;
        for (int i = 0; i < numChildren; i++)
        {
            Destroy(tileParent.transform.GetChild(i).gameObject);
        }
    }

}

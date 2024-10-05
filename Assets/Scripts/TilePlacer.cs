using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilePlacer : MonoBehaviour
{
    [SerializeField] private GameObject floorTile;
    [SerializeField] private GameObject boundryTile;
    [SerializeField] private GameObject tileParent;
    public void PlaceFloorTiles(HashSet<Vector2Int> tilePosition)
    {
        PlaceTiles(tilePosition, floorTile);
    }
    //test
    public void PlaceBoundryTiles(HashSet<Vector2Int> tilePosition)
    {
        PlaceTiles(tilePosition, boundryTile);
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

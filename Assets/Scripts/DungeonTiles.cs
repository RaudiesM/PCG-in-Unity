using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct DungeonTiles
{
    private HashSet<Vector2Int> roomTiles;
    private HashSet<Vector2Int> corridorTiles;

    public bool TryGetRooms(out HashSet<Vector2Int> rooms) 
    {
        rooms = roomTiles;
        return roomTiles.Count > 0; 
    }
    public bool TryGetCorridors(out HashSet<Vector2Int> corridors) 
    { 
        corridors =  corridorTiles;
        return corridors.Count > 0;
    }

    public void AddRoom(HashSet<Vector2Int> newTiles)
    {
        roomTiles.UnionWith(newTiles);
    }

    public void AddCorridor(HashSet<Vector2Int> newTiles)
    {
        corridorTiles.UnionWith(newTiles);
    }

    public void Clear()
    {
        roomTiles.Clear();
        corridorTiles.Clear();
    }

}

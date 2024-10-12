using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct DungeonRoom
{
    HashSet<Vector2Int> roomTiles;

    public DungeonRoom(HashSet<Vector2Int> _roomTiles)
    {
        roomTiles = _roomTiles;
    }

    public void AddRoomTiles(Vector2Int _roomTile)
    {
        roomTiles.Add(_roomTile);
    }
    public void AddRoomTiles(HashSet<Vector2Int> _roomTiles)
    {
        roomTiles.UnionWith(_roomTiles);
    }

    public HashSet<Vector2Int> GetRoomTiles()
    {
        return roomTiles;
    }
}

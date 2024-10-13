
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct DungeonTiles
{
    private HashSet<Vector2Int> corridorTiles;
    private HashSet<DungeonRoom> dungeonRooms;
    private AlgorithmType thisAlgorithmType;
    public DungeonTiles(AlgorithmType _algorithmType)
    {
        thisAlgorithmType = _algorithmType;   
        corridorTiles = new HashSet<Vector2Int>();
        dungeonRooms = new HashSet<DungeonRoom>();
    }

    public int Count()
    {
        return GetDungeonRoomTiles().Count + corridorTiles.Count;
    }

    private HashSet<Vector2Int> GetDungeonRoomTiles()
    {
       HashSet<Vector2Int> roomTiles = new HashSet<Vector2Int>();
       foreach (var room in dungeonRooms) 
       { 
            roomTiles.UnionWith(room.GetRoomTiles());    
       }
       return roomTiles;

    }

    public bool TryGetRooms(out HashSet<DungeonRoom> rooms) 
    {
        rooms = dungeonRooms;
        return GetDungeonRoomTiles().Count > 0; 
    }
    public bool TryGetCorridors(out HashSet<Vector2Int> corridors) 
    { 
       corridors = new HashSet<Vector2Int>();
       HashSet<Vector2Int> roomTiles = GetDungeonRoomTiles();
       foreach(var corridor in corridorTiles)
       {
            if (!roomTiles.Contains(corridor))
            {
                corridors.Add(corridor);
            }
       }
       return corridors.Count > 0;
    }

    public void AddCorridorTile(Vector2Int tile)
    {
        corridorTiles.Add(tile);
    }

    public void AddRoom(HashSet<Vector2Int> newTiles)
    {
        dungeonRooms.Add(new DungeonRoom(newTiles));
    }

    public void AddRoom(DungeonRoom room)
    {
        dungeonRooms.Add(room);
    }

    public void AddRoom(HashSet<DungeonRoom> rooms)
    {
        dungeonRooms.UnionWith(rooms);
    }

    public void AddCorridor(HashSet<Vector2Int> newTiles)
    {
        corridorTiles.UnionWith(newTiles);
    }

    public void SetCorridor(HashSet<Vector2Int> newTiles)
    {
        corridorTiles = newTiles;
    }

    public void Clear()
    {
        dungeonRooms.Clear();
        corridorTiles.Clear();
    }

}

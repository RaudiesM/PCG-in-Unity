
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        CombineOverlappingRooms();
        return GetDungeonRoomTiles().Count > 0; 
    }
    public bool TryGetCorridors(out HashSet<Vector2Int> corridors) 
    {
        //corridors = corridorTiles;
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
        CombineOverlappingRooms();
    }

    private void CombineOverlappingRooms()
    {
        Debug.Log("Anzahl Räume(before): "+dungeonRooms.Count);
        HashSet<int> checkedRooms = new HashSet<int>();
        List<DungeonRoom> newDungeonRooms = dungeonRooms.ToList<DungeonRoom>();
        HashSet<DungeonRoom> overlappingRooms = new HashSet<DungeonRoom>();

        for(int i = 0; i < newDungeonRooms.Count; i++)
        {
            overlappingRooms.Clear();
            HashSet<Vector2Int> roomTiles = newDungeonRooms[i].GetRoomTiles();
            for(int j = i; j < newDungeonRooms.Count; j++)
            {
                if (checkedRooms.Contains(j) || i==j)
                    continue;
                HashSet<Vector2Int> otherRoomTiles = newDungeonRooms[j].GetRoomTiles();
                if (otherRoomTiles.Overlaps(roomTiles))
                {
                    checkedRooms.Add(j);
                    overlappingRooms.Add(newDungeonRooms[j]);
                }
            }
            if(overlappingRooms.Count > 0)
            {
                dungeonRooms.Remove(newDungeonRooms[i]);
                foreach(var room in overlappingRooms)
                {
                    Debug.Log("Combining");
                    roomTiles.UnionWith(room.GetRoomTiles());
                    dungeonRooms.Remove(room);
                }
                dungeonRooms.Add(new DungeonRoom(roomTiles));
            }
        }
        Debug.Log("Anzahl Räume(after): " + dungeonRooms.Count);
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

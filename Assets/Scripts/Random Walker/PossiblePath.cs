using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PossiblePath 
{
    private Vector2Int direction;
    private int distance;
    private bool canPlaceRoom;
    private Bounds room;

    public PossiblePath(Vector2Int _direction, int _distance, bool _canPlaceRoom = false, int _roomSizeX = 0, int _roomSizeY=0)
    {
        this.direction = _direction;
        this.distance = _distance;
        this.canPlaceRoom = _canPlaceRoom;
        this.room = new Bounds(Vector3.zero, new Vector3(_roomSizeX, _roomSizeY));
    }
 
    public Vector2Int GetDirection() { return direction; }
    public int GetDistance() { return distance; }
    public bool CanPlaceRoom() {  return canPlaceRoom; }
    public Bounds GetRoom() {  return room; }

    public void SetRoomPosition(Vector2Int _position) 
    {
        room.center = _position + new Vector2(0.5f, 0.5f);
    }

}

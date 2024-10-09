using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct RoomPoints
{
    private List<Vector2Int> roomPoints;


    #region Constructor Functions
    public RoomPoints(Vector2Int newRoomPoint)
    {
        roomPoints = new List<Vector2Int>();
        roomPoints.Add(newRoomPoint);
    }
    public RoomPoints(Vector3Int newRoomPoint)
    {
        roomPoints = new List<Vector2Int>();
        AddRoomPoints(newRoomPoint);
    }
    public RoomPoints(List<Vector2Int> newRoomPoints)
    {
        roomPoints = newRoomPoints;
    }
    public RoomPoints(RoomPoints roomPointsA)
    {
        roomPoints = new List<Vector2Int>();
        AddRoomPoints(roomPointsA);
    }
    public RoomPoints(RoomPoints roomPointsA, RoomPoints roomPointsB)
    {
        roomPoints = new List<Vector2Int>();
        AddRoomPoints(roomPointsA);
        AddRoomPoints(roomPointsB);
    }
    #endregion

    #region functions
    public void AddRoomPoints(Vector2Int newRoomPoint)
    {
        roomPoints.Add(newRoomPoint);
    }
    public void AddRoomPoints(Vector3Int newRoomPoint)
    {
        roomPoints.Add(new Vector2Int(newRoomPoint.x, newRoomPoint.y));
    }
    public void AddRoomPoints(List<Vector2Int> newRoomPoints)
    {
        foreach(Vector2Int point in newRoomPoints)
        {
            roomPoints.Add(point);
        }
    }
    public void AddRoomPoints(RoomPoints newRoomPoints)
    {
        foreach(Vector2Int point in newRoomPoints.GetRoomPoints())
        {
            roomPoints.Add(point);
        }
    }

    public List<Vector2Int> GetRoomPoints()
    {
        return roomPoints;
    }

    public Vector2Int GetClosestPoint(Vector2Int pointRef)
    {
        Vector2Int closestPoint = new Vector2Int();
        float distance = 0;
        foreach(var point in roomPoints)
        {
            float newDistance = Vector2Int.Distance(pointRef, point);
            if(newDistance < distance || distance == 0)
            {
                distance = newDistance;
                closestPoint = point;
            }
        }
        return closestPoint;
    }
    public Vector2Int GetAveragePoint()
    {
        Vector2Int averagePoint = new Vector2Int();
        foreach(Vector2Int point in roomPoints)
        {
            averagePoint += point;
        }
        averagePoint /= roomPoints.Count;
        return averagePoint;
    }
    #endregion
}

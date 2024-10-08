using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct RoomPoints
{
    private List<Vector2> roomPoints;


    #region Constructor Functions
    public RoomPoints(Vector2 newRoomPoint)
    {
        roomPoints = new List<Vector2>();
        roomPoints.Add(newRoomPoint);
    }
    public RoomPoints(List<Vector2> newRoomPoints)
    {
        roomPoints = newRoomPoints;
    }
    public RoomPoints(RoomPoints newRoomPoints)
    {
        roomPoints = new List<Vector2>();
        foreach(Vector2 point in newRoomPoints.GetRoomPoints())
        {
            roomPoints.Add(point);
        }
    }
    #endregion

    #region functions
    public void AddRoomPoints(Vector2 newRoomPoint)
    {
        roomPoints.Add(newRoomPoint);
    }
    public void AddRoomPoints(RoomPoints newRoomPoints)
    {
        foreach(Vector2 point in newRoomPoints.GetRoomPoints())
        {
            roomPoints.Add(point);
        }
    }

    public List<Vector2> GetRoomPoints()
    {
        return roomPoints;
    }

    public Vector2 GetClosestPoint(Vector2 pointRef)
    {
        Vector2 closestPoint = new Vector2();
        float distance = 0;
        foreach(Vector2 point in roomPoints)
        {
            float newDistance = Vector2.Distance(pointRef, point);
            if(newDistance < distance || distance == 0)
            {
                distance = newDistance;
                closestPoint = point;
            }
        }
        return closestPoint;
    }
    public Vector2 GetAveragePoint()
    {
        Vector2 averagePoint = new Vector2();
        foreach(Vector2 point in roomPoints)
        {
            averagePoint += point;
        }
        averagePoint /= roomPoints.Count;
        return averagePoint;
    }
    #endregion
}

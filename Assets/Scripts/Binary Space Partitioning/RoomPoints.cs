using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct RoomPoints
{
    private List<Vector2Int> roomPoints;

    #region Constructor Functions
    public RoomPoints(Vector3Int newPostition)
    {
        roomPoints = new List<Vector2Int>();
        AddRoomPoints(newPostition);
    }
    public RoomPoints(RoomPoints roomPointsA, RoomPoints roomPointsB)
    {
        //constructor variant to directly combine to points of two spaces
        roomPoints = new List<Vector2Int>();
        AddRoomPoints(roomPointsA);
        AddRoomPoints(roomPointsB);
    }
    #endregion
    #region Add funcitons
    public void AddRoomPoints(Vector3Int newPosition)
    {
        roomPoints.Add(new Vector2Int(newPosition.x, newPosition.y));
    }
    public void AddRoomPoints(RoomPoints newRoomPoints)
    {
        foreach(Vector2Int point in newRoomPoints.GetRoomPoints())
        {
            roomPoints.Add(point);
        }
    }

    #endregion
    #region Get functions

    public List<Vector2Int> GetRoomPoints()
    {
        return roomPoints;
    }

    public Vector2Int GetClosestPoint(Vector2Int pointRef, Vector2Int emergencyPoint = new Vector2Int())
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

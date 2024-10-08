using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testscript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        RoomPoints newPoints = new RoomPoints(new Vector2(1, 1));
        newPoints.AddRoomPoints(new Vector2(1, 4));
        newPoints.AddRoomPoints(new Vector2(4, 1));
        newPoints.AddRoomPoints(new Vector2(4, 4));
        RoomPoints newestPoints = new RoomPoints(newPoints);
        foreach(var points in newestPoints.GetRoomPoints())
        {
            Debug.Log(points);
        }
        Debug.Log($"ClosestPoint to 2/2: {newestPoints.GetClosestPoint(new Vector2(2,2))}");
        Debug.Log(newestPoints.GetAveragePoint());
    }
}

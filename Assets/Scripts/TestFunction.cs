using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestFunction : MonoBehaviour
{
    public int roomMax;
    public Bounds room1;
    public Bounds room2;
    public Vector2Int curPos;
    public Vector2 direction;

    public GameObject tile;

    private HashSet<Bounds> roomList = new HashSet<Bounds>();

    private void Start()
    {
        roomList.Add(room1);
        roomList.Add(room2);
        //Debug.Log(GetDistanceToRooms(curPos, direction));
    }

    private int GetDistanceToRooms(Vector2Int curPos, Vector2 direction)
    {
        int maxSize = 0;
        Vector2 newPosition = new Vector2(curPos.x + 0.5f, curPos.y + 0.5f);
        Vector2 newRoomSize = new Vector2();

        for (int i = 1; i <= roomMax; i++)
        {
            maxSize = i;
            if (direction == Vector2.up || direction == Vector2.down)
            {
                newRoomSize = new Vector2Int(3,i +1);
            }
            else if (direction == Vector2.left || direction == Vector2.right)
            {
                newRoomSize = new Vector2Int(i+1, 3);
            }
            Bounds newRoom = new Bounds(newPosition, newRoomSize);
            GameObject goX = Instantiate(tile);
            goX.transform.position = newRoom.center;
            goX.transform.localScale = newRoom.size;
            Debug.Log("newRoom: "+newRoom);

            foreach (Bounds room in roomList)
            {
                if (newRoom.Intersects(room))
                {
                    return (maxSize-2)/ 2;
                }
            }
        }
        return maxSize / 2;
    }
}

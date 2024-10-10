using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RandomWalker_Algorithm : MonoBehaviour
{
    [SerializeField] private DungeonType thisDungeon;
    [Range(50, 5000)] private int maxTiles;
    [Range(0, 100)] private int roomSpawnRate;

    private List<Vector3> tileList = new List<Vector3>();

    public HashSet<Vector3> GetDungeonTiles()
    {
        tileList.Clear();
        switch (thisDungeon)
        {
            case DungeonType.Caverns:
                BaseRandomWalker();
                break;
            case DungeonType.Rooms:
                RoomWalker();
                break;
            case DungeonType.Winding:
                WindingWalker();
                break;
        }

        return tileList.ToHashSet();
    }

    private void BaseRandomWalker()
    {
        Vector3 curPos = Vector3.zero;
        tileList.Add(curPos);
        while (tileList.Count < maxTiles)
        {
            curPos += RandomDirection();
            if (!tileList.Contains(curPos))
            {
                tileList.Add(curPos);
            }
        }
    }

    private void RoomWalker()
    {
        Vector3 curPos = Vector3.zero;
        tileList.Add(curPos);
        while (tileList.Count < maxTiles)
        {
            curPos = SetHallway(curPos);
            SetRoom(curPos);
        }
    }
    private void WindingWalker()
    {
        Vector3 curPos = Vector3.zero;
        tileList.Add(curPos);

        while (tileList.Count < maxTiles)
        {
            curPos = SetHallway(curPos);
            int rollForRoom = Random.Range(1, 101);
            if(rollForRoom <= roomSpawnRate)
            {
                SetRoom(curPos);
            }
        }
    }

    private void SetRoom(Vector3 curPos)
    {
        int height = Random.Range(1, 5);
        int width = Random.Range(1, 5);
        for (int w = -width; w <= width; w++)
        {
            for (int h = -height; h <= height; h++)
            {
                Vector3 offset = new Vector3(w, h, 0);
                if (!tileList.Contains(curPos + offset))
                {
                    tileList.Add(curPos + offset);
                }

            }
        }
    }

    private Vector3 SetHallway(Vector3 curPos)
    {
        Vector3 walkDir = RandomDirection();
        int walkLength = Random.Range(9, 18);
        for (int i = 0; i < walkLength; i++)
        {
            curPos += walkDir;
            if (!tileList.Contains(curPos))
            {
                tileList.Add(curPos);
            }
        }
        return curPos;
    }

    private Vector3 RandomDirection()
    {
        switch (Random.Range(1, 5))
        {
            case 1:
                return Vector3.up;
            case 2:
                return Vector3.down;
            case 3:
                return Vector3.left;
            case 4:
                return Vector3.right;
        }
        return Vector3.zero;
    }

    
}

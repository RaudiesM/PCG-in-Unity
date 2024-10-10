using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RandomWalker_Algorithm : MonoBehaviour
{
    [SerializeField] private DungeonType currentType;
    [SerializeField] private BoundsInt fieldSize;
    [Range(50, 5000)]
    [SerializeField] private int maxTiles;
    [Range(0, 100)]
    [SerializeField] private int roomSpawnRate;

    private List<Vector2Int> tileList = new List<Vector2Int>();
    private Vector2Int startPosition;
    public HashSet<Vector2Int> GetDungeonTiles()
    {
        tileList.Clear();
        startPosition = new Vector2Int(Mathf.FloorToInt(fieldSize.center.x), Mathf.FloorToInt(fieldSize.center.y));

        switch (currentType)
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
        Vector2Int curPos = startPosition;
        tileList.Add(curPos);
        while (tileList.Count < maxTiles)
        {
            curPos += RandomDirection(curPos);
            if (!tileList.Contains(curPos))
            {
                tileList.Add(curPos);
            }
        }
    }

    private void RoomWalker()
    {
        Vector2Int curPos = startPosition;
        tileList.Add(curPos);
        while (tileList.Count < maxTiles)
        {
            curPos = SetHallway(curPos);
            SetRoom(curPos);
        }
    }
    private void WindingWalker()
    {
        Vector2Int curPos = startPosition;
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

    private void SetRoom(Vector2Int curPos)
    {
        int height = Random.Range(1, 5);
        int width = Random.Range(1, 5);
        for (int w = -width; w <= width; w++)
        {
            for (int h = -height; h <= height; h++)
            {
                Vector2Int offset = new Vector2Int(w, h);
                if (!tileList.Contains(curPos + offset))
                {
                    tileList.Add(curPos + offset);
                }

            }
        }
    }

    private Vector2Int SetHallway(Vector2Int curPos)
    {
        Vector2Int walkDir = RandomDirection(curPos);
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

    private Vector2Int RandomDirection()
    {
        switch (Random.Range(1, 5))
        {
            case 1:
                return Vector2Int.up;
            case 2:
                return Vector2Int.down;
            case 3:
                return Vector2Int.left;
            case 4:
                return Vector2Int.right;
        }
        return Vector2Int.zero;
    }

    private Vector2Int RandomDirection(Vector2Int pos)
    {
        List<Vector2Int> directionList = GetListOfDirections(pos);
        if (directionList.Count > 0)
        {
            int randNumb = Random.Range(0, directionList.Count);

            return directionList[randNumb];
        }
        return Vector2Int.zero;
    }

    private List<Vector2Int> GetListOfDirections(Vector2Int pos)
    {
        List<Vector2Int> directionList = new List<Vector2Int>();
        if (pos.y < fieldSize.yMax-1)
        {
            directionList.Add(Vector2Int.up);
        }
        if (pos.y > fieldSize.yMin)
        {
            directionList.Add(Vector2Int.down);
        }
        if (pos.x > fieldSize.xMin)
        {
            directionList.Add(Vector2Int.left);
        }
        if (pos.x < fieldSize.xMax-1)
        {
            directionList.Add(Vector2Int.right);
        }
        return directionList;
    }
}

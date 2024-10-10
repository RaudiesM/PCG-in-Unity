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

    [SerializeField] private int roomMin;
    [SerializeField] private int roomMax;
    [SerializeField] private int hallwayLengthMin;
    [SerializeField] private int hallwayLengthMax;

    private List<Vector2Int> tileList = new List<Vector2Int>();
    private Stack<Vector2Int> safePositions = new Stack<Vector2Int>();
    private Vector2Int startPosition;
    public HashSet<Vector2Int> GetDungeonTiles()
    {
        tileList.Clear();
        startPosition = new Vector2Int(Mathf.FloorToInt(fieldSize.center.x), Mathf.FloorToInt(fieldSize.center.y));
        if(currentType == DungeonType.Caverns)
        {
            BaseRandomWalker();
        }else if(currentType == DungeonType.Rooms)
        {
            RoomWalker();
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
        SetRoom(curPos);
        while (tileList.Count < maxTiles)
        {
            curPos = SetHallway(curPos);
            int rollForRoom = Random.Range(1, 101);
            if (rollForRoom <= roomSpawnRate)
            {
                SetRoom(curPos);
            }
        }
    }

    private void SetRoom(Vector2Int curPos)
    {
        int newYMax = GetMaxSize(curPos, Vector2Int.up);
        int newXMax = GetMaxSize(curPos, Vector2Int.left);

        int heightMax = newYMax < roomMax ? newYMax : roomMax;
        int weightMax = newXMax < roomMax ? newXMax : roomMax;


        int height = Random.Range(roomMin, heightMax);
        int width = Random.Range(roomMin, weightMax);

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

    private int GetMaxSize(Vector2Int curPos, Vector2Int direction)
    {
        int distance = GetDistanceToBorder(curPos, direction);
        int oppositeDistance = GetDistanceToBorder(curPos, -direction);
        int maxValue = oppositeDistance < distance ? oppositeDistance : distance;
        return maxValue;
    }

    private Vector2Int SetHallway(Vector2Int curPos)
    {
        bool moreThenOneOption = false;
        Vector2Int walkDir = RandomDirection(curPos, hallwayLengthMin, out moreThenOneOption);
        while(walkDir == Vector2Int.zero && safePositions.Count > 0)
        {
            curPos = safePositions.Pop();
            walkDir = RandomDirection(curPos, hallwayLengthMin + 2* roomMin, out moreThenOneOption);
        }
        if (moreThenOneOption)
        {
            safePositions.Push(curPos);
        }
        int distanceToBorder = GetDistanceToBorder(curPos, walkDir);
        int newMax = hallwayLengthMax < distanceToBorder ? hallwayLengthMax : distanceToBorder;
        int walkLength = Random.Range(hallwayLengthMin, newMax - roomMin);
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

    private int GetDistanceToBorder(Vector2Int curPos, Vector2Int walkDir)
    {
        int newDistance = 0;
        if(walkDir == Vector2Int.down)
        {
            newDistance = curPos.y - fieldSize.yMin;
        }else if(walkDir == Vector2Int.up)
        {
            newDistance = fieldSize.yMax - curPos.y;
        }
        else if (walkDir == Vector2Int.left)
        {
            newDistance = curPos.x - fieldSize.xMin;
        }
        else if(walkDir == Vector2Int.right)
        {
            newDistance = fieldSize.xMax - curPos.x;
        }
        return newDistance;
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

    private Vector2Int RandomDirection(Vector2Int pos, int length, out bool moreThenOneOption)
    {
        List<Vector2Int> directionList = GetListOfDirections(pos, length);
        moreThenOneOption = directionList.Count > 1;
        if (directionList.Count > 0)
        {
            int randNumb = Random.Range(0, directionList.Count);

            return directionList[randNumb];
        }
        return Vector2Int.zero;
    }

    private List<Vector2Int> GetListOfDirections(Vector2Int pos, int length = 0)
    {
        List<Vector2Int> directionList = new List<Vector2Int>();
        if (pos.y + length < fieldSize.yMax-1)
        {
            directionList.Add(Vector2Int.up);
        }
        if (pos.y - length > fieldSize.yMin)
        {
            directionList.Add(Vector2Int.down);
        }
        if (pos.x + length < fieldSize.xMax-1)
        {
            directionList.Add(Vector2Int.right);
        }
        if (pos.x - length > fieldSize.xMin)
        {
            directionList.Add(Vector2Int.left);
        }
        return directionList;
    }
}

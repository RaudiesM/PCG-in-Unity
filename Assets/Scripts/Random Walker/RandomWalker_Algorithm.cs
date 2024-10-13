using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


public class RandomWalker_Algorithm : DungeonAlgorithmBase
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

    private DungeonTiles tileList = new DungeonTiles(AlgorithmType.RandomWalker);
    private HashSet<Bounds> roomList = new HashSet<Bounds>();
    private Stack<Vector2Int> safePositions = new Stack<Vector2Int>();
    private Vector2Int currentPosition;

    public override DungeonTiles GenerateDungeonTiles()
    {
        tileList.Clear();
        roomList.Clear();
        currentPosition = new Vector2Int(Mathf.FloorToInt(fieldSize.center.x), Mathf.FloorToInt(fieldSize.center.y));

        int safetyCheck = 0;

        if (currentType == DungeonType.Caverns)
        {
            while (tileList.Count() < maxTiles && safetyCheck < 1000)
            {
                int tileCount = tileList.Count();

                BaseRandomWalker();

                if (tileCount == tileList.Count())
                {
                    safetyCheck++;
                }
                else
                {
                    safetyCheck = 0;
                }
            }
        }
        else if(currentType == DungeonType.Rooms)
        {
            while (tileList.Count() < maxTiles && safetyCheck < 1000)
            {
                int tileCount = tileList.Count();
                
                RoomWalker();
                
                if (tileCount == tileList.Count())
                {
                    safetyCheck++;
                }
                else
                {
                    safetyCheck = 0;
                }
            }
        }
        return tileList;
    }

    public override DungeonTiles SetUpGeneration()
    {
        tileList.Clear();
        roomList.Clear();
        currentPosition = new Vector2Int(Mathf.FloorToInt(fieldSize.center.x), Mathf.FloorToInt(fieldSize.center.y));

        if (currentType == DungeonType.Caverns)
        {
            BaseRandomWalker();
        }
        else if (currentType == DungeonType.Rooms)
        {
            RoomWalker();
        }
        return tileList;
    }

    public override DungeonTiles ContinueIterating()
    {
        if (tileList.Count() >= maxTiles)
            return tileList;

        if (currentType == DungeonType.Caverns)
        {
            BaseRandomWalker();
        }
        else if (currentType == DungeonType.Rooms)
        {
            RoomWalker();
        }
        return tileList;
    }

    private void BaseRandomWalker()
    {
        tileList.AddCorridorTile(currentPosition);
        currentPosition += RandomDirection(currentPosition);
        tileList.AddCorridorTile(currentPosition);
    }

    private void RoomWalker()
    {
        HashSet<Vector2Int> hallwayTiles = new HashSet<Vector2Int>();
        HashSet<Vector2Int> roomTiles = new HashSet<Vector2Int>();

        
        tileList.AddCorridorTile(currentPosition);
        tileList.AddRoom(SetRoom(currentPosition));
        hallwayTiles = SetHallway(currentPosition, out currentPosition);
        int rollForRoom = Random.Range(1, 101);
        if (rollForRoom <= roomSpawnRate)
        {
            roomTiles = SetRoom(currentPosition);
            if(roomTiles.Count == 0 && safePositions.Count >= 0)
            {
                Debug.Log("Whats going on");
                currentPosition = safePositions.Pop();
            }
            else
            {
                tileList.AddRoom(roomTiles);
                tileList.AddCorridor(hallwayTiles);
            }
        }
    }

    private HashSet<Vector2Int> SetRoom(Vector2Int curPos)
    {
        HashSet<Vector2Int> roomTiles = new HashSet<Vector2Int>();

        int newYMax = GetMaxSize(curPos, Vector2Int.up);
        int newXMax = GetMaxSize(curPos, Vector2Int.left);

        //Debug.Log($"My Max Size {newXMax}/{newYMax}");
        if(newYMax <= 0 || newXMax <= 0)
        {
            return new HashSet<Vector2Int>();
        }
        int heightMax = newYMax < roomMax ? newYMax : roomMax;
        int weightMax = newXMax < roomMax ? newXMax : roomMax;

        int height = Random.Range(roomMin, heightMax);
        int width = Random.Range(roomMin, weightMax);

        Bounds newRoom = new Bounds(curPos+new Vector2(0.5f, 0.5f), new Vector2(2*width+1, 2*height + 1));
        roomList.Add(newRoom);
        //Debug.Log("<color=green>NewRoom:</color> "+newRoom);
        for (int w = -width; w <= width; w++)
        {
            for (int h = -height; h <= height; h++)
            {
                Vector2Int offset = new Vector2Int(w, h);
                if (!roomTiles.Contains(curPos + offset))
                {
                    roomTiles.Add(curPos + offset);
                }

            }
        }
        return roomTiles;
    }

    private int GetMaxSize(Vector2Int curPos, Vector2Int direction)
    {
        int maxValue = 0;
        maxValue = GetDistanceToBorder(curPos, direction);
        
        int oppositeDistance = GetDistanceToBorder(curPos, -direction);
        int distanceToRooms = GetDistanceToRooms(curPos, direction);
        if(distanceToRooms == 0)
        {
            Debug.Log("Overlapping");
        }
        maxValue = oppositeDistance < maxValue ? oppositeDistance : maxValue;
        maxValue = distanceToRooms < maxValue ? distanceToRooms : maxValue;

        return maxValue;
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
                newRoomSize = new Vector2(3, i + 0.5f);
            }
            else if (direction == Vector2.left || direction == Vector2.right)
            {
                newRoomSize = new Vector2(i + 0.5f, 3);
            }
            Bounds newRoom = new Bounds(newPosition, newRoomSize);
            
            foreach (Bounds room in roomList)
            {
                if (newRoom.Intersects(room))
                {
                    return (maxSize - 2);
                }
            }
        }
        return maxSize;
    }

    private HashSet<Vector2Int> SetHallway(Vector2Int curPos, out Vector2Int returnCurPos)
    {
        bool moreThenOneOption = false;
        HashSet<Vector2Int> hallwayTiles = new HashSet<Vector2Int>();
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
            if (!hallwayTiles.Contains(curPos))
            {
                hallwayTiles.Add(curPos);
            }
        }
        returnCurPos = curPos;
        return hallwayTiles;
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

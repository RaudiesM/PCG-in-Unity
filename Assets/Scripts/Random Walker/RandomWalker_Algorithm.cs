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
    [SerializeField] private bool isLookingAhead;

    [SerializeField] private int roomMin;
    [SerializeField] private int roomMax;
    [SerializeField] private int hallwayLengthMin;
    [SerializeField] private int hallwayLengthMax;

    private DungeonTiles dungeonTiles = new DungeonTiles(AlgorithmType.RandomWalker);
    private HashSet<Bounds> roomList = new HashSet<Bounds>();
    private HashSet<Vector2Int> tileList = new HashSet<Vector2Int>();
    private Stack<Vector2Int> safePositions = new Stack<Vector2Int>();
    private Vector2Int currentPosition;
    private bool firstGeneration = false;

    #region Generation Methods
    public override DungeonTiles GenerateDungeonTiles()
    {
        ReadyGenerartion();

        int safetyCheck = 0;

        if (currentType == DungeonType.Caverns)
        {
            while (dungeonTiles.Count() < maxTiles && safetyCheck < 1000)
            {
                int tileCount = dungeonTiles.Count();

                BaseRandomWalker();

                if (tileCount == dungeonTiles.Count())
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
            firstGeneration = true;
            while (dungeonTiles.Count() < maxTiles && safetyCheck < 1000)
            {
                int tileCount = dungeonTiles.Count();
                
                RoomWalker();
                
                if (tileCount == dungeonTiles.Count())
                {
                    safetyCheck++;
                }
                else
                {
                    safetyCheck = 0;
                }
            }
        }
        return dungeonTiles;
    }

    public override DungeonTiles SetUpGeneration()
    {
        ReadyGenerartion();

        if (currentType == DungeonType.Caverns)
        {
            BaseRandomWalker();
        }
        else if (currentType == DungeonType.Rooms)
        {
            firstGeneration = true;
            RoomWalker();
        }
        return dungeonTiles;
    }

    private void ReadyGenerartion()
    {
        dungeonTiles.Clear();
        roomList.Clear();
        safePositions.Clear();
        tileList.Clear();
        //int randomX = Random.Range(fieldSize.xMin + roomMin, fieldSize.xMax - roomMin);
        //int randomY = Random.Range(fieldSize.yMin + roomMin, fieldSize.yMax - roomMin);
        int randomX = (int) fieldSize.center.x;
        int randomY = (int)fieldSize.center.y;
        currentPosition = new Vector2Int(randomX, randomY);
    }

    public override DungeonTiles ContinueIterating()
    {
        if (dungeonTiles.Count() >= maxTiles)
            return dungeonTiles;

        if (currentType == DungeonType.Caverns)
        {
            BaseRandomWalker();
        }
        else if (currentType == DungeonType.Rooms)
        {
            RoomWalker();
        }
        return dungeonTiles;
    }
    #endregion

    private void BaseRandomWalker()
    {
        dungeonTiles.AddCorridorTile(currentPosition);
        currentPosition += RandomDirection(currentPosition);
        dungeonTiles.AddCorridorTile(currentPosition);
    }

    private void RoomWalker()
    {
        HashSet<Vector2Int> hallwayTiles = new HashSet<Vector2Int>();
        HashSet<Vector2Int> roomTiles = new HashSet<Vector2Int>();
        
        if (firstGeneration)
        {
            dungeonTiles.AddCorridorTile(currentPosition);
            dungeonTiles.AddRoom(SetRoom(currentPosition));
            firstGeneration = false;
            safePositions.Push(currentPosition);
        }

        HashSet<PossiblePath> possiblePaths = GetPossiblePaths(currentPosition);
        while (possiblePaths.Count == 0 && safePositions.Count > 0)
        {
            currentPosition = safePositions.Pop();
            possiblePaths = GetPossiblePaths(currentPosition);
        }
        if (possiblePaths.Count > 1)
        {
            safePositions.Push(currentPosition);
        }

        PossiblePath currentPath = RandomPath(possiblePaths);
        hallwayTiles = SetHallway(currentPosition, currentPath, out currentPosition);

        int rollForRoom = Random.Range(1, 101);
        if (rollForRoom <= roomSpawnRate && currentPath.CanPlaceRoom())
        {
            roomTiles = SetRoom(currentPosition, currentPath);
            dungeonTiles.AddRoom(roomTiles);
        }
        dungeonTiles.AddCorridor(hallwayTiles);
    }

    private HashSet<PossiblePath> GetPossiblePaths(Vector2Int currentPosition)
    {
        HashSet<PossiblePath> possiblePaths = new HashSet<PossiblePath>();
        HashSet<Vector2Int> directionList = GetPossibleDirections(currentPosition);
        
        foreach(Vector2Int direction in directionList)
        {
            bool canPlaceRoom = false;
            int xSize = 0;
            int ySize = 0;
            int length = 0;
            int sizeAlongDirection = 0;
          
            int maxDistance = GetMaxDistance(currentPosition, direction, hallwayLengthMin, hallwayLengthMax+roomMax);
            if(maxDistance < hallwayLengthMin)
            {
                continue;
            }

            if (maxDistance >= hallwayLengthMin + roomMin)
            {
                canPlaceRoom = true;
                Vector2Int curPos = currentPosition + (direction * hallwayLengthMin);

                int yUp = GetMaxDistance(curPos, Vector2Int.up, roomMin, roomMax);
                int yDown = GetMaxDistance(curPos, Vector2Int.down, roomMin, roomMax);

                int xLeft = GetMaxDistance(curPos, Vector2Int.left, roomMin, roomMax);
                int xRight = GetMaxDistance(curPos, Vector2Int.right, roomMin, roomMax);

                int yMax = yDown < yUp ? yDown : yUp;
                int xMax = xLeft < xRight ? xLeft : xRight;
                //Debug.DrawLine(new Vector3(curPos.x, curPos.y - yDown), new Vector3(curPos.x, curPos.y+yUp), Color.red, 5);
                //Debug.DrawLine(new Vector3(curPos.x -xLeft, curPos.y), new Vector3(curPos.x + xRight, curPos.y), Color.blue, 5);
                if (yMax + xMax < roomMin * 2)
                {
                    continue;
                }
                ySize = Random.Range(roomMin, yMax + 1);
                xSize = Random.Range(roomMin, xMax + 1);

                if(direction == Vector2.up || direction == Vector2.down)
                {
                    sizeAlongDirection = ySize;
                }else if(direction == Vector2.right || direction == Vector2.left)
                {
                    sizeAlongDirection = xSize;
                }
                
            }

            int newMaxDistance = maxDistance - sizeAlongDirection;
            length = newMaxDistance < hallwayLengthMax ? newMaxDistance : hallwayLengthMax;

            PossiblePath newPossiblePath = new PossiblePath(direction, length, canPlaceRoom, xSize, ySize);
            possiblePaths.Add(newPossiblePath);
            
        }
        return possiblePaths;
    }
    private HashSet<Vector2Int> SetRoom(Vector2Int curPos)
    {
        HashSet<Vector2Int> roomTiles = new HashSet<Vector2Int>();

        int newYMax = GetMaxSize(curPos, Vector2Int.up);
        int newXMax = GetMaxSize(curPos, Vector2Int.left);

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
    private HashSet<Vector2Int> SetRoom(Vector2Int curPos, PossiblePath currentPath)
    {
        HashSet<Vector2Int> roomTiles = new HashSet<Vector2Int>();
        currentPath.SetRoomPosition(curPos);
        Bounds newRoom = currentPath.GetRoom();
        int width = (int) newRoom.size.x;
        int height = (int) newRoom.size.y;

        roomList.Add(newRoom);
        for (int w = -width; w <= width; w++)
        {
            for (int h = -height; h <= height; h++)
            {
                Vector2Int offset = new Vector2Int(w, h);
                roomTiles.Add(curPos + offset);
            }
        }
        tileList.UnionWith(roomTiles);
        return roomTiles;
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
//        int distanceToCorridor = GetDistanceToCorridor(curPos, walkDir);
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
    private HashSet<Vector2Int> SetHallway(Vector2Int position, PossiblePath currentPath, out Vector2Int returnCurPos)
    {
        HashSet<Vector2Int> hallwayTiles = new HashSet<Vector2Int>();
        Vector2Int curPos = position;
        Vector2Int walkDirection = currentPath.GetDirection();
        int distance = currentPath.GetDistance();

        for (int i = 0; i < distance; i++)
        {
            curPos += currentPath.GetDirection();
            hallwayTiles.Add(curPos);
        }
        returnCurPos = curPos;
        tileList.UnionWith(hallwayTiles);
        return hallwayTiles;
    }

    private int GetMaxSize(Vector2Int curPos, Vector2Int direction)
    {
        int maxValue = 0;
        int distanceToBorder = GetDistanceToBorder(curPos, direction);
        int oppositeDistance = GetDistanceToBorder(curPos, -direction);
        int distanceToRooms = GetDistanceToRooms(curPos, direction);
        maxValue = oppositeDistance < distanceToBorder ? oppositeDistance : distanceToBorder;
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
        return newDistance-=1;
    }
    private int GetMaxDistance(Vector2Int curPos, Vector2Int walkDir, int minValue = 0, int maxValue = 0)
    {
        int distanceToBorder = GetDistanceToBorder(curPos, walkDir);
        int startPosition = 1;
        if(distanceToBorder < minValue)
        {
            return 0;
        }
        else
        {
            maxValue = distanceToBorder < maxValue ? distanceToBorder : maxValue;
        }

        if (minValue != roomMin || maxValue != roomMax)
        {
            Vector3 roomSize = GetRoom(curPos).size;
            startPosition = (int) Mathf.Abs((roomSize.x * walkDir.x)+(roomSize.y * walkDir.y)) + 1;
            //UtilityFunctions.MarkPosition(newPosition, Color.red, 5);
        }
        

        Vector2Int newPosition = new Vector2Int();        
        for(int i = startPosition; i <= maxValue; i++)
        {
           newPosition = curPos + (walkDir * i);
            if(minValue == roomMin && maxValue == roomMax)
            {
                //UtilityFunctions.MarkPosition(newPosition, Color.red, 5);
            }
            if (tileList.Contains(newPosition))
            {
                UtilityFunctions.MarkPosition(newPosition, Color.red);
               if(i <= minValue)
               {
                   return 0;
               }
               else
               {
                   return i--;
               }
            }

        }
        return maxValue--;
    }

    private Vector2Int RandomDirection(Vector2Int pos)
    {
        HashSet<Vector2Int> directionList = GetPossibleDirections(pos);
        if (directionList.Count > 0)
        {
            int randNumb = Random.Range(0, directionList.Count);
            List<Vector2Int> resultList = directionList.ToList();
            return resultList[randNumb];
        }
        return Vector2Int.zero;
    }
    private Vector2Int RandomDirection(Vector2Int pos, int length, out bool moreThenOneOption)
    {
        HashSet<Vector2Int> directionList = GetPossibleDirections(pos, length);
        moreThenOneOption = directionList.Count > 1;
        if (directionList.Count > 0)
        {
            int randNumb = Random.Range(0, directionList.Count);
            List<Vector2Int> resultList = directionList.ToList();
            return resultList[randNumb];
        }
        return Vector2Int.zero;
    }
    private HashSet<Vector2Int> GetPossibleDirections(Vector2Int pos, int length = 1)
    {
        HashSet<Vector2Int> directionList = new HashSet<Vector2Int>();
        if (pos.y + length < fieldSize.yMax-1)
        {
            directionList.Add(Vector2Int.up);
        }
        if (pos.y - length > fieldSize.yMin)
        {
            directionList.Add(Vector2Int.down);
        }
        if (pos.x + length < fieldSize.xMax - 1)
        {
            directionList.Add(Vector2Int.right);
        }
        if (pos.x - length > fieldSize.xMin)
        {
            directionList.Add(Vector2Int.left);
        }
        return directionList;
    }
    private Bounds GetRoom(Vector2Int pos)
    {
        Bounds newRoom = new Bounds();
        Vector3 position = new Vector3(pos.x +0.5f, pos.y+0.5f);
        foreach(var room in roomList)
        {
            if(room.center == position)
            {
                newRoom = room;
            }
        }
        return newRoom;
    }
    private PossiblePath RandomPath(HashSet<PossiblePath> possiblePaths)
    {
        List<PossiblePath> paths = possiblePaths.ToList();
        if (possiblePaths.Count > 1)
        {
            int randomID = Random.Range(0, possiblePaths.Count);
            return paths[randomID];
        }
        return paths[0];
    }

}

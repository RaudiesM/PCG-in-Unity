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

    private DungeonTiles dungeonTiles = new DungeonTiles(AlgorithmType.RandomWalker);
    private Stack<Vector2Int> safePositions = new Stack<Vector2Int>();
    private Vector2Int currentPosition;
    private bool firstGeneration = false;

    #region Generation Methods
    public override DungeonTiles GenerateDungeonTiles()
    {
        dungeonTiles.Clear();
        currentPosition = new Vector2Int(Mathf.FloorToInt(fieldSize.center.x), Mathf.FloorToInt(fieldSize.center.y));

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
        dungeonTiles.Clear();
        currentPosition = new Vector2Int(Mathf.FloorToInt(fieldSize.center.x), Mathf.FloorToInt(fieldSize.center.y));

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
        }

        hallwayTiles = SetHallway(currentPosition, out currentPosition);
        int rollForRoom = Random.Range(1, 101);
        if (rollForRoom <= roomSpawnRate)
        {
            roomTiles = SetRoom(currentPosition);
            dungeonTiles.AddRoom(roomTiles);
        } 
        dungeonTiles.AddCorridor(hallwayTiles);
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

        for (int w = -width; w <= width; w++)
        {
            for (int h = -height; h <= height; h++)
            {
                Vector2Int offset = new Vector2Int(w, h);
                roomTiles.Add(curPos + offset);
            }
        }
        return roomTiles;
    }

    private int GetMaxSize(Vector2Int curPos, Vector2Int direction)
    {
        int maxValue = 0;
        int distance = GetDistanceToBorder(curPos, direction);
        int oppositeDistance = GetDistanceToBorder(curPos, -direction);
        
        maxValue = distance < oppositeDistance ? distance : oppositeDistance;
        return maxValue;
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
        List<Vector2Int> directionList = GetPossibleDirections(pos);
        if (directionList.Count > 0)
        {
            int randNumb = Random.Range(0, directionList.Count);

            return directionList[randNumb];
        }
        return Vector2Int.zero;
    }

    private Vector2Int RandomDirection(Vector2Int pos, int length, out bool moreThenOneOption)
    {
        List<Vector2Int> directionList = GetPossibleDirections(pos, length);
        moreThenOneOption = directionList.Count > 1;
        if (directionList.Count > 0)
        {
            int randNumb = Random.Range(0, directionList.Count);

            return directionList[randNumb];
        }
        return Vector2Int.zero;
    }

    private List<Vector2Int> GetPossibleDirections(Vector2Int pos, int length = 0)
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
}

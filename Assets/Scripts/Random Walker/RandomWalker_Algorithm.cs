using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


public class RandomWalker_Algorithm : DungeonAlgorithmBase
{
    [SerializeField] private DungeonType currentType;
    [SerializeField] private BoundsInt fieldSize;
    [Range(0, 1)]
    [SerializeField] private float fillPercentage = 0.5f;
    [Range(0, 1)]
    [SerializeField] private float baseRoomSpawnRate;
    [Range(0.01f, 0.1f)]
    [SerializeField] private float increaseSpawnRate;
    [SerializeField] private bool isCombiningRooms = true;
    [SerializeField] private int roomMin;
    [SerializeField] private int roomMax;
    [SerializeField] private int hallwayLengthMin;
    [SerializeField] private int hallwayLengthMax;

    private int dungeonSizeInTiles;
    private DungeonTiles dungeonTiles = new DungeonTiles(AlgorithmType.RandomWalker);
    private Stack<Vector2Int> safePositions = new Stack<Vector2Int>();
    private Vector2Int currentPosition;
    private bool firstGeneration = false;
    private float currentRoomSpawnRate;

    public override EvaluationBase GetAlgorithmData()
    {
        EvaluationBase evaluationData = new RW_Evaluation
            (
                fieldSize.size.x * fieldSize.size.y, 
                Mathf.RoundToInt(fillPercentage * 100), 
                currentType, 
                Mathf.RoundToInt(baseRoomSpawnRate*100), 
                isCombiningRooms
             );
        return evaluationData;
    }

    

    #region Generation Methods
    public override DungeonTiles GenerateDungeonTiles()
    {
        PrepareGeneration();

        int safetyCheck = 0;

        if (currentType == DungeonType.Caverns)
        {
            while (dungeonTiles.Count() < dungeonSizeInTiles && safetyCheck < 1000)
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
        else if (currentType == DungeonType.Rooms)
        {
            firstGeneration = true;
            while (dungeonTiles.Count() < dungeonSizeInTiles && safetyCheck < 1000)
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

        if(isCombiningRooms) 
            dungeonTiles = ReorganiseDungeonRooms();

        return dungeonTiles;
    }

    private void PrepareGeneration()
    {
        dungeonTiles.Clear();
        GetMaxFillTiles();
        currentRoomSpawnRate = baseRoomSpawnRate;
        currentPosition = GetRandomStartPosition();
    }

    private Vector2Int GetRandomStartPosition()
    {
        int randomY = Random.Range(fieldSize.yMin + 1, fieldSize.yMax);
        int randomX = Random.Range(fieldSize.xMin + 1, fieldSize.xMax);
        return new Vector2Int(randomX, randomY);
    }

    public override DungeonTiles SetUpGeneration()
    {
        PrepareGeneration();
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
        if (dungeonTiles.Count() >= dungeonSizeInTiles)
            return dungeonTiles;

        if (currentType == DungeonType.Caverns)
        {
            BaseRandomWalker();
        }
        else if (currentType == DungeonType.Rooms)
        {
            RoomWalker();
        }

        if(isCombiningRooms)
            dungeonTiles = ReorganiseDungeonRooms();
        
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
        if (isSpawningRoom())
        {
            roomTiles = SetRoom(currentPosition);
            dungeonTiles.AddRoom(roomTiles);
        } 
        dungeonTiles.AddCorridor(hallwayTiles);
    }

    private bool isSpawningRoom()
    {
        if(Random.value <= currentRoomSpawnRate)
        {
            currentRoomSpawnRate = baseRoomSpawnRate;
            return true;
        }
        else
        {
            currentRoomSpawnRate += increaseSpawnRate;
        }
        return false;
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

    private DungeonTiles ReorganiseDungeonRooms()
    {
        DungeonTiles newDungeonTiles = new DungeonTiles(AlgorithmType.RandomWalker);
        HashSet<Vector2Int> corridorTiles = new HashSet<Vector2Int>();
        dungeonTiles.TryGetCorridors(out corridorTiles);
        newDungeonTiles.SetCorridor(corridorTiles);
        HashSet<DungeonRoom> dungeonRoom = new HashSet<DungeonRoom>();
        HashSet<Vector2Int> tiles = new HashSet<Vector2Int>();
        HashSet<Vector2Int> checkedTiles = new HashSet<Vector2Int>();

        dungeonTiles.TryGetRooms(out dungeonRoom);
        foreach (DungeonRoom room in dungeonRoom)
        {
            tiles.UnionWith(room.GetRoomTiles());
        }

        foreach (var tile in tiles)
        {
            if (checkedTiles.Contains(tile))
                continue;

            HashSet<Vector2Int> currentTiles = new HashSet<Vector2Int>();
            Vector2Int currentPosition = tile;
            Queue<Vector2Int> lastSafePoints = new Queue<Vector2Int>();
            bool isChecking = true;
            int safetyCheck = 0;
            while (isChecking && safetyCheck <= 1000000)
            {
                safetyCheck++;
                bool neighbourIsSet = false;
                int possibleNeighbours = 0;
                Vector2Int lastSafePoint = currentPosition;
                foreach (var neighbour in UtilityFunctions.GetNeighbourCell(currentPosition))
                {
                    if (tiles.Contains(neighbour) && currentTiles.Contains(neighbour) == false)
                    {
                        possibleNeighbours++;
                        if (neighbourIsSet == false)
                        {
                            currentPosition = neighbour;
                            neighbourIsSet = true;
                        }
                    }
                }
                if (possibleNeighbours > 1)
                {
                    lastSafePoints.Enqueue(lastSafePoint);
                }
                if (currentPosition == lastSafePoint && lastSafePoints.Count > 0)
                {
                    currentPosition = lastSafePoints.Dequeue();
                }
                else if (lastSafePoints.Count == 0)
                {
                    isChecking = false;
                }

                currentTiles.Add(currentPosition);
            }
            checkedTiles.UnionWith(currentTiles);
            
            newDungeonTiles.AddRoom(new DungeonRoom(currentTiles));
        }
        return newDungeonTiles;
    }
    private void GetMaxFillTiles()
    {
        int maxTiles = fieldSize.size.x * fieldSize.size.y;
        dungeonSizeInTiles = Mathf.FloorToInt(maxTiles * fillPercentage);
    }
}

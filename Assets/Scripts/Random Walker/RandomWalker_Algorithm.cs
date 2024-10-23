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
    
    [Header("Room Generation")]
    [Range(0, 1)]
    [SerializeField] private float baseRoomSpawnRate;
    [Range(0.01f, 0.1f)]
    [SerializeField] private float increaseSpawnRate;
    [SerializeField] private bool isCombiningRooms = true;
    
    [Header("Measurements")]
    [SerializeField] private int roomMin;
    [SerializeField] private int roomMax;
    
    [Space]
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
                currentType.ToString(), 
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
            while (dungeonTiles.Count() < dungeonSizeInTiles)
            {
                int tileCount = dungeonTiles.Count();

                BaseRandomWalker();

                if (tileCount == dungeonTiles.Count())
                {
                    safetyCheck++;
                 
                    if (safetyCheck >= 1000 * fieldSize.size.x * fieldSize.size.y)
                        break;
                }
            }
        }
        else if (currentType == DungeonType.Rooms)
        {
            firstGeneration = true;
            while (dungeonTiles.Count() < dungeonSizeInTiles)
            {
                int tileCount = dungeonTiles.Count();

                RoomWalker();

                if (tileCount == dungeonTiles.Count())
                {
                    safetyCheck++;

                    if(safetyCheck >= 1000 * fieldSize.size.x * fieldSize.size.y)
                        break;
                    
                }
            }
        }

        if(isCombiningRooms) 
            dungeonTiles = CombineOverlappingRooms();

        return dungeonTiles;
    }
    public override DungeonTiles FirstGeneration()
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
            dungeonTiles = CombineOverlappingRooms();
        
        return dungeonTiles;
    }
    #endregion

    #region RandomWalkerVariants
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
            roomTiles = SetRoom(currentPosition);
            dungeonTiles.AddRoom(roomTiles);
            firstGeneration = false;
            return;
        }

        hallwayTiles = SetHallway(ref currentPosition);
        if (isSpawningRoom())
        {
            roomTiles = SetRoom(currentPosition);
            dungeonTiles.AddRoom(roomTiles);
        } 
        dungeonTiles.AddCorridor(hallwayTiles);
    }
    #endregion
    #region Set
    private HashSet<Vector2Int> SetRoom(Vector2Int curPos)
    {
        HashSet<Vector2Int> roomTiles = new HashSet<Vector2Int>();

        int newYMax = GetMaxDistanceToBorder(curPos, Vector2Int.up);
        int newXMax = GetMaxDistanceToBorder(curPos, Vector2Int.left);

        if(newYMax <= roomMin || newXMax <= roomMin )
        {
            //if there is no space to place Room, return
            return new HashSet<Vector2Int>();
        }
        //check if distance to Border is smaller than max room size
        int heightMax = newYMax < roomMax ? newYMax : roomMax;
        int weightMax = newXMax < roomMax ? newXMax : roomMax;

        int height = Random.Range(roomMin, heightMax);
        int width = Random.Range(roomMin, weightMax);

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
    private HashSet<Vector2Int> SetHallway(ref Vector2Int curPos)
    {
        bool moreThenOneOption = false;
        HashSet<Vector2Int> hallwayTiles = new HashSet<Vector2Int>();
        Vector2Int walkDir = RandomDirection(curPos, hallwayLengthMin, out moreThenOneOption);

        while (walkDir == Vector2Int.zero)
        {
            //iterate till there the walker finds a direction to walk to
            curPos = safePositions.Pop();
            walkDir = RandomDirection(curPos, hallwayLengthMin + 2 * roomMin, out moreThenOneOption);
            if (safePositions.Count <= 0)
            {
                break;
            }
        }
        if (moreThenOneOption)
        {
            safePositions.Push(curPos);
        }

        int walkLength = GetRandomWalkLength(curPos, walkDir);

        for (int i = 0; i < walkLength; i++)
        {
            curPos += walkDir;
            hallwayTiles.Add(curPos);
        }
        return hallwayTiles;
    }
    #endregion
    #region Get
    private int GetMaxDistanceToBorder(Vector2Int curPos, Vector2Int direction)
    {
        int maxValue = 0;
        int distance = GetDistanceToBorder(curPos, direction);
        int oppositeDistance = GetDistanceToBorder(curPos, -direction);
        
        maxValue = distance < oppositeDistance ? distance : oppositeDistance;
        return maxValue;
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
    private List<Vector2Int> GetPossibleDirections(Vector2Int pos, int length = 0)
    {
        //look in wich direction the walker can go without goint out of bounds
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
    private void GetMaxFillTiles()
    {
        int maxTiles = fieldSize.size.x * fieldSize.size.y;
        dungeonSizeInTiles = Mathf.FloorToInt(maxTiles * fillPercentage);
    }
    #region Get Random
    private int GetRandomWalkLength(Vector2Int curPos, Vector2Int walkDir)
    {
        int distanceToBorder = GetDistanceToBorder(curPos, walkDir);
        int newMax = hallwayLengthMax < distanceToBorder ? hallwayLengthMax : distanceToBorder;
        int walkLength = Random.Range(hallwayLengthMin, newMax - roomMin);
        return walkLength;
    }
    private Vector2Int GetRandomStartPosition()
    {
        //adding one to not start next to the border
        int randomY = Random.Range(fieldSize.yMin + 1, fieldSize.yMax);
        int randomX = Random.Range(fieldSize.xMin + 1, fieldSize.xMax);
        return new Vector2Int(randomX, randomY);
    }
    private Vector2Int RandomDirection(Vector2Int pos)
    {
        List<Vector2Int> directionList = GetPossibleDirections(pos);
        return ChooseDirection(directionList);
    }
    private Vector2Int RandomDirection(Vector2Int pos, int length, out bool moreThenOneOption)
    {
        List<Vector2Int> directionList = GetPossibleDirections(pos, length);
        moreThenOneOption = directionList.Count > 1;
        return ChooseDirection(directionList);
    }
    #endregion
    #endregion
    private static Vector2Int ChooseDirection(List<Vector2Int> directionList)
    {
        if (directionList.Count > 0)
        {
            int randNumb = Random.Range(0, directionList.Count);

            return directionList[randNumb];
        }
        return Vector2Int.zero;
    }
    private DungeonTiles CombineOverlappingRooms()
    {
        DungeonTiles newDungeonTiles = new DungeonTiles(AlgorithmType.RandomWalker);
        HashSet<Vector2Int> corridorTiles = dungeonTiles.GetCorridors();
        newDungeonTiles.SetCorridor(corridorTiles);


        HashSet<Vector2Int> tilesToCheck = new HashSet<Vector2Int>();
        HashSet<Vector2Int> checkedTiles = new HashSet<Vector2Int>();

        tilesToCheck.UnionWith(dungeonTiles.GetDungeonRoomTiles());

        foreach (var tile in tilesToCheck)
        {
            if (checkedTiles.Contains(tile))
                continue;

            HashSet<Vector2Int> currentTiles = new HashSet<Vector2Int>();
            Vector2Int currentPosition = tile;

            currentTiles = UtilityFunctions.GetConnectedTiles(tilesToCheck, currentTiles, ref currentPosition);
            checkedTiles.UnionWith(currentTiles);
            
            newDungeonTiles.AddRoom(new DungeonRoom(currentTiles));
        }
        return newDungeonTiles;
    }
    private void PrepareGeneration()
    {
        dungeonTiles.Clear();
        GetMaxFillTiles();
        currentRoomSpawnRate = baseRoomSpawnRate;
        currentPosition = GetRandomStartPosition();
    }
    private bool isSpawningRoom()
    {
        if(Random.value <= currentRoomSpawnRate)
        {
            currentRoomSpawnRate = baseRoomSpawnRate;
            return true;
        }

        currentRoomSpawnRate += increaseSpawnRate;        
        return false;
    }
}

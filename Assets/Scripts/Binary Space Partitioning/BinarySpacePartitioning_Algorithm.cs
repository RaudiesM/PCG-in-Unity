using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine;

public class BinarySpacePartitioning_Algorithm : DungeonAlgorithmBase 
{
    [SerializeField] private BoundsInt fieldSize = new BoundsInt();
    [Space]
    [SerializeField] private int numRooms;

    [SerializeField] private List<string> indexOptions = new List<string>();

    [Header("Room Measurements")]
    [SerializeField] private int minSize;
    [Range(0, 1)]
    [SerializeField] private float roomPercentage;
    [Space]
    [SerializeField] private int minYSize;
    [SerializeField] private int minXSize;
    [Space]
    [SerializeField] private int offset;

    [Header("Skeletton Room")]
    [SerializeField] private int skelettonRoomSize = 4;

    private Dictionary<string, BoundsInt> dungeonRooms = new Dictionary<string, BoundsInt>();
    private Dictionary<string, RoomPoints> roomsToConnect = new Dictionary<string, RoomPoints>();
    private HashSet<Vector2Int> corridorTiles = new HashSet<Vector2Int>();
    private int currentIndexNum = 0;

    public override EvaluationBase GetAlgorithmData()
    {
        EvaluationBase evaluationData = new BSP_Evaluation(fieldSize.size.x * fieldSize.size.y, numRooms, minSize, skelettonRoomSize);
        return evaluationData;
    }
    private void Start()
    {
        CheckGivenValues();
    }

    #region Generation Methods
    public override DungeonTiles GenerateDungeonTiles()
    {
        ClearDictionaries();
        corridorTiles.Clear();
        DungeonTiles dungeonTiles = new DungeonTiles(AlgorithmType.BinarySpacePartitioning);
        HashSet<DungeonRoom> newDungeonRooms = new HashSet<DungeonRoom>();
        bool isDoneSplitting = false;

        while (isDoneSplitting == false)
        {
            isDoneSplitting = IterateOverRooms();
        }

        HashSet<BoundsInt> newRooms = PlaceRoom();
        newDungeonRooms = UtilityFunctions.ConvertBoundsIntToRooms(newRooms);
        dungeonTiles.AddRooms(newDungeonRooms);
        ConnectRooms(dungeonTiles);
        dungeonTiles.AddCorridor(corridorTiles);

        return dungeonTiles;
    }
    public override DungeonTiles FirstGeneration()
    {
        DungeonTiles newDungeonTiles = new DungeonTiles(AlgorithmType.BinarySpacePartitioning);
        ClearDictionaries();
        HashSet<BoundsInt> newRooms = new HashSet<BoundsInt>();
        HashSet<DungeonRoom> newDungeonRooms = new HashSet<DungeonRoom>();

        IterateOverRooms();
        foreach (var room in dungeonRooms.Values)
        {
            newRooms.Add(room);
        }

        newDungeonRooms = UtilityFunctions.ConvertBoundsIntToRooms(newRooms);
        newDungeonTiles.AddRooms(newDungeonRooms);

        return newDungeonTiles;
    }
    public override DungeonTiles ContinueIterating()
    {
        HashSet<BoundsInt> newRooms = new HashSet<BoundsInt>();
        DungeonTiles dungeonTiles = new DungeonTiles(AlgorithmType.BinarySpacePartitioning);
        HashSet<DungeonRoom> newDungeonRooms = new HashSet<DungeonRoom>();

        bool isDoneSplitting = IterateOverRooms();

        if (isDoneSplitting)
        {
            corridorTiles.Clear();

            newRooms = PlaceRoom();
            newDungeonRooms = UtilityFunctions.ConvertBoundsIntToRooms(newRooms);
            dungeonTiles.AddRooms(newDungeonRooms);
            ConnectRooms(dungeonTiles);
            dungeonTiles.AddCorridor(corridorTiles);
        }
        else
        {
            foreach (var room in dungeonRooms.Values)
            {
                newRooms.Add(room);
            }
            newDungeonRooms = UtilityFunctions.ConvertBoundsIntToRooms(newRooms);
            dungeonTiles.AddRooms(newDungeonRooms);
        }

        return dungeonTiles;
    }
    public DungeonTiles Hybrid_GenerateDungeonTiles(out HashSet<BoundsInt> outDungeonRooms, out HashSet<Vector3Int> newCenter)
    {
        ClearDictionaries();
        corridorTiles.Clear();

        DungeonTiles dungeonTiles = new DungeonTiles(AlgorithmType.BinarySpacePartitioning);

        bool isDoneSplitting = false;
        while (isDoneSplitting == false)
        {
            isDoneSplitting = IterateOverRooms();
        }
        outDungeonRooms = new HashSet<BoundsInt>();
        foreach (var room in dungeonRooms)
        {
            outDungeonRooms.Add(room.Value);
        }

        HashSet<BoundsInt> newDungeonRooms = PlaceRoomSkeletton(out newCenter);
        dungeonTiles.AddRooms(UtilityFunctions.ConvertBoundsIntToRooms(newDungeonRooms));

        ConnectRooms(dungeonTiles);
        dungeonTiles.AddCorridor(corridorTiles);
        return dungeonTiles;
    }
    #endregion
    #region Rooms
    private HashSet<BoundsInt> SplitSpace(BoundsInt room)
    {
        HashSet<BoundsInt> result = new HashSet<BoundsInt>();

        //check if the room can be splitt into two (both sub rooms are atleast minSize
        bool canSplitVertical = (room.size.x >= 2 * minXSize) && (minXSize * room.size.y >= minSize);
        bool canSplitHorizontal = (room.size.y >= 2 * minYSize) && (minYSize * room.size.x >= minSize);
        bool canSplitBoth = canSplitHorizontal && canSplitVertical;

        if (canSplitHorizontal == false && canSplitVertical == false)
            return result;

        BoundsInt newRoomA = new BoundsInt();
        BoundsInt newRoomB = new BoundsInt();

        #region SplitVertical
        if (canSplitBoth && Random.value <= 0.5f || canSplitHorizontal == false && canSplitVertical)
        {
            int randomValueA = GetRandomSize(room.size.x, minXSize);
            int randomValueB = room.size.x - randomValueA;

            newRoomA = new BoundsInt(room.xMin - randomValueB / 2, room.yMin, 0, randomValueA, room.size.y, 0);
            newRoomB = new BoundsInt(room.xMin + randomValueA / 2, room.yMin, 0, randomValueB, room.size.y, 0);

        }
        #endregion
        #region SplitHorizontal
        else if (canSplitHorizontal)
        {
            int randomValueA = GetRandomSize(room.size.y, minYSize);
            int randomValueB = room.size.y - randomValueA;

            newRoomA = new BoundsInt(room.xMin, room.yMin - randomValueB / 2, 0, room.size.x, randomValueA, 0);
            newRoomB = new BoundsInt(room.xMin, room.yMin + randomValueA / 2, 0, room.size.x, randomValueB, 0);
        }

        result.Add(newRoomA);
        result.Add(newRoomB);
        #endregion
        return result;
    }
    private HashSet<BoundsInt> PlaceRoom() 
    {
        HashSet<BoundsInt> dungeonRoomTiles = new HashSet<BoundsInt>();
        Dictionary<string, BoundsInt> newDungeonRooms = new Dictionary<string, BoundsInt>();

        foreach (var rooms in dungeonRooms)
        {
            BoundsInt newRoom = CreateRoomVariance(rooms.Value);

            dungeonRoomTiles.Add(newRoom);
            newDungeonRooms.Add(rooms.Key, newRoom);
            
            roomsToConnect.Add(rooms.Key, new RoomPoints(newRoom.position));
        }
        dungeonRooms = newDungeonRooms;
        return dungeonRoomTiles;
    }
    private HashSet<BoundsInt> PlaceRoomSkeletton(out HashSet<Vector3Int> newCenter)
    {
        HashSet<BoundsInt> dungeonRoomTiles = new HashSet<BoundsInt>();
        newCenter = new HashSet<Vector3Int>();
        Dictionary<string, BoundsInt> newDungeonRooms = new Dictionary<string, BoundsInt>();

        foreach (var rooms in dungeonRooms)
        {
            Vector3Int newSize = new Vector3Int(skelettonRoomSize, skelettonRoomSize);
            Vector3Int variantPosition = VariantPosition(rooms.Value, newSize);
            newCenter.Add(variantPosition);

            BoundsInt newRoom = new BoundsInt(variantPosition, newSize);
            dungeonRoomTiles.Add(newRoom);
            newDungeonRooms.Add(rooms.Key, newRoom);

            roomsToConnect.Add(rooms.Key, new RoomPoints(newRoom.position));
        }

        return dungeonRoomTiles;
    }
    private bool IterateOverRooms()
    {
        int numSplitRooms = 0;
        Queue<string> roomQueue = FillQueueWithRoomsToSplitt();

        while (roomQueue.Count > 0 && dungeonRooms.Count < numRooms)
        {
            string roomIndex = roomQueue.Dequeue();
            HashSet<BoundsInt> newRooms = SplitSpace(dungeonRooms[roomIndex]);

            if(newRooms.Count > 0)
            {
                dungeonRooms.Remove(roomIndex);
                ResetIndexNumber();

                foreach(BoundsInt newRoom in newRooms)
                {
                    string newRoomIndex = GetNewIndex(roomIndex);
                    dungeonRooms.Add(newRoomIndex, newRoom);
                    numSplitRooms++;
                }
            }
        }
        return numSplitRooms == 0;
    }
    private Queue<string> FillQueueWithRoomsToSplitt()
    {
        Queue<string> newQueue = new Queue<string>();
        if (dungeonRooms.Count > 0)
        {
            foreach (var room in dungeonRooms.Keys)
            {
                newQueue.Enqueue(room);
            }
        }
        else
        {
            newQueue.Enqueue(indexOptions[0]);
            dungeonRooms.Add(indexOptions[0], fieldSize);
        }
        return newQueue;
    }

    #region RoomVariance
    private BoundsInt CreateRoomVariance(BoundsInt rooms)
    {
        Vector3Int newSize = VariantSize(rooms);
        Vector3Int newPosition = VariantPosition(rooms, newSize);
        rooms = new BoundsInt(newPosition, newSize);
        return rooms;
    }

    private Vector3Int VariantPosition(BoundsInt rooms, Vector3Int newSize)
    {
        //calculate the possible offset of the new room inside the old room
        int xDiff = rooms.size.x - newSize.x;
        int yDiff = rooms.size.y - newSize.y;

        int xRandom = Random.Range(-xDiff / 2, xDiff / 2);
        int yRandom = Random.Range(-yDiff / 2, yDiff / 2);

        //if the new randomValue is bigger then the offset the new room should have to other rooms. In that case: subtract the offset.
        xRandom -= xRandom > offset ? offset : 0;
        yRandom -= yRandom > offset ? offset : 0;

        return new Vector3Int(rooms.position.x + xRandom, rooms.position.y + yRandom);
    }

    private Vector3Int VariantSize(BoundsInt rooms)
    {
        int xRandom = rooms.size.x;
        int yRandom = rooms.size.y;
        
        if (rooms.size.x > rooms.size.y)
        {
            int potYMin = Mathf.FloorToInt(rooms.size.y * roomPercentage);
            int yMin = potYMin > minYSize ? potYMin : minYSize;
            yRandom = Random.Range(yMin, rooms.size.y);
            
            int xMin = minSize / yRandom > minXSize ? minSize / yRandom : minXSize;
            xRandom = Random.Range(xMin, rooms.size.x);
        }
        else if (rooms.size.y >= rooms.size.x)
        {
            int potXMin = Mathf.FloorToInt(rooms.size.x * roomPercentage);
            int xMin = potXMin > minXSize ? potXMin : minXSize;
            xRandom = Random.Range(xMin, rooms.size.x);

            int yMin = minSize / yRandom > minYSize ? minSize / yRandom : minYSize;
            yRandom = Random.Range(yMin, rooms.size.y);
        }
        yRandom -= offset;
        xRandom -= offset;
        return new Vector3Int(xRandom, yRandom);
    }
    private int GetRandomSize(int roomSize, int minSize)
    {
        int randomValue = Random.Range(minSize, roomSize - minSize);

        if (randomValue % 2 > 0)
        {
            if (randomValue > minSize)
            {
                randomValue += 1;
            }
            else
            {
                randomValue -= 1;
            }
        }

        return randomValue;
    }
    #endregion
    #endregion
    #region Corridors
    private void ConnectRooms(DungeonTiles _dungeonTiles)
    {
        Queue<string> roomIndex = new Queue<string>();
        Queue<string> checkedIDs = new Queue<string>();

        foreach(string index in dungeonRooms.Keys)
        {
            roomIndex.Enqueue(index);
        }
        while (roomIndex.Count > 1)
        {
            string currentID = roomIndex.Dequeue();
            string parentID = currentID.Substring(0, currentID.Length - 1);
            string endIsAorB = currentID.Substring(currentID.Length - 1);
            string siblingID = GetSiblingIndex(parentID, endIsAorB);

            if (roomsToConnect.ContainsKey(siblingID))
            {
                RoomPoints roomPointsA = roomsToConnect[currentID];
                RoomPoints roomPointsB = roomsToConnect[siblingID];

                Vector2Int pointA = new Vector2Int();
                Vector2Int pointB = new Vector2Int();

                if (roomPointsA.GetRoomPoints().Count + roomPointsB.GetRoomPoints().Count > 2)
                {
                    Vector2Int closestPointB = roomsToConnect[siblingID].GetAveragePoint();
                    pointA = roomsToConnect[currentID].GetClosestPoint(closestPointB);
                    pointB = roomsToConnect[siblingID].GetClosestPoint(pointA);
                }
                else
                {
                    pointA = roomPointsA.GetRoomPoints()[0];
                    pointB = roomPointsB.GetRoomPoints()[0];
                }
                
                SetCorridor(pointA, pointB, _dungeonTiles);
                RoomPoints newRoomPoints = new RoomPoints(roomPointsA, roomPointsB);
                roomsToConnect.Add(parentID, newRoomPoints);

                roomsToConnect.Remove(currentID);
                roomsToConnect.Remove(siblingID);

                roomIndex.Enqueue(parentID);
                checkedIDs.Enqueue(currentID);
            }
            else if(checkedIDs.Contains(siblingID) == false)
            {
                roomIndex.Enqueue(currentID);
            }
        }
    }
    private void SetCorridor(Vector2Int pointA, Vector2Int pointB, DungeonTiles _dungeonTiles)
    {
        Vector2Int pointAB = new Vector2Int(pointA.x, pointB.y);
        Vector2Int pointBA = new Vector2Int(pointB.x, pointA.y);

        HashSet<Vector2Int> pathA = new HashSet<Vector2Int>();
        HashSet<Vector2Int> pathB = new HashSet<Vector2Int>();

        HashSet<Vector2Int> corridorList = new HashSet<Vector2Int>();

        pathA = GetCorridorPath(pointA, pointB, pointAB, _dungeonTiles);
        pathB = GetCorridorPath(pointA, pointB, pointBA, _dungeonTiles);

        Debug.ClearDeveloperConsole();

        pathA = (pathA.Count <= 1 && pathB.Count >= 1) ? pathB : pathA;
        pathB = (pathB.Count <= 1 && pathA.Count >= 1) ? pathA : pathB;

        if (pathA.Count == pathB.Count)
        {
            corridorList = Random.value <= 0.5f ? pathA : pathB;
        }
        else
        {
            corridorList = pathA.Count < pathB.Count ? pathA : pathB;
            
        }

        foreach (var  corridor in corridorList)
        {
            corridorTiles.Add(corridor);
        }
    }
    private HashSet<Vector2Int> GetCorridorPath(Vector2Int pointA, Vector2Int pointB, Vector2Int middlePoint, DungeonTiles _dungeonTiles)
    {
        HashSet<Vector2Int> result = new HashSet<Vector2Int>();
        HashSet<Vector2Int> refTiles = new HashSet<Vector2Int>();
        refTiles.UnionWith(corridorTiles);
        HashSet<DungeonRoom> rooms = new HashSet<DungeonRoom>();
        _dungeonTiles.TryGetRooms(out rooms);
        foreach (var room in rooms)
        {
            refTiles.UnionWith(room.GetRoomTiles());
        }

        Vector2Int startPoint = new Vector2Int();
        Vector2Int goalPoint = new Vector2Int();


        if (pointA.x == middlePoint.x)
        {
            startPoint = pointA;
            goalPoint = pointB;
        }
        else if (pointB.x == middlePoint.x)
        {
            startPoint = pointB;
            goalPoint = pointA;
        }

        result.UnionWith(GoAlongAxis(startPoint, startPoint.y, middlePoint, middlePoint.y, refTiles, false));
        result.UnionWith(GoAlongAxis(middlePoint, middlePoint.x, goalPoint, goalPoint.x, refTiles, true));

        return result;
    }
    private HashSet<Vector2Int> GoAlongAxis(Vector2Int vectorA, int valueA, Vector2Int vectorB, int valueB, HashSet<Vector2Int> refTiles, bool alongYAxis)
    {
        Vector2Int startVector = new Vector2Int();
        Vector2Int goalVector = new Vector2Int();

        HashSet<Vector2Int> results = new HashSet<Vector2Int>();
        if (valueA < valueB)
        {
            startVector = vectorA;
            goalVector = vectorB;
        }
        else
        {
            startVector = vectorB;
            goalVector = vectorA;
        }

        if (alongYAxis)
        {
            for (int x = startVector.x; x <= goalVector.x; x++)
            {
                Vector2Int currentPosition = new Vector2Int(x, startVector.y);
                if (refTiles.Contains(currentPosition) == false)
                {
                    results.Add(currentPosition);
                }
            }
        }
        else
        {
            for (int y = startVector.y; y <= goalVector.y; y++)
            {
                Vector2Int currentPosition = new Vector2Int(startVector.x, y);
                if (refTiles.Contains(currentPosition) == false)
                {
                    results.Add(currentPosition);
                }
            }
        }
        return results;
    }
    #endregion
    #region Index
    private string GetSiblingIndex(string parentString, string ownID)
    {
        string siblingIndex = parentString;
        if (ownID == indexOptions[0])
        {
            siblingIndex += indexOptions[1];

        }
        else if (ownID == indexOptions[1])
        {
            siblingIndex += indexOptions[0];
        }
        return siblingIndex;
    }
    private string GetNewIndex(string index)
    {
        string returnString = index;
        if (currentIndexNum > indexOptions.Count)
        {
            currentIndexNum = 0;
            return returnString += "0";
        }

        returnString += indexOptions[currentIndexNum];
        currentIndexNum++;
        return returnString;
    }
    private void ResetIndexNumber()
    {
        currentIndexNum = 0;
    }
    #endregion
    
    
    private void ClearDictionaries()
    {
        dungeonRooms.Clear();
        roomsToConnect.Clear();
    }
    private void CheckGivenValues()
    {
        int dungeonSizeInt = fieldSize.size.x * fieldSize.size.y;
        if (numRooms == 0)
        {
            numRooms = 10;
        }
        if (numRooms * minSize > dungeonSizeInt)
        {
            minSize = dungeonSizeInt / numRooms;
        }
        if (minSize < minXSize * minYSize)
        {
            minXSize = Mathf.FloorToInt((float)(Math.Sqrt(minSize)));
            minYSize = minXSize;
        }

        minXSize += offset;
        minYSize += offset;
        minSize += offset * offset;
    }
}

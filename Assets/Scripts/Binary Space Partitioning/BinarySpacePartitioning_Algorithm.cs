using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine;

public class BinarySpacePartitioning_Algorithm : DungeonAlgorithmBase
{
    [SerializeField] private BoundsInt fieldSize = new BoundsInt();
    [SerializeField] private int numRooms;

    [SerializeField] private int minSize;
    [Range(0, 1)]
    [SerializeField] private float roomPercentage;
    [SerializeField] private int minYSize;
    [SerializeField] private int minXSize;

    [SerializeField] private int boneRoomSize = 4;

    [SerializeField] private int offset;

    private Dictionary<string, BoundsInt> dungeonRooms = new Dictionary<string, BoundsInt>();
    private Dictionary<string, RoomPoints> roomsToConnect = new Dictionary<string, RoomPoints>();
    private HashSet<Vector2Int> corridorTiles = new HashSet<Vector2Int>();
    private int currentIndexNum = 0;

    public override EvaluationBase GetAlgorithmData()
    {
        EvaluationBase evaluationData = new BSP_Evaluation(fieldSize.size.x * fieldSize.size.y, numRooms, minSize, boneRoomSize);
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
        DungeonTiles dungeonTiles = new DungeonTiles(AlgorithmType.BinarySpacepartitioning);

        bool isDoneSplitting = false;
        while (isDoneSplitting == false)
        {
            isDoneSplitting = IterateOverRooms();
        }
        HashSet<BoundsInt> newDungeonRooms = PlaceRoom();
        dungeonTiles.AddRoom(UtilityFunctions.ConvertBoundsIntToRooms(newDungeonRooms));
        corridorTiles.Clear();
        ConnectRooms(dungeonTiles);
        dungeonTiles.AddCorridor(corridorTiles);

        return dungeonTiles;
    }

    public DungeonTiles GenerateDungeonTiles(out HashSet<BoundsInt> outDungeonRooms, out HashSet<Vector3Int> newCenter)
    {
        ClearDictionaries();
        DungeonTiles dungeonTiles = new DungeonTiles(AlgorithmType.BinarySpacepartitioning);

        bool isDoneSplitting = false;
        while (isDoneSplitting == false)
        {
            isDoneSplitting = IterateOverRooms();
        }
        outDungeonRooms = new HashSet<BoundsInt>();
        foreach(var room in dungeonRooms)
        {
            outDungeonRooms.Add(room.Value);
        }
        HashSet<BoundsInt>newDungeonRooms = PlaceRoomSkeletton(out newCenter);
        dungeonTiles.AddRoom(UtilityFunctions.ConvertBoundsIntToRooms(newDungeonRooms));
        corridorTiles.Clear();
        ConnectRooms(dungeonTiles);
        dungeonTiles.AddCorridor(corridorTiles);
        return dungeonTiles;
    }

    public override DungeonTiles FirstGeneration()
    {
        DungeonTiles newDungeonTiles = new DungeonTiles(AlgorithmType.BinarySpacepartitioning);
        ClearDictionaries();
        HashSet<BoundsInt> newDungeonRooms = new HashSet<BoundsInt>();
        IterateOverRooms();
        foreach (var room in dungeonRooms.Values)
        {
            newDungeonRooms.Add(room);
            //Debug.Log($"Room [Pos.: {room.Value.position}] [ID: {room.Key}] ");
        }
        newDungeonTiles.AddRoom(UtilityFunctions.ConvertBoundsIntToRooms(newDungeonRooms));
        return newDungeonTiles;
    }

    public override DungeonTiles ContinueIterating()
    {
        HashSet<BoundsInt> newDungeonRooms = new HashSet<BoundsInt>();
        DungeonTiles dungeonTiles = new DungeonTiles(AlgorithmType.BinarySpacepartitioning);

        bool isDoneSplitting = IterateOverRooms();

        if (isDoneSplitting)
        {
            Debug.Log("I am done");
            newDungeonRooms = PlaceRoom();
            dungeonTiles.AddRoom(UtilityFunctions.ConvertBoundsIntToRooms(newDungeonRooms));
            corridorTiles.Clear();
            ConnectRooms(dungeonTiles);
            dungeonTiles.AddCorridor(corridorTiles);
        }
        else
        {
            foreach (var room in dungeonRooms.Values)
            {
                newDungeonRooms.Add(room);
                //Debug.Log($"Room [Pos.: {room.Value.position}] [ID: {room.Key}] ");
            }
            dungeonTiles.AddRoom(UtilityFunctions.ConvertBoundsIntToRooms(newDungeonRooms));
        }

        return dungeonTiles;
    }
    #endregion

    private void CheckGivenValues()
    {
        int dungeonSizeInt = fieldSize.size.x * fieldSize.size.y;
        if (numRooms > dungeonSizeInt || numRooms == 0)
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
        minSize += offset*offset;
    }

    private void ConnectRooms(DungeonTiles _dungeonTiles)
    {
        Queue<string> roomIndex = new Queue<string>();
        Queue<string> doneIDs = new Queue<string>();
        foreach(string index in dungeonRooms.Keys)
        {
            roomIndex.Enqueue(index);
        }
        //Debug.Log("<color=black> Room IDs:</color>");
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

                //Debug.Log($"<color=cyan>Connecting Rooms</color> {currentID} & {siblingID} ");
                //Debug.Log($"<color=magenta> newParent: </color> {parentID}");
                roomIndex.Enqueue(parentID);
                doneIDs.Enqueue(currentID);
            }
            else if(doneIDs.Contains(siblingID) == false)
            {
                roomIndex.Enqueue(currentID);
            }
        }
        //Debug.Log("<color=black> End Room IDs.</color>");
    }

    private void SetCorridor(Vector2Int pointA, Vector2Int pointB, DungeonTiles _dungeonTiles)
    {
        //Debug.Log("Connecting!");
        Vector2Int pointAB = new Vector2Int(pointA.x, pointB.y);
        Vector2Int pointBA = new Vector2Int(pointB.x, pointA.y);

        HashSet<Vector2Int> pathA = new HashSet<Vector2Int>();
        HashSet<Vector2Int> pathB = new HashSet<Vector2Int>();

        HashSet<Vector2Int> corridorList = new HashSet<Vector2Int>();

        pathA = GetCorridorPath(pointA, pointB, pointAB, _dungeonTiles);
        pathB = GetCorridorPath(pointA, pointB, pointBA, _dungeonTiles);

        Debug.ClearDeveloperConsole();

        if (pathA.Count <= 1)
        {
            pathA = pathB;
        }
        else if(pathB.Count <= 1)
        {
            pathB = pathA;
        }

        if (pathA.Count == pathB.Count)
        {
            corridorList = Random.value <= 0.5f ? pathA : pathB;
        }
        else
        {
            corridorList = pathA.Count < pathB.Count ? pathA : pathB;
            
        }

        /*
        if (corridorList == pathA)
        {

            Debug.DrawLine(new Vector3(pointA.x, pointA.y), new Vector3(pointAB.x, pointAB.y), Color.blue, 1);
            Debug.DrawLine(new Vector3(pointAB.x, pointAB.y), new Vector3(pointB.x, pointB.y), Color.blue, 1);
        }
        else
        {
            Debug.DrawLine(new Vector3(pointA.x, pointA.y), new Vector3(pointBA.x, pointBA.y), Color.red, 1);
            Debug.DrawLine(new Vector3(pointBA.x, pointBA.y), new Vector3(pointB.x, pointB.y), Color.red, 1);
        }
        */

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

        Vector2Int startVector = new Vector2Int();
        Vector2Int goalVector = new Vector2Int();

        Vector2Int startPoint = new Vector2Int();
        Vector2Int goalPoint = new Vector2Int();


        if(pointA.x == middlePoint.x)
        {
            startPoint = pointA;
            goalPoint = pointB;
        }
        else if(pointB.x == middlePoint.x)
        {
            startPoint = pointB;
            goalPoint = pointA;
        }

        #region GoAlongXAxis
        if (startPoint.y < middlePoint.y)
        {
            startVector = startPoint;
            goalVector = middlePoint;
        }
        else
        {
            startVector = middlePoint;
            goalVector = startPoint;
        }
        
        for(int y = startVector.y; y <= goalVector.y; y++)
        {
            Vector2Int currentPosition = new Vector2Int(startVector.x, y);
            if(refTiles.Contains(currentPosition) == false)
            {
                result.Add(currentPosition);
            }
        }
        #endregion
        #region GoAlongYAxis
        if (goalPoint.x < middlePoint.x)
        {
            startVector = goalPoint;
            goalVector = middlePoint;
        }
        else
        {
            startVector = middlePoint;
            goalVector = goalPoint;
        }

        for (int x = startVector.x; x <= goalVector.x; x++)
        {
            Vector2Int currentPosition = new Vector2Int(x, startVector.y);
            if (refTiles.Contains(currentPosition) == false)
            {
                result.Add(currentPosition);
            }
        }
        #endregion
        /*
        if(result.Count < 2) 
        {
            Debug.DrawLine(new Vector3(startPoint.x, startPoint.y), new Vector3(middlePoint.x, middlePoint.y), Color.cyan, 100);
            Debug.DrawLine(new Vector3(middlePoint.x, middlePoint.y), new Vector3(goalPoint.x, goalPoint.y), Color.blue, 100);
            Debug.Log($"startPoint {startPoint}");
            Debug.Log($"goalPoint {goalPoint}");

            Debug.Log($"point A {pointA}");
            Debug.Log($"point B {pointB}");

            Debug.Log($"middlePoint {middlePoint}");
        }
        */
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
            Vector3Int newSize = new Vector3Int(boneRoomSize, boneRoomSize);
            Vector3Int variantPosition = VariantPosition(rooms.Value, newSize);
            newCenter.Add(variantPosition);
            BoundsInt newRoom = new BoundsInt(variantPosition, newSize);
            dungeonRoomTiles.Add(newRoom);
            newDungeonRooms.Add(rooms.Key, newRoom);
            roomsToConnect.Add(rooms.Key, new RoomPoints(newRoom.position));
        }
        return dungeonRoomTiles;

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
        int xDiff = rooms.size.x - newSize.x;
        int yDiff = rooms.size.y - newSize.y;

        int xRandom = Random.Range(-xDiff/2, xDiff/2);
        int yRandom = Random.Range(-yDiff/2, yDiff/2);

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
    #endregion

    private bool IterateOverRooms()
    {
        int numSplitRooms = 0;
        Queue<string> roomQueue = FillQueue();
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

    private Queue<string> FillQueue()
    {
        Queue<string> newQueue = new Queue<string>();
        if(dungeonRooms.Count > 0) { 
            foreach(var room in dungeonRooms.Keys)
            {
                newQueue.Enqueue(room);
            }
        }
        else
        {
            newQueue.Enqueue("A");
            dungeonRooms.Add("A", fieldSize);
        }
        return newQueue;
    }
    private HashSet<BoundsInt> SplitSpace(BoundsInt room)
    {
        HashSet<BoundsInt> result = new HashSet<BoundsInt>();

        //prüfe ob der Raum in 2 Teile geteilt werden kann (beide Räume die mindest Größe erfüllen)
        bool canSplitVertical = (room.size.x >= 2*minXSize) && (minXSize * room.size.y >= minSize);
        bool canSplitHorizontal = (room.size.y >= 2*minYSize) && (minYSize * room.size.x >= minSize);
        bool canSplitBoth = canSplitHorizontal && canSplitVertical;

        #region SplitVertical
        if (canSplitBoth && Random.value <= 0.5f || canSplitHorizontal == false && canSplitVertical)
        {
            //Debug.Log("<color=red>Vertical Slice</color>");
            
            int randomValueA = Random.Range(minXSize, room.size.x-minXSize);
            if(randomValueA%2 > 0)
            {
                if(randomValueA > minXSize)
                {
                    randomValueA += 1;
                }
                else
                {
                    randomValueA -= 1;
                }
            }
            int randomValueB = room.size.x - randomValueA;

            

            BoundsInt newRoomA = new BoundsInt(room.xMin-randomValueB/2, room.yMin, 0, randomValueA, room.size.y, 0);
            BoundsInt newRoomB = new BoundsInt(room.xMin+randomValueA/2, room.yMin, 0, randomValueB, room.size.y, 0);

            //Debug.Log("New Room Size: A(" + newRoomA.size + ") / B(" + newRoomB.size + ")");
            //Debug.Log("New Room Position: A(" + newRoomA.position + ") / B(" + newRoomB.position + ")");
            result.Add(newRoomA);
            result.Add(newRoomB);

        }
        #endregion
        #region SplitHorizontal
        else if (canSplitHorizontal)
        {
            //Debug.Log("<color=red>Horizontal Slice</color>");
           
            int randomValueA = Random.Range(minYSize, room.size.y - minYSize);
            if (randomValueA % 2 > 0)
            {
                if (randomValueA > minYSize)
                {
                    randomValueA += 1;
                }
                else
                {
                    randomValueA -= 1;
                }
            }
            int randomValueB = room.size.y - randomValueA;

            BoundsInt newRoomA = new BoundsInt(room.xMin, room.yMin-randomValueB/2, 0, room.size.x, randomValueA, 0);
            BoundsInt newRoomB = new BoundsInt(room.xMin, room.yMin+randomValueA/2, 0, room.size.x, randomValueB, 0);

            //Debug.Log("New Room Size: A("+newRoomA.size+") / B("+newRoomB.size+")");
            //Debug.Log("New Room Position: A(" + newRoomA.position + ") / B(" + newRoomB.position + ")");
            result.Add(newRoomA);
            result.Add(newRoomB);
        }
        #endregion
        return result;
    }


    private string GetSiblingIndex(string parentString, string ownID)
    {
        string siblingIndex = parentString;
        if (ownID == "A")
        {
            siblingIndex += "B";

        }
        else if (ownID == "B")
        {
            siblingIndex += "A";
        }
        return siblingIndex;
    }
    private void ResetIndexNumber()
    {
        currentIndexNum = 0;
    }
    private void ClearDictionaries()
    {
        dungeonRooms.Clear();
        roomsToConnect.Clear();
    }
    private string GetNewIndex(string index)
    {
        string returnString = index;
        if(currentIndexNum == 0)
        {
            returnString += "A";
        }else if(currentIndexNum == 1)
        {
            returnString += "B";
        }else
        {
            returnString += "0";
        }
        currentIndexNum++;
        return returnString;
    }
}

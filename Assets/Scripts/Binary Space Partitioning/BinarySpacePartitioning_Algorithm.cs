using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine;



public class BinarySpacePartitioning_Algorithm : MonoBehaviour
{
    [SerializeField] private BoundsInt dungeonSize = new BoundsInt();
    [SerializeField] private int maxNumRooms;

    [SerializeField] private int minSize;
    [Range(0, 1)]
    [SerializeField] private float roomPercentage;
    [SerializeField] private int minYSize;
    [SerializeField] private int minXSize;

    [SerializeField] private int offset;

    private Dictionary<string, BoundsInt> dungeonRooms = new Dictionary<string, BoundsInt>();
    private Dictionary<string, RoomPoints> roomsToConnect = new Dictionary<string, RoomPoints>();
    private HashSet<Vector2Int> corridorTiles = new HashSet<Vector2Int>();
    private int currentIndexNum = 0;

    private void Start()
    {
        CheckGivenValues();
    }
    public HashSet<Bounds> SetupGeneration()
    {
        ClearDictionaries();
        HashSet<BoundsInt> newDungeonRooms = new HashSet<BoundsInt>();
        IterateOverRooms();
        foreach (var room in dungeonRooms)
        {
            newDungeonRooms.Add(room.Value);
            //Debug.Log($"Room [Pos.: {room.Value.position}] [ID: {room.Key}] ");
        }
        return ConvertBounds(newDungeonRooms);
    }

    public DungeonTiles ContinueIterating(out HashSet<Bounds> newBounds)
    {
        HashSet<BoundsInt> newDungeonRooms = new HashSet<BoundsInt>();
        newBounds = new HashSet<Bounds>();
        DungeonTiles dungeonTiles = new DungeonTiles(AlgorithmType.BinarySpacepartitioning);

        bool isDoneSplitting = IterateOverRooms();

        if (isDoneSplitting)
        {
            Debug.Log("I am done");
            newDungeonRooms = PlaceRooms();
            dungeonTiles.AddRoom(ConvertRoomsToTiles(newDungeonRooms));
            corridorTiles.Clear();
            ConnectRooms();
            dungeonTiles.AddCorridor(corridorTiles);
        }
        else
        {
            foreach(var room in dungeonRooms)
            {
                newDungeonRooms.Add(room.Value);
                //Debug.Log($"Room [Pos.: {room.Value.position}] [ID: {room.Key}] ");
            }
            newBounds = ConvertBounds(newDungeonRooms);
        }

        return dungeonTiles;
    }

    public DungeonTiles GenerateDungeonTiles()
    {
        ClearDictionaries();
        DungeonTiles dungeonTiles = new DungeonTiles(AlgorithmType.BinarySpacepartitioning);

        bool isDoneSplitting = false;
        while (isDoneSplitting == false)
        {
            isDoneSplitting = IterateOverRooms();
        }

        HashSet<BoundsInt> newDungeonRooms = PlaceRooms();
        dungeonTiles.AddRoom(ConvertRoomsToTiles(newDungeonRooms));
        corridorTiles.Clear();
        ConnectRooms();
        dungeonTiles.AddCorridor(corridorTiles);

        return dungeonTiles;
    }

    private void CheckGivenValues()
    {
        int dungeonSizeInt = dungeonSize.size.x * dungeonSize.size.y;
        if (maxNumRooms > dungeonSizeInt || maxNumRooms == 0)
        {
            maxNumRooms = 10;
        }
        if (maxNumRooms * minSize > dungeonSizeInt)
        {
            minSize = dungeonSizeInt / maxNumRooms;
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

    private HashSet<DungeonRoom> ConvertRoomsToTiles(HashSet<BoundsInt> rooms)
    {
        HashSet<DungeonRoom> roomTiles = new HashSet<DungeonRoom>();
        corridorTiles.Clear();
        foreach(var room in rooms)
        {
            DungeonRoom newRoom = new DungeonRoom(new HashSet<Vector2Int>());
            for (int i = room.xMin - room.size.x / 2; i < room.xMax - room.size.x / 2; i++)
            {
                for (int j = room.yMin - room.size.y / 2; j < room.yMax - room.size.y / 2; j++)
                {
                    newRoom.AddRoomTiles(new Vector2Int(i, j));
                }
            }
            roomTiles.Add(newRoom);
        }
        return roomTiles;
    }


    private void ConnectRooms()
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
                
                SetCorridor(pointA, pointB);
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

    private void SetCorridor(Vector2Int pointA, Vector2Int pointB)
    {
        //Debug.Log("Connecting!");
        Vector2Int pointAB = new Vector2Int(pointA.x, pointB.y);
        Vector2Int pointBA = new Vector2Int(pointB.x, pointA.y);

        HashSet<Vector2Int> pathA = new HashSet<Vector2Int>();
        HashSet<Vector2Int> pathB = new HashSet<Vector2Int>();

        HashSet<Vector2Int> corridorList = new HashSet<Vector2Int>();

        pathA = GetCorridorPath(pointA, pointB, pointAB);
        pathB = GetCorridorPath(pointA, pointB, pointBA);

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

    private HashSet<Vector2Int> GetCorridorPath(Vector2Int pointA, Vector2Int pointB, Vector2Int middlePoint)
    {
        HashSet<Vector2Int> result = new HashSet<Vector2Int>();
        
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
            if(corridorTiles.Contains(currentPosition) == false)
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
            if (corridorTiles.Contains(currentPosition) == false)
            {
                result.Add(currentPosition);
            }
        }
        #endregion

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
        return result;
    }


    private HashSet<BoundsInt> PlaceRooms()
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
        int xDiff = rooms.size.x - newSize.x-offset;
        int yDiff = rooms.size.y - newSize.y-offset;

        int xRandom = Random.Range(-xDiff/2, xDiff/2);
        int yRandom = Random.Range(-yDiff/2, yDiff/2);

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

            yRandom = Random.Range(yMin, rooms.size.y - offset);
            xRandom = Random.Range(minSize / yRandom, rooms.size.x - offset);
        }
        else if (rooms.size.y >= rooms.size.x)
        {
            int potXMin = Mathf.FloorToInt(rooms.size.x * roomPercentage);
            int xMin = potXMin > minXSize ? potXMin : minXSize;

            xRandom = Random.Range(xMin, rooms.size.x - offset);
            yRandom = Random.Range(minSize / xRandom, rooms.size.y - offset);
        }

        return new Vector3Int(xRandom, yRandom);
    }
    #endregion

    private bool IterateOverRooms()
    {
        int numSplitRooms = 0;
        Queue<string> roomQueue = FillQueue();
        while (roomQueue.Count > 0 && dungeonRooms.Count < maxNumRooms)
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
            dungeonRooms.Add("A", dungeonSize);
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
        }else if( currentIndexNum == 2)
        {
            returnString += "C";
        }
        currentIndexNum++;
        return returnString;
    }
    private HashSet<Bounds> ConvertBounds(HashSet<BoundsInt> oldBounds)
    {
        HashSet<Bounds> newBounds = new HashSet<Bounds>();
        foreach (var bounds in oldBounds)
        {
            newBounds.Add(new Bounds(bounds.position, bounds.size));
        }
        return newBounds;
    }
}

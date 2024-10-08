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
    [SerializeField] private int minYSize;
    [SerializeField] private int minXSize;

    [SerializeField] private int offset;

    private Dictionary<string, BoundsInt> dungeonRooms = new Dictionary<string, BoundsInt>();
    private Dictionary<string, RoomPoints> roomsToConnect = new Dictionary<string, RoomPoints>();
    private int currentIndexNum = 0;

    private void Start()
    {
        CheckGivenValues();
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

    public HashSet<Vector2Int> GetDungeonTiles()
    {
        throw new NotImplementedException();
    }

    public HashSet<BoundsInt> NewRooms()
    {
        dungeonRooms.Clear();
        HashSet<BoundsInt> newDungeonRooms = new HashSet<BoundsInt>();
        IterateOverRooms();
        foreach (var room in dungeonRooms)
        {
            newDungeonRooms.Add(room.Value);
            Debug.Log($"Room [Pos.: {room.Value.position}] [ID: {room.Key}] ");
        }
        return newDungeonRooms;
    }
    public HashSet<BoundsInt> GetRooms(out HashSet<Vector2Int> roomTiles)
    {
        HashSet<BoundsInt> newDungeonRooms = new HashSet<BoundsInt>();
        roomTiles = new HashSet<Vector2Int>();
        bool isDoneSplitting = IterateOverRooms();

        if (isDoneSplitting)
        {
            Debug.Log("I am done");
            //roomTiles = PlaceRooms();
            newDungeonRooms = PlaceRooms();
            ConnectRooms();
        }
        else
        {
            foreach(var room in dungeonRooms)
            {

                newDungeonRooms.Add(room.Value);
                Debug.Log($"Room [Pos.: {room.Value.position}] [ID: {room.Key}] ");
            }
        }
        return newDungeonRooms;
    }

    private void ConnectRooms()
    {
        Queue<string> roomIndex = new Queue<string>();
        Queue<string> doneIDs = new Queue<string>();
        foreach(string index in dungeonRooms.Keys)
        {
            roomIndex.Enqueue(index);
        }
        Debug.Log("<color=black> Room IDs:</color>");
        while (roomIndex.Count > 1)
        {
            string currentID = roomIndex.Dequeue();
            string parentID = currentID.Substring(0, currentID.Length - 1);
            string endIsAorB = currentID.Substring(currentID.Length - 1);
            string siblingID = GetSiblingIndex(parentID, endIsAorB);
            
            if (roomIndex.Contains(siblingID))
            {
                Debug.Log($"<color=cyan>Connecting Rooms</color> {currentID} & {siblingID} ");
                if (dungeonRooms.ContainsKey(currentID))
                {
                    Debug.Log($"Room Positions: {dungeonRooms[currentID]}");
                }
                if (dungeonRooms.ContainsKey(siblingID))
                {
                    Debug.Log($"Sibling Room Positions: {dungeonRooms[siblingID]}");
                }
                Debug.Log($"<color=magenta> newParent: </color> {parentID}");
                roomIndex.Enqueue(parentID);
                doneIDs.Enqueue(currentID);
            }
            else if(doneIDs.Contains(siblingID) == false)
            {
                roomIndex.Enqueue(currentID);
            }
        }
        Debug.Log("<color=black> End Room IDs.</color>");
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

    private HashSet<BoundsInt> PlaceRooms()
    {
        HashSet<BoundsInt> dungeonTiles = new HashSet<BoundsInt>();
        Dictionary<string, BoundsInt> newDungeonRooms = new Dictionary<string, BoundsInt>();
        foreach (var rooms in dungeonRooms)
        {
            BoundsInt newRoom = CreateRoomVariance(rooms.Value);
            dungeonTiles.Add(newRoom);
            newDungeonRooms.Add(rooms.Key, newRoom);
        }
        dungeonRooms = newDungeonRooms;
        return dungeonTiles;
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

        return new Vector3Int(rooms.position.x + xRandom, rooms.position.y + yRandom);
    }

    private Vector3Int VariantSize(BoundsInt rooms)
    {
        int xRandom = rooms.size.x;
        int yRandom = rooms.size.y;
        if (rooms.size.x > rooms.size.y)
        {
            yRandom = Random.Range(minYSize, rooms.size.y - offset);
            xRandom = Random.Range(minSize / yRandom, rooms.size.x - offset);
        }
        else if (rooms.size.y >= rooms.size.x)
        {
            xRandom = Random.Range(minXSize, rooms.size.x - offset);
            yRandom = Random.Range(minSize / xRandom, rooms.size.y - offset);
        }
        //Debug.Log($"x: {xRandom} / y: {yRandom}");
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
            Debug.Log("<color=red>Vertical Slice</color>");
            
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

            Debug.Log("New Room Size: A(" + newRoomA.size + ") / B(" + newRoomB.size + ")");
            Debug.Log("New Room Position: A(" + newRoomA.position + ") / B(" + newRoomB.position + ")");
            result.Add(newRoomA);
            result.Add(newRoomB);

        }
        #endregion
        #region SplitHorizontal
        else if (canSplitHorizontal)
        {
            Debug.Log("<color=red>Horizontal Slice</color>");
           
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

            Debug.Log("New Room Size: A("+newRoomA.size+") / B("+newRoomB.size+")");
            Debug.Log("New Room Position: A(" + newRoomA.position + ") / B(" + newRoomB.position + ")");
            result.Add(newRoomA);
            result.Add(newRoomB);
        }
        #endregion
        return result;
    }


    private void ResetIndexNumber()
    {
        currentIndexNum = 0;
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
}

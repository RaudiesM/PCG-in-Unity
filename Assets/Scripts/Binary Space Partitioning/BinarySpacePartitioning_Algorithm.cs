using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine;



public class BinarySpacePartitioning_Algorithm : MonoBehaviour
{
    [SerializeField] private BoundsInt DungeonSize = new BoundsInt();
    [SerializeField] private int minSize;
    [SerializeField] private int minYSize;
    [SerializeField] private int minXSize;

        private HashSet<BoundsInt> dungeonRooms =new HashSet<BoundsInt>();
    public HashSet<Vector2Int> GetDungeonTiles()
    {
        throw new NotImplementedException();
    }

    public HashSet<BoundsInt> NewRooms()
    {
        dungeonRooms.Clear();
        IterateOverRooms();
        return dungeonRooms;
    }
    public HashSet<BoundsInt> GetRooms(out HashSet<Vector2Int> roomTiles)
    {
        roomTiles = new HashSet<Vector2Int>();
        bool isDoneSplitting = IterateOverRooms();
        if (isDoneSplitting)
        {
            Debug.Log("I am done");
            roomTiles = PlaceRooms();
        }
        return dungeonRooms;
    }

    private HashSet<Vector2Int> PlaceRooms()
    {
        HashSet<Vector2Int> dungeonTiles = new HashSet<Vector2Int>();
        foreach (var rooms in dungeonRooms) { 
            BoundsInt newRoom = CreateRoomVariance(rooms);
            foreach(var boundInt in newRoom.allPositionsWithin)
            {
                Debug.Log("Pos: " + boundInt.x + " / " + boundInt.y);
            }
        }
        return dungeonTiles;
    }

    private BoundsInt CreateRoomVariance(BoundsInt rooms)
    {
        return rooms;
    }

    private bool IterateOverRooms()
    {
        int numSplitRooms = 0;
        Queue<BoundsInt> roomQueue = FillQueue();
        while (roomQueue.Count > 0)
        {
            BoundsInt room = roomQueue.Dequeue();
            HashSet<BoundsInt> newRooms = SplitSpace(room);
            if(newRooms.Count > 0)
            {
                dungeonRooms.Remove(room);
                foreach(BoundsInt newRoom in newRooms)
                {
                    dungeonRooms.Add(newRoom);
                    numSplitRooms++;
                }
            }
        }
        return numSplitRooms == 0;
    }

    private Queue<BoundsInt> FillQueue()
    {
        Queue<BoundsInt> newQueue = new Queue<BoundsInt>();
        if(dungeonRooms.Count > 0) { 
            foreach(BoundsInt room in dungeonRooms)
            {
                newQueue.Enqueue(room);
            }
        }
        else
        {
            newQueue.Enqueue(DungeonSize);
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
            result.Add(newRoomA);
            result.Add(newRoomB);
        }
        #endregion
        return result;
    }
}

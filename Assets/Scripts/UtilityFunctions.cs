using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UtilityFunctions
{
    public static HashSet<Vector2Int> GetNeighbourCell(Vector2Int position)
    {
        HashSet<Vector2Int> neighbours = new HashSet<Vector2Int>();
        neighbours.Add(position + Vector2Int.up);
        neighbours.Add(position + Vector2Int.down);
        neighbours.Add(position + Vector2Int.left);
        neighbours.Add(position + Vector2Int.right);
        return neighbours;
    }

    public static HashSet<DungeonRoom> ConvertBoundsIntToRooms(HashSet<BoundsInt> rooms)
    {
        HashSet<DungeonRoom> dungeonRooms = new HashSet<DungeonRoom>();
        foreach (var room in rooms)
        {
            DungeonRoom newRoom = new DungeonRoom(new HashSet<Vector2Int>());
            for (int i = room.xMin - room.size.x / 2; i < room.xMax - room.size.x / 2; i++)
            {
                for (int j = room.yMin - room.size.y / 2; j < room.yMax - room.size.y / 2; j++)
                {
                    newRoom.AddRoomTiles(new Vector2Int(i, j));
                }
            }
            dungeonRooms.Add(newRoom);
        }
        return dungeonRooms;
    }

    public static Vector2Int GetRandomPointWithinBounds(int newXMin, int newYMin, int newXMax, int newYMax, int offset)
    {
        int xValue = Random.Range(newXMin + offset, newXMax - offset);
        int yValue = Random.Range(newYMin + offset, newYMax - offset);
        return new Vector2Int(xValue, yValue);
    }

    public static HashSet<Vector2Int> GetConnectedTiles(HashSet<Vector2Int> tilesToCheck, HashSet<Vector2Int> currentTiles, ref Vector2Int currentPosition)
    {
        Queue<Vector2Int> lastSafePoints = new Queue<Vector2Int>();
        bool isChecking = true;
        while (isChecking)
        {
            bool neighbourIsSet = false;
            int possibleNeighbours = 0;
            Vector2Int lastSafePoint = currentPosition;
            foreach (var neighbour in UtilityFunctions.GetNeighbourCell(currentPosition))
            {
                if (tilesToCheck.Contains(neighbour) && currentTiles.Contains(neighbour) == false)
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
        return currentTiles;
    }

    public static void MarkPosition(Vector2 position, Color color, int duration = 5)
    {
        Debug.DrawLine(position-Vector2.up, position-Vector2.down, color, duration);
        Debug.DrawLine(position - Vector2.left, position - Vector2.right, color, duration);
    }
}

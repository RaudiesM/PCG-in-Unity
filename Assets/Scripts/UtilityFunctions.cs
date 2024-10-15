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
    public static HashSet<Bounds> ConvertBounds(HashSet<BoundsInt> oldBounds)
    {
        HashSet<Bounds> newBounds = new HashSet<Bounds>();
        foreach (var bounds in oldBounds)
        {
            newBounds.Add(new Bounds(bounds.position, bounds.size));
        }
        return newBounds;
    }

    public static void MarkPosition(Vector2 position, Color color, int duration = 5)
    {
        Debug.DrawLine(position-Vector2.up, position-Vector2.down, color, duration);
        Debug.DrawLine(position - Vector2.left, position - Vector2.right, color, duration);
    }
}

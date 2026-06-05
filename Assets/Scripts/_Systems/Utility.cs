using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility
{
    public static List<Vector2> Surrounding_Directions()
    {
        List<Vector2> directions = new(8);

        for (int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                if (x == 0 && y == 0) continue;
                directions.Add(new Vector2(x, y));
            }
        }
        return directions;
    }

    public static List<Vector2> Surrounding_Positions(Vector2 pivotPos)
    {
        List<Vector2> positions = new();
        List<Vector2> directions = Surrounding_Directions();

        for (int i = 0; i < directions.Count; i++)
        {
            positions.Add(pivotPos + directions[i]);
        }
        return positions;
    }

    public static Vector2 Grid_Direction(Vector2 pivotPos, Vector2 targetPos)
    {
        Vector2 delta = targetPos - pivotPos;

        int x = Mathf.RoundToInt(delta.x);
        int y = Mathf.RoundToInt(delta.y);

        return new Vector2(Mathf.Clamp(x, -1, 1), Mathf.Clamp(y, -1, 1));
    }

    public static int Chebyshev_Distance(Vector2 a, Vector2 b)
    {
        float dx = Mathf.Abs(a.x - b.x);
        float dy = Mathf.Abs(a.y - b.y);

        return (int)Mathf.Max(dx, dy);
    }
}

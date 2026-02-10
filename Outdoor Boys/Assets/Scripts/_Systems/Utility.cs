using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility
{
    public static List<Vector2> Surrounding_Positions(Vector2 pivotPos)
    {
        List<Vector2> positions = new(8);

        for (int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                if (x == 0 && y == 0) continue; // skip center

                positions.Add(new Vector2(pivotPos.x + x, pivotPos.y + y));
            }
        }

        return positions;
    }
}

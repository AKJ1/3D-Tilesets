using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class OpExtentions
{
    public static Matrix4x4 LerpTo(this Matrix4x4 from, Matrix4x4 to, float time)
    {
        Matrix4x4 ret = new Matrix4x4();
        for (int i = 0; i < 16; i++)
            ret[i] = Mathf.Lerp(from[i], to[i], time);
        return ret;
    }

    public static Vector3Int ToVector3Int(this (int x, int y, int z) val)
    {
        return new Vector3Int();
    }

    public static Vector3Int ToIntVector(this Vector3 vec)
    {
        return new Vector3Int((int)vec.x, (int)vec.y,(int)vec.z);
    }

    public static Vector3 SnapToClosestCellInGrid(this Vector3 vec, Vector3 gridSize)
    {
        for (int i = 0; i < 3; i++)
        {
            var remnant = vec[i] % gridSize[i];
            vec[i] = remnant > (gridSize[i] / 2) ? vec[i] + (gridSize[i]-remnant) : vec[i] - remnant;
        }
        return vec;
    }

    public static Vector3Int ToOrdinalDirectionVector(this Vector3 vec)
    {
        List<Vector3> ordinalVectors = new List<Vector3>()
        {
            Vector3.up,
            Vector3.down,
            Vector3.left,
            Vector3.right,
            Vector3.forward,
            Vector3.back
        };

        Vector3 bestVec = Vector3.zero;
        float bestDot = 0;
        for (int i = 0; i < ordinalVectors.Count; i++)
        {
            var dot = Vector3.Dot(vec, ordinalVectors[i]);
            if ((1 - dot) < (1-bestDot))
            {
                bestVec = ordinalVectors[i];
                bestDot = dot;
            }
        }
        return bestVec.ToIntVector();
    }
    
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// using System.Numerics;

namespace Terrain.Ext
{
    public static class Array2D
    {
        public static bool CompareSlices<T>(T[,] t1, T[,] t2)
        {
            return t1.Cast<T>().SequenceEqual(t2.Cast<T>());
        }
        
        public static T[,] CentralSlice<T>(T[,] src, int radX, int radY)
        {
            int midPointY = src.GetLength(0) / 2;
            int midPointX = src.GetLength(1) / 2;
            if (midPointX % 2 != 0 || midPointY % 2 != 0)
            {
                throw new Exception("2D Arrays using the central slice funciton are always expected to have an odd size!");
            }
            return GetSlice(src, midPointY - radY, midPointX - radX,(radX * 2) + 1,(radY * 2) + 1);
        }

        public static void SetCentralSlice<T>(T[,] src, T[,] dst)
        {
            var ldx = dst.GetLength(1) - src.GetLength(1);
            var ldy = dst.GetLength(0) - src.GetLength(0);
            if (ldx % 2 != 0 || ldy % 2 != 0)
            {
                throw new Exception("2D Arrays using the central slice funciton are always expected to have an odd size!");
            }
            SetSlice(src, dst, ldy/2, ldx/2 );
        }
        
        public static T[,] GetSlice<T>(T[,] src, int idxI, int idxJ, int lx, int ly)
        {
            T[,] rslt = new T[ly,lx];
            for (int i = idxI; i < idxI+ly; i++)
            {
                for (int j = idxJ; j < idxJ+lx; j++)
                {
                    rslt[i-idxI, j-idxJ] = src[i, j];
                }
            }
            return rslt;
        }
        
        public static T[,] ToTwoDimensionalArray<T>(this IEnumerable<IEnumerable<T>> enumerable)
        {
            var lines = enumerable.Select(inner => inner.ToArray()).ToArray();
            var columnCount = lines.Max(columns => columns.Length);
            var twa = new T[lines.Length, columnCount];
            for (var lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                var line = lines[lineIndex];
                for (var columnIndex = 0; columnIndex < line.Length; columnIndex++)
                {
                    twa[lineIndex, columnIndex] = line[columnIndex];
                }
            }
            return twa;
        }

        /// <summary>
        /// Gets a 2d array slice with an option for a default value when trying to get an out of bounds item.
        /// </summary>
        /// <param name="src"> Source Array to Take a 2D Slice From</param>
        /// <param name="idxI"> Start Row for the array slice (i/y)</param>
        /// <param name="idxJ"> Start Column for the array slice (j/x) f</param>
        /// <param name="lx"> Width of Slice </param>
        /// <param name="ly"> Height of Slice </param>
        /// <param name="empty">default item when out of bounds of source array</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>2d array slice</returns>
        public static T[,] GetSliceSafe<T>(T[,] src, int idxI, int idxJ, int lx, int ly, T empty = default)
        {
            T[,] rslt = new T[ly,lx];
            for (int i = idxI; i < idxI+ly; i++)
            {
                for (int j = idxJ; j < idxJ+lx; j++)
                {
                    if (i < 0 || j < 0 || i > src.GetLength(0)-1 || j > src.GetLength(1)-1)
                    {
                        rslt[i - idxI, j - idxJ] = empty;
                    }
                    else
                    {
                        rslt[i - idxI, j - idxJ] = src[i, j];
                    }
                }
            }
            return rslt;
        }
        
        public static (int, int, int) TransformIndex((int,int,int) index, int stepsX, int stepsY, (int,int,int) maxVal, Vector3Int scale)
        {
           // Matrix4x4 rotationMatrix = Matrix4x4.CreateFromYawPitchRoll((float)Math.PI * stepsX, (float)Math.PI * stepsY,0);
           // Vector3 halfMaxVal = new Vector3(maxVal.Item1-1, maxVal.Item2-1, maxVal.Item3-1) / 2;
           // Vector3 indexVector = new Vector3(index.Item1, index.Item2, index.Item3) - halfMaxVal;
           // Vector3 result = Vector3.Transform(indexVector, rotationMatrix) + halfMaxVal;
           
           Vector3 halfMaxVal = new Vector3(maxVal.Item1-1, maxVal.Item2-1, maxVal.Item3-1) / 2;
           Vector3 idx = new Vector3(index.Item1, index.Item2, index.Item3) - halfMaxVal;
           Matrix4x4 mtx = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 90 * stepsY, 90 * stepsX), scale);
           Vector3 result = (mtx * idx);
           result += halfMaxVal;
           
           return ((int)Math.Round(result.x), (int)Math.Round(result.y), (int)Math.Round(result.z));
        }
        
        public static (int, int, int) TransformIndex((int, int, int) index, Quaternion rotation, int maxVal, Vector3Int scale)
        {
            return TransformIndex(index, rotation, (maxVal, maxVal, maxVal), scale);
        }

        public static (int, int, int) TransformIndex((int, int, int) index, Quaternion rotation, (int,int,int) maxVal, Vector3Int scale)
        {
           Vector3 halfMaxVal = new Vector3(maxVal.Item1-1, maxVal.Item2-1, maxVal.Item3-1) / 2;
           Vector3 idx = new Vector3(index.Item1, index.Item2, index.Item3) - halfMaxVal;
           Matrix4x4 mtx = Matrix4x4.TRS(Vector3.zero, rotation, scale);
           Vector3 result = (mtx * idx);
           result += halfMaxVal;
           return ((int)Math.Round(result.x), (int)Math.Round(result.y), (int)Math.Round(result.z));
        }

        public static (int, int) TransformIndex((int, int) index, int steps, (int,int) maxVal)
        {
            var result = TransformIndex((index.Item1, index.Item2, 0), steps, 0, (maxVal.Item1, maxVal.Item2, 0), Vector3Int.one);
            return (result.Item1, result.Item2);
        }
        
        public static (int, int) TransformIndex((int, int) index, int steps, int maxVal, Vector3Int scale)
        {
            var result = TransformIndex((index.Item1, index.Item2, 0), steps, 0, (maxVal, maxVal, 0), scale);
            return (result.Item1, result.Item2);
        }

        public static void SetSlice<T>(T[,] src, T[,] dst, int idxI, int idxJ)
        {
            var ddx =dst.GetLength(1) - src.GetLength(1);
            var ddy =dst.GetLength(0) - src.GetLength(0);
            for (int i = idxI; i <= idxI+ddy; i++)
            {
                for (int j = idxJ; j <= idxJ+ddx; j++)
                {
                    dst[i, j] = src[i-idxI, j-idxJ];
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

// using System.Numerics;

namespace Terrain.Ext
{
    public class Array2DTests
    {

        [Test]
        public void TestArray2DGetSlice()
        {
            string[,] testArr = new string[,]
            {
                {"k", "k", "k", "k", "k"},
                {"k", "w", "o", "l", "k"},
                {"k", "o", "l", "o", "k"},
                {"k", "l", "o", "l", "k"},
                {"k", "k", "k", "k", "k"}
            };
            
            string[,] targetArr = new string[,]
            {
                {"w", "o", "l"},
                {"o", "l", "o"},
                {"l", "o", "l"},
            };
            var subArr = Array2D.CentralSlice(testArr, 1, 1);
            var flatTest = string.Join("",subArr.Cast<string>().Select(s => s).ToArray());
            var flatTarget = string.Join("",targetArr.Cast<string>().Select(s => s).ToArray());
            Assert.AreEqual(flatTarget, flatTest);
        }

        [Test]
        public void TestArray2DComparisons()
        {
            char[,] testArr = new char[,]
            {
                {'l', 'l', 'l'},
                {'l', 'l', 'l'},
                {'l', 'l', 'l'},
            };
            char[,] correctArr = new char[,]
            {
                {'l', 'l', 'l'},
                {'l', 'l', 'l'},
                {'l', 'l', 'l'},
            };
            char[,] wrongArr = new char[,]
            {
                {'l', 'l', 'l'},
                {'k', 'e', 'k'},
                {'l', 'l', 'l'},
            };

            var shouldBeWrong = Array2D.CompareSlices(testArr, wrongArr);
            var shouldBeRight = Array2D.CompareSlices(testArr, correctArr);
            
            Assert.IsFalse(shouldBeWrong);
            Assert.IsTrue(shouldBeRight);
        }

        [Test]
        public void TestArray2DRotation()
        {
            string[,] testArr = new string[,]
            {
                {"a1", "a2", "a", "a4", "a5"},
                {"b1", "b2", "b", "b4", "b5"},
                {"c1", "c2", "c", "c4", "c5"},
                {"d1", "d2", "d", "d4", "d5"},
                {"e1", "e2", "e", "e4", "e5"}
            };
            string[,] testArr2 = new string[5,5];
            for (int t1 = 0; t1 < 4; t1++)
            {
                string str = "";
                for (int i = 0; i < testArr.GetLength(0); i++)
                {
                    for (int j = 0; j < testArr.GetLength(1); j++)
                    {
                        var idx = Array2D.TransformIndex((i, j), t1, (5, 5));
                        testArr2[i,j] = testArr[idx.Item1, idx.Item2];
                        str += testArr2[i, j];
                    }
                    str += "\n";
                }
                Debug.Log(str);
                // Debug.Log("Rotated Once");
            }
            Assert.IsTrue(true);
        }
        
        [Test]
        public void TestArray2DSubstitute()
        {
            string[,] testArr = new string[,]
            {
                {"k", "k", "k", "k", "k"},
                {"k", "k", "k", "k", "k"},
                {"k", "k", "k", "k", "k"},
                {"k", "k", "k", "k", "k"},
                {"k", "k", "k", "k", "k"}
            };
            string[,] replaceArr = new string[3,3]
            {
                {"l", "l", "l"},
                {"l", "l", "l"},
                {"l", "l", "l"},
            };

            string[,] targetArr = new string[5, 5]
            {
                {"k", "k", "k", "k", "k"},
                {"k", "l", "l", "l", "k"},
                {"k", "l", "l", "l", "k"},
                {"k", "l", "l", "l", "k"},
                {"k", "k", "k", "k", "k"}
            };
            Array2D.SetCentralSlice(replaceArr, testArr);
            var flatTest = string.Join("", testArr.Cast<string>().Select(s => s).ToArray());
            var flatTar = string.Join("", targetArr.Cast<string>().Select(s => s).ToArray());
            Assert.AreEqual(flatTar, flatTest);
        }
    }
}
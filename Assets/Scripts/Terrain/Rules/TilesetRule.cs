using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Terrain.Ext;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Terrain
{
    [CreateAssetMenu(fileName = "New Rule", menuName = "Tiles/Rule", order = 0)]
    public class TilesetRule : ScriptableObject, ITerrainRule
    {
        public const string TerrainCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ.,-=";
        
        private const string EmptyRuleData = ".;.;.;.;.;\n.;.;.;.;.;\n.;.;.;.;.;\n.;.;.;.;.;\n.;.;.;.;.;";

        public const char ExpSep = ';';

        public int Priority;
        
        [Range(0,2)]
        public int RuleSize;

        [FormerlySerializedAs("ApplicableTransformations")] [SerializeField]
        public List<string> Transformations;

        public string[,] ActiveRuleSlice
        {
            get
            {
                RefreshSlice();
                return activeSliceCache;
            }
        }
        
        private string[,] activeSliceCache;

        public string[,] RuleData
        {
            get
            {
                if (ruleData == null)
                {
                    LoadData();
                }
                return ruleData;
            }
        }

        [NonSerialized]
        public Vector2Int RotStepIdxTransform;

        [NonSerialized]
        public Vector3Int ScaleIdxTransform = Vector3Int.one;

        public string[,] ruleData;

        [SerializeField] 
        // [HideInInspector]
        private string SavedRuleData = "";

        

        private void RefreshSlice()
        {
            activeSliceCache = Array2D.CentralSlice(RuleData, RuleSize, RuleSize);
        }

        private void OnValidate()
        {
            if (Transformations == null)
            {
                Transformations = new List<string>();
            }

            if (Transformations.Count == 0)
            {
                Transformations.Add("ID");
            }
            CommitData();
            // var arr = RuleData.Split('\n').Select(s => s.ToCharArray()).Cast<char[,]>();
            // if (RuleData.Count(d => d == '\n') != 4)
            // {
            //     RuleData = SavedRuleData;
            // }
            // if (RuleData.Split('\n').Any(s => s.Length != 5))
            // {
            //     RuleData = SavedRuleData;
            // }
        }

        public TerrainTile TargetTile;

        public void CommitData()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < RuleData.GetLength(0); i++)
            {
                for (int j = 0; j < RuleData.GetLength(1); j++)
                {
                    sb.Append(RuleData[i, j]);
                    sb.Append(ExpSep);
                }
                sb.Append('\n');
            }
            this.SavedRuleData = sb.ToString();
        }

        private void LoadData()
        {
            // Array2D.TransformIndex()
            if (string.IsNullOrEmpty(SavedRuleData))
            {
                SavedRuleData = EmptyRuleData;
            }
            ruleData = new string[5,5];
            var arr = SavedRuleData.Split('\n');
            for (int i = 0; i < arr.Length; i++)
            {
                if (string.IsNullOrEmpty(arr[i]))
                {
                    break;
                }
                var subArr = arr[i].Split(ExpSep);
                for (int j = 0; j < subArr.Length; j++)
                {
                    if (string.IsNullOrEmpty(subArr[j]))
                    {
                        break;
                    }
                    ruleData[i, j] = subArr[j];
                }
            }
        }
        
        private Regex[][] rgxCache = new Regex[5][];

        private Regex GetRegex(int i, int j)
        {
            try
            {
                if (rgxCache[i] == null)
                {
                    rgxCache[i] = new Regex[5];
                }

                if (rgxCache[i][j] == null)
                {
                    rgxCache[i][j] = new Regex(RuleData[i, j]);
                }

                return rgxCache[i][j];
            }
            catch (Exception e)
            {
                // Debug.Log($"error on index[{i},{j}]");
                throw e;
            }
        }

        public bool IsValid(char[,] context)
        {
            int start = 2 - this.RuleSize;
            Regex rgx = null;
            for (int i = start; i < RuleData.GetLength(1)-this.RuleSize; i++)
            {
                for (int j = start; j < RuleData.GetLength(1)-this.RuleSize; j++)
                {
                    (int x, int y) idx = Array2D.TransformIndex((j, i), RotStepIdxTransform.x, 5, ScaleIdxTransform);
                    rgx = GetRegex(idx.y,idx.x);
                    if (!rgx.IsMatch(context[i, j].ToString()))
                    {
                        return false;
                    }
                }   
            }
            return true;
        }
    }
}
// Rule has a regex array RuleData. It provides data for IsValid to check against the terrain chunk being processed by TilesetTerrain. IsValid goes over a [1,1] to [5,5] sized array and checks whether every regex cell is valid.
//
// TilesetTerrain has a big level string that's edited visually, that goes over every element, takes a char[5,5] of the surroundings of the current index, and checks it against every terrain rule.

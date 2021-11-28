using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Terrain.Data;
using Terrain.Ext;
using UnityEditor;
using UnityEngine;

namespace Terrain
{
    [CreateAssetMenu(fileName = "New TerrainTileset", menuName = "Terrain/Tileset", order = 0)]
    public class TerrainTileset : ScriptableObject
    {
       static string Level = 
      @".................................
        ........ggggggg.................. 
        .....gggggggggggg................ 
        ..gggggggggggggg.gggg...ggg...... 
        ..gg.ggggggggggggggggggggggggggg. 
        ..gggggggggggggg....ggg...gggg... 
        .....gggggggggg.................. 
        ......ggggg.ggg.................. 
        ................................. 
        ";
       
        public List<TilesetRule> Rules;

        public Material TerrainMaterial;

        public TilesetDomain Domain;
        
        /// <summary>
        /// A list with the names of the transformations ( for legibility )
        /// that each rule could potentially look for.
        /// The Indexing item applies step-based transforms for reading the array ( 1 step = 90degrees )
        /// Effectively rotating the array on read.
        /// 
        /// The Vector3 is just a  euler angles rotation
        /// that gets applied to object that results from the rule
        /// when it gets instantiated.
        /// </summary>
        [SerializeField] public List<TerrainSymmetryEntry> Symmetries;
        
        
        // public SerializableDictionary<string,Vector3> RotationalSteps = new SerializableDictionary<string, Vector3>();

        public Vector3 TileSize;

        public Vector3 BlockSize;

        public int MaxRuleSize;

        public void ResolveToTiles(string nearby)
        {
        }
        
        public void CreateTerrain(GameObject terrainParent)
        {
            int yPos = 0;
            int xPos = 0;
            List<CombineInstance> combi = new List<CombineInstance>();
            Level = Level.Replace("g", "ggg");
            Level = Level.Replace(".", "...");
            Level = string.Join("",Level.Split('\n').Select(s => s + "\n" + s + "\n"+ s + "\n"));
            var chararr = Level.Replace(" ", "").Split('\n').Select(s => s.ToCharArray()).ToTwoDimensionalArray();
            
            GameObject newObj = new GameObject();
            newObj.AddComponent<MeshFilter>();
            newObj.AddComponent<MeshRenderer>();
            
            for (int i = 0; i < Level.Length; i++)
            {
                char[,] slice = Array2D.GetSliceSafe(chararr, yPos - 2, xPos - 2, 5, 5, '.');
                GameObject targetGameObject = newObj;
                Mesh targetMesh = null;
                
                Vector3 targetOffset = Vector3.zero;
                Quaternion targetRot = Quaternion.identity;
                Vector3Int targetScale = Vector3Int.one;
                Vector3 symmetryRotation = Vector3.zero;
                // Matrix4x4 targetTransformation = Matrix4x4.identity;

                bool validFlag = false;
                bool matched = false;
                if (Rules != null)
                {
                    foreach (var rule in Rules.OrderByDescending(r => r.Priority))
                    {
                        foreach (var symmetry in Symmetries.Where(s => rule.Transformations.Contains(s.Name)))
                        {
                            targetScale = new Vector3Int(symmetry.FlipX ? -1 : 1, symmetry.FlipY ? -1 : 1, symmetry.FlipZ ? -1 : 1);
                            rule.ScaleIdxTransform = targetScale;
                            rule.RotStepIdxTransform = symmetry.IndexingRotateSteps;
                            if (rule.IsValid(slice))
                            {
                                matched = true;
                                if (rule.TargetTile == null)
                                {
                                    break;
                                }
                                targetGameObject = rule.TargetTile.gameObject;
                                targetMesh = rule.TargetTile.Mesh;
                                targetOffset = Quaternion.Euler(symmetry.RotationEuler) * rule.TargetTile.transform.localPosition;
                                targetRot = rule.TargetTile.transform.rotation;
                                symmetryRotation = symmetry.RotationEuler;
                                validFlag = true;
                                break;
                            }
                        }
                        if (matched)
                        {
                            break;
                        }
                    }
                }
                // Debug.Log("valid flag is " + validFlag);

                if (validFlag)
                {
                    var instance = GameObject.Instantiate(targetGameObject,
                        position: new Vector3(xPos * TileSize.x, 0, -yPos * TileSize.z) + targetOffset,
                        rotation: targetRot,
                        parent: terrainParent.transform);
                    instance.transform.Rotate(symmetryRotation, Space.World);
                    instance.transform.localScale = targetScale;

                    var renderer = instance.GetComponent<MeshRenderer>();
                    var filter = instance.GetComponent<MeshFilter>();

                    filter.mesh = targetMesh;
                    renderer.material = TerrainMaterial;
                }
                xPos++;
                if (Level[i] == '\n')
                {
                    yPos++;
                    xPos = 0;
                }
            }
        }

        public static string CreateProcessableString2D(char[,] arr)
        {
            if (arr.GetLength(0) % 2 != 1)
            {
                throw new Exception("Array isn't of proper size. It's impossible to know which is the central element.");
            }
            StringBuilder sb =new StringBuilder();
            for (int y = 0; y < arr.GetLength(0); y++)
            {
                for (int x = 0; x < arr.GetLength(1); x++)
                {
                    sb.Append(arr[y, x] == default(char) ? '.' : arr[y,x]);
                }

                if (y != arr.GetLength(0) - 1)
                {
                    sb.Append("\n");
                }
            }
            return sb.ToString();
            
        }
        // public static string CreateProcessableString(char[,,] arr)
        // {
        //     if (arr.GetLength(0) % 2 != 1)
        //     {
        //         throw new Exception("Array isn't of proper size. It's impossible to know which is the central element.");
        //     }
        //     StringBuilder sb =new StringBuilder();
        //     for (int z = 0; z < arr.GetLength(0); z++)
        //     {
        //         for (int y = 0; y < arr.GetLength(1); y++)
        //         {
        //             for (int x = 0; x < arr.GetLength(2); x++)
        //             {
        //                 sb.Append(arr[z, y, x] == default(char) ? '.' : arr[z,y,x]);
        //             }
        //             sb.Append("\n");
        //         }
        //         sb.Append("\t");
        //     }
        //     return sb.ToString();
        // }

        
        //TODO ; change xpand factor
        //TODO ; support 3d
        private (string BlockString, string TileString, int ExpandFactor) ExpandTerrainString(string str, TilesetDomain mode)
        {
            string expanded = str;
            StringBuilder expandSb = new StringBuilder();

            for (int i = 0; i < str.Length; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if ((str[i] == '\n' || str[i] == '\t') && j > 0) break;
                    expandSb.Append(str[i]);
                }
            }

            expanded = expandSb.ToString();
            expanded = string.Join("",expanded.Split('\n').Select(s => s + "\n" + s + "\n"+ s + "\n"));
            if (mode == TilesetDomain.XYZ)
            {
                expanded = expanded.Replace("\t\t\t", "\t");
                expanded = string.Join("",expanded.Split('\t').Select(s => s + "\t" + s + "\t"+ s + "\t"));
            }
            return (str,expanded, 3);
        }

        public List<TerrainTileInstance> ResolveTiles(string blockString)
        {
            return ResolveTiles(ExpandTerrainString(blockString, TilesetDomain.XZ));
        }
        
        public List<TerrainTileInstance> ResolveTiles((string BlockString, string TileString, int ExpandFactor) input)
        {
            // input.Split('\t').Select(s => s.Split('\n').Select(ss => ss.ToCharArray()).ToTwoDimensionalArray())
            var blockArr = input.BlockString.Replace(" ", "").Split('\n').Select(s => s.ToCharArray()).ToTwoDimensionalArray();
            var tileArr = input.TileString.Replace(" ", "").Split('\n').Select(s => s.ToCharArray()).ToTwoDimensionalArray();
            List<TerrainTileInstance> result = new List<TerrainTileInstance>();

            var offset = (int)Mathf.Floor(input.ExpandFactor);
            var centerOffset = new Vector3((BlockSize.x / 2), (BlockSize.y / 2), (BlockSize.z / 2));
            Vector3Int targetScale = Vector3Int.zero;
            for (int i = 0; i < blockArr.GetLength(0); i++)
            {
                for (int j = 0; j < blockArr.GetLength(1); j++)
                {
                    (int y, int x) center = (offset + i, offset + j);
                    center.x -= 2;
                    center.y -= 2;
                    char[,] slice = Array2D.GetSliceSafe(tileArr, center.y, center.x, 5, 5, '.');
                    if (Rules != null)
                    {
                        bool isValid = false;
                        foreach (var rule in Rules.OrderByDescending(r => r.Priority))
                        {
                            foreach (var symmetry in Symmetries.Where(s => rule.Transformations.Contains(s.Name)))
                            {
                                targetScale = new Vector3Int(symmetry.FlipX ? -1 : 1, symmetry.FlipY ? -1 : 1, symmetry.FlipZ ? -1 : 1);
                                rule.ScaleIdxTransform = targetScale;
                                rule.RotStepIdxTransform = symmetry.IndexingRotateSteps;
                                if (rule.IsValid(slice))
                                {
                                    isValid = true;
                                    if (rule.TargetTile == null)
                                    {
                                        break;
                                    }

                                    TerrainTileInstance inst = new TerrainTileInstance()
                                    {
                                        Spawnable = rule.TargetTile.gameObject,
                                        Mesh = rule.TargetTile.Mesh,
                                        IsCenter = center == (Mathf.Floor(blockArr.GetLength(0)/2f),Mathf.Floor(blockArr.GetLength(1)/2f)),
                                        PositionOffset = Quaternion.Euler(symmetry.RotationEuler) * rule.TargetTile.transform.localPosition + (new Vector3( j * TileSize.x, 0, (blockArr.GetLength(0) - i) * TileSize.z) - centerOffset),
                                        RotationOffset = rule.TargetTile.transform.rotation,
                                        WorldspaceEuler = symmetry.RotationEuler// rule.TargetTile.transform.rotation * Quaternion.Euler(symmetry.RotationEuler)
                                    };
                                    result.Add( inst );
                                    break;
                                }
                            }
                            if (isValid)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            return result;
        }
        
        public void OnValidate()
        {
            if (Symmetries == null)
            {
                Symmetries = new List<TerrainSymmetryEntry>();
            }

            if (!Symmetries.Any())
            {
                Symmetries.Add(new TerrainSymmetryEntry()
                {
                    Name = "ID",
                });
            }

            if (Rules != null)
            {
                MaxRuleSize = Rules.OrderByDescending(r => r.RuleSize).First().RuleSize;
            }
        }
    }

    public enum TilesetDomain
    {
        X = 1 << 0,
        Y = 1 << 1,
        Z = 1 << 2,
        XZ = X | Z,
        XY = X | Y,
        YZ = Y | Z,
        XYZ = X | Y | Z
    }
}
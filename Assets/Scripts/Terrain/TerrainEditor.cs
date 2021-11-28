using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Utility;
using Utility.Graph;

namespace Terrain
{
    // [ExecuteInEditMode]
    public class TerrainEditor : MonoBehaviour
    {
        public TerrainTileset Tileset;

        public GameObject TerrainEditSquare;

        public GridGraph<TerrainBlock> Graph = new GridGraph<TerrainBlock>();

        public TerrainBlock SeedBlock;

        [HideInInspector]
        public TerrainBlock BlockBlank;

        private GameObject blockPreview;
        
        private GameObject hitDebug;
        
        private bool centeringSet;
        private Vector3 centeringVector;

        public InputField DebugInputField;

        public int GroundMask => LayerMask.NameToLayer("Ground");
        
        private void Start()
        {
            blockPreview = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            
            hitDebug = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hitDebug.GetComponent<Renderer>().material.color = Color.cyan;
            
            BlockBlank = Instantiate(SeedBlock);
            BlockBlank.gameObject.SetActive(false);
            
            Graph = new GridGraph<TerrainBlock>();
            Graph.AddNode(SeedBlock, Vector3Int.zero);
            ResolveNearby(SeedBlock);
        }

        public string ExportCurrent()
        {
            var sorted = this.Graph.IndexedNodes
                .OrderBy(n => n.Value.Index.y)
                .ThenBy(n => n.Value.Index.z)
                .ThenBy(n => n.Value.Index.x).ToArray();

            Vector3Int lowestIndex = Vector3Int.zero;
            lowestIndex.x = this.Graph.IndexedNodes.OrderBy(n => n.Value.Index.x).First().Value.Index.x;
            lowestIndex.y = this.Graph.IndexedNodes.OrderBy(n => n.Value.Index.y).First().Value.Index.y;
            lowestIndex.z = this.Graph.IndexedNodes.OrderBy(n => n.Value.Index.z).First().Value.Index.z;
            
            Vector3Int highestIndex = Vector3Int.zero;
            highestIndex.x = this.Graph.IndexedNodes.OrderBy(n => n.Value.Index.x).Last().Value.Index.x;
            highestIndex.y = this.Graph.IndexedNodes.OrderBy(n => n.Value.Index.y).Last().Value.Index.y;
            highestIndex.z = this.Graph.IndexedNodes.OrderBy(n => n.Value.Index.z).Last().Value.Index.z;
            

            Vector3Int size = highestIndex - lowestIndex + Vector3Int.one;
            Debug.Log(size);

            StringBuilder level = new StringBuilder();
            for (int i = 0; i < size.y; i++)
            {
                for (int j = 0; j < size.z; j++)
                {
                    for (int k = 0; k < size.x; k++)
                    {
                        var val = Graph[lowestIndex+new Vector3Int(k, i, j)];
                        if (val != null)
                        {
                            level.Append(val.Value.BlockType);
                        }else
                        {
                            level.Append(".");
                        }
                    }
                    level.Append("z");
                }
                level.Append('y');
            }

            (Vector3Int offset, string levelString) levelDataObj = (lowestIndex, level.ToString());
            var result = JsonUtility.ToJson(levelDataObj);
            return result;
        }
        
        public void Import(string dataJson)
        {
            var lvl = JsonUtility.FromJson<(Vector3Int offset, string levelString)>(dataJson);
            Vector3Int indexer = lvl.offset;
            HashSet<Vector3Int> validIndecies = new HashSet<Vector3Int>();
            for (int i = 0; i < lvl.levelString.Length; i++)
            {
                if (lvl.levelString[i] == 'y')
                {
                    indexer.x = lvl.offset.x;
                    indexer.z = lvl.offset.z;
                    continue;
                }
                if (lvl.levelString[i] == 'z')
                {
                    indexer.x = lvl.offset.x;
                    indexer.z += 1;
                    continue;
                }

                if (lvl.levelString[i] != '.')
                {
                    AddBlock(indexer, lvl.levelString[i]);
                    validIndecies.Add(indexer);
                }
                indexer.x += 1;
            }

            foreach (var idx in validIndecies)
            {
                if (idx.x == -6 && idx.z == -9)
                {
                    Debug.Log("Log");
                }
                ResolveToTiles(idx);
            }
        }

        public void AddBlock(Vector3Int index, char blockValue)
        {
            var val = Vector3.Scale(new Vector3(index.x,index.y,index.z), Tileset.BlockSize);
            var instance = GameObject.Instantiate(BlockBlank, val, Quaternion.identity);
            instance.gameObject.SetActive(true);
            instance.BlockType = blockValue;
            Graph.AddNode(instance, index);
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    for (int k = -1; k <= 1; k++)
                    {
                        if (Mathf.Abs(i) + Mathf.Abs(j) + Mathf.Abs(k) > 1) continue; // Only update ordinal neighbours
                        var offset = new Vector3Int(i, j, k);
                        Graph.UpdateNeighbours(index + offset);
                    }
                }
            }          
        }

        public void AddBlock(TerrainBlock source, Vector3Int directionOffset)
        {
            Debug.Log("Adding Block");
            var node = Graph.GetNode(source);
            var newIndex = node.Index + directionOffset;
            if (Graph[newIndex] != null) return;
            var size = directionOffset.x != 0 ? Tileset.BlockSize.x :
                directionOffset.y != 0 ? Tileset.BlockSize.y :
                directionOffset.z != 0 ? Tileset.BlockSize.z : 0;
            
            var instance = GameObject.Instantiate(BlockBlank, source.transform.position + new Vector3(directionOffset.x, directionOffset.y, directionOffset.z) * size, Quaternion.identity);
            instance.gameObject.SetActive(true);
            Graph.AddNode(instance, newIndex);
            
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    for (int k = -1; k <= 1; k++)
                    {
                        if (Mathf.Abs(i) + Mathf.Abs(j) + Mathf.Abs(k) > 1) continue; // Only update ordinal neighbours
                        var offset = new Vector3Int(i, j, k);
                        Graph.UpdateNeighbours(newIndex + offset);
                    }
                }
            }
            ResolveNearby(newIndex);
        }
        

        private void Update()
        {
            GetPointerInput();
            DebugNeighbours();
            GetEditInput();
            GetResolveInputDebug();

            if (Input.GetKeyDown(KeyCode.Alpha0)) {
                var result = ExportCurrent();
                Debug.Log(result);
                DebugInputField.text = result;
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Import(DebugInputField.text);
            }
        }

        public void GetResolveInputDebug()
        {
            if (Input.GetKeyUp(KeyCode.R))
            {
                foreach (var node in Graph.IndexedNodes.Values)
                {
                    ResolveToTiles(node.Index);
                }
            }
        }

        public void ResolveNearby(TerrainBlock block)
        {
            var node = Graph.GetNode(block);
            ResolveNearby(node.Index);
        }

        public void ResolveNearby(Vector3Int idx)
        {
            foreach (var upd in Graph[idx].GetInRange(2))
            {
                ResolveToTiles(upd.Index);
            }
        }

        public void RemoveBlock(TerrainBlock block)
        {
            if (block.Equals(null)) return;
            block.BlockType = '.';
            ResolveNearby(block);
            Graph.RemoveNode(block);
            Destroy(block.gameObject);
            Debug.Log("REMOVING");
        }

        private Transform highlighted;
        private List<Renderer> swapped =new List<Renderer>();
        // private TerrainBlock MousedOverBlock;
        private (bool IsHit, Vector3 HitPoint, TerrainBlock Block) MouseOverData;

        public void GetPointerInput()
        {
            Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            LayerMask lm = new LayerMask();
            var layer =LayerMask.NameToLayer("Kek");
            if (Physics.Raycast(r, out hit, 10000f, lm))
            {
                if (hit.transform.HasComponent<TerrainBlock>())
                {
                    var block = hit.transform.GetComponent<TerrainBlock>();
                    MouseOverData.Block = block;
                    MouseOverData.HitPoint = hit.point;
                    MouseOverData.IsHit = true;
                }
                else
                {
                    MouseOverData.IsHit = false;
                    return;
                }
            }
        }

        private (bool IsValid, Vector3 Position, TerrainBlock SnapNeighbour) previousSnapData;
        
        private Vector3 prevSnapRayPos;

        public void ResolveToTiles(Vector3Int source) 
        {
            var sourceBlock = Graph[source];
            // sqrt to account for diagonals, 2 to account for rule
            var searchDist =  (Mathf.Sqrt(2) * 1.001f) + (Tileset.MaxRuleSize-1);
            var nearbyBlocks = sourceBlock.GetNearbyContiguous(b =>
            {
                // account for non-uniform terrains
                var scaleVec = new Vector3(1 / Tileset.BlockSize.x, 1 / Tileset.BlockSize.y, 1 / Tileset.BlockSize.z);
                var cur = Vector3.Scale(b.Value.transform.position, scaleVec);
                var src = Vector3.Scale(sourceBlock.Value.transform.position, scaleVec);
                var dist = Vector3.Distance(src, cur);
                if (dist < searchDist)
                {
                    return true;
                }
                return false;
            });

            int sz = Tileset.MaxRuleSize * 2 + 1;
            char[,] surroundings = new char[sz,sz];
            // char[,,] surroundings = new char[sz,sz,sz];
            Vector3Int centerBlockIndex = sourceBlock.Index;
            Vector3Int offsetIndex = centerBlockIndex - new Vector3Int(Tileset.MaxRuleSize, Tileset.MaxRuleSize, Tileset.MaxRuleSize);
            
            foreach (var block in nearbyBlocks)
            {
                var ai = block.Index - offsetIndex;
                surroundings[sz-1-ai.z, ai.x] = block.Value.BlockType; // I have no idea why this gets inverted
            }

            var processable = TerrainTileset.CreateProcessableString2D(surroundings);
            var result = Tileset.ResolveTiles(processable);
            if (!centeringSet)
            {
                var center = result.First(r => r.IsCenter);
                this.centeringVector = center.PositionOffset/2;
                this.centeringVector.y -= 1;
                centeringSet = true;
            }
            sourceBlock.Value.SetTiles(result, this.centeringVector);

            // Tileset.CreateTerrain();
        }
        
        #region CRUD

        public string SaveToString()
        {
            return "";
        }

        public void RestoreFromString(string terrain)
        {
            
        }
        
        #endregion

        public void GetEditInput()
        {
            var szBlock = new []{Tileset.BlockSize.x, Tileset.BlockSize.y, Tileset.BlockSize.z}.OrderByDescending(sz => sz).First();
            
            Plane p = new Plane(Vector3.up, MouseOverData.IsHit ? MouseOverData.HitPoint : SeedBlock.transform.position);
            Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
            float enterPoint;
            p.Raycast(r, out enterPoint);
            var matchedPosition = r.GetPoint(enterPoint);
            prevSnapRayPos = matchedPosition;
            
            var closest = Graph.IndexedNodes.Values.OrderBy(b => Vector3.Distance(b.Value.transform.position, matchedPosition)).First();
            if (Vector3.Distance(closest.Value.transform.position, prevSnapRayPos) < szBlock * 1.35f)
            {
                var closestPos = closest.Value.transform.position;
                var dir = (prevSnapRayPos - closestPos).ToOrdinalDirectionVector();
                var newPos = closestPos + Vector3.Scale(dir, Tileset.BlockSize);
                
                blockPreview.gameObject.SetActive(true);   
                blockPreview.transform.position = newPos;
                previousSnapData.Position = newPos;
                previousSnapData.SnapNeighbour = closest.Value;
                previousSnapData.IsValid = true;
            }
            else
            {
                if (previousSnapData.IsValid)
                {
                    if (Vector3.Distance(previousSnapData.Position, matchedPosition) > szBlock)
                    {
                        previousSnapData.IsValid = false;
                    };
                }
                blockPreview.gameObject.SetActive(previousSnapData.IsValid);
                blockPreview.gameObject.transform.position = previousSnapData.Position;
            }
            if (Input.GetMouseButtonUp(0) && previousSnapData.IsValid)
            {
                var dirVec = ( previousSnapData.Position - previousSnapData.SnapNeighbour.transform.position).ToOrdinalDirectionVector();
                Debug.Log($"Spawning at {dirVec}");
                AddBlock(previousSnapData.SnapNeighbour, dirVec);
            }
            
            if (Input.GetMouseButtonUp(1))
            {
                if (MouseOverData.Block == SeedBlock)
                {
                    var rend = SeedBlock.GetComponentsInChildren<Renderer>();
                    foreach (var rnd in rend)
                    {
                        swapped.Add(rnd);
                        rnd.material.color = Color.red;
                    }
                }
                else
                {
                    RemoveBlock(MouseOverData.Block);
                    previousSnapData.IsValid = false;
                }
            }
        }
        
        private void DebugNeighbours()
        {
            if (!previousSnapData.IsValid)
            {
                foreach (var item in swapped)
                {
                    if (item != null)
                    {
                        item.material.color = Color.gray;
                    }
                }
            }
            Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(r, out hit, 10000f, GroundMask ))
            {
                if (hit.transform.HasComponent<TerrainBlock>())
                {
                    if (highlighted != hit.transform)
                    {
                        highlighted = hit.transform;
                        foreach (var item in swapped)
                        {
                            if (item != null)
                            {
                                item.material.color = Color.gray;
                            }
                        }
                    }
                    else
                    {
                        return;
                    }
                    var block = hit.transform.GetComponent<TerrainBlock>();
                    var node = Graph.GetNode(block);
                    MouseOverData.Block = node.Value;
                    var neighbours = Graph.IndexedNodes[node.Index].Neighbours.Select(s => s.Value);
                    var renderers = block.GetComponentsInChildren<Renderer>();
                    foreach (var rnd in renderers)
                    {
                        rnd.material.color = Color.yellow;
                        swapped.Add(rnd);
                        
                    }
                    foreach (var n in neighbours)
                    {
                        if (n != null)
                        {
                            var nRenderers=n.Value.GetComponentsInChildren<Renderer>();
                            foreach (var nRnd in nRenderers)
                            {
                                swapped.Add(nRnd);
                                nRnd.material.color = Color.cyan;
                            }
                        }
                    }
                }
            }
        }
        
        private void OnGUI()
        {
            hitDebug.transform.position = prevSnapRayPos;
            var curDbg = previousSnapData.IsValid ? previousSnapData.Position.ToString() : "INVALID";
            GUI.Box(new Rect(10,10, 100, 20), $"raw-pos: {prevSnapRayPos}");
            GUI.Box(new Rect(10,40, 100, 20), $"snap-pos: {curDbg}");
        }
    }

    public class TerrainLayer
    {
        public List<GameObject> Items;
        
    }

    public class TileInstance
    {
        public string[,] EvaluatedContext;

        public TerrainTileset Tileset;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utility.Graph
{
    public class GridGraph <T>
    {
        public Dictionary<Vector3Int, GridGraphNode<T>> IndexedNodes;
        public GridGraphNode<T> this[Vector3Int idx]
        {
            get => IndexedNodes.ContainsKey(idx) ? IndexedNodes[idx] : null;
            protected set
            {
                if (value == null)
                {
                    GridGraphNode<T> toRemove = this[idx]; 
                    IndexedNodes.Remove(idx);
                    foreach (var neighbour in toRemove.Neighbours)
                    {
                        neighbour.Value?.RemoveNeighbour(toRemove);
                    }
                }
                else if (IndexedNodes.ContainsKey(idx))
                {
                    IndexedNodes[idx] = value;
                }
                else
                {
                    IndexedNodes.Add(idx, value);
                }
            }
        }

        public void AddNode(T val, Vector3Int idx)
        {
            if (IndexedNodes == null)
            {
                IndexedNodes = new Dictionary<Vector3Int, GridGraphNode<T>>();
            }
            this[idx] = new GridGraphNode<T>(val, idx);
        }

        public void RemoveNode(T val)
        {
            if (IndexedNodes.Values.Any(n => n.Value.Equals(val)))
            {
                var targetNode = IndexedNodes.Values.First(n => n.Value.Equals(val));
                this[targetNode.Index] = null;
            };
        }

        public void RemoveNode(Vector3Int idx)
        {
            this[idx] = null;
        }

        public GridGraphNode<T> GetNode(T val)
        {
            if (IndexedNodes.Values.Any(n => n.Value.Equals(val)))
            {
                return IndexedNodes.Values.First(n => n.Value.Equals(val));
            }
            return null;
        }

        public void UpdateNeighbours(Vector3Int position)
        {
            if (!IndexedNodes.ContainsKey(position)) return;
            GridGraphNode<T> nodeAtPos = IndexedNodes[position];
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    for (int k = -1; k <= 1; k++)
                    {
                        if (Mathf.Abs(i)+Mathf.Abs(j)+Mathf.Abs(k) != 1) continue; // Only update ordinal neighbours
                        var offset = new Vector3Int(i, j, k);
                        var current = position + offset;
                        var key = GridGraphNode<T>.DirByVec[offset];
                        
                        if (IndexedNodes.ContainsKey(current))
                        {
                            nodeAtPos.Neighbours[key] = IndexedNodes[current];
                        }
                        else
                        {
                            nodeAtPos.Neighbours[key] = null;
                        }
                    }
                }
            }
        }
    }

    public class GridGraphNode<T>
    {
        public static Dictionary<Vector3, OrdinalDirection> DirByVec = new Dictionary< Vector3,OrdinalDirection>()
        {
            { Vector3.up,OrdinalDirection.Up},
            { Vector3.down,OrdinalDirection.Down},
            { Vector3.left,OrdinalDirection.Left},
            { Vector3.right,OrdinalDirection.Right},
            { Vector3.forward,OrdinalDirection.Forward},
            { Vector3.back,OrdinalDirection.Back}
        };
        public static Dictionary<OrdinalDirection, Vector3> VecByDir = new Dictionary<OrdinalDirection, Vector3>()
        {
            {OrdinalDirection.Up, Vector3.up},
            {OrdinalDirection.Down, Vector3.down},
            {OrdinalDirection.Left, Vector3.left},
            {OrdinalDirection.Right, Vector3.right},
            {OrdinalDirection.Forward, Vector3.forward},
            {OrdinalDirection.Back, Vector3.back}
        };
        
        public T Value;

        public Vector3Int Index;
        
        public Dictionary<OrdinalDirection, GridGraphNode<T>> Neighbours;

        public GridGraphNode(T value, Vector3Int index)
        {
            this.Index = index;
            this.Value = value;
            Neighbours = new Dictionary<OrdinalDirection, GridGraphNode<T>>();
            foreach (var pair in VecByDir)
            {
                this.Neighbours.Add(pair.Key, null);
            }
        }
        
        public GridGraphNode(T value, Vector3Int index, Dictionary<OrdinalDirection, GridGraphNode<T>> neighbours)
        {
            this.Value = value;
            this.Index = index;
            this.Neighbours = neighbours;
            foreach (var pair in VecByDir.Where(dir => !neighbours.ContainsKey(dir.Key)))
            {
                this.Neighbours.Add(pair.Key, null);
            }
        }
        
        #region CRUD
        
        public void RemoveNeighbour(OrdinalDirection dir)
        {
            Neighbours.Remove(dir);
        }
        public void RemoveNeighbour(GridGraphNode<T> val)
        {
            var toRemove = this.Neighbours.First(n => n.Value == val);
            Neighbours.Remove(toRemove.Key);
        }

        public void RemoveNeighbour(T val)
        {
            Func<KeyValuePair<OrdinalDirection, GridGraphNode<T>>, bool> pred = (n) =>
            {
                if (n.Value != null)
                {
                    return n.Value.Value.Equals(val);
                }

                return false;
            };
            if (!this.Neighbours.Any(pred)) return;
            var target = (this.Neighbours.First(pred));
            Neighbours.Remove(target.Key);
        }

        public void AddNeighbour(OrdinalDirection dir, GridGraphNode<T> val)
        {
            if (Neighbours.ContainsKey(dir))
            {
                if (Neighbours[dir] != null)
                {
                    throw new Exception("Cannot replace existing neighbour, must be destroyed soon");
                }
            }
            Neighbours.Add(dir, val);
        }
        
        #endregion

        #region Search Logic
        /// <summary>
        /// Returns all neighbours that match the predicate that are connected to each other
        /// If the initial node does not match the condition, no elements will be returned.
        /// </summary>
        /// <param name="condition">the match condition</param>
        public IEnumerable<GridGraphNode<T>> GetNearbyContiguous(Predicate<GridGraphNode<T>> condition)
        {
            return GetNearbyContiguous(condition, new HashSet<GridGraphNode<T>>(), new HashSet<GridGraphNode<T>>());
        }

        public IEnumerable<GridGraphNode<T>> GetInRange(int range)
        {
            return this.GetNearbyContiguous(n =>
            {
                Vector3Int delta = Index - n.Index;
                for (int i = 0; i < 3; i++)
                {
                    if (Mathf.Abs(delta[i]) > range)
                    {
                        return false;
                    }
                }
                return true;
            });
        }
        
        private IEnumerable<GridGraphNode<T>> GetNearbyContiguous(Predicate<GridGraphNode<T>> condition, HashSet<GridGraphNode<T>> visited, HashSet<GridGraphNode<T>> matches)
        {
            visited.Add(this);
            bool nodeIsValid = condition.Invoke(this);
            if (nodeIsValid)
            {
                matches.Add(this);
                HashSet<GridGraphNode<T>> validNeighbours = new HashSet<GridGraphNode<T>>(Neighbours.Values.Where(n => n != null));
                validNeighbours.ExceptWith(visited);
                foreach (var neighbour in validNeighbours)
                {
                    neighbour.GetNearbyContiguous(condition, visited, matches);
                }
            }
            return matches.ToList();
        }
        #endregion
    }

    public enum OrdinalDirection
    {
        Up,Down,Left,Right,Forward,Back
    }
}
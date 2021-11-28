using System;
using System.Collections.Generic;
using UnityEngine;

namespace Terrain
{
    public class TerrainTile : MonoBehaviour
    {
        public Mesh Mesh;
        
        private void OnValidate()
        {
            try
            {
                if (Mesh == null)
                {
                    Mesh = GetComponent<MeshFilter>().sharedMesh;
                }
            }
            catch (Exception e)
            {
                // No Mesh Filter
            }
        }
    }

    public class TerrainTileInstance
    {
        public GameObject Spawnable;
        
        public Mesh Mesh;

        public bool IsCenter;

        public Vector3 PositionOffset;
        
        public Quaternion RotationOffset;

        public Vector3 WorldspaceEuler;
    }
}
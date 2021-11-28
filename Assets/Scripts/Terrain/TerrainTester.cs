using System;
using UnityEngine;
using UnityEngine.Rendering;
using Matrix4x4 = System.Numerics.Matrix4x4;

namespace Terrain
{
    [ExecuteInEditMode]
    public class TerrainTester : MonoBehaviour
    {
        public TerrainTileset Tileset;

        public MeshFilter TargetMesh;

        public GameObject TerrainParent;
        
        private Mesh CombiMesh;

        private int SubmeshIDCurrent = 0;

        // public void Update()
        // {
        //     if (Input.GetKeyDown(KeyCode.Z))
        //     {
        //         Vector2 v2i = new Vector2Int(10,32);
        //         System.Numerics.Matrix4x4 mat = new Matrix4x4();
        //         mat.M11 = v2i.x;
        //         mat.M22 = v2i.y;
        //         Matrix4x4 inverted;
        //         Matrix4x4.Invert(mat, out inverted);
        //         Matrix4x4 transposed = Matrix4x4.Transpose(mat);
        //         var index = (10, 32);
        //         
        //     }
        //     if (Input.GetKeyDown(KeyCode.T))
        //     {
        //         SubmeshIDCurrent = 1;
        //         CombiMesh = new Mesh();
        //         Tileset.CreateTerrain(TerrainParent);
        //     }
        // }

        private void OnDrawGizmosSelected()
        {
            #if UNITY_EDITOR
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 10);
            // throw new NotImplementedException();
            #endif
        }

        public void OnDrawGizmos()
        {
            #if UNITY_EDITOR
            Gizmos.color = Color.cyan;
            Gizmos.DrawCube(transform.position, Vector3.one * 0.2f);
            #endif
        }

        // public void CreateTerrain()
        // {
        //     
        // }
    }
}
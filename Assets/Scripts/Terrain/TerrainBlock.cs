using System;
using System.Collections.Generic;
using UnityEngine;

namespace Terrain
{
    public class TerrainBlock : MonoBehaviour
    {
        public GameObject Preview;
        
        public char BlockType; // g for ground . for empty 
            
        public List<TerrainTile> Tiles;  

        public TerrainEditor Editor;

        // public Vector3Int BlockIndex;


        private void Awake()
        {
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                // InitializeExtensionQuads();
            }
        }

        public void SetTiles(List<TerrainTileInstance> result, Vector3 editorOffset)
        {
            var prend = Preview.gameObject.GetComponent<Renderer>();
            prend.enabled = false;
            for (int i = 0; i < Tiles.Count; i++)
            {
                Destroy(Tiles[i].gameObject);
            }
            Tiles.Clear();
            // var center = result.First(r => r.IsCenter);
            // var centeringPos = prend.bounds.size;
            // centeringPos.y = 0;
            
            for (int i = 0; i < result.Count; i++)
            {
                GameObject instance = Instantiate(result[i].Spawnable, transform);
                instance.transform.rotation = result[i].RotationOffset;
                instance.transform.localPosition = result[i].PositionOffset -editorOffset;
                instance.transform.Rotate(result[i].WorldspaceEuler, Space.World);
                Tiles.Add(instance.GetComponent<TerrainTile>());
            }
        }
    }
}
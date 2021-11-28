using UnityEngine;

namespace Terrain.Data
{
    [System.Serializable]
    public class TerrainSymmetryEntry
    {
        public string Name;
        /// <summary>
        /// 1 step = 90 degrees, so [2,3] rotates by 180 and then 270 degrees
        /// </summary>
        public Vector2Int IndexingRotateSteps;

        public Vector3 RotationEuler;

        public bool FlipX;

        public bool FlipY;

        public bool FlipZ;
    }
}
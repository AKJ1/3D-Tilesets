using System;
using UnityEngine;

namespace Terrain
{
    public class TerrainExpansionListener : MonoBehaviour
    {
        public Action<int> OnClicked;

        private bool IsMouseOver;
        
        private static readonly int Emission = Shader.PropertyToID("_EmissionColor");
        private static readonly int Base = Shader.PropertyToID("_BaseColor");

        public float SelectedBrightness = 0.7f;
        public float UnselectedBrightness = 0.4f;

        public Color SelectedTint = Color.yellow;

        private Color selectedColor;
        private Color unselectedColor;
        
        LayerMask TerrainEditorMask => LayerMask.NameToLayer("TerrainEditor");

        private void Start()
        {
            selectedColor = new Color(0,0,0,SelectedBrightness);
            unselectedColor = new Color(0,0,0,UnselectedBrightness);
            gameObject.layer = TerrainEditorMask;
        }

        public void Update()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit = new RaycastHit();
            IsMouseOver = false;
            if (Physics.Raycast(ray,  out hit, 1000000f, TerrainEditorMask))
            {
                // Debug.Log("Mouse Over");
                // Debug.Log($"Hit transform is {hit.transform.name}, while i am {transform.name}");
                if (hit.transform == transform)
                {
                    IsMouseOver = true;
                }
            }

            if (IsMouseOver)
            {
                Color c = new Color(0,0,0, SelectedBrightness);
                GetComponent<Renderer>().material.SetColor(Emission, SelectedTint);
                GetComponent<Renderer>().material.SetColor(Base, selectedColor);
                // GetComponent<Renderer>().material.SetColor("_Emission"); = Color.yellow;
                // GetComponent<Renderer>().material.color = Color.yellow;
                // Left mouse button - exxxpand
                if (Input.GetMouseButtonUp(0))
                {
                    OnClicked?.Invoke(1);
                }
                // Right mouse button - shrinkkk
                if (Input.GetMouseButtonUp(1))
                {
                    OnClicked?.Invoke(-1);
                }
            }
            else
            {
                GetComponent<Renderer>().material.SetColor(Emission, Color.white);
                GetComponent<Renderer>().material.SetColor( Base, unselectedColor);
                // GetComponent<Renderer>().material.color = Color.white;
            }
        }
    }
}
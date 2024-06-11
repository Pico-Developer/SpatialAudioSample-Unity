/*******************************************************************************
Copyright © 2015-2024 PICO Technology Co., Ltd.All rights reserved.

NOTICE：All information contained herein is, and remains the property of
PICO Technology Co., Ltd. The intellectual and technical concepts
contained herein are proprietary to PICO Technology Co., Ltd. and may be
covered by patents, patents in process, and are protected by trade secret or
copyright law. Dissemination of this information or reproduction of this
material is strictly forbidden unless prior written permission is obtained from
PICO Technology Co., Ltd.
*******************************************************************************/
using System.Collections.Generic;
using UnityEngine;

namespace PicoSpatialAudioSample.Runtime
{
    public class SourceDirectivityController : MonoBehaviour
    {
        [SerializeField] private PXR_Audio_Spatializer_AudioSource source;
        public List<MeshFilter> meshFilters;
        [Range(0.0f, 1.0f)] public float alpha = 0.0f;
        public float order = 1.0f;
        private bool _polarPatternChanged = false;

        private Mesh _displayMesh;

        // Start is called before the first frame update
        void Start()
        {
            _displayMesh = new Mesh();
            UpdatePolarPatternMesh(_displayMesh, alpha, order);
            foreach (MeshFilter meshFilter in meshFilters)
            {
                meshFilter.mesh = _displayMesh;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (_polarPatternChanged)
            {
                source.SetDirectivity(alpha, order);
                UpdatePolarPatternMesh(_displayMesh, alpha, order);
                _polarPatternChanged = false;
            }
        }

        public void SetAlpha(float alpha)
        {
            this.alpha = alpha;
            _polarPatternChanged = true;
        }

        public void SetOrder(float order)
        {
            this.order = order;
            _polarPatternChanged = true;
        }

        public void SetVisibility(bool visible)
        {
            foreach (MeshFilter meshFilter in meshFilters)
            {
                meshFilter.GetComponent<MeshRenderer>().enabled = visible;
            }
        }

        private void UpdatePolarPatternMesh(Mesh mesh, float alpha, float order)
        {
            if (mesh == null)
                mesh = new Mesh();
            Vector2[] cardioidVertices2D = GeneratePolarPatternVertices2D(alpha, order, 90);
            int numVertices = cardioidVertices2D.Length * 2;
            Vector3[] vertices = new Vector3[numVertices];
            for (int i = 0; i < cardioidVertices2D.Length; ++i)
            {
                var vertex2D = cardioidVertices2D[i];
                vertices[i] = new Vector3(vertex2D.x, 0.0f, vertex2D.y);
                vertices[cardioidVertices2D.Length + i] = Quaternion.AngleAxis(45, Vector3.forward) *
                                                          new Vector3(vertex2D.x, 0.0f, vertex2D.y);
            }

            int[] indices = new int[cardioidVertices2D.Length * 2 * 3];
            for (int idx = 0; idx < cardioidVertices2D.Length - 1; ++idx)
            {
                indices[idx * 6 + 0] = idx;
                indices[idx * 6 + 1] = idx + 1;
                indices[idx * 6 + 2] = idx + cardioidVertices2D.Length;
                indices[idx * 6 + 3] = idx + 1;
                indices[idx * 6 + 4] = idx + cardioidVertices2D.Length + 1;
                indices[idx * 6 + 5] = idx + cardioidVertices2D.Length;
            }

            // Construct a new mesh for the gizmo.
            mesh.vertices = vertices;
            mesh.triangles = indices;
            mesh.RecalculateNormals();
        }

        private Vector2[] GeneratePolarPatternVertices2D(float alpha, float order, int numVertices)
        {
            Vector2[] points = new Vector2[numVertices];
            float interval = Mathf.PI / (numVertices - 1);
            for (int i = 0; i < numVertices; ++i)
            {
                float theta = 0.0f;
                if (i != numVertices - 1)
                    theta = i * interval;
                else
                    theta = Mathf.PI;
                // Magnitude |r| for |theta| in radians.
                float r = Mathf.Pow(Mathf.Abs((1 - alpha) + alpha * Mathf.Cos(theta)), order);
                points[i] = new Vector2(r * Mathf.Sin(theta), r * Mathf.Cos(theta));
            }

            return points;
        }

        private void OnDrawGizmosSelected()
        {
            _polarPatternChanged = true;
        }
    }
}
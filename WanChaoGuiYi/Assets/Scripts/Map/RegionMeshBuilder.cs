using System.Collections.Generic;
using UnityEngine;

namespace WanChaoGuiYi
{
    public static class RegionMeshBuilder
    {
        public static Mesh Build(MapRegionShapeDefinition shape)
        {
            if (shape == null || shape.boundary == null || shape.boundary.Length < 3)
            {
                return null;
            }

            Vector3[] vertices = new Vector3[shape.boundary.Length];
            for (int i = 0; i < shape.boundary.Length; i++)
            {
                vertices[i] = new Vector3(shape.boundary[i].x, shape.boundary[i].y, 0f);
            }

            int[] triangles = Triangulate(vertices);
            if (triangles.Length < 3) return null;

            Mesh mesh = new Mesh();
            mesh.name = "RegionMesh_" + shape.regionId;
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            return mesh;
        }

        private static int[] Triangulate(Vector3[] vertices)
        {
            List<int> indices = new List<int>();
            List<int> remaining = new List<int>();

            bool clockwise = CalculateSignedArea(vertices) < 0f;
            for (int i = 0; i < vertices.Length; i++)
            {
                remaining.Add(i);
            }

            int guard = vertices.Length * vertices.Length;
            while (remaining.Count > 3 && guard > 0)
            {
                guard--;
                bool clipped = false;

                for (int i = 0; i < remaining.Count; i++)
                {
                    int previousIndex = remaining[(i - 1 + remaining.Count) % remaining.Count];
                    int currentIndex = remaining[i];
                    int nextIndex = remaining[(i + 1) % remaining.Count];

                    if (!IsEar(vertices, remaining, previousIndex, currentIndex, nextIndex, clockwise))
                    {
                        continue;
                    }

                    if (clockwise)
                    {
                        indices.Add(previousIndex);
                        indices.Add(currentIndex);
                        indices.Add(nextIndex);
                    }
                    else
                    {
                        indices.Add(previousIndex);
                        indices.Add(nextIndex);
                        indices.Add(currentIndex);
                    }

                    remaining.RemoveAt(i);
                    clipped = true;
                    break;
                }

                if (!clipped)
                {
                    return FanTriangulate(vertices, clockwise);
                }
            }

            if (remaining.Count == 3)
            {
                if (clockwise)
                {
                    indices.Add(remaining[0]);
                    indices.Add(remaining[1]);
                    indices.Add(remaining[2]);
                }
                else
                {
                    indices.Add(remaining[0]);
                    indices.Add(remaining[2]);
                    indices.Add(remaining[1]);
                }
            }

            return indices.ToArray();
        }

        private static int[] FanTriangulate(Vector3[] vertices, bool clockwise)
        {
            List<int> indices = new List<int>();
            for (int i = 1; i < vertices.Length - 1; i++)
            {
                if (clockwise)
                {
                    indices.Add(0);
                    indices.Add(i);
                    indices.Add(i + 1);
                }
                else
                {
                    indices.Add(0);
                    indices.Add(i + 1);
                    indices.Add(i);
                }
            }

            return indices.ToArray();
        }

        private static bool IsEar(Vector3[] vertices, List<int> remaining, int previousIndex, int currentIndex, int nextIndex, bool clockwise)
        {
            Vector2 a = ToVector2(vertices[previousIndex]);
            Vector2 b = ToVector2(vertices[currentIndex]);
            Vector2 c = ToVector2(vertices[nextIndex]);

            float cross = Cross(a, b, c);
            if (clockwise)
            {
                if (cross >= 0f) return false;
            }
            else
            {
                if (cross <= 0f) return false;
            }

            for (int i = 0; i < remaining.Count; i++)
            {
                int pointIndex = remaining[i];
                if (pointIndex == previousIndex || pointIndex == currentIndex || pointIndex == nextIndex) continue;

                if (PointInTriangle(ToVector2(vertices[pointIndex]), a, b, c))
                {
                    return false;
                }
            }

            return true;
        }

        private static float CalculateSignedArea(Vector3[] vertices)
        {
            float area = 0f;
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 a = vertices[i];
                Vector3 b = vertices[(i + 1) % vertices.Length];
                area += a.x * b.y - b.x * a.y;
            }

            return area * 0.5f;
        }

        private static float Cross(Vector2 a, Vector2 b, Vector2 c)
        {
            return (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
        }

        private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float c1 = Cross(p, a, b);
            float c2 = Cross(p, b, c);
            float c3 = Cross(p, c, a);
            bool hasNegative = c1 < 0f || c2 < 0f || c3 < 0f;
            bool hasPositive = c1 > 0f || c2 > 0f || c3 > 0f;
            return !(hasNegative && hasPositive);
        }

        private static Vector2 ToVector2(Vector3 point)
        {
            return new Vector2(point.x, point.y);
        }
    }
}

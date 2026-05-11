using System.Collections.Generic;
using UnityEngine;

namespace WanChaoGuiYi
{
    public static class RegionMeshBuilder
    {
        private const float Epsilon = 0.0001f;

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

            if (!IsValidSimplePolygon(vertices))
            {
                return null;
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
                    return new int[0];
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

        private static bool IsValidSimplePolygon(Vector3[] vertices)
        {
            if (vertices == null || vertices.Length < 3) return false;
            if (Mathf.Abs(CalculateSignedArea(vertices)) <= Epsilon) return false;

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector2 a1 = ToVector2(vertices[i]);
                Vector2 a2 = ToVector2(vertices[(i + 1) % vertices.Length]);
                if ((a2 - a1).sqrMagnitude <= Epsilon * Epsilon)
                {
                    return false;
                }

                for (int j = i + 1; j < vertices.Length; j++)
                {
                    if (i == j) continue;
                    if ((i + 1) % vertices.Length == j) continue;
                    if (i == (j + 1) % vertices.Length) continue;

                    Vector2 b1 = ToVector2(vertices[j]);
                    Vector2 b2 = ToVector2(vertices[(j + 1) % vertices.Length]);
                    if (SegmentsIntersect(a1, a2, b1, b2))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool IsEar(Vector3[] vertices, List<int> remaining, int previousIndex, int currentIndex, int nextIndex, bool clockwise)
        {
            Vector2 a = ToVector2(vertices[previousIndex]);
            Vector2 b = ToVector2(vertices[currentIndex]);
            Vector2 c = ToVector2(vertices[nextIndex]);

            float cross = Cross(a, b, c);
            if (clockwise)
            {
                if (cross >= -Epsilon) return false;
            }
            else
            {
                if (cross <= Epsilon) return false;
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

        private static bool SegmentsIntersect(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
        {
            float d1 = Cross(a1, a2, b1);
            float d2 = Cross(a1, a2, b2);
            float d3 = Cross(b1, b2, a1);
            float d4 = Cross(b1, b2, a2);

            if (((d1 > Epsilon && d2 < -Epsilon) || (d1 < -Epsilon && d2 > Epsilon)) &&
                ((d3 > Epsilon && d4 < -Epsilon) || (d3 < -Epsilon && d4 > Epsilon)))
            {
                return true;
            }

            return IsPointOnSegment(b1, a1, a2) ||
                   IsPointOnSegment(b2, a1, a2) ||
                   IsPointOnSegment(a1, b1, b2) ||
                   IsPointOnSegment(a2, b1, b2);
        }

        private static bool IsPointOnSegment(Vector2 point, Vector2 start, Vector2 end)
        {
            if (Mathf.Abs(Cross(start, end, point)) > Epsilon) return false;
            return point.x >= Mathf.Min(start.x, end.x) - Epsilon &&
                   point.x <= Mathf.Max(start.x, end.x) + Epsilon &&
                   point.y >= Mathf.Min(start.y, end.y) - Epsilon &&
                   point.y <= Mathf.Max(start.y, end.y) + Epsilon;
        }

        private static Vector2 ToVector2(Vector3 point)
        {
            return new Vector2(point.x, point.y);
        }
    }
}

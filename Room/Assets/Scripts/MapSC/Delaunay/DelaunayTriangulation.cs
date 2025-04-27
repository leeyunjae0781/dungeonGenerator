using System.Collections.Generic;
using UnityEngine;

public static class DelaunayTriangulation
{
    public class Edge
    {
        public Vector2 v0;
        public Vector2 v1;
        public float weight;

        public Edge(Vector2 v0, Vector2 v1)
        {
            this.v0 = v0;
            this.v1 = v1;
            weight = Vector2.Distance(v0, v1);
        }

        public override bool Equals(object other)
        {
            if (false == (other is Edge))
            {
                return false;
            }

            return Equals((Edge)other);
        }

        public bool Equals(Edge edge)
        {
            return ((this.v0.Equals(edge.v0) && this.v1.Equals(edge.v1)) || (this.v0.Equals(edge.v1) && this.v1.Equals(edge.v0)));
        }

        public override int GetHashCode()
        {
            return v0.GetHashCode() ^ (v1.GetHashCode() << 2);
        }
    }

    public class Circle
    {
        public Vector2 center;
        public float radius;


        public Circle(Vector2 center, float radius)
        {
            this.center = center;
            this.radius = radius;
        }

        public bool Contains(Vector2 point)
        {
            float d = Vector2.Distance(center, point);
            if (radius < d)
            {
                return false;
            }

            return true;
        }
    }

    public class Triangle
    {
        public Vector2 a;
        public Vector2 b;
        public Vector2 c;
        public Circle circumCircle;
        public List<Edge> edges;

        public Triangle(Vector2 a, Vector2 b, Vector2 c)
        {
            this.a = a;
            this.b = b;
            this.c = c;

            this.circumCircle = calcCircumCircle(a, b, c);
            this.edges = new List<Edge>();
            this.edges.Add(new Edge(this.a, this.b));
            this.edges.Add(new Edge(this.b, this.c));
            this.edges.Add(new Edge(this.c, this.a));

        }

        public override bool Equals(object other)
        {
            if (false == (other is Triangle))
            {
                return false;
            }

            return Equals((Triangle)other);
        }

        public override int GetHashCode()
        {
            return a.GetHashCode() ^ (b.GetHashCode() << 2) ^ (c.GetHashCode() >> 2);
        }

        public bool Equals(Triangle triangle)
        {
            return this.a == triangle.a && this.b == triangle.b && this.c == triangle.c;
        }

        private Circle calcCircumCircle(Vector2 a, Vector2 b, Vector2 c)
        {
            // 출처: 삼각형 외접원 구하기 - https://kukuta.tistory.com/444

            if (a == b || b == c || c == a) // 같은 점이 있음. 삼각형 아님. 외접원 구할 수 없음.
            {
                return null;
            }

            float mab = (b.x - a.x) / (b.y - a.y) * -1.0f;  // 직선 ab에 수직이등분선의 기울기
            float a1 = (b.x + a.x) / 2.0f;                  // 직선 ab의 x축 중심 좌표
            float b1 = (b.y + a.y) / 2.0f;                  // 직선 ab의 y축 중심 좌표

            // 직선 bc
            float mbc = (b.x - c.x) / (b.y - c.y) * -1.0f;  // 직선 bc에 수직이등분선의 기울기
            float a2 = (b.x + c.x) / 2.0f;                  // 직선 bc의 x축 중심 좌표
            float b2 = (b.y + c.y) / 2.0f;                  // 직선 bc의 y축 중심 좌표

            if (mab == mbc)     // 두 수직이등분선의 기울기가 같음. 평행함. 
            {
                return null;    // 교점 구할 수 없음
            }

            float x = (mab * a1 - mbc * a2 + b2 - b1) / (mab - mbc);
            float y = mab * (x - a1) + b1;

            if (b.x == a.x)     // 수직이등분선의 기울기가 0인 경우(수평선)
            {
                x = a2 + (b1 - b2) / mbc;
                y = b1;
            }

            if (b.y == a.y)     // 수직이등분선의 기울기가 무한인 경우(수직선)
            {
                x = a1;
                if (0.0f == mbc)
                {
                    y = b2;
                }
                else
                {
                    y = mbc * (a1 - a2) + b2;
                }
            }

            if (b.x == c.x)     // 수직이등분선의 기울기가 0인 경우(수평선)
            {
                x = a1 + (b2 - b1) / mab;
                y = b2;
            }

            if (b.y == c.y)     // 수직이등분선의 기울기가 무한인 경우(수직선)
            {
                x = a2;
                if (0.0f == mab)
                {
                    y = b1;
                }
                else
                {
                    y = mab * (a2 - a1) + b1;
                }
            }

            Vector2 center = new Vector2(x, y);
            float radius = Vector2.Distance(center, a);

            return new Circle(center, radius);
        }
    }

    private class DisjointSet
    {
        private Dictionary<Vector2, Vector2> parent = new Dictionary<Vector2, Vector2>();

        public void MakeSet(List<Vector2> points)
        {
            foreach (var p in points)
            {
                parent[p] = p;
            }
        }

        public Vector2 Find(Vector2 p)
        {
            if (!parent.ContainsKey(p))
                parent[p] = p;

            if (parent[p] != p)
                parent[p] = Find(parent[p]);

            return parent[p];
        }

        public void Union(Vector2 a, Vector2 b)
        {
            Vector2 rootA = Find(a);
            Vector2 rootB = Find(b);
            if (rootA != rootB)
            {
                parent[rootB] = rootA;
            }
        }
    }


    public static List<Triangle> CreateDelaunay(List<Vector2> roomCenters)
    {
        Triangle superTriangle = null;
        List<Triangle> triangles = new List<Triangle>();
    
        List<Vector2> points = new List<Vector2>();


        // 모든 점을 포함하는 존나 큰 삼각형 찍기
        superTriangle = CreateSuperTriangle(roomCenters);
        
        if (null == superTriangle)
        {
            Debug.Log("슈퍼삼각형이없습니다!");
            return triangles;
        }

        triangles.Add(superTriangle);

        foreach (var point in roomCenters)
        {
            // 방 중심을 마구찍으면서 삼각형들 갱신
            AddPoint(point, triangles);
        }

        RemoveSuperTriangle(superTriangle, triangles);

        
        return triangles;
    }
    private static void AddPoint(Vector2 point, List<Triangle> triangles)
    {
        // 추가 되는 외접원에 점이 포함되어 삭제될 삼각형 목록
        List<Triangle> badTriangles = new List<Triangle>();
        foreach (var triangle in triangles)
        {
            if (true == triangle.circumCircle.Contains(point)) // 삼각형의 외접원에 점이 포함 되는가
            {
                badTriangles.Add(triangle);
            }
        }

        // 삼각형 삭제로 인한 공백 영역의 경계 찾기
        List<Edge> polygon = new List<Edge>();
        foreach (var triangle in badTriangles)
        {
            List<Edge> edges = triangle.edges;

            foreach (Edge edge in edges)
            {
                // find unique edge
                // 삼각형과 공유 되지 않는 변만이 공백 영역의 경계가 된다.
                bool unique = true;
                foreach (var other in badTriangles)
                {
                    if (true == triangle.Equals(other))
                    {
                        continue;
                    }

                    foreach (var otherEdge in other.edges)
                    {
                        if (true == edge.Equals(otherEdge))
                        {
                            unique = false;
                            break;
                        }
                    }

                    if (false == unique)
                    {
                        break;
                    }
                }

                if (true == unique)
                {
                    polygon.Add(edge);
                }
            }
        }

        foreach (var badTriangle in badTriangles)
        {
            triangles.Remove(badTriangle);
        }

        // 공백 경계와 새로 추가된 점들을 연결해 새로운 삼각형 생성
        foreach (Edge edge in polygon)
        {
            Triangle triangle = CreateTriangle(edge.v0, edge.v1, point);
            if (null == triangle)
            {
                continue;
            }
            triangles.Add(triangle);
        }
    }
    private static void RemoveSuperTriangle(Triangle superTriangle, List<Triangle> triangles)
    {
        if (null == superTriangle)
        {
            return;
        }

        List<Triangle> remove = new List<Triangle>();
        foreach (var triangle in triangles)
        {
            if (true == (triangle.a == superTriangle.a || triangle.a == superTriangle.b || triangle.a == superTriangle.c ||
                         triangle.b == superTriangle.a || triangle.b == superTriangle.b || triangle.b == superTriangle.c ||
                         triangle.c == superTriangle.a || triangle.c == superTriangle.b || triangle.c == superTriangle.c
               )
            )
            {
                remove.Add(triangle);
            }
        }

        foreach (var triangle in remove)
        {
            triangles.Remove(triangle);
        }
    }

    private static Triangle CreateSuperTriangle(List<Vector2> points)
    {
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;

        foreach (var point in points)
        {
            minX = Mathf.Min(minX, point.x);
            maxX = Mathf.Max(maxX, point.x);
            minY = Mathf.Min(minY, point.y);
            maxY = Mathf.Max(maxY, point.y);
        }

        float dx = maxX - minX;
        float dy = maxY - minY;

        // super triangle을 포인트 리스트 보다 크게 잡는 이유는
        // super triangle의 변과 포인트가 겹치게 되면 삼각형이 아닌 직선이 되므로 델로네 삼각분할을 적용할 수 없기 때문이다.
        // 어떤 글에서는 이거보다 2배는 더 큰 삼각형이 필요하다는데 우리맵은 작아서 괜찮을 거 같다.
        Vector2 a = new Vector2(minX - dx, minY - dy);
        Vector2 b = new Vector2(minX - dx, maxY + dy * 3);
        Vector2 c = new Vector2(maxX + dx * 3, minY - dy);

        // super triangle이 직선인 경우 리턴
        if (a == b || b == c || c == a)
        {
            return null;
        }

        return CreateTriangle(a, b, c);
    }
    private static Triangle CreateTriangle(Vector2 a, Vector2 b, Vector2 c)
    {
        if (a == b || b == c || c == a)
        {
            return null;
        }

        Triangle triangle = new Triangle(a, b, c);
        
        Circle circle = triangle.circumCircle;

        return triangle;
    }

    public static List<Edge> GenerateMST(List<Vector2> points, List<Edge> edges)
    {
        List<Edge> result = new List<Edge>();

        // 1. 엣지 가중치 순으로 정렬
        edges.Sort((a, b) => a.weight.CompareTo(b.weight));

        // 2. 각각 점을 따로 그룹으로 만들어줌
        DisjointSet set = new DisjointSet();
        set.MakeSet(points);

        // 3. 엣지 하나씩 보면서 MST 만들기
        foreach (var edge in edges)
        {
            if (set.Find(edge.v0) != set.Find(edge.v1))
            {
                result.Add(edge);
                set.Union(edge.v0, edge.v1);
            }
        }

        return result;
    }
}
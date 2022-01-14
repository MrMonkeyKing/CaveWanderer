using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Generator
{
    public class MarchingSquares : MeshGenerator

    {
        public MeshFilter walls;

        public SquareGrid MySquareGrid;
        private List<Vector3> _vertecies;
        private List<int> _triangles;

        private Dictionary<int, List<Triangle>> _triangleDictionary = new Dictionary<int, List<Triangle>>();
        private List<List<int>> _outlines = new List<List<int>>();
        private HashSet<int> _checkedVertecies = new HashSet<int>();

        public override void GenerateMesh(int[,] map, float squareSize)
        {
            _outlines.Clear();
            _checkedVertecies.Clear();
            _triangleDictionary.Clear();

            MySquareGrid = new SquareGrid(map, squareSize);
            _vertecies = new List<Vector3>();
            _triangles = new List<int>();


            for (int x = 0; x < MySquareGrid.Squares.GetLength(0); x++)
            {
                for (int y = 0; y < MySquareGrid.Squares.GetLength(1); y++)
                {
                    TriangulateSquare(MySquareGrid.Squares[x, y]);
                }
            }

            Mesh mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = mesh;

            mesh.vertices = _vertecies.ToArray();
            mesh.triangles = _triangles.ToArray();

            CreateWallMesh();
        }

        #region private methods

        private void CreateWallMesh()
        {
            CalculateMeshOutlines();

            List<Vector3> wallVertecies = new List<Vector3>();
            List<int> wallTriangles = new List<int>();
            Mesh wallMesh = new Mesh();
            float wallHeight = 5;

            foreach (List<int> outline in _outlines)
            {
                for (int i = 0; i < outline.Count - 1; i++)
                {
                    int startIndex = wallVertecies.Count;
                    wallVertecies.Add(_vertecies[outline[i]]); // left
                    wallVertecies.Add(_vertecies[outline[i + 1]]); // right
                    wallVertecies.Add(_vertecies[outline[i]] - Vector3.up * wallHeight); // bottom left
                    wallVertecies.Add(_vertecies[outline[i + 1]] - Vector3.up * wallHeight); // bottom right

                    wallTriangles.Add(startIndex + 0);
                    wallTriangles.Add(startIndex + 2);
                    wallTriangles.Add(startIndex + 3);
                    wallTriangles.Add(startIndex + 3);
                    wallTriangles.Add(startIndex + 1);
                    wallTriangles.Add(startIndex + 0);
                }
            }

            wallMesh.vertices = wallVertecies.ToArray();
            wallMesh.triangles = wallTriangles.ToArray();

            walls.mesh = wallMesh;
        }

        private void TriangulateSquare(Square square)
        {
            switch (square.Configuration)
            {
                case 0:
                    break;
                // 1 point
                case 1:
                    MeshFromPoints(square.CenterLeft, square.CenterBottom, square.BottomLeft);
                    break;
                case 2:
                    MeshFromPoints(square.BottomRight, square.CenterBottom, square.CenterRight);
                    break;
                case 4:
                    MeshFromPoints(square.TopRight, square.CenterRight, square.CenterTop);
                    break;
                case 8:
                    MeshFromPoints(square.TopLeft, square.CenterTop, square.CenterLeft);
                    break;
                // 2 points
                case 3:
                    MeshFromPoints(square.CenterRight, square.BottomRight, square.BottomLeft, square.CenterLeft);
                    break;
                case 6:
                    MeshFromPoints(square.CenterTop, square.TopRight, square.BottomRight, square.CenterBottom);
                    break;
                case 9:
                    MeshFromPoints(square.TopLeft, square.CenterTop, square.CenterBottom, square.BottomLeft);
                    break;
                case 12:
                    MeshFromPoints(square.TopLeft, square.TopRight, square.CenterRight, square.CenterLeft);
                    break;
                case 5:
                    MeshFromPoints(square.CenterTop, square.TopRight, square.CenterRight, square.CenterBottom,
                        square.BottomLeft, square.CenterLeft);
                    break;
                case 10:
                    MeshFromPoints(square.TopLeft, square.CenterTop, square.CenterRight, square.BottomRight,
                        square.CenterBottom, square.CenterLeft);
                    break;
                // 3 points
                case 7:
                    MeshFromPoints(square.CenterTop, square.TopRight, square.BottomRight, square.BottomLeft,
                        square.CenterLeft);
                    break;
                case 11:
                    MeshFromPoints(square.TopLeft, square.CenterTop, square.CenterRight, square.BottomRight,
                        square.BottomLeft);
                    break;
                case 13:
                    MeshFromPoints(square.TopLeft, square.TopRight, square.CenterRight, square.CenterBottom,
                        square.BottomLeft);
                    break;
                case 14:
                    MeshFromPoints(square.TopLeft, square.TopRight, square.BottomRight, square.CenterBottom,
                        square.CenterLeft);
                    break;
                // 4 points
                case 15:
                    MeshFromPoints(square.TopLeft, square.TopRight, square.BottomRight, square.BottomLeft);
                    _checkedVertecies.Add(square.TopLeft.VertexIndex);
                    _checkedVertecies.Add(square.TopRight.VertexIndex);
                    _checkedVertecies.Add(square.BottomRight.VertexIndex);
                    _checkedVertecies.Add(square.BottomLeft.VertexIndex);
                    break;
            }
        }

        private void MeshFromPoints(params Node[] points)
        {
            AssignVertecies(points);

            if (points.Length >= 3)
            {
                CreateTriangle(points[0], points[1], points[2]);
            }

            if (points.Length >= 4)
            {
                CreateTriangle(points[0], points[2], points[3]);
            }

            if (points.Length >= 5)
            {
                CreateTriangle(points[0], points[3], points[4]);
            }

            if (points.Length >= 6)
            {
                CreateTriangle(points[0], points[4], points[5]);
            }
        }

        private void AssignVertecies(Node[] points)
        {
            for (int i = 0; i < points.Length; i++)
            {
                if (points[i].VertexIndex == -1)
                {
                    points[i].VertexIndex = _vertecies.Count;
                    _vertecies.Add(points[i].Position);
                }
            }
        }

        private void CreateTriangle(Node a, Node b, Node c)
        {
            _triangles.Add(a.VertexIndex);
            _triangles.Add(b.VertexIndex);
            _triangles.Add(c.VertexIndex);

            Triangle triangle = new Triangle(a.VertexIndex, b.VertexIndex, c.VertexIndex);
            AddTriangleToDictionary(triangle.VertexIndexA, triangle);
            AddTriangleToDictionary(triangle.VertexIndexB, triangle);
            AddTriangleToDictionary(triangle.VertexIndexC, triangle);
        }

        private void AddTriangleToDictionary(int vertexIndexKey, Triangle triangle)
        {
            if (_triangleDictionary.ContainsKey(vertexIndexKey))
            {
                _triangleDictionary[vertexIndexKey].Add(triangle);
            }
            else
            {
                List<Triangle> triangleList = new List<Triangle>();
                triangleList.Add(triangle);
                _triangleDictionary.Add(vertexIndexKey, triangleList);
            }
        }

        private void CalculateMeshOutlines()
        {
            for (int vertexIndex = 0; vertexIndex < _vertecies.Count; vertexIndex++)
            {
                if (!_checkedVertecies.Contains(vertexIndex))
                {
                    int newOutlineVertex = GetConnectedOutlineVertex(vertexIndex);
                    if (newOutlineVertex != -1)
                    {
                        _checkedVertecies.Add(vertexIndex);

                        List<int> newOutline = new List<int>();
                        newOutline.Add(vertexIndex);
                        _outlines.Add(newOutline);
                        FollowOutline(newOutlineVertex, _outlines.Count - 1);
                        _outlines[_outlines.Count - 1].Add(vertexIndex);
                    }
                }
            }
        }

        private void FollowOutline(int vertexIndex, int outlineIndex)
        {
            _outlines[outlineIndex].Add(vertexIndex);
            _checkedVertecies.Add(vertexIndex);
            int nextVertexIndex = GetConnectedOutlineVertex(vertexIndex);

            if (nextVertexIndex != -1)
            {
                FollowOutline(nextVertexIndex, outlineIndex);
            }
        }

        private int GetConnectedOutlineVertex(int vertexIndex)
        {
            List<Triangle> trianglesContainingVertex = _triangleDictionary[vertexIndex];

            for (int i = 0; i < trianglesContainingVertex.Count; i++)
            {
                Triangle triangle = trianglesContainingVertex[i];

                for (int j = 0; j < 3; j++)
                {
                    int vertexB = triangle[j];

                    if (vertexB != vertexIndex && !_checkedVertecies.Contains(vertexB))
                    {
                        if (IsOutlineEdge(vertexIndex, vertexB))
                        {
                            return vertexB;
                        }
                    }
                }
            }

            return -1;
        }

        private bool IsOutlineEdge(int vertexA, int vertexB)
        {
            List<Triangle> trianglesVertexA = _triangleDictionary[vertexA];
            int sharedTriangleCount = 0;

            for (int i = 0; i < trianglesVertexA.Count; i++)
            {
                if (trianglesVertexA[i].Contains(vertexB))
                {
                    sharedTriangleCount++;
                    if (sharedTriangleCount > 1)
                    {
                        break;
                    }
                }
            }

            return sharedTriangleCount == 1;
        }

        #endregion

        #region Data/Structs

        private struct Triangle
        {
            public int VertexIndexA;
            public int VertexIndexB;
            public int VertexIndexC;
            private int[] _vertecies;

            public Triangle(int a, int b, int c)
            {
                VertexIndexA = a;
                VertexIndexB = b;
                VertexIndexC = c;

                _vertecies = new int[3];
                _vertecies[0] = a;
                _vertecies[1] = b;
                _vertecies[2] = c;
            }

            public int this[int i] => _vertecies[i];

            public bool Contains(int vertexIndex)
            {
                return vertexIndex == VertexIndexA || vertexIndex == VertexIndexB || vertexIndex == VertexIndexC;
            }
        }

        public class SquareGrid
        {
            public Square[,] Squares;

            public SquareGrid(int[,] map, float squareSize)
            {
                int nodeCountX = map.GetLength(0);
                int nodeCountY = map.GetLength(1);
                float mapWidth = nodeCountX * squareSize;
                float mapHeight = nodeCountY * squareSize;

                ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];

                for (int x = 0; x < nodeCountX; x++)
                {
                    for (int y = 0; y < nodeCountY; y++)
                    {
                        Vector3 pos = new Vector3(-mapWidth / 2 + x * squareSize, 0,
                            -mapHeight / 2 + y * squareSize);
                        controlNodes[x, y] = new ControlNode(pos, map[x, y] == 1, squareSize);
                    }
                }

                Squares = new Square[nodeCountX - 1, nodeCountY - 1];

                for (int x = 0; x < nodeCountX - 1; x++)
                {
                    for (int y = 0; y < nodeCountY - 1; y++)
                    {
                        Squares[x, y] = new Square(controlNodes[x, y + 1], controlNodes[x + 1, y + 1],
                            controlNodes[x + 1, y], controlNodes[x, y]);
                    }
                }
            }
        }

        public class Square
        {
            public ControlNode TopLeft, TopRight, BottomRight, BottomLeft;
            public Node CenterTop, CenterRight, CenterBottom, CenterLeft;
            public int Configuration;

            public Square(ControlNode topLeft, ControlNode topRight, ControlNode bottomRight, ControlNode bottomLeft)
            {
                TopLeft = topLeft;
                TopRight = topRight;
                BottomRight = bottomRight;
                BottomLeft = bottomLeft;

                CenterTop = TopLeft.Right;
                CenterRight = BottomRight.Above;
                CenterBottom = BottomLeft.Right;
                CenterLeft = BottomLeft.Above;

                if (TopLeft.Active)
                {
                    Configuration += 8;
                }

                if (TopRight.Active)
                {
                    Configuration += 4;
                }

                if (BottomRight.Active)
                {
                    Configuration += 2;
                }

                if (BottomLeft.Active)
                {
                    Configuration += 1;
                }
            }
        }

        public class Node
        {
            public Vector3 Position;
            public int VertexIndex = -1;

            public Node(Vector3 position)
            {
                Position = position;
            }
        }

        public class ControlNode : Node
        {
            public bool Active;
            public Node Above, Right;

            public ControlNode(Vector3 position, bool active, float spuareSize) : base(position)
            {
                Active = active;
                Above = new Node(position + Vector3.forward * spuareSize / 2);
                Right = new Node(position + Vector3.right * spuareSize / 2);
            }
        }

        #endregion

        private void OnDrawGizmos()
        {
            if (MySquareGrid != null)
            {
                for (int x = 0; x < MySquareGrid.Squares.GetLength(0); x++)
                {
                    for (int y = 0; y < MySquareGrid.Squares.GetLength(1); y++)
                    {
                        Gizmos.color = MySquareGrid.Squares[x, y].TopLeft.Active ? Color.red : Color.cyan;
                        Gizmos.DrawCube(MySquareGrid.Squares[x, y].TopLeft.Position, Vector3.one * 0.4f);

                        Gizmos.color = MySquareGrid.Squares[x, y].TopRight.Active ? Color.red : Color.cyan;
                        Gizmos.DrawCube(MySquareGrid.Squares[x, y].TopRight.Position, Vector3.one * 0.4f);

                        Gizmos.color = MySquareGrid.Squares[x, y].BottomRight.Active ? Color.red : Color.cyan;
                        Gizmos.DrawCube(MySquareGrid.Squares[x, y].BottomRight.Position, Vector3.one * 0.4f);

                        Gizmos.color = MySquareGrid.Squares[x, y].BottomLeft.Active ? Color.red : Color.cyan;
                        Gizmos.DrawCube(MySquareGrid.Squares[x, y].BottomLeft.Position, Vector3.one * 0.4f);

                        Gizmos.color = Color.magenta;
                        Gizmos.DrawCube(MySquareGrid.Squares[x, y].CenterTop.Position, Vector3.one * 0.2f);

                        Gizmos.color = Color.magenta;
                        Gizmos.DrawCube(MySquareGrid.Squares[x, y].CenterRight.Position, Vector3.one * 0.2f);

                        Gizmos.color = Color.magenta;
                        Gizmos.DrawCube(MySquareGrid.Squares[x, y].CenterBottom.Position, Vector3.one * 0.2f);

                        Gizmos.color = Color.magenta;
                        Gizmos.DrawCube(MySquareGrid.Squares[x, y].CenterLeft.Position, Vector3.one * 0.2f);
                        
                        Gizmos.color = Color.magenta;
                        Gizmos.DrawCube(MySquareGrid.Squares[x, y].TopLeft.Above.Position, Vector3.one * 0.2f);
                        
                        // Gizmos.color = Color.magenta;
                        // Gizmos.DrawCube(MySquareGrid.Squares[x, y].TopRight.Above.Position, Vector3.one * 0.2f);

                        // Gizmos.color = Color.magenta;
                        // Gizmos.DrawCube(MySquareGrid.Squares[x, y].TopRight.Right.Position, Vector3.one * 0.2f);
                        
                        Gizmos.color = Color.magenta;
                        Gizmos.DrawCube(MySquareGrid.Squares[x, y].BottomRight.Right.Position, Vector3.one * 0.2f);
                    }
                }
            }
        }
    }
}
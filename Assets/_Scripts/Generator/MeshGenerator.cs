using System;
using UnityEngine;

namespace _Scripts.Generator
{
    public class MeshGenerator : MonoBehaviour
    {
        public SquareGrid MySquareGrid;

        public void GenerateMesh(int[,] map, float squareSize)
        {
            MySquareGrid = new SquareGrid(map, squareSize);
        }

        private void OnDrawGizmos()
        {
            if (MySquareGrid != null)
            {
                for (int x = 0; x < MySquareGrid.Squares.GetLength(0); x++)
                {
                    for (int y = 0; y < MySquareGrid.Squares.GetLength(1); y++)
                    {
                        Gizmos.color = MySquareGrid.Squares[x, y].TopLeft.Active ? Color.black : Color.white;
                        Gizmos.DrawCube(MySquareGrid.Squares[x,y].TopLeft.Position, Vector3.one * 0.4f);
                        
                        Gizmos.color = MySquareGrid.Squares[x, y].TopRight.Active ? Color.black : Color.white;
                        Gizmos.DrawCube(MySquareGrid.Squares[x,y].TopRight.Position, Vector3.one * 0.4f);
                        
                        Gizmos.color = MySquareGrid.Squares[x, y].BottomRight.Active ? Color.black : Color.white;
                        Gizmos.DrawCube(MySquareGrid.Squares[x,y].BottomRight.Position, Vector3.one * 0.4f);
                        
                        Gizmos.color = MySquareGrid.Squares[x, y].BottomLeft.Active ? Color.black : Color.white;
                        Gizmos.DrawCube(MySquareGrid.Squares[x,y].BottomLeft.Position, Vector3.one * 0.4f);
                        
                        Gizmos.color = Color.gray;
                        Gizmos.DrawCube(MySquareGrid.Squares[x,y].CenterTop.Position, Vector3.one * 0.2f);
                        
                        Gizmos.color = Color.gray;
                        Gizmos.DrawCube(MySquareGrid.Squares[x,y].CenterRight.Position, Vector3.one * 0.2f);
                        
                        Gizmos.color = Color.gray;
                        Gizmos.DrawCube(MySquareGrid.Squares[x,y].CenterBottom.Position, Vector3.one * 0.2f);
                        
                        Gizmos.color = Color.gray;
                        Gizmos.DrawCube(MySquareGrid.Squares[x,y].CenterLeft.Position, Vector3.one * 0.2f);
                    }
                }
                
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
                        Vector3 pos = new Vector3(-mapWidth / 2 + x * squareSize + squareSize / 2, 0,
                            -mapHeight / 2 + y * squareSize + squareSize / 2);
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
            public int configuration;

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
                    configuration += 8;
                }

                if (TopRight.Active)
                {
                    configuration += 4;
                }

                if (BottomRight.Active)
                {
                    configuration += 2;
                }

                if (BottomLeft.Active)
                {
                    configuration += 1;
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
                this.Active = active;
                Above = new Node(position + Vector3.forward * spuareSize / 2);
                Right = new Node(position + Vector3.right * spuareSize / 2);
            }
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Generator
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class MarchingCubes : MeshGenerator
    {
        private List<Vector3> _vertecies;
        private List<int> _triangles;

        private CubeCloud _myCubeCloud;

        public override void GenerateMesh(int[,,] map, float cubeSize, int surfaceLevel)
        {
            _vertecies = new List<Vector3>();
            _triangles = new List<int>();

            _myCubeCloud = new CubeCloud(map, cubeSize, surfaceLevel);

            for (int x = 0; x < _myCubeCloud.Cubes.GetLength(0); x++)
            {
                for (int y = 0; y < _myCubeCloud.Cubes.GetLength(1); y++)
                {
                    for (int z = 0; z < _myCubeCloud.Cubes.GetLength(2); z++)
                    {
                        TriangulateCube(_myCubeCloud.Cubes[x, y, z]);
                    }
                }
            }

            Mesh mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = mesh;

            mesh.vertices = _vertecies.ToArray();
            mesh.triangles = _triangles.ToArray();
        }

        private void TriangulateCube(Cube cube)
        {
            // Optimization calculate edge nodes when needed instead of all when cube is created

            List<Node> cutEdgeNodes = new List<Node>();

            if (LookUpTable.EdgeTable[cube.CubeConfiguration] == 0)
            {
                return;
            }

            if ((LookUpTable.EdgeTable[cube.CubeConfiguration] & 1) != 0)
            {
                cutEdgeNodes.Add(cube.EdgeNodes[0]);
            }

            if ((LookUpTable.EdgeTable[cube.CubeConfiguration] & 2) != 0)
            {
                cutEdgeNodes.Add(cube.EdgeNodes[1]);
            }

            if ((LookUpTable.EdgeTable[cube.CubeConfiguration] & 4) != 0)
            {
                cutEdgeNodes.Add(cube.EdgeNodes[2]);
            }

            if ((LookUpTable.EdgeTable[cube.CubeConfiguration] & 8) != 0)
            {
                cutEdgeNodes.Add(cube.EdgeNodes[3]);
            }

            if ((LookUpTable.EdgeTable[cube.CubeConfiguration] & 16) != 0)
            {
                cutEdgeNodes.Add(cube.EdgeNodes[4]);
            }

            if ((LookUpTable.EdgeTable[cube.CubeConfiguration] & 32) != 0)
            {
                cutEdgeNodes.Add(cube.EdgeNodes[5]);
            }

            if ((LookUpTable.EdgeTable[cube.CubeConfiguration] & 64) != 0)
            {
                cutEdgeNodes.Add(cube.EdgeNodes[6]);
            }

            if ((LookUpTable.EdgeTable[cube.CubeConfiguration] & 128) != 0)
            {
                cutEdgeNodes.Add(cube.EdgeNodes[7]);
            }

            if ((LookUpTable.EdgeTable[cube.CubeConfiguration] & 256) != 0)
            {
                cutEdgeNodes.Add(cube.EdgeNodes[8]);
            }

            if ((LookUpTable.EdgeTable[cube.CubeConfiguration] & 512) != 0)
            {
                cutEdgeNodes.Add(cube.EdgeNodes[9]);
            }

            if ((LookUpTable.EdgeTable[cube.CubeConfiguration] & 1024) != 0)
            {
                cutEdgeNodes.Add(cube.EdgeNodes[10]);
            }

            if ((LookUpTable.EdgeTable[cube.CubeConfiguration] & 2048) != 0)
            {
                cutEdgeNodes.Add(cube.EdgeNodes[11]);
            }

            AssignVertecies(cutEdgeNodes.ToArray());
            MeshFromCube(cube);
        }

        private void MeshFromCube(Cube cube)
        {
            // how to create the triangles
            int[] triangulationArray = LookUpTable.TriangleTable[cube.CubeConfiguration];
            for (int i = 0; i < triangulationArray.Length; i += 3)
            {
                if (triangulationArray[i] == -1)
                {
                    break;
                }

                int edgeNodeIndexA = triangulationArray[i];
                int edgeNodeIndexB = triangulationArray[i + 1];
                int edgeNodeIndexC = triangulationArray[i + 2];

                CreateTriangle(cube.EdgeNodes[edgeNodeIndexA], cube.EdgeNodes[edgeNodeIndexB],
                    cube.EdgeNodes[edgeNodeIndexC]);
            }
        }

        private void AssignVertecies(params Node[] points)
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
        }

        #region Data/Structs

        private class CubeCloud
        {
            public Cube[,,] Cubes;

            public CubeCloud(int[,,] map, float cubeSize, int surfaceLevel)
            {
                int nodeCountX = map.GetLength(0);
                int nodeCountY = map.GetLength(1);
                int nodeCountZ = map.GetLength(2);

                float mapWidth = nodeCountX * cubeSize;
                float mapHeight = nodeCountY * cubeSize;
                float mapDepth = nodeCountZ * cubeSize;

                ControlNode[,,] controlNodes = new ControlNode[nodeCountX, nodeCountY, nodeCountZ];

                for (int x = 0; x < nodeCountX; x++)
                {
                    for (int y = 0; y < nodeCountY; y++)
                    {
                        for (int z = 0; z < nodeCountZ; z++)
                        {
                            Vector3 pos = new Vector3(-mapWidth / 2 + x * cubeSize, -mapHeight / 2 + y * cubeSize,
                                -mapDepth / 2 + z * cubeSize);
                            controlNodes[x, y, z] = new ControlNode(pos, map[x, y, z] >= surfaceLevel, cubeSize, map,
                                new Vector3(x, y, z), surfaceLevel);
                        }
                    }
                }

                Cubes = new Cube[nodeCountX - 1, nodeCountY - 1, nodeCountZ - 1];

                for (int x = 0; x < nodeCountX - 1; x++)
                {
                    for (int y = 0; y < nodeCountY - 1; y++)
                    {
                        for (int z = 0; z < nodeCountZ - 1; z++)
                        {
                            Cubes[x, y, z] = new Cube(
                                controlNodes[x, y, z + 1],
                                controlNodes[x + 1, y, z + 1],
                                controlNodes[x + 1, y, z],
                                controlNodes[x, y, z],
                                controlNodes[x, y + 1, z + 1],
                                controlNodes[x + 1, y + 1, z + 1],
                                controlNodes[x + 1, y + 1, z],
                                controlNodes[x, y + 1, z]
                            );
                        }
                    }
                }
            }
        }

        /*
         *                      CN4_________EN4__________CN5
         *                        /|                    /|
         *                       / |                   / |
         *                  EN7-/  |              EN5-/  |
         *                     /___|______EN6________/   |
         *                 CN7|    |-EN8          CN6|   |-EN9
         *                    |    |                 |   |
         *                    |    |                 |   |
         *               EN11-|    |            EN10-|   |
         *                    |    |________EN0______|___|
         *                    |   /CN0               |   /CN1
         *                    |  /                   |  /
         *                    | /-EN3                | /-EN1
         *                    |/________EN2__________|/
         *                   CN3                    CN2
        */
        private class Cube
        {
            public ControlNode Cn0, Cn1, Cn2, Cn3, Cn4, Cn5, Cn6, Cn7;

            // variable amount of EdgeNodes necessary min 3 max 12 (max not needed)
            public Node En0, En1, En2, En3, En4, En5, En6, En7, En8, En9, En10, En11;
            public Dictionary<int, Node> EdgeNodes;

            public int CubeConfiguration = 0;

            public Cube(ControlNode cn0, ControlNode cn1, ControlNode cn2,
                ControlNode cn3, ControlNode cn4, ControlNode cn5,
                ControlNode cn6, ControlNode cn7)
            {

                Cn0 = cn0;
                Cn1 = cn1;
                Cn2 = cn2;
                Cn3 = cn3;
                Cn4 = cn4;
                Cn5 = cn5;
                Cn6 = cn6;
                Cn7 = cn7;

                // calculate EdgeNodes during triangulation (SetEdgeNodes) 
                En0 = Cn0.Right;
                En1 = Cn2.InFront;
                En2 = Cn3.Right;
                En3 = Cn3.InFront;

                En4 = Cn4.Right;
                En5 = Cn6.InFront;
                En6 = Cn7.Right;
                En7 = Cn7.InFront;

                En8 = Cn0.Above;
                En9 = Cn1.Above;
                En10 = Cn2.Above;
                En11 = Cn3.Above;

                // Edge nodes organized in a Dictionary
                EdgeNodes = new Dictionary<int, Node>();

                EdgeNodes.Add(0, En0);
                EdgeNodes.Add(1, En1);
                EdgeNodes.Add(2, En2);
                EdgeNodes.Add(3, En3);

                EdgeNodes.Add(4, En4);
                EdgeNodes.Add(5, En5);
                EdgeNodes.Add(6, En6);
                EdgeNodes.Add(7, En7);

                EdgeNodes.Add(8, En8);
                EdgeNodes.Add(9, En9);
                EdgeNodes.Add(10, En10);
                EdgeNodes.Add(11, En11);


                if (!Cn0.Active)
                {
                    CubeConfiguration += 1;
                }

                if (!Cn1.Active)
                {
                    CubeConfiguration += 2;
                }

                if (!Cn2.Active)
                {
                    CubeConfiguration += 4;
                }

                if (!Cn3.Active)
                {
                    CubeConfiguration += 8;
                }

                if (!Cn4.Active)
                {
                    CubeConfiguration += 16;
                }

                if (!Cn5.Active)
                {
                    CubeConfiguration += 32;
                }

                if (!Cn6.Active)
                {
                    CubeConfiguration += 64;
                }

                if (!Cn7.Active)
                {
                    CubeConfiguration += 128;
                }
            }
        }

        private class Node
        {
            public Vector3 Position;
            public int VertexIndex = -1;

            public Node(Vector3 position)
            {
                Position = position;
            }
        }

        private class ControlNode : Node
        {
            public bool Active;
            public Node Right, Above, InFront;

            public ControlNode(Vector3 position, bool active, float cubeSize, int[,,] map, Vector3 index,
                int surfaceLevel) : base(
                position)
            {
                Active = active;

                // EdgeNode Calculation during triangulation to minimize the amount of calculated EdgeNodes

                int myValue = map[(int) index.x, (int) index.y, (int) index.z];
                int modX = (int) index.x + 1;
                int modY = (int) index.y + 1;
                int modZ = (int) index.z + 1;

                if (modX < map.GetLength(0))
                {
                    int rightNeighborValue = map[modX, (int) index.y, (int) index.z];
                    Right = new Node(position + Vector3.right * cubeSize *
                        CalculateSurfacePercentage(myValue, rightNeighborValue, surfaceLevel));
                }

                if (modY < map.GetLength(1))
                {
                    int aboveNeighbourValue = map[(int) index.x, modY, (int) index.z];
                    Above = new Node(position + Vector3.up * cubeSize *
                        CalculateSurfacePercentage(myValue, aboveNeighbourValue, surfaceLevel));
                }

                if (modZ < map.GetLength(2))
                {
                    int frontNeighbourValue = map[(int) index.x, (int) index.y, modZ];
                    InFront = new Node(position + Vector3.forward * cubeSize *
                        CalculateSurfacePercentage(myValue, frontNeighbourValue, surfaceLevel));
                }
            }

            private float CalculateSurfacePercentage(int myValue, int neighbourValue, int surfaceLevel)
            {
                if (myValue == neighbourValue)
                {
                    return 0.5f;
                }

                float range = Mathf.Abs(myValue - neighbourValue) * 100;
                float shiftedSurface = surfaceLevel + Mathf.Abs(Mathf.Min(myValue, neighbourValue));

                float surfacePercentage = shiftedSurface / range;

                if (myValue > neighbourValue)
                {
                    surfacePercentage = 1 - surfacePercentage;
                }

                return surfacePercentage;
            }
        }

        #endregion

        // private void OnDrawGizmos()
        // {
        //     if (_myCubeCloud != null)
        //     {
        //         for (int x = 0; x < _myCubeCloud.Cubes.GetLength(0); x++)
        //         {
        //             for (int y = 0; y < _myCubeCloud.Cubes.GetLength(1); y++)
        //             {
        //                 for (int z = 0; z < _myCubeCloud.Cubes.GetLength(2); z++)
        //                 {
        //                     // Gizmos.color = _myCubeCloud.Cubes[x, y, z].CN0.Active ? Color.red : Color.cyan;
        //                     // Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].CN0.Position, Vector3.one * 0.4f);
        //                     //
        //                     // Gizmos.color = _myCubeCloud.Cubes[x, y, z].CN1.Active ? Color.red : Color.cyan;
        //                     // Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].CN1.Position, Vector3.one * 0.4f);
        //                     //
        //                     // Gizmos.color = _myCubeCloud.Cubes[x, y, z].CN2.Active ? Color.red : Color.cyan;
        //                     // Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].CN2.Position, Vector3.one * 0.4f);
        //                     //
        //                     // Gizmos.color = _myCubeCloud.Cubes[x, y, z].CN3.Active ? Color.red : Color.cyan;
        //                     // Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].CN3.Position, Vector3.one * 0.4f);
        //                     //
        //                     // Gizmos.color = _myCubeCloud.Cubes[x, y, z].CN4.Active ? Color.red : Color.cyan;
        //                     // Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].CN4.Position, Vector3.one * 0.4f);
        //                     //
        //                     // Gizmos.color = _myCubeCloud.Cubes[x, y, z].CN5.Active ? Color.red : Color.cyan;
        //                     // Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].CN5.Position, Vector3.one * 0.4f);
        //                     //
        //                     // Gizmos.color = _myCubeCloud.Cubes[x, y, z].CN6.Active ? Color.red : Color.cyan;
        //                     // Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].CN6.Position, Vector3.one * 0.4f);
        //                     //
        //                     // Gizmos.color = _myCubeCloud.Cubes[x, y, z].CN7.Active ? Color.red : Color.cyan;
        //                     // Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].CN7.Position, Vector3.one * 0.4f);
        //
        //                     if (_myCubeCloud.Cubes[x, y, z].CN0.Active)
        //                     {
        //                         Gizmos.color = Color.red;
        //                         Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].CN0.Position, Vector3.one * 0.2f);
        //                     }
        //
        //                     if (_myCubeCloud.Cubes[x, y, z].CN1.Active)
        //                     {
        //                         Gizmos.color = Color.red;
        //                         Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].CN1.Position, Vector3.one * 0.2f);
        //                     }
        //                     
        //                     if (_myCubeCloud.Cubes[x, y, z].CN2.Active)
        //                     {
        //                         Gizmos.color = Color.red;
        //                         Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].CN2.Position, Vector3.one * 0.2f);
        //                     }
        //                     
        //                     if (_myCubeCloud.Cubes[x, y, z].CN3.Active)
        //                     {
        //                         Gizmos.color = Color.red;
        //                         Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].CN3.Position, Vector3.one * 0.2f);
        //                     }
        //                     
        //                     if (_myCubeCloud.Cubes[x, y, z].CN4.Active)
        //                     {
        //                         Gizmos.color = Color.red;
        //                         Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].CN4.Position, Vector3.one * 0.2f);
        //                     }
        //                     
        //                     if (_myCubeCloud.Cubes[x, y, z].CN5.Active)
        //                     {
        //                         Gizmos.color = Color.red;
        //                         Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].CN5.Position, Vector3.one * 0.2f);
        //                     }
        //                     
        //                     if (_myCubeCloud.Cubes[x, y, z].CN6.Active)
        //                     {
        //                         Gizmos.color = Color.red;
        //                         Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].CN6.Position, Vector3.one * 0.2f);
        //                     }
        //                     
        //                     if (_myCubeCloud.Cubes[x, y, z].CN7.Active)
        //                     {
        //                         Gizmos.color = Color.red;
        //                         Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].CN7.Position, Vector3.one * 0.2f);
        //                     }
        //
        //                     Gizmos.color = Color.magenta;
        //                     Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].EN0.Position, Vector3.one * 0.05f);
        //
        //                     Gizmos.color = Color.magenta;
        //                     Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].EN1.Position, Vector3.one * 0.05f);
        //
        //                     Gizmos.color = Color.magenta;
        //                     Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].EN2.Position, Vector3.one * 0.05f);
        //
        //                     Gizmos.color = Color.magenta;
        //                     Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].EN3.Position, Vector3.one * 0.05f);
        //
        //                     Gizmos.color = Color.magenta;
        //                     Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].EN4.Position, Vector3.one * 0.05f);
        //
        //                     Gizmos.color = Color.magenta;
        //                     Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].EN5.Position, Vector3.one * 0.05f);
        //
        //                     Gizmos.color = Color.magenta;
        //                     Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].EN6.Position, Vector3.one * 0.05f);
        //
        //                     Gizmos.color = Color.magenta;
        //                     Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].EN7.Position, Vector3.one * 0.05f);
        //
        //                     Gizmos.color = Color.magenta;
        //                     Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].EN8.Position, Vector3.one * 0.05f);
        //
        //                     Gizmos.color = Color.magenta;
        //                     Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].EN9.Position, Vector3.one * 0.05f);
        //
        //                     Gizmos.color = Color.magenta;
        //                     Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].EN10.Position, Vector3.one * 0.05f);
        //
        //                     Gizmos.color = Color.magenta;
        //                     Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].EN11.Position, Vector3.one * 0.05f);
        //                 }
        //             }
        //         }
        //     }
        // }
    }
}
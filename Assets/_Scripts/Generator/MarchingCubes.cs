using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Generator
{
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
        }

        private void TriangulateCube(Cube cube)
        {
            // Debug.Log("CubeConfig: " + cube.CubeConfiguration);
            // Debug.Log("Edge Table Value: " + LookUpTable.EdgeTable[cube.CubeConfiguration]);
            
            // switch (LookUpTable.EdgeTable[cube.CubeConfiguration])
            // {
            //     /* Maybe not switch but lookUp table */
            // }
            
            // CalculateEdgeNodes();
            //
        }

        private void MeshFromPoints( /*params Node[] points*/)
        {
        }

        private void AssignVertecies()
        {
        }

        private void CreateTriangle()
        {
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
            // put them in arrays
            public ControlNode CN0, CN1, CN2, CN3, CN4, CN5, CN6, CN7;

            public Node EN0, EN1, EN2, EN3, EN4, EN5, EN6, EN7, EN8, EN9, EN10, EN11;

            public int CubeConfiguration = 0;

            public Cube(ControlNode cn0, ControlNode cn1, ControlNode cn2,
                ControlNode cn3, ControlNode cn4, ControlNode cn5,
                ControlNode cn6, ControlNode cn7)
            {
                CN0 = cn0;
                CN1 = cn1;
                CN2 = cn2;
                CN3 = cn3;
                CN4 = cn4;
                CN5 = cn5;
                CN6 = cn6;
                CN7 = cn7;

                // calculate EdgeNodes during triangulation (SetEdgeNodes) 
                EN0 = CN0.Right;
                EN1 = CN2.InFront;
                EN2 = CN3.Right;
                EN3 = CN3.InFront;
                
                EN4 = CN4.Right;
                EN5 = CN6.InFront;
                EN6 = CN7.Right;
                EN7 = CN7.InFront;
                
                EN8 = CN0.Above;
                EN9 = CN1.Above;
                EN10 = CN2.Above;
                EN11 = CN3.Above;

                if (!CN0.Active)
                {
                    CubeConfiguration += 1;
                }

                if (!CN1.Active)
                {
                    CubeConfiguration += 2;
                }

                if (!CN2.Active)
                {
                    CubeConfiguration += 4;
                }

                if (!CN3.Active)
                {
                    CubeConfiguration += 8;
                }

                if (!CN4.Active)
                {
                    CubeConfiguration += 16;
                }

                if (!CN5.Active)
                {
                    CubeConfiguration += 32;
                }

                if (!CN6.Active)
                {
                    CubeConfiguration += 64;
                }

                if (!CN7.Active)
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

            public ControlNode(Vector3 position, bool active, float cubeSize, int[,,] map, Vector3 index, int surfaceLevel) : base(
                position)
            {
                Active = active;
                
                // EdgeNode Calculation during triangulation to minimize the amount of calculated EdgeNodes
                
                int myValue = map[(int)index.x, (int)index.y, (int)index.z];
                int modX = (int) index.x + 1;
                int modY = (int) index.y + 1;
                int modZ = (int) index.z + 1;
                
                // Debug.Log("x: " + index.x + ", modX: " + modX + ", mapLength x: " + map.GetLength(0));
                // Debug.Log("y: " + index.y + ", modY: " + modY + ", mapLength y: " + map.GetLength(1));
                // Debug.Log("z: " + index.z + ", modZ: " + modZ + ", mapLength z: " + map.GetLength(2));
                
                if (modX < map.GetLength(0))
                {
                    int rightNeighborValue = map[modX, (int) index.y, (int) index.z];
                    Right = new Node(position + Vector3.right * cubeSize * CalculateSurfacePercentage(myValue,rightNeighborValue,surfaceLevel));
                }
                
                if (modY < map.GetLength(1))
                {
                    int aboveNeighbourValue = map[(int) index.x, modY, (int) index.z];
                    Above = new Node(position + Vector3.up * cubeSize * CalculateSurfacePercentage(myValue,aboveNeighbourValue,surfaceLevel));
                }

                if (modZ < map.GetLength(2))
                {
                    int frontNeighbourValue = map[(int) index.x, (int) index.y, modZ];
                    InFront = new Node(position + Vector3.forward * cubeSize * CalculateSurfacePercentage(myValue,frontNeighbourValue,surfaceLevel));   
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

        private void OnDrawGizmos()
        {
            if (_myCubeCloud != null)
            {
                for (int x = 0; x < _myCubeCloud.Cubes.GetLength(0); x++)
                {
                    for (int y = 0; y < _myCubeCloud.Cubes.GetLength(1); y++)
                    {
                        for (int z = 0; z < _myCubeCloud.Cubes.GetLength(2); z++)
                        {
                            Gizmos.color = _myCubeCloud.Cubes[x, y, z].CN0.Active ? Color.red : Color.cyan;
                            Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].CN0.Position, Vector3.one * 0.4f);

                            Gizmos.color = _myCubeCloud.Cubes[x, y, z].CN1.Active ? Color.red : Color.cyan;
                            Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].CN1.Position, Vector3.one * 0.4f);

                            Gizmos.color = _myCubeCloud.Cubes[x, y, z].CN2.Active ? Color.red : Color.cyan;
                            Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].CN2.Position, Vector3.one * 0.4f);

                            Gizmos.color = _myCubeCloud.Cubes[x, y, z].CN3.Active ? Color.red : Color.cyan;
                            Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].CN3.Position, Vector3.one * 0.4f);

                            Gizmos.color = _myCubeCloud.Cubes[x, y, z].CN4.Active ? Color.red : Color.cyan;
                            Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].CN4.Position, Vector3.one * 0.4f);

                            Gizmos.color = _myCubeCloud.Cubes[x, y, z].CN5.Active ? Color.red : Color.cyan;
                            Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].CN5.Position, Vector3.one * 0.4f);

                            Gizmos.color = _myCubeCloud.Cubes[x, y, z].CN6.Active ? Color.red : Color.cyan;
                            Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].CN6.Position, Vector3.one * 0.4f);

                            Gizmos.color = _myCubeCloud.Cubes[x, y, z].CN7.Active ? Color.red : Color.cyan;
                            Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].CN7.Position, Vector3.one * 0.4f);

                            Gizmos.color = Color.magenta;
                            Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].EN0.Position, Vector3.one * 0.2f);

                            Gizmos.color = Color.magenta;
                            Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].EN1.Position, Vector3.one * 0.2f);

                            Gizmos.color = Color.magenta;
                            Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].EN2.Position, Vector3.one * 0.2f);

                            Gizmos.color = Color.magenta;
                            Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].EN3.Position, Vector3.one * 0.2f);

                            Gizmos.color = Color.magenta;
                            Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].EN4.Position, Vector3.one * 0.2f);

                            Gizmos.color = Color.magenta;
                            Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].EN5.Position, Vector3.one * 0.2f);

                            Gizmos.color = Color.magenta;
                            Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].EN6.Position, Vector3.one * 0.2f);

                            Gizmos.color = Color.magenta;
                            Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].EN7.Position, Vector3.one * 0.2f);

                            Gizmos.color = Color.magenta;
                            Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].EN8.Position, Vector3.one * 0.2f);

                            Gizmos.color = Color.magenta;
                            Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].EN9.Position, Vector3.one * 0.2f);

                            Gizmos.color = Color.magenta;
                            Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].EN10.Position, Vector3.one * 0.2f);

                            Gizmos.color = Color.magenta;
                            Gizmos.DrawCube(_myCubeCloud.Cubes[x, y, z].EN11.Position, Vector3.one * 0.2f);
                        }
                    }
                }
            }
        }
    }
}
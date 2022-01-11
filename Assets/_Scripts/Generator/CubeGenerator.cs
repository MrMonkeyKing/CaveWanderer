using UnityEngine;

namespace _Scripts.Generator
{
    [RequireComponent(typeof(MarchingCubes))]
    public class CubeGenerator : MonoBehaviour
    {
        public int width;
        public int height;
        public int depth;

        public string seed;
        public bool useRandomSeed;

        [Range(1, 100)] public int maxRandom = 32;
        [Range(0, 100)] public int surfaceLevel;

        private int[,,] _map;

        private void Start()
        {
            GenerateMap();

            if (surfaceLevel > maxRandom - 1)
            {
                surfaceLevel = maxRandom - 1;
            }
        }

        private void Update()
        {
            if (surfaceLevel > maxRandom - 1)
            {
                surfaceLevel = maxRandom - 1;
            }

            /* use for test purposes needs to be excluded in final game */
            if (Input.GetMouseButtonDown(0))
            {
                GenerateMap();
            }
        }

        private void GenerateMap()
        {
            _map = new int[width, height, depth];
            /* random initialization of map */
            RandomFillMap();

            /* smoothing of the map */
            SmoothMap();
            
            /* generating a mesh out of our map */
            MarchingCubes cubes = GetComponent<MarchingCubes>();
            cubes.GenerateMesh(_map, 1, surfaceLevel);
        }

        private void RandomFillMap()
        {
            if (useRandomSeed)
            {
                seed = Time.time.ToString();
            }

            System.Random pseudoRandom = new System.Random(seed.GetHashCode());

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        _map[x, y, z] = pseudoRandom.Next(0, maxRandom);
                    }
                }
            }
        }

        private void SmoothMap()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        int smoothedMapValue = GetSurroundingVoxelValue(new Vector3(x, y, z));
                        _map[x, y, z] = smoothedMapValue;
                    }
                }
            }
        }

        private int GetSurroundingVoxelValue(Vector3 pos)
        {
            int combinedVoxelValue = 0;
            int voxelCount = 0;

            for (int x = (int) pos.x - 1; x <= pos.x + 1; x++)
            {
                for (int y = (int) pos.y - 1; y <= pos.y + 1; y++)
                {
                    for (int z = (int) pos.z - 1; z <= pos.z + 1; z++)
                    {
                        if (x >= 0 && x < width && y >= 0 && y < height && z >= 0 && z < depth)
                        {
                            if (x != pos.x && y != pos.y && z != pos.z)
                            {
                                voxelCount++;
                                combinedVoxelValue += _map[x, y, z];
                            }   
                        }
                    }
                }
            }

            return voxelCount > 0 ? combinedVoxelValue / voxelCount : 0;
        }

        // private void OnDrawGizmos()
        // {
        //     if (_map != null)
        //     {
        //         for (int x = 0; x < width; x++)
        //         {
        //             for (int y = 0; y < height; y++)
        //             {
        //                 for (int z = 0; z < depth; z++)
        //                 {
        //                     if (_map[x, y, z] >= surfaceLevel)
        //                     {
        //                         float scaledMapValue = (float) _map[x, y, z] / maxRandom;
        //                         Gizmos.color = Color.HSVToRGB(0f, 0, scaledMapValue);
        //                         Vector3 pos = new Vector3(-width / 2 + x, -height / 2 + y, -depth / 2 + z);
        //                         Gizmos.DrawCube(pos, Vector3.one * .3f);
        //                     }
        //                 }
        //             }
        //         }
        //     }
        // }
    }
}
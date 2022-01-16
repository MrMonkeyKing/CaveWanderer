using _Scripts.Generator.Noise;
using UnityEngine;

namespace _Scripts.Generator
{
    public class _3DCaveGenerator : MonoBehaviour
    {
        public int width;
        public int height;
        public int depth;

        public string seed;
        public bool useRandomSeed;
        [Range(1, 20)] public int noiseScale = 1;

        [Range(1, 100)] public int surfaceLevel = 1;

        private int _floorLevel = 2;
        private int _maxRandom = 100;
        private int[,,] _map;

        private void Awake()
        {
            GenerateMap();

            if (surfaceLevel > _maxRandom - 1)
            {
                surfaceLevel = _maxRandom - 1;
            }
        }

        private void Update()
        {
            if (surfaceLevel > _maxRandom - 1)
            {
                surfaceLevel = _maxRandom - 1;
            }

            /* use for test purposes needs to be excluded in final game */
            if (Input.GetMouseButtonDown(0))
            {
                GenerateMap();
            }
        }

        public void OnValidate()
        {
            if (_map != null)
            {
                GenerateMap();
            }
        }

        private void GenerateMap()
        {
            _map = new int[width, height, depth];

            RandomFillMap();

            MarchingCubes cubes = GetComponent<MarchingCubes>();
            cubes.GenerateMesh(_map, 1, surfaceLevel);
        }

        private void RandomFillMap()
        {
            if (useRandomSeed)
            {
                seed = Time.time.ToString();
            }

            OpenSimplexNoise noise = new OpenSimplexNoise(seed.GetHashCode());

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        if (x > 0 && x < width - 1 && y > 0 && y < height - 1 && z > 0 && z < depth - 1)
                        {
                            if (y < _floorLevel)
                            {
                                // forced floor generation 
                                _map[x, y, z] = 100;
                            }
                            else
                            {
                                int randomValue = (int) ((noise.Evaluate((float) x / noiseScale, (float) y / noiseScale, (float) z / noiseScale) + 1) * 0.5 * _maxRandom);
                                _map[x, y, z] = Mathf.Clamp(randomValue, 0, _maxRandom);
                            }
                        }
                        else
                        {
                            // most outer layer is empty space so wall meshes will be generated
                            _map[x, y, z] = 0;
                        }
                    }
                }
            }
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
        //                         float scaledMapValue = (float) _map[x, y, z] / _maxRandom;
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
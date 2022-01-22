using UnityEngine;

namespace _Scripts.Generator
{
    /*
     * DISCLAIMER:
     *
     * This Script has been implemented according to Sebastian Lagues tutorial series on cave generation,
     * the only difference we are not calling the mesh generation function
     *
     * source: https://github.com/SebLague/Procedural-Cave-Generation
     *
     * 
     * LICENSE:
     *
     * MIT License
     * Copyright (c) 2016 Sebastian
     * 
     * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
     * documentation files (the "Software"), to deal in the Software without restriction, including without limitation
     * the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,
     * and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
     * 
     * The above copyright notice and this permission notice shall be included in all copies or substantial portions
     * of the Software.
     *
     * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
     * TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
     * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
     * CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
     * IN THE SOFTWARE.
     * 
     */
    public class _2DCaveGenExample : MonoBehaviour
    {
        public int width;
        public int height;

        public string seed;
        public bool useRandomSeed;

        [Range(0, 100)] public int randomFillPercentage;

        private int[,] _map;
        
        private void Awake()
        {
            GenerateMap();
        }

        private void Update()
        { 
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

        /*
         * Here we generate the Map
         * first we initialize it with random values
         * then we apply rules to change the look of our map
         *
         * we could mix those steps to get a certain kind of look
         */
        private void GenerateMap()
        {
            _map = new int[width, height];
            RandomFillMap();

            /*
             * in here we call different rule sets which will be applied to the map
             * can be modified to get different results
             */
            for (int i = 0; i < 5; i++)
            {
                SmoothMap();
            }
        }

        /*
         * Fills map with random values 0 to 1
         */
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
                    if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                    {
                        _map[x, y] = 1;
                    }
                    else
                    {
                        _map[x, y] = (pseudoRandom.Next(0, 100) < randomFillPercentage) ? 1 : 0;
                    }
                }
            }
        }

        /*
         * Here we apply rules to how the random generated Pixel map is changed/smoothed
         * can be modified in order to get other results
         */
        private void SmoothMap()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int neighbourWallTiles = GetSurroundingWallCount(x, y);
                    if (neighbourWallTiles > 4)
                    {
                        _map[x, y] = 1;
                    }
                    else if (neighbourWallTiles < 4)
                    {
                        _map[x, y] = 0;
                    }
                }
            }
        }

        private int GetSurroundingWallCount(int gridX, int gridY)
        {
            int wallCount = 0;
            for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
            {
                for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
                {
                    if (neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < height)
                    {
                        if (neighbourX != gridX || neighbourY != gridY)
                        {
                            wallCount += _map[neighbourX, neighbourY];
                        }
                    }
                    else
                    {
                        wallCount++;
                    }
                }
            }

            return wallCount;
        }

        private void OnDrawGizmos()
        {
            if (_map != null)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        Gizmos.color = (_map[x, y] == 1) ? Color.black : Color.white;
                        Vector3 pos = new Vector3(-width / 2 + x, 0, -height / 2 + y);
                        Gizmos.DrawCube(pos, Vector3.one);
                    }
                }
            }
        }
    }
}
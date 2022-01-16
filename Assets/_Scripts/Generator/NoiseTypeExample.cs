using _Scripts.Generator.Noise;
using UnityEngine;

namespace _Scripts.Generator
{
    public class NoiseTypeExample : MonoBehaviour
    {
        public int width;
        public int height;

        [Range(1, 20)] public int noiseScale = 1;

        private int _maxRandom = 100;
        private int[,] _map;
        private int _seed;

        private bool _pseudoRandom = true;

        public void Awake()
        {
            GenerateMap();
        }

        public void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                GenerateMap();
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                SwitchState();
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
            if (_pseudoRandom)
            {
                GeneratePseudoRandomNoiseMap();   
            }
            else
            {
                GenerateSimplexNoiseMap();
            }
        }

        private void GeneratePseudoRandomNoiseMap()
        {
            _pseudoRandom = true;
            
            _map = new int[width, height];
            _seed = Time.time.ToString().GetHashCode();
            
            System.Random pseudoRandom = new System.Random(_seed.GetHashCode());

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // PseudoRandom
                    _map[x, y] = pseudoRandom.Next(_maxRandom);
                }
            }
        }

        private void GenerateSimplexNoiseMap()
        {
            _pseudoRandom = false;
            
            _map = new int[width, height];
            _seed = Time.time.ToString().GetHashCode();
            
            OpenSimplexNoise noise = new OpenSimplexNoise(_seed);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Simplex
                    int randomValue = (int) ((noise.Evaluate((float)x / noiseScale, (float)y / noiseScale) + 1) * 0.5f * _maxRandom);
                    _map[x, y] = randomValue;
                }
            }
        }

        private void SwitchState()
        {
            _pseudoRandom = !_pseudoRandom;
        }

        private void OnDrawGizmos()
        {
            if (_map != null)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        float scaledMapValue = (float) _map[x, y] / _maxRandom;
                        Gizmos.color = Color.HSVToRGB(0f, 0, scaledMapValue);
                        Vector3 pos = new Vector3(-width / 2 + x, 0,-height / 2 + y);
                        Gizmos.DrawCube(pos, Vector3.one);
                    }
                }
            }
        }
    }
}
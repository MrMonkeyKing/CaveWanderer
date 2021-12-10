using UnityEngine;

namespace _Scripts.Generator
{
    public class MapGenerator : MonoBehaviour
    {
        public int width;
        public int height;

        public string seed;
        public bool usRandomSeed;

        [Range(0,100)]
        public int fillPercentage;

        private int[,] _map;

        private void Start()
        {
            GenerateMap();
        }

        private void GenerateMap()
        {
            _map = new int[width, height];
        }
    }
}
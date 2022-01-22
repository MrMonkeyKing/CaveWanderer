using System;
using UnityEngine;

namespace _Scripts.Generator.Meshes
{
    public class MeshGenerator : MonoBehaviour
    {
        public virtual void GenerateMesh(int[,] map, float squareSize)
        {
            /* this method is meant to be overridden */
            throw new NotImplementedException();
        }
        
        public virtual void GenerateMesh(int[,,] map, float cubeSize, int surfaceLevel)
        {
            /* this method is meant to be overridden */
            throw new NotImplementedException();
        }
    }
}
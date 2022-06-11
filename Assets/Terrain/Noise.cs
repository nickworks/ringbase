using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{

    static public void Seed() {
        Random.InitState(100);
    }

    static public float Perlin(float x, float y, float z) {
        float val = 0;

        val += Mathf.PerlinNoise(x, y);
        val += Mathf.PerlinNoise(-z, x);
        val += Mathf.PerlinNoise(-y, z);

        return val/3;
    }
    static public float Perlin(Vector3 pos) {
        return Perlin(pos.x, pos.y, pos.z);
    }
}

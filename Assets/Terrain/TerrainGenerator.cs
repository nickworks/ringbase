using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(MeshFilter))]
public class TerrainGenerator : MonoBehaviour
{

    public enum BlendMode {
        Average,
        Add,
        Mult,
        Div,
        Sub
    }

    [System.Serializable]
    public class NoiseField {
        public bool isOn = true;
        public BlendMode blendMode;
        public Vector3 offset;
        [Range(5, 50)]
        public float zoom = 20;
        [Range(0, 1)]
        public float flattenAmount = 0;
        [Range(-50, 50)]
        public float flattenOffset = 0;
    }

    [Range(4, 40)]
    public int resolution = 10;

    [Range(0, 1)]
    public float densityThreshold = 0.5f;

    public NoiseField[] fields;

    private MeshFilter meshFilter;
    private MeshCollider meshCollider;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
    }

    private void OnValidate() {
        BuildMesh();
    }

    void BuildMesh() {

        // sample our "noise field" to determine solidity?

        bool[,,] voxels = new bool[resolution,resolution,resolution];

        for (int x = 0; x < voxels.GetLength(0); x++) {
            for (int y = 0; y < voxels.GetLength(1); y++) {
                for (int z = 0; z < voxels.GetLength(2); z++) {

                    Vector3 pos = new Vector3(x, y, z);

                    float density = 0;

                    foreach(NoiseField field in fields) {

                        Vector3 noisePos = (pos + transform.position) / field.zoom + field.offset;
                        float d = Noise.Perlin(noisePos);
                        d -= ((y + field.flattenOffset) / 100f) * field.flattenAmount;

                        switch (field.blendMode) {
                            case BlendMode.Average:
                                density = (density + d) / 2;
                                break;
                            case BlendMode.Add:
                                density += d;
                                break;
                            case BlendMode.Mult:
                                density *= d;
                                break;
                            case BlendMode.Div:
                                density /= d;
                                break;
                            case BlendMode.Sub:
                                density -= d;
                                break;
                        }
                    }                   
                    voxels[x, y, z] = (density > densityThreshold);
                }
            }
        }

        // make storage for geometry:
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Vector3> norms = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();

        // generating the geomtery:

        for (int x = 0; x < voxels.GetLength(0); x++) {
            for (int y = 0; y < voxels.GetLength(1); y++) {
                for (int z = 0; z < voxels.GetLength(2); z++) {

                    if (voxels[x, y, z]) {

                        byte sides = 0;
                        if (!Lookup(voxels, x, y + 1, z)) sides |= 01;
                        if (!Lookup(voxels, x, y - 1, z)) sides |= 02;
                        if (!Lookup(voxels, x + 1, y, z)) sides |= 04;
                        if (!Lookup(voxels, x - 1, y, z)) sides |= 08;
                        if (!Lookup(voxels, x, y, z + 1)) sides |= 16;
                        if (!Lookup(voxels, x, y, z - 1)) sides |= 32;

                        AddCube(new Vector3(x, y, z), sides, verts, tris, norms, uvs);
                    }

                    
                }
            }
        }

        // make mesh from geometry:
        Mesh mesh = new Mesh();
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.normals = norms.ToArray();
        mesh.uv = uvs.ToArray();

        if(!meshFilter) meshFilter = GetComponent<MeshFilter>();
        if (!meshCollider) meshCollider = GetComponent<MeshCollider>();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }
    bool Lookup(bool[,,] arr, int x, int y, int z) {

        if (x < 0) return false;
        if (y < 0) return false;
        if (z < 0) return false;
        if (x >= arr.GetLength(0)) return false;
        if (y >= arr.GetLength(1)) return false;
        if (z >= arr.GetLength(2)) return false;

        return arr[x, y, z];
    }
    void AddCube(
        Vector3 position,
        byte sides,
        List<Vector3> verts,
        List<int> tris,
        List<Vector3> norms,
        List<Vector2> uvs) {

        if((sides & 01) > 0) AddQuad(position + new Vector3(0, +0.5f, 0), Quaternion.Euler(0, 0, 000), verts, tris, norms, uvs);
        if((sides & 02) > 0) AddQuad(position + new Vector3(0, -0.5f, 0), Quaternion.Euler(0, 0, 180), verts, tris, norms, uvs);
        if((sides & 04) > 0) AddQuad(position + new Vector3(+0.5f, 0, 0), Quaternion.Euler(0, 0, -90), verts, tris, norms, uvs);
        if((sides & 08) > 0) AddQuad(position + new Vector3(-0.5f, 0, 0), Quaternion.Euler(0, 0, +90), verts, tris, norms, uvs);
        if((sides & 16) > 0) AddQuad(position + new Vector3(0, 0, +0.5f), Quaternion.Euler(+90, 0, 0), verts, tris, norms, uvs);
        if((sides & 32) > 0) AddQuad(position + new Vector3(0, 0, -0.5f), Quaternion.Euler(-90, 0, 0), verts, tris, norms, uvs);
    }


    void AddQuad(
        Vector3 position,
        Quaternion rotation,
        List<Vector3> verts,
        List<int> tris,
        List<Vector3> norms,
        List<Vector2> uvs
        ) {

        int num = verts.Count;

        verts.Add( position + rotation * new Vector3(+0.5f, 0, +0.5f) );
        verts.Add( position + rotation * new Vector3(+0.5f, 0, -0.5f) );
        verts.Add( position + rotation * new Vector3(-0.5f, 0, -0.5f) );
        verts.Add( position + rotation * new Vector3(-0.5f, 0, +0.5f) );

        tris.Add(num + 0);
        tris.Add(num + 1);
        tris.Add(num + 3);
        tris.Add(num + 1);
        tris.Add(num + 2);
        tris.Add(num + 3);

        norms.Add(rotation * new Vector3(0, 1, 0));
        norms.Add(rotation * new Vector3(0, 1, 0));
        norms.Add(rotation * new Vector3(0, 1, 0));
        norms.Add(rotation * new Vector3(0, 1, 0));

        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(0, 0));

    }

    void BuildMeshQuad() {

        Mesh mesh = new Mesh();

        // set mesh's vertices
        Vector3[] verts = new Vector3[] {
            new Vector3(+0.5f, 0, +0.5f),
            new Vector3(+0.5f, 0, -0.5f),
            new Vector3(-0.5f, 0, -0.5f),
            new Vector3(-0.5f, 0, +0.5f)
        };

        // set mesh's triangles
        int[] tris = new int[] {
            0, 1, 3,
            1, 2, 3
        };

        // set mesh's normals
        Vector3[] norms = new Vector3[] {
            new Vector3(0, 1, 0),
            new Vector3(0, 1, 0),
            new Vector3(0, 1, 0),
            new Vector3(0, 1, 0)
        };


        // set mesh's UVs
        Vector2[] uvs = new Vector2[] {
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1),
            new Vector2(0, 0)
        };


        // set mesh's color


        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.normals = norms;
        mesh.uv = uvs;

        meshFilter.mesh = mesh;

    }


}

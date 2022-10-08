using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(MeshFilter))]
public class ProceduralTerrain : MonoBehaviour
{

    float[,,] densities = null;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    bool showEdges = false;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
    }
    public async Task MarchGeometryAsync()
    {
        if(!WorldSeed.singleton) {
            print("FAIL! no singleton...");
            return;
        }

        // calculate density values:
        densities = await WorldSeed.singleton.GetDensityField(transform.position);
        
        // do the cube marching:
        Mesh mesh = await MeshTools.CubeMarcher.MakeMeshAsync(
            WorldSeed.singleton.voxelSize,
            WorldSeed.singleton.threshold,
            densities);

        if(meshFilter == null) meshFilter = GetComponent<MeshFilter>();
        if(meshCollider == null) meshCollider = GetComponent<MeshCollider>();
        if(meshFilter != null) meshFilter.mesh = mesh;
        if(meshCollider != null) meshCollider.sharedMesh = mesh;
    }
    void OnDrawGizmos(){
        if(showEdges == false) return; // do nothing...
        Vector3 size = Vector3.one * WorldSeed.singleton.terrainSize * WorldSeed.singleton.voxelSize;
        Gizmos.DrawWireCube(transform.position + size/2, size);
    }
    
}

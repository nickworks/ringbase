using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(MeshFilter))]
public class ProceduralMesh : MonoBehaviour
{
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
    }
    public void MakeMesh(List<Triangle> tris){
        if(!meshFilter) meshFilter = GetComponent<MeshFilter>();
        if(!meshCollider) meshCollider = GetComponent<MeshCollider>();

        Mesh mesh = MeshTools.FromTris(tris);

        if(meshFilter) meshFilter.mesh = mesh;
        if(meshCollider) meshCollider.sharedMesh = mesh;
        
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }
}
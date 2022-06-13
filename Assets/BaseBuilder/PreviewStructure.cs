using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewStructure : MonoBehaviour
{
    public Transform art;
    private MeshRenderer[] meshes;
    public bool doesCollide {
        get;
        private set;
    }
    public LayerMask collidesWith;

    void Start(){
        
    }
    protected virtual void OnDrawGizmos(){

        var m = Gizmos.matrix;
        Gizmos.matrix = art ? art.localToWorldMatrix : transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        Gizmos.matrix = m;
    }
    public virtual void DoCollisionCheck(){
        Transform xform = art ? art : transform;

        doesCollide = (Physics.CheckBox(
            xform.position,
            xform.lossyScale/2,
            xform.rotation,
            collidesWith));

        UpdateMaterial();
    }
    public void UpdateMaterial(){
        bool isOkay = !doesCollide;
        if(meshes == null){
            meshes = art.GetComponentsInChildren<MeshRenderer>();
        }
        foreach(MeshRenderer mesh in meshes){
            mesh.material.color = isOkay ? new Color(0, 1, 0, .25f) : new Color(1, 0, 0, .25f);
        }
    }
}
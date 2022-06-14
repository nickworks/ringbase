using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structure : MonoBehaviour
{
    public BuilderNode startNodeVertical;
    public BuilderNode startNodeLateral;
    void Start()
    {
        BuilderNode[] nodes = GetComponentsInChildren<BuilderNode>();
    }

    public PreviewStructure previewPrefab;

    
    public virtual PreviewStructure MakePreview(BuilderNode parent, BuilderNode.BuildDirection buildDirection){
        if(previewPrefab == null) return null;

        //preview.transform.rotation *= 

        PreviewStructure preview = Instantiate(previewPrefab);

        BuilderNode startingNode = null;
        if(buildDirection == BuilderNode.BuildDirection.Vertical) startingNode = startNodeVertical;
        if(buildDirection == BuilderNode.BuildDirection.Lateral) startingNode = startNodeLateral;
        Quaternion rotation = Quaternion.identity;

        if(startingNode != null){
            
            Quaternion alignGeometryWithForward = Quaternion.FromToRotation(startingNode.transform.up, Vector3.forward);
            Quaternion turnForwardToParent = Quaternion.LookRotation(-parent.transform.up, parent.transform.forward);
            preview.transform.rotation =  turnForwardToParent * alignGeometryWithForward;
            preview.transform.position += parent.transform.position - (preview.transform.TransformPoint(startingNode.transform.localPosition));
            preview.transform.parent = parent.transform;
        }

        //preview.transform.localPosition += localOffset;
        
        return preview;
    }
}

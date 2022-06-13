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

    
    public virtual PreviewStructure MakePreview(Transform parent, BuilderNode.BuildDirection buildDirection){
        if(previewPrefab == null) return null;

        PreviewStructure preview = Instantiate(previewPrefab, parent);

        Vector3 localOffset = Vector3.zero;
        if(buildDirection == BuilderNode.BuildDirection.Vertical){
            localOffset = -startNodeVertical.transform.localPosition;
        }
        if(buildDirection == BuilderNode.BuildDirection.Lateral){
            localOffset = -startNodeLateral.transform.localPosition;
        }

        preview.transform.localPosition += localOffset;
        
        return preview;
    }
}

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

    
    void Update()
    {
        
    }
}

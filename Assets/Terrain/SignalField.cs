using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum SignalType
{
	Add,
	Subtract,
	Multiply,
	Average,
	Fill,
	Invert,
	Square,
	Block,
	None
};

[System.Serializable]
public class SignalField : ISerializationCallbackReceiver
{
	public string name;
    public SignalType type;
    public Vector3 center;
    public float zoom;
    public float outputMultiplier;
    public float densityBias;
    public float flattenToPlane;
    public float flattenToSphere;
    public float flattenOffset;

	public SignalField(
        string name = "Example Field",
        float zoom = 1,
        float outputMultiplier = 1,
        Vector3? center = null,
        float densityBias = 0,
        float flattenToPlane =0,
        float flattenToSphere = 0,
        float flattenOffset = 0,
        SignalType type = SignalType.Add)
	{
		this.name = name;
		this.zoom = zoom;
		this.outputMultiplier = outputMultiplier;
		this.center = center != null ? (Vector3)center : Vector3.zero;
		this.densityBias = densityBias;
		this.flattenToPlane = flattenToPlane;
		this.flattenToSphere = flattenToSphere;
		this.flattenOffset = flattenOffset;
		this.type = type;
	}
    [SerializeField] private bool _serialized = false;
    public void OnBeforeSerialize()
    {
    }
 
    public void OnAfterDeserialize()
    {
        if (_serialized == false) {
            
            name = "Example Field";
            zoom = 1;
            outputMultiplier = 1;
            center = Vector3.zero; // pig
            densityBias = 0;
            flattenToPlane =0;
            flattenToSphere = 0;
            flattenOffset = 0;
            type = SignalType.Add;
            
            _serialized = true;
        }
    }
}
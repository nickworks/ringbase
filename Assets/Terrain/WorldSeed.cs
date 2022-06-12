using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

public class WorldSeed : MonoBehaviour
{
    
    [Range(0,1)]
    public float threshold = 0;

    [Range(.25f,2.0f)]
    public float voxelSize = 1;

    [Range(1,50)]
    public int terrainSize = 12;

    [Range(0,5)]
    public int viewDistance = 1;

    public List<SignalField> TheSignalFields;

    public PlayerMovement player;
    private Index3D _previousPlayerPosition;

    public ProceduralTerrain prefabChunk;

    public struct Index3D {
        int x, y, z;
        public Index3D(int x, int y, int z){
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public Vector3 position {
            get {
                WorldSeed seed = WorldSeed.singleton;
                float size = seed ? (seed.terrainSize * seed.voxelSize) : 1;
                return new Vector3(
                    x * size,
                    y * size,
                    z * size
                );
            }
        }
        static public Index3D From(Vector3 pos){
            WorldSeed seed = WorldSeed.singleton;
            float size = seed ? (seed.terrainSize * seed.voxelSize) : 1;
            return new Index3D(
                (int)Mathf.Floor(pos.x / size),
                (int)Mathf.Floor(pos.y / size),
                (int)Mathf.Floor(pos.z / size)
            );
        }
        static public Index3D operator +(Index3D a, Index3D b){
            return new Index3D(
                a.x + b.x,
                a.y + b.y,
                a.z + b.z
            );
        }
        static public bool operator ==(Index3D a, Index3D b){
            if(a.x != b.x) return false;
            if(a.y != b.y) return false;
            if(a.z != b.z) return false;
            return true;
        }
        static public bool operator !=(Index3D a, Index3D b){
            return !(a == b);
        }
        public override bool Equals(object obj)
        {
            return (obj is Index3D p && p == this);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hashcode = 1430287;
                hashcode = hashcode * 7302013 ^ x.GetHashCode();
                hashcode = hashcode * 7302013 ^ y.GetHashCode();
                hashcode = hashcode * 7302013 ^ z.GetHashCode();
                return hashcode;
            }
        }
    }
    Dictionary<Index3D,ProceduralTerrain> chunks = new Dictionary<Index3D, ProceduralTerrain>();

    List<ProceduralTerrain> buildQueue = new List<ProceduralTerrain>();

    CancellationTokenSource cancellationTokenSource;

    void Start(){
        if(singleton){
            Destroy(gameObject);
            return;
        }
        singleton = this;
        _previousPlayerPosition = Index3D.From(player ? player.transform.position : Vector3.zero);
        BeginQueue();   
    }
    private void BeginQueue(){
        StopQueue();
        cancellationTokenSource = new CancellationTokenSource();
        Task t = ProcessQueue(cancellationTokenSource.Token);
    }
    private void StopQueue(){
        if(cancellationTokenSource != null) cancellationTokenSource.Cancel();
    }
    async Task ProcessQueue(CancellationToken cancellationToken){
        while(true){
            if (buildQueue.Count > 1){
                Task t = buildQueue[0].MarchGeometryAsync();
                buildQueue.RemoveAt(0);
            }
            await Task.Delay(10);
            if (cancellationToken.IsCancellationRequested) break;
        }
    }
    void OnDestroy(){
        StopQueue();
        if(singleton == this){
            singleton = null;
        }
    }
    void Update(){
        if(player){
            Index3D p = Index3D.From(player.transform.position);
            if(p != _previousPlayerPosition){
                LoadRegion(p);
            }
            _previousPlayerPosition = p;
        }
    }
    public void LoadRegion(Index3D p){
        int d = viewDistance;
        for(int x = -d; x <= d; x++){
            for(int y = -d; y <= d; y++){
                for(int z = -d; z <= d; z++){
                    LaunchChunk(p + new Index3D(x, y, z));
                }   
            }   
        }
    }
    public void LaunchChunk(Index3D p){
        if(chunks.ContainsKey(p)) {
            //print("chunk already exists...");
            return;
        }
        if(prefabChunk == null) {
            //print("prefab is null");
            return;
        }

        //print("launching chunk...");
        ProceduralTerrain chunk = Instantiate(prefabChunk, p.position, Quaternion.identity);
        chunks.Add(p, chunk);
        buildQueue.Add(chunk);
    }
    public void BuildWorld(){
        singleton = this;
    }
    public async Task<float[,,]> GetDensityField(Vector3 worldLocation){

        int res = terrainSize + 1; // number of densities needed = voxels + 1
        float[,,] densities = new float[res, res, res];

        int steps = 10;
        int chunks_per_frame = res / steps;

        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                for (int z = 0; z < res; z++)
                {
                    // local location:
                    Vector3 locloc = new Vector3(x, y, z) * voxelSize;

                    // world location:
                    Vector3 seed = worldLocation + locloc;

                    // get density:
                    densities[x,y,z] = GetDensitySample(seed);
                }
            }
            if(x % chunks_per_frame == 0) await Task.Yield();
        }
        return densities;
    }
    public float GetDensitySample(Vector3 pos){
        float res = 0;

        for (int i = 0; i < TheSignalFields.Count; i++)
        {

            SignalField field = TheSignalFields[i];

            if (field.type == SignalType.Fill) {
                res = field.densityBias;
                continue; // no perlin noise needed for fill, continue to next SignalField
            }
            else if (field.type == SignalType.Square) {
                res *= res;
                continue; // no perlin noise needed for Square, continue to next SignalField
            }
            else if (field.type == SignalType.Invert) {
                res = 1 - res;
                continue; // no perlin noise needed for invert, continue to next SignalField
            }

            // use position to sample perlin noise
            float val = Noise.Perlin((pos + field.center) * field.zoom); // 0 to 1

            // apply sphere flattening:
            if (field.flattenToSphere > 0) {

                float thresh = field.flattenOffset * field.flattenOffset;
                float dist = (pos - field.center).sqrMagnitude;
                float sphereDensity = (thresh - dist) / thresh;

                // f = k / dis
                // dis goes up, f should go down


                // close to center 1
                // near flattenOffset .5
                // far away 0

                val = Mathf.Lerp(val, sphereDensity, field.flattenToSphere);
            }
            // apply plane flattening:
            if (field.flattenToPlane > 0) {

                val += (field.flattenOffset - pos.z) * field.flattenToPlane * field.flattenToPlane * field.flattenToPlane * .01f;
            }

            // limit the overall influence of each signal to just 0 to 1
            if (val < 0) val = 0;
            if (val > 1) val = 1;

            // adjust the final density using the densityBias:
            val *= field.outputMultiplier;
            val += field.densityBias;


            // adjust how various fields are mixed together:
            switch (field.type)
            {
            case SignalType.Fill: // this should never happen, but just in case...
                res = field.densityBias;
                break;
            case SignalType.Block:
                if (
                    Mathf.Abs(pos.x - field.center.x) < 1000 &&
                    Mathf.Abs(pos.y - field.center.y) < 1000 &&
                    Mathf.Abs(pos.z - field.center.z) < 1000) {
                    res = 1; // solid
                }
                else {
                    res = 0; // air
                }
                break;
            case SignalType.Add:
                res += val;
                break;
            case SignalType.Subtract:
                res -= val;
                break;
            case SignalType.Multiply:
                res *= val;
                break;
            case SignalType.Average:
                res = (val + res) / 2;
                break;
            case SignalType.None:
                break;
            }
            if (res > 1) res = 1;
            if (res < 0) res = 0;
        }
        return res;
    }
    static public WorldSeed singleton {
        get;
        private set;
    }
}

[CustomEditor(typeof(WorldSeed))]
public class WorldSeedEditor : Editor {
    override public void OnInspectorGUI(){
        base.OnInspectorGUI();

        if(GUILayout.Button("Build")){
            (target as WorldSeed).BuildWorld();
        }
    }
}
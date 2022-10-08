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
        /// returns the world space position of the corner of the box
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

    BuildQueue queue = new BuildQueue();

    void Start(){
        if(singleton){
            Destroy(gameObject);
            return;
        }
        singleton = this;
        _previousPlayerPosition = Index3D.From(player ? player.transform.position : Vector3.zero);   
    }
    class BuildQueue {
        List<KeyValuePair<float,ProceduralTerrain>> queue = new List<KeyValuePair<float,ProceduralTerrain>>();
        public BuildQueue(){

        }
        public void Add(ProceduralTerrain chunk, float priority){
            queue.Add(new KeyValuePair<float, ProceduralTerrain>(priority, chunk));
        }
        CancellationTokenSource cancellationTokenSource;
        public void BeginQueue(){
            if(cancellationTokenSource == null) {
                cancellationTokenSource = new CancellationTokenSource();
                Task t = ProcessQueue(cancellationTokenSource.Token);
            }
        }
        public void StopQueue(){
            if(cancellationTokenSource != null) cancellationTokenSource.Cancel();
            cancellationTokenSource = null;
            queue.Clear();
        }
        private async Task ProcessQueue(CancellationToken cancellationToken){
            while(true){
                //print("queue running");
                if (queue.Count > 1){

                    float threshold = float.MaxValue;
                    int index = 0;
                    for(int i = 0; i < queue.Count; i++){
                        if(queue[i].Key < threshold) {
                            threshold = queue[i].Key;
                            index = i;
                        }
                    }
                    //print($"about to march queue#{index} a chunk with priority of {queue[index].Value}");
                    // CANNOT await this next step, the queue may change
                    Task t = queue[index].Value.MarchGeometryAsync();
                    queue.RemoveAt(index);
                } else {
                    //print("queue.Count is ZERO");
                }
                await Task.Yield();
                if (cancellationToken.IsCancellationRequested) break;
            }
        }
    }
    void OnDestroy(){
        queue.StopQueue();
        queue = null;
        if(singleton == this){
            singleton = null;
        }
    }
    void Update(){
        LoadRegionsAroundPlayer();
    }
    public void LoadRegionsAroundPlayer(bool ignorePreviousPosition = false){

        if(player == null) {
            print("there is no player...");
            return;
        }

        // get player position:
        Index3D pos3D = Index3D.From(player.transform.position);

        // if in same spot as last frame, do nothing
        if(pos3D == _previousPlayerPosition && ignorePreviousPosition == false) return;
        print("player has entered new region");

        _previousPlayerPosition = pos3D;

        // begin queue if it's not already running
        queue.BeginQueue();

        int d = viewDistance;

        for(int x = -d; x <= d; x++){
            for(int y = -d; y <= d; y++){
                for(int z = -d; z <= d; z++){
                    Index3D offset = new Index3D(x, y, z); 
                    Vector3 vectorToPlayer = (pos3D + offset).position - player.transform.position;

                    float distance_to_player = vectorToPlayer.magnitude;

                    // 0 if behind the player
                    // 1 if in front of the player
                    float align_with_players_view = (Vector3.Dot(player.cam.transform.forward, vectorToPlayer) + 1)/2;

                    float delay = distance_to_player + (1 - align_with_players_view);

                    LaunchChunkWithDelay(pos3D + offset, delay);
                }   
            }   
        }
    }
    public void LaunchChunkWithDelay(Index3D p, float delay){
        if(chunks.ContainsKey(p)) {
            //print("chunk already exists...");
            return;
        }
        if(prefabChunk == null) {
            //print("prefab is null");
            return;
        }

        print("launching chunk...");
        ProceduralTerrain chunk = Instantiate(prefabChunk, p.position, Quaternion.identity);
        chunks.Add(p, chunk);
        queue.Add(chunk, delay);
    }
    public void BuildWorld(){
        if(!Application.isPlaying) return;
        singleton = this;
        ClearWorld();
        LoadRegionsAroundPlayer(true);
    }
    public void ClearWorld(){
        
        if(!Application.isPlaying) return;

        // stop and empty the queue
        queue.StopQueue();

        // destroy existing chunks:
        foreach(KeyValuePair<Index3D, ProceduralTerrain> kp in chunks){
            if(kp.Value == null) continue;
            Destroy(kp.Value.gameObject);
        }
        chunks.Clear();
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

        WorldSeed seed = (target as WorldSeed);

        if(GUILayout.Button("Build")) seed.BuildWorld();
        if(GUILayout.Button("Clear")) seed.ClearWorld();
        
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuilderNode : MonoBehaviour, IPlayerInteractable
{
    public float meters_in_front_of_player = 15;
    private Structure selectedStructure = null;
    private PreviewStructure preview = null;
    private Structure structure1 = null;
    private Structure structure2 = null;

    public enum BuildDirection {
        Vertical,
        Lateral,
    }
    public bool OnlyAvailableAsARoot = false;

    public float rotateSpeed = 45;
    private float roll = 0;
    private float yaw = 0;
    private float pitch = 0;
    private float limitYaw = 90;
    private float limitPitch = 30;
    private float limitRoll = 30;

    class States {
        public abstract class State {
            protected BuilderNode node;
            public virtual void OnBegin(BuilderNode node){
                this.node = node;
            }
            public virtual void OnEnd(){}
            public virtual State Update(){
                return null;
            }
            public virtual State Interact(){
                return null;
            }
            public virtual State Cancel(){
                return null;
            }
        }
        public static State GetDefault(){
            return new Idle();
        }
        class Idle : State {
            override public State Interact(){
                if(node.structure2 == null){
                    return new Picking();
                }
                return null;
            }
            override public State Cancel(){
                return null;
            }
        }
        class Picking : State {
            public override void OnBegin(BuilderNode node)
            {
                base.OnBegin(node);
                Interact();
            }
            public override State Update()
            {
                if(node.preview != null) return new Placing();
                return null;
            }
            override public State Interact(){
                // show interface to pick preview:
                node.PickStructure();
                return null;
            }
            override public State Cancel(){
                return null;
            }
            
        }
        class Placing : State {
            override public State Interact(){
                if(!node.preview.doesCollide) return new Constructing();
                return null;
            }
            override public State Cancel(){
                return new Picking();
            }
            public override State Update()
            {
                node.RotatePreview();
                if(Input.GetButtonDown("Fire1")) {
                    return Interact();
                }
                return null;
            }
        }
        class Constructing : State {
            float seconds_remaining;
            private float seconds_total = 3;
            public float percent {
                get {
                    return Mathf.Clamp(1 - (seconds_remaining / seconds_total), 0, 1);
                }
            }
            public override void OnBegin(BuilderNode node)
            {
                base.OnBegin(node);
                seconds_remaining = seconds_total;
            }
            public override State Update()
            {
                // hold to build:
                if(Input.GetButton("Fire1")) seconds_remaining -= Time.deltaTime;
                
                // if done building:
                if(seconds_remaining <= 0){
                    node.BuildStructure();
                }
                if(node.structure2 != null){
                    return new Idle();
                }
                return null;
            }
            override public State Interact(){
                return null;
            }
            override public State Cancel(){
                return new Placing();
            }
        }
    }

    States.State state;
    PlayerInteract player;
    public BuildDirection buildDirection = BuildDirection.Vertical;
    Quaternion startingRotation;

    void Start(){
        startingRotation = transform.localRotation;
        if(OnlyAvailableAsARoot) Destroy(gameObject);
    }
    private void SwitchState(States.State nextState){
        if(nextState == null) return;
        if(state != null) state.OnEnd();
        state = nextState;
        state.OnBegin(this);
    }
    public void Interact(PlayerInteract player)
    {
        this.player = player;
        if(state != null) SwitchState(state.Interact());
    }
    public void Cancel()
    {
        if(state != null) SwitchState(state.Cancel());
    }
    public void Update()
    {
        if(state == null) SwitchState(States.GetDefault());
        if(state != null) SwitchState(state.Update());
    }

    private void PickStructure(){

        // tell player to launch picker GUI:
        if(player) player.PickStructure(this);
    }
    public void PickStructure(Structure structure){
        selectedStructure = structure;
        SetPreview(selectedStructure);
    }
    private void SetPreview<T>(T prefab) where T : Structure {

        if(prefab == null) return;
        if(preview != null) Destroy(preview.gameObject);
        
        preview = prefab.MakePreview(this, buildDirection);
        RotatePreview();
    }

    Vector3 lookNode;
    void OnDrawGizmos(){
        Gizmos.DrawWireSphere(lookNode, .1f);
    }
    private void RotatePreview(bool forceCollisionCheck = true){
        // rotate the preview so that
        if(preview == null) return;

        if(Input.GetButton("Fire2")){
            forceCollisionCheck = true;
            if(buildDirection == BuildDirection.Vertical){
                
            }
            if(buildDirection == BuildDirection.Lateral){    
            }
            yaw += Input.GetAxisRaw("LookRoll") * rotateSpeed * Time.deltaTime;
            pitch += Input.GetAxisRaw("MoveForward") * rotateSpeed * Time.deltaTime;
            roll += Input.GetAxisRaw("MoveRight") * rotateSpeed * Time.deltaTime;

            pitch = Mathf.Clamp(pitch, -limitPitch, limitPitch);
            yaw = Mathf.Clamp(yaw, -limitYaw, limitYaw);
            roll = Mathf.Clamp(roll, -limitRoll, limitRoll);

            transform.localRotation = startingRotation * Quaternion.Euler(pitch, yaw, roll);

        }
        if(forceCollisionCheck) DoCollisionCheck();
    }
    private void DoCollisionCheck(){
        if(preview == null) return;
        preview.DoCollisionCheck();
    }
    private void BuildStructure(){
        if(preview == null) return; // no preview to clone...
        if(structure2 != null) return; // slot already has a structure!

        structure2 = Instantiate(selectedStructure, preview.transform.position, preview.transform.rotation);
    }
}

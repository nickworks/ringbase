using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuilderNode : MonoBehaviour, IPlayerInteractable
{
    public RectRoom prefabRoom;
    private RectRoomPreview preview = null;
    private RectRoom structure1 = null;
    private RectRoom structure2 = null;

    public enum BuildDirection {
        Vertical,
        Lateral,
    }
    public bool OnlyAvailableAsARoot = false;

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
                return new Constructing();
            }
            override public State Cancel(){
                return new Picking();
            }
            public override State Update()
            {
                node.RotatePreview();
                if(Input.GetButtonDown("Fire1")) {
                    // TODO: do a collision check...
                    return new Constructing();
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
    PlayerMovement player;
    public BuildDirection buildDirection = BuildDirection.Vertical;
    Quaternion startingRotation;

    void Start(){
        startingRotation = transform.rotation;
        if(OnlyAvailableAsARoot) Destroy(gameObject);
    }
    private void SwitchState(States.State nextState){
        if(nextState == null) return;
        if(state != null) state.OnEnd();
        state = nextState;
        state.OnBegin(this);
    }
    public void Interact(PlayerMovement player)
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

        // TODO: show gui
        // pick a prefab to build?
        // or maybe a class?

        // if no cancel, then:
        SetPreview(prefabRoom);
    }
    private void SetPreview<T>(T prefab) where T : RectRoom {

        if(prefab == null) return;
        if(preview != null) Destroy(preview.gameObject);
        
        preview = prefab.MakePreview(transform, buildDirection);
        RotatePreview();
    }
    private void RotatePreview(){
        // rotate the preview so that
        if(preview == null) return;

        float meters_in_front_of_player = 5;
        Vector3 pos = player.transform.position + player.transform.forward * meters_in_front_of_player;

        Vector3 dir_to_pos = pos - transform.position;

        // calc rotation
        Quaternion targetRot = startingRotation;
        if(buildDirection == BuildDirection.Vertical){
            targetRot = Quaternion.FromToRotation(Vector3.up, dir_to_pos);
        }
        if(buildDirection == BuildDirection.Lateral){    
            targetRot = Quaternion.LookRotation(dir_to_pos, transform.up);
        }
        // limit rotation
        Quaternion finalRot = Quaternion.RotateTowards(startingRotation, targetRot, 20);
        transform.rotation = finalRot;
    }
    private void BuildStructure(){
        if(preview == null) return; // no preview to clone...
        if(structure2 != null) return; // slot already has a structure!

        structure2 = Instantiate(prefabRoom, preview.transform.position, preview.transform.rotation);
    }
}

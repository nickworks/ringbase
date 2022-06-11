using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{

    public float walkSpeed = 5f;
    public float jetpackSpeed = 15f;
    public float jumpImpulse = 10f;
    public Vector3 lookSensitivity = new Vector3(5, -5, .5f);
    private float yaw = 0;
    private float pitch = 0;
    private float roll = 0;
    private Rigidbody body;
    private Camera cam;

    public bool gravityIsOn = false;
    public bool playerIsGrounded = false;
    public Vector3 gravityDirection = Vector3.down;
    public float gravityAmount = 10;
    Vector3 lastPosition = Vector3.zero;
    Vector3 groundNormal = Vector3.up;

    Vector3 velocity = Vector3.zero;

    public class States {
        public abstract class State {
            protected PlayerMovement script = null;
            public abstract void Look();
            public abstract void UpdateVelocity();
            public virtual void OnBegin(PlayerMovement script){
                this.script = script;
            }
            public virtual void OnEnd() {}
        }
        public class Gravity : State {
            public override void Look()
            {
                Vector3 rot = script.GetLookInput();

                script.yaw += rot.y;
                script.pitch += rot.x;

                script.transform.rotation = Quaternion.FromToRotation(Vector3.down, script.gravityDirection) * Quaternion.Euler(0, script.yaw, 0);
                script.cam.transform.localEulerAngles = new Vector3(script.pitch, 0, 0);
            }
            public override void UpdateVelocity()
            {

                Vector3 input = script.GetMoveInput();
                if (input.sqrMagnitude > 1) input.Normalize();

                // gravity:
                script.ApplyGravity();
                
                bool jump = Input.GetButtonDown("Jump");
                if(script.playerIsGrounded){
                    if(jump){
                        script.Jump();
                        return;
                    }
                    script.SetVelocity(input * script.walkSpeed);
                }
            }
        }
        public class ZeroG : State {
            public override void Look()
            {
                Vector3 rot = script.GetLookInput();

                script.transform.localRotation *= Quaternion.Euler(rot);
                script.cam.transform.localEulerAngles = new Vector3(0, 0, 0);
            }
            public override void UpdateVelocity()
            {
                Vector3 desiredVelocity = script.GetMoveInput() * script.jetpackSpeed;
                //if (input.sqrMagnitude > 1) input.Normalize();
                
                if(script.playerIsGrounded){
                    print("zero G but on the ground");
                }
                
                script.SetVelocity(desiredVelocity);
            }
        }
    }
    private States.State state;

    void Start()
    {
        body = GetComponent<Rigidbody>();
        cam = GetComponentInChildren<Camera>();
        lastPosition = transform.position;
    }

    void Update()
    {

        //CalcVelocity();
        if(state == null) SwitchState(new States.ZeroG());
        state.Look();
        state.UpdateVelocity();

        if(Input.GetButtonDown("Gravity")) {
            gravityIsOn = !gravityIsOn;
            print($"gravity {(gravityIsOn?"on":"off")}");
            SwitchState(gravityIsOn ? new States.Gravity() : new States.ZeroG());
        }
    }
    void FixedUpdate(){
        playerIsGrounded = false;
    }
    private void SwitchState(States.State state){
        if(state == null) return;
        if(this.state != null) this.state.OnEnd();
        this.state = state;
        this.state.OnBegin(this);
    }
    private void SetVelocity(Vector3 desiredVelocity){
        body.velocity = Vector3.MoveTowards(body.velocity, desiredVelocity, 10 * Time.deltaTime);
    }
    private void AddVelocity(Vector3 desiredVelocity){
        body.velocity += desiredVelocity;
    }
    private void Jump(){
        body.velocity += jumpImpulse * -gravityDirection;
        playerIsGrounded = false;
    }
    private Vector3 GetLookInput(){
        float y = Input.GetAxisRaw("Mouse X") * lookSensitivity.x;
        float x = Input.GetAxisRaw("Mouse Y") * lookSensitivity.y;
        float z = Input.GetAxisRaw("LookRoll") * lookSensitivity.z;
        return new Vector3(x, y, z);
    }
    private Vector3 GetMoveInput(bool allDirections = true){
        float z = Input.GetAxisRaw("MoveForward");
        float x = Input.GetAxisRaw("MoveRight");
        float y = Input.GetAxisRaw("MoveUp");
        if(!allDirections) return transform.forward * z + transform.right * x;
        return cam.transform.forward * z + cam.transform.right * x + cam.transform.up * y;
    }
    private void ApplyGravity(){
        body.velocity += gravityDirection * gravityAmount * Time.deltaTime;
    }
    void OnControllerColliderHit(ControllerColliderHit hit){
        CalcVelocity();
    }
    void CalcVelocity(){
        Vector3 v = (transform.position - lastPosition)/Time.deltaTime;
        lastPosition = transform.position;
        if(v.sqrMagnitude < velocity.sqrMagnitude)  velocity = v;
    }
    Vector3 ProjectAlongGroundPlane (Vector3 v) {
		return v - groundNormal * Vector3.Dot(v, groundNormal);
	}
    void OnCollisionEnter (Collision collision) {
		EvaluateCollision(collision);
	}

	void OnCollisionStay (Collision collision) {
		EvaluateCollision(collision);
	}
    void EvaluateCollision (Collision collision) {

		for (int i = 0; i < collision.contactCount; i++) {
			Vector3 normal = collision.GetContact(i).normal;
            
            if(Vector3.Dot(normal, gravityDirection) <= -.9f){
                playerIsGrounded = true;
                groundNormal = normal;
            }
		}
	}
}

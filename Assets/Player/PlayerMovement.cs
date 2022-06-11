using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{

    public float walkSpeed = 5f;
    public float jetpackSpeed = 15f;
    public float jumpImpulse = 10f;
    public Vector2 lookSensitivity = new Vector2(2, -2);
    private float yaw = 0;
    private float pitch = 0;
    private float roll = 0;
    private Rigidbody body;
    private Camera cam;

    bool gravityIsOn = false;
    bool playerIsGrounded = false;
    Vector3 gravityDirection = Vector3.right;
    float gravityAmount = 10;
    Vector3 lastPosition = Vector3.zero;

    Vector3 velocity = Vector3.zero;

    void Start()
    {
        body = GetComponent<Rigidbody>();
        cam = GetComponentInChildren<Camera>();
        lastPosition = transform.position;
    }

    void Update()
    {

        //CalcVelocity();

        if(gravityIsOn){
            DoLookGravity();
            DoMoveGravity();
        } else {
            DoLookZeroG();
            DoMoveZeroG();
        }

        if(Input.GetButtonDown("Gravity")) {
            gravityIsOn = !gravityIsOn;
            print($"gravity {(gravityIsOn?"on":"off")}");
        }
    }
    void FixedUpdate(){
        playerIsGrounded = false;
    }

    private void DoLookGravity()
    {
        float mx = Input.GetAxisRaw("Mouse X");
        float my = Input.GetAxisRaw("Mouse Y");

        yaw += mx * lookSensitivity.x;
        pitch += my * lookSensitivity.y;

        transform.rotation = Quaternion.FromToRotation(Vector3.down, gravityDirection) * Quaternion.Euler(0, yaw, 0);
        cam.transform.localEulerAngles = new Vector3(pitch, 0, 0);
    }
    private void DoLookZeroG()
    {
        float mx = Input.GetAxisRaw("Mouse X");
        float my = Input.GetAxisRaw("Mouse Y");
        float lookRoll = Input.GetAxisRaw("LookRoll");


        float ry = mx * lookSensitivity.x;
        float rx = my * lookSensitivity.y;
        float rz = lookRoll;

        transform.localRotation *= Quaternion.Euler(rx,ry,rz);
        cam.transform.localEulerAngles = new Vector3(0, 0, 0);
    }


    private void DoMoveGravity()
    {
        float z = Input.GetAxisRaw("MoveForward");
        float x = Input.GetAxisRaw("MoveRight");

        bool jump = Input.GetButtonDown("Jump");

        Vector3 input = transform.forward * z + transform.right * x;
        if (input.sqrMagnitude > 1) input.Normalize();

        if(!playerIsGrounded){
            body.velocity += gravityDirection * gravityAmount * Time.deltaTime;
        } else {
            body.velocity += input * walkSpeed * Time.deltaTime;
            if(jump){
                body.velocity += 5 * -gravityDirection;
                playerIsGrounded = false;
            }
        }
    }
    private void DoMoveZeroG(){


        float z = Input.GetAxisRaw("MoveForward");
        float x = Input.GetAxisRaw("MoveRight");
        float y = Input.GetAxisRaw("MoveUp");
        
        Vector3 input = cam.transform.forward * z + cam.transform.right * x + cam.transform.up * y;
        //if (input.sqrMagnitude > 1) input.Normalize();
        
        if(playerIsGrounded){
            print("zero G but on the ground");
        }

        body.velocity += input * jetpackSpeed * Time.deltaTime;
    }
    void OnControllerColliderHit(ControllerColliderHit hit){
        CalcVelocity();
    }
    void CalcVelocity(){
        Vector3 v = (transform.position - lastPosition)/Time.deltaTime;
        lastPosition = transform.position;
        if(v.sqrMagnitude < velocity.sqrMagnitude) velocity = v;
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
            playerIsGrounded |= Vector3.Dot(normal, gravityDirection) <= -.9f;
		}
	}
}

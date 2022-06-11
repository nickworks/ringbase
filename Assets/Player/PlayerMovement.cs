using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{

    public float walkSpeed = 5f;
    public float jetpackSpeed = 15f;
    public Vector2 lookSensitivity = new Vector2(2, -2);
    private float yaw = 0;
    private float pitch = 0;
    private CharacterController pawn;
    private Camera cam;

    bool gravityIsOn = false;
    Vector3 gravityDirection = Vector3.down;
    float gravityAmount = 10;
    Vector3 lastPosition = Vector3.zero;

    Vector3 velocity = Vector3.zero;

    void Start()
    {
        pawn = GetComponent<CharacterController>();
        cam = GetComponentInChildren<Camera>();
        lastPosition = transform.position;
    }

    void Update()
    {
        DoLook();

        //CalcVelocity();

        if(gravityIsOn)
            DoMoveGravity();
        else
            DoMoveZeroG();

        if(Input.GetButtonDown("Gravity")) {
            gravityIsOn = !gravityIsOn;
            print($"gravity {(gravityIsOn?"on":"off")}");
        }
    }

    private void DoLook()
    {
        float mx = Input.GetAxisRaw("Mouse X");
        float my = Input.GetAxisRaw("Mouse Y");

        yaw += mx * lookSensitivity.x;
        pitch += my * lookSensitivity.y;

        transform.eulerAngles = new Vector3(0, yaw, 0);
        cam.transform.localEulerAngles = new Vector3(pitch, 0, 0);
    }

    private void DoMoveGravity()
    {
        float z = Input.GetAxisRaw("MoveForward");
        float x = Input.GetAxisRaw("MoveRight");

        Vector3 input = transform.forward * z + transform.right * x;
        if (input.sqrMagnitude > 1) input.Normalize();

        if(!pawn.isGrounded){
            velocity += gravityDirection * gravityAmount * Time.deltaTime;
        } else {
            velocity = input * walkSpeed;
        }
        
        pawn.Move(velocity * Time.deltaTime);
    }
    private void DoMoveZeroG(){


        float z = Input.GetAxisRaw("MoveForward");
        float x = Input.GetAxisRaw("MoveRight");
        float y = Input.GetAxisRaw("MoveUp");
        
        Vector3 input = cam.transform.forward * z + cam.transform.right * x + cam.transform.up * y;
        //if (input.sqrMagnitude > 1) input.Normalize();
        
        velocity += input * jetpackSpeed * Time.deltaTime;
        pawn.Move(velocity * Time.deltaTime);
    }
    void OnControllerColliderHit(ControllerColliderHit hit){
        CalcVelocity();
    }
    void CalcVelocity(){
        Vector3 v = (transform.position - lastPosition)/Time.deltaTime;
        lastPosition = transform.position;
        if(v.sqrMagnitude < velocity.sqrMagnitude) velocity = v;
    }

}

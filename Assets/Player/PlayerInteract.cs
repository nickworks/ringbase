using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    
    Camera cam;
    void Start()
    {
        cam = GetComponentInChildren<Camera>();
    }
    void Update()
    {
        bool wantsToInteract = Input.GetButtonDown("Fire1");
        if(wantsToInteract){
            
            float distance = 5;
            Debug.DrawRay(cam.transform.position, cam.transform.forward * distance, Color.white, 1f);

            if(Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, distance)){
                print("hit!");
                AirLock door = hit.collider.GetComponentInParent<AirLock>();
                PlayerMovement movement = GetComponent<PlayerMovement>();
                if(door != null && movement != null){
                    door.Interact(movement);
                }
            }
        }
    }
}

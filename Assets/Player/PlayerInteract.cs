using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    
    public StructurePicker prefabStructurePicker;

    Modal currentGUI = null;

    Camera cam;
    void Start()
    {
        cam = GetComponentInChildren<Camera>();
    }
    void Update()
    {
        if(currentGUI == null) CheckMouseForInput();
    }
    private void CheckMouseForInput(){
        bool wantsToInteract = Input.GetButtonDown("Fire1");
        if(wantsToInteract){
            
            float distance = 5;
            Debug.DrawRay(cam.transform.position, cam.transform.forward * distance, Color.white, 1f);

            if(Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, distance)){

                IPlayerInteractable interactable = hit.collider.GetComponentInParent<IPlayerInteractable>();
                if(interactable != null){
                    interactable.Interact(this);
                }
            }
        }
    }

    public void PickStructure(BuilderNode node){
        
        if(currentGUI != null) return;
        if(prefabStructurePicker == null) return;

        currentGUI = Instantiate(prefabStructurePicker);

        PlayerMovement mover = GetComponent<PlayerMovement>();
        mover.shouldIgnoreInput = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        currentGUI.onCancel.AddListener(()=>{
            mover.shouldIgnoreInput = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        });
        currentGUI.onSubmit.AddListener(()=>{
            mover.shouldIgnoreInput = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            node.PickStructure(LibraryStructures.singleton.GetPrefab());
        });
    }

}

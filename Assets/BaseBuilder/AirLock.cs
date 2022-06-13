using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirLock : MonoBehaviour, IPlayerInteractable
{
    public RectRoom room;

    public void Interact(PlayerInteract player){

        PlayerMovement pMove = player.GetComponent<PlayerMovement>();

        Vector3 vToPlayer = player.transform.position - transform.position;
        float alignment = Vector3.Dot(vToPlayer, transform.forward);

        bool goingInside = (alignment > 0);

        if(goingInside){
            pMove.GravityOn(-room.transform.up);
            pMove.transform.position = transform.position - transform.forward;
        } else {
            pMove.GravityOff();
            pMove.transform.position = transform.position + transform.forward;
        }
    }
}

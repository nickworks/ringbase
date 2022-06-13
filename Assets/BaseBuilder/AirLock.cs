using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirLock : MonoBehaviour, IPlayerInteractable
{
    public RectRoom room;

    public void Interact(PlayerMovement player){

        Vector3 vToPlayer = player.transform.position - transform.position;
        float alignment = Vector3.Dot(vToPlayer, transform.forward);

        bool goingInside = (alignment > 0);

        if(goingInside){
            player.GravityOn(-room.transform.up);
            player.transform.position = transform.position - transform.forward;
        } else {
            player.GravityOff();
            player.transform.position = transform.position + transform.forward;
        }
    }
}

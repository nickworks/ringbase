using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirLock : MonoBehaviour
{
    public RectRoom room;

    public void Interact(PlayerMovement player){
        
        player.GravityOn(-room.transform.up);
        player.transform.position = this.transform.position - this.transform.forward;
    }
}

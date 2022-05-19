using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedBullet : NetworkedBehaviour
{
    // TODO: Make sure this is true when creating this object on the server
    public bool isServer = false;

    float lifeTime = 5f;

    // Update is called once per frame
    void Update()
    {
        // just fly in fired direction
        transform.Translate(transform.forward * Time.deltaTime * 10);

        if ( isServer ) {
            // (optional) TODO: raycast and handle collision with player objects?
            //  if hit
            //      TODO: Destroy bullet, broadcast destroy, (optional) send HitMessage (need to make it first!)


            // TODO: Decrement lifeTime
            // if < 0
            //      TODO: Broadcast Destroy
        }
    }
}

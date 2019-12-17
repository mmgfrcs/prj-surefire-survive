using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionMessenger : MonoBehaviour {

    void Collide(string cType, Collision coll)
    {
        Rigidbody pt = transform.parent.parent.GetComponent<Rigidbody>();
        print("Rigidbody: " + pt.gameObject.name);
        pt.SendMessage("OnCollision" + cType, coll);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Collide("Enter", collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        Collide("Stay", collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        Collide("Exit", collision);
    }
}

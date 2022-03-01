using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallCollision : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "ActiveBalls")
        {
            this.GetComponent<Rigidbody>().velocity = Vector2.zero;
            this.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
            this.gameObject.tag = "ActiveBalls";

            ContactPoint contact = collision.contacts[0];

            Vector3 CollisionDir = contact.point - collision.transform.position;


            int currentIdx = collision.transform.GetSiblingIndex();

        }
    }
}

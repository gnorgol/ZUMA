using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;
public class BallCollision : MonoBehaviour
{
    bool stopCollision = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "ActiveBalls" && !stopCollision)
        {
            stopCollision = true;
            this.gameObject.GetComponent<BallCollision>().enabled = false;
           
            this.GetComponent<Rigidbody>().velocity = Vector2.zero;
            this.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            this.gameObject.tag = "ActiveBalls";
            ContactPoint contact = collision.contacts[0];
            Vector3 CollisionDir = contact.point - collision.transform.position;
            float angle = Vector3.Angle(CollisionDir, collision.transform.forward);
            if (angle > 90)
            {
                //Place la ball devant
                EventManager.Instance.Raise(new putBallForwardEvent() { target = collision.gameObject, ball = this.gameObject });
            }
            else
            {
                //Place la ball a l'arrier 
                EventManager.Instance.Raise(new putBallBackEvent() { target = collision.gameObject, ball = this.gameObject });
            }
            EventManager.Instance.Raise(new CheckMatchBallsEvent() { ball = this.gameObject });
            //this.gameObject.GetComponent<BallCollision>().enabled = false;

        }
    }
}

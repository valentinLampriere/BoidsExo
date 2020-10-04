using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

public class holeBehaviour : MonoBehaviour
{
    Collider2D collider;
    Vector3 velocity;
    void Start() {
        velocity = new Vector3(0, -1, 0);
        collider = GetComponent<Collider2D>();
    }
    private void FixedUpdate() {
        if (transform.position.y > 4 || transform.position.y < -4)
            velocity.y = -velocity.y;
        transform.position += velocity * Time.fixedDeltaTime;
        foreach(GameObject g in GameObject.FindGameObjectsWithTag("Ball")) {
            if(Vector3.Distance(transform.position, g.transform.position) < 0.5f) {
                Destroy(g);
            }
        }
    }
}

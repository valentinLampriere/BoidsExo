using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.VFX;

public class Boid : MonoBehaviour {
    public Collider2D boidCollider;

    public void Start() {
        boidCollider = GetComponent<Collider2D>();
    }

    public Vector3 calcVelocity(Vector3 direction, float speed) {
        return direction * Time.deltaTime * speed;
    }

    public void Move(Vector3 velocity) {
        transform.up = velocity;
        transform.position += velocity;
    }
}

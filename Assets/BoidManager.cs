using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour {
    [Header("Field settings")]
    [Range(5f, 20f)]
    public float widthOfArea = 20f;
    [Range(3.3f, 9.8f)]
    public float heightOfArea = 9.8f;
    [Range(1, 250)]
    public int amountOfBoids = 10;
    [Space]
    [Header("Boids settings")]
    [Range(1f, 5f)]
    public float boidVisionDistance = 5f;
    [Range(0.2f, 1f)]
    public float boidMinimumDistance = 0.4f;

    [Space]
    [Header("Boids stats")]
    public bool goThroughtWalls = false;
    [Range(0f, 1f)]
    public float cohesionStrength = 0.1f;
    [Range(0f, 1f)]
    public float separationStrength = 0.8f;
    [Range(0f, 1f)]
    public float alignementStrength = 0.1f;
    [Range(0.03f, 2f)]
    public float boidSpeed = 0.5f;

    public GameObject boid_gameObject;
    public GameObject obstacle_gameObject;

    List<Boid> allBoids = new List<Boid>();
    List<Collider2D> allObstacles = new List<Collider2D>();

    [HideInInspector]
    public Boid activeBoid;

    // Start is called before the first frame update
    void Start() {
        for (int i = 0; i < amountOfBoids; i++) {
            // Spawn a boid
            GameObject objBoid = Instantiate(boid_gameObject, UnityEngine.Random.insideUnitCircle * Mathf.Min(widthOfArea / 3, heightOfArea / 3) - new Vector2(5, 0), Quaternion.Euler(Vector3.forward * UnityEngine.Random.Range(0f, 360f)), transform);
            Boid b = objBoid.GetComponent<Boid>();
            b.tag = "Boid";
            objBoid.name = "Boid " + i;
            if (b != null) {
                allBoids.Add(b);
                b.Init(this);
            }
        }
    }

    void Update() {

        // Spawn obstacles on click
        if (Input.GetButtonDown("Fire2")) {
            Vector3 objectPosition;
            Vector3 mousePos = Input.mousePosition;
            objectPosition = Camera.main.ScreenToWorldPoint(mousePos);
            objectPosition.z = 0;
            GameObject o = Instantiate(obstacle_gameObject, objectPosition, Quaternion.identity);
            o.tag = "Obstacle";
            allObstacles.Add(o.GetComponent<Collider2D>());
        }


        // Player input
        if (Input.GetMouseButtonDown(0)) {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if (hit.collider != null) {
                Boid b = hit.collider.gameObject.GetComponent<Boid>();
                if (b != null) {
                    if (activeBoid != null)
                        activeBoid.SetState(Boid.boidState.FLOCKING);
                    activeBoid = b;
                    b.SetState(Boid.boidState.MOVABLE);
                } else {
                    if (activeBoid != null)
                        activeBoid.SetState(Boid.boidState.FLOCKING);
                    activeBoid = null;
                }
            } else {
                if (activeBoid != null)
                    activeBoid.SetState(Boid.boidState.FLOCKING);
                activeBoid = null;
            }
        }
        if (Input.GetKeyDown(KeyCode.Space)) {
            if (activeBoid != null) {
                Collider2D[] closeBoids = Physics2D.OverlapCircleAll(activeBoid.transform.position, boidVisionDistance / 2);
                foreach (Collider2D c in closeBoids) {
                    Boid b = c.gameObject.GetComponent<Boid>();
                    if (b != null && b.name != activeBoid.name) {
                        b.SetState(Boid.boidState.FIRED);
                    }
                }
            }
        }
    }   

    public int GetCloseObjects(Boid boid, out List<Transform> boids, out List<Transform> objects) {
        boids = new List<Transform>();
        objects = new List<Transform>();
        Collider2D[] contextColliders = Physics2D.OverlapCircleAll(boid.transform.position, boidVisionDistance);
        int nb = 0;
        foreach (Collider2D c in contextColliders) {
            if (c != boid.boidCollider && c.gameObject.CompareTag("Boid")) {
                boids.Add(c.transform);
            }
            if (c != boid.boidCollider && c.gameObject.CompareTag("Obstacle")) {
                objects.Add(c.transform);
            }
            nb++;
        }
        return nb;
    }
}

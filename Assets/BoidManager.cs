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
    [Range(0.01f, 5.0f)]
    public float boidSpeed = 0.5f;

    public GameObject boid_gameObject;
    public GameObject obstacle_gameObject;

    List<Boid> allBoids = new List<Boid>();
    List<Collider2D> allObstacles = new List<Collider2D>();

    private Boid activeBoid;

    // Start is called before the first frame update
    void Start() {
        for (int i = 0; i < amountOfBoids; i++) {
            // Spawn a boid
            GameObject objBoid = Instantiate(boid_gameObject, UnityEngine.Random.insideUnitCircle * Mathf.Min(widthOfArea, heightOfArea) * 0.5f, Quaternion.Euler(Vector3.forward * UnityEngine.Random.Range(0f, 360f)), transform);
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
        /*foreach(Boid boid in allBoids) {
            List<Transform> closeBoids;
            List<Transform> closeObjects;
            GetCloseObjects(boid, out closeBoids, out closeObjects);
            //List<Transform> closeBoids = GetCloseObjects(boid);

            Vector3 cohesionVelocity = MoveCohesion(boid, closeBoids);
            Vector3 alignementVelocity = MoveAlignement(boid, closeBoids);
            Vector3 separationVelocity = MoveSeparation(boid, closeBoids);

            Vector3 moveDirection = MergeVelocities(boid.transform.up, cohesionVelocity, alignementVelocity, separationVelocity);

            boid.Move(ManageVelocity(boid, moveDirection, closeObjects));
        }*/

        // Spawn obstacles on click
        if (Input.GetButtonDown("Fire1")) {
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
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) {
                Boid b = hit.collider.gameObject.GetComponent<Boid>();
                if (b != null)
                    activeBoid = b;
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
            if (c != boid.boidCollider && !c.gameObject.CompareTag("Boid")) {
                objects.Add(c.transform);
            }
            nb++;
        }
        return nb;
    }
}

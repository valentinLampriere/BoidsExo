using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour {
    [Header("Field settings")]
    [Range(5f, 20f)]
    public float widthOfArea = 20;
    [Range(3.3f, 10f)]
    public float heightOfArea = 10;
    [Range(1, 100)]
    public int amountOfBoids = 10;
    [Space]
    [Header("Boids settings")]
    [Range(1f, 5f)]
    public float boidVisionDistance = 5f;
    [Range(0f, 3f)]
    public float boidMinimumDistance = 2f;

    public float boidVisionAngle = 2f;
    [Range(0.1f, 3.0f)]
    public float boidSpeed = 1f;

    public GameObject boid;

    List<Boid> allBoids = new List<Boid>();

    // Start is called before the first frame update
    void Start() {
        Vector3 position = gameObject.transform.position;
        for (int i = 0; i < amountOfBoids; i++) {
            // Spawn a boid
            GameObject objBoid = Instantiate(boid, Random.insideUnitCircle * Mathf.Min(widthOfArea, heightOfArea) * 0.4f, Quaternion.Euler(Vector3.forward * Random.Range(0f, 360f)), transform);
            Boid b = objBoid.GetComponent<Boid>();
            objBoid.name = "Boid " + i;
            if (b != null) {
                allBoids.Add(b);
            }
        }
    }

    void Update() {
        foreach(Boid boid in allBoids) {
            //List<Transform> closeObjects = getCloseObject(boid.transform);

            foreach(Transform b in getCloseObjects(boid.transform))

            boid.Move(boid.transform.up);
        }
    }

    List<Transform> getCloseObjects(Transform origin) {
        List<Transform> objects = new List<Transform>();
        Collider2D[] colliders = Physics2D.OverlapCircleAll(origin.position, boidVisionDistance);

        foreach(Collider2D c in colliders) {
            if (c.transform != origin) {
                objects.Add(c.transform);
            }
        }
        return objects;
    }
}

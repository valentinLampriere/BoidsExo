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
    [Range(0f, 2.0f)]
    public float boidSpeed = 0.5f;

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

    Vector3 moveCohesion(Boid boid, List<Transform> closeBoids) {
        Vector3 cohesionVelocity = Vector3.zero;
        if (closeBoids.Count == 0) return cohesionVelocity;
        foreach (Transform _boid in closeBoids) {
            cohesionVelocity += _boid.position;
        }
        cohesionVelocity /= closeBoids.Count;
        cohesionVelocity -= boid.transform.position;
        return cohesionVelocity;
    }

    Vector3 moveSeparation(Boid boid, List<Transform> closeBoids) {
        Vector3 separationVelocity = Vector3.zero;
        int numClose = 0;
        if (closeBoids.Count == 0) return separationVelocity;
        foreach (Transform _boid in closeBoids) {
            float distance = Vector3.Distance(boid.transform.position, _boid.position);
            if (distance <= boidMinimumDistance) {
                separationVelocity += boid.transform.position - _boid.position;
                numClose++;
            }
        }
        if (numClose > 0) {
            separationVelocity /= numClose;
        }
        return separationVelocity;
    }

    Vector3 moveAlignement(Boid boid, List<Transform> closeBoids) {
        Vector3 alignementVelocity = Vector3.zero;
        if (closeBoids.Count == 0) return alignementVelocity;
        foreach (Transform _boid in closeBoids) {
            alignementVelocity += _boid.position;
        }
        alignementVelocity /= closeBoids.Count;

        return alignementVelocity;
    }

    void Update() {
        foreach(Boid boid in allBoids) {
            //List<Transform> closeBoids = new List<Transform>();
            List<Transform> closeBoids = GetCloseObjects(boid);
            /*foreach (Boid _boid in allBoids) {
                if (boid == _boid) continue;
                float distance = Vector3.Distance(boid.transform.position, _boid.transform.position);
                if (distance < boidVisionDistance)
                    closeBoids.Add(_boid.transform);
            }*/

            Vector3 cohesionVelocity = moveCohesion(boid, closeBoids);
            Vector3 separationVelocity = moveSeparation(boid, closeBoids);
            Vector3 alignementVelocity = moveAlignement(boid, closeBoids);

            Vector3 moveVelocity = (boid.transform.up * 10 + cohesionVelocity + alignementVelocity + separationVelocity).normalized;
            
            boid.Move(moveVelocity, boidSpeed);
        }
    }

    List<Transform> GetCloseObjects(Boid boid) {
        List<Transform> context = new List<Transform>();
        Collider2D[] contextColliders = Physics2D.OverlapCircleAll(boid.transform.position, boidVisionDistance);
        foreach (Collider2D c in contextColliders) {
            if (c != boid.boidCollider) {
                context.Add(c.transform);
            }
        }
        return context;
    }
}

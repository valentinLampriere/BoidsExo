using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour {
    [Header("Field settings")]
    [Range(5f, 20f)]
    public float widthOfArea = 20;
    [Range(3.3f, 10f)]
    public float heightOfArea = 10;
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
    [Range(0f, 0.5f)]
    public float alignementStrength = 0.1f;
    [Range(0.01f, 5.0f)]
    public float boidSpeed = 0.5f;

    public GameObject boid_gameObject;

    List<Boid> allBoids = new List<Boid>();

    // Start is called before the first frame update
    void Start() {
        for (int i = 0; i < amountOfBoids; i++) {
            // Spawn a boid
            GameObject objBoid = Instantiate(boid_gameObject, Random.insideUnitCircle * Mathf.Min(widthOfArea, heightOfArea) * 0.5f, Quaternion.Euler(Vector3.forward * Random.Range(0f, 360f)), transform);
            Boid b = objBoid.GetComponent<Boid>();
            objBoid.name = "Boid " + i;
            if (b != null) {
                allBoids.Add(b);
            }
        }
    }

    Vector3 MoveCohesion(Boid boid, List<Transform> closeBoids) {
        Vector3 cohesionVelocity = Vector3.zero;
        if (closeBoids.Count == 0) return cohesionVelocity;
        foreach (Transform _boid in closeBoids) {
            cohesionVelocity += _boid.position;
        }
        cohesionVelocity /= closeBoids.Count;
        cohesionVelocity -= boid.transform.position;
        return cohesionVelocity * cohesionStrength;
    }

    Vector3 MoveSeparation(Boid boid, List<Transform> closeBoids) {
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
        return separationVelocity * separationStrength;
    }

    Vector3 MoveAlignement(Boid boid, List<Transform> closeBoids) {
        Vector3 alignementVelocity = Vector3.zero;
        if (closeBoids.Count == 0) return alignementVelocity;
        foreach (Transform _boid in closeBoids) {
            alignementVelocity += _boid.up;
        }
        alignementVelocity /= closeBoids.Count;

        return alignementVelocity * alignementStrength;
    }

    void Update() {
        foreach(Boid boid in allBoids) {
            List<Transform> closeBoids = GetCloseObjects(boid);

            Vector3 cohesionVelocity = MoveCohesion(boid, closeBoids);
            Vector3 alignementVelocity = MoveAlignement(boid, closeBoids);
            Vector3 separationVelocity = MoveSeparation(boid, closeBoids);

            Vector3 moveDirection = mergeVelocities(boid.transform.up, cohesionVelocity, alignementVelocity, separationVelocity);
            
            boid.Move(manageVelocity(boid, moveDirection));
        }
    }

    Vector3 mergeVelocities(Vector3 boidDirection, Vector3 cohesionVelocity, Vector3 alignementVelocity, Vector3 separationVelocity) {
        Vector3 direction = (boidDirection * 10f + cohesionVelocity + alignementVelocity + separationVelocity).normalized;
        return direction;
    }

    Vector3 manageVelocity(Boid boid, Vector3 moveDirection) {
        float borderW = widthOfArea / 30;
        float borderH = widthOfArea / 30;
        Vector3 newVelocity = boid.calcVelocity(moveDirection, boidSpeed);
        if (!goThroughtWalls) {
            if (newVelocity.x < 0 && boid.transform.position.x + newVelocity.x < -widthOfArea / 2 + borderW || newVelocity.x > 0 && boid.transform.position.x + newVelocity.x > widthOfArea / 2 - borderW) {
                newVelocity.x = -newVelocity.x;
            }
            if (newVelocity.y < 0 && boid.transform.position.y + newVelocity.y < -heightOfArea / 2 + borderH || newVelocity.y > 0 && boid.transform.position.y + newVelocity.y > heightOfArea / 2 - borderH) {
                newVelocity.y = -newVelocity.y;
            }
        } else {
            if (newVelocity.x < 0 && boid.transform.position.x + newVelocity.x < -widthOfArea / 2 + borderW) {
                boid.transform.position = new Vector3(widthOfArea / 2 - borderW, boid.transform.position.y);
            } else if (newVelocity.x > 0 && boid.transform.position.x + newVelocity.x > widthOfArea / 2 - borderW) {
                boid.transform.position = new Vector3(-widthOfArea / 2 + borderW, boid.transform.position.y);
            }
            if (newVelocity.y < 0 && boid.transform.position.y + newVelocity.y < -heightOfArea / 2 + borderH) {
                boid.transform.position = new Vector3(boid.transform.position.x, heightOfArea / 2 - borderH);
            } else if (newVelocity.y > 0 && boid.transform.position.y + newVelocity.y > heightOfArea / 2 - borderH) {
                boid.transform.position = new Vector3(boid.transform.position.x, -heightOfArea / 2 + borderH);
            }
        }
        return newVelocity;
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

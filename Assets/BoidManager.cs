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
        return separationVelocity * separationStrength * 2f;
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
            List<Transform> closeBoids;
            List<Transform> closeObjects;
            GetCloseObjects(boid, out closeBoids, out closeObjects);
            //List<Transform> closeBoids = GetCloseObjects(boid);

            Vector3 cohesionVelocity = MoveCohesion(boid, closeBoids);
            Vector3 alignementVelocity = MoveAlignement(boid, closeBoids);
            Vector3 separationVelocity = MoveSeparation(boid, closeBoids);

            Vector3 moveDirection = MergeVelocities(boid.transform.up, cohesionVelocity, alignementVelocity, separationVelocity);

            boid.Move(ManageVelocity(boid, moveDirection, closeObjects));
        }

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
    }

    Vector3 MergeVelocities(Vector3 boidDirection, Vector3 cohesionVelocity, Vector3 alignementVelocity, Vector3 separationVelocity) {
        Vector3 direction = (boidDirection * 8f + cohesionVelocity + alignementVelocity + separationVelocity);
        return direction;
    }

    Vector3 ManageVelocity(Boid boid, Vector3 moveDirection, List<Transform> closeObjects) {
        Vector3 newVelocity = moveDirection;

        newVelocity += AvoidObstacles(boid.transform, closeObjects);
        newVelocity = boid.calcVelocity(newVelocity, boidSpeed);
        newVelocity = ManageFieldLimit(boid, newVelocity);
        //Debug.Log(AvoidObstacles(boid.transform, closeObjects));
        return newVelocity;
    }    

    Vector3 AvoidObstacles(Transform boidTransform, List<Transform> closeObjects) {
        Vector3 freeDirection;
        int i = 0;
        bool checkLeft = false;

        RaycastHit2D hit = Physics2D.Raycast(boidTransform.position, boidTransform.up, boidVisionDistance, LayerMask.GetMask("Obstacles"));

        if (closeObjects.Count == 0 || hit.collider == null)
            return Vector3.zero;

        while (IsObstacleInDirection(boidTransform.position, boidTransform.up, i, checkLeft, out freeDirection)) {
            if (i >= 8)
                return -boidTransform.up;
            if (checkLeft) i++;
            checkLeft = !checkLeft;
        }

        return freeDirection;
    }
    bool IsObstacleInDirection(Vector3 origin, Vector3 direction, int offset, bool checkLeft, out Vector3 freeDirection) {
        float cos = Mathf.Cos(Mathf.PI / 8f * offset);
        float sin = Mathf.Sin(Mathf.PI / 8f * offset);
        if (checkLeft) {
            cos = Mathf.Cos(-Mathf.PI / 8f * offset);
            sin = Mathf.Sin(-Mathf.PI / 8f * offset);
        }

        float dX = direction.x * cos - direction.y * sin;
        float dY = direction.x * sin + direction.y * cos;

        Vector3 rayDirection = new Vector3(dX, dY);
        RaycastHit2D hit = Physics2D.Raycast(origin, rayDirection, boidVisionDistance, LayerMask.GetMask("Obstacles"));
        if (hit.collider != null) {
            Debug.DrawLine(origin, origin + direction, Color.green);
            freeDirection = Vector3.zero;
            return true;
        } else {
            Debug.DrawLine(origin, origin + direction, Color.red);
            freeDirection = rayDirection;
            return false;
        }
    }

    Vector3 ManageFieldLimit(Boid boid, Vector3 velocity) {
        if (!goThroughtWalls) {
            if (velocity.x < 0 && boid.transform.position.x + velocity.x < -widthOfArea / 2 || velocity.x > 0 && boid.transform.position.x + velocity.x > widthOfArea / 2) {
                velocity.x = -velocity.x;
            }
            if (velocity.y < 0 && boid.transform.position.y + velocity.y < -heightOfArea / 2 || velocity.y > 0 && boid.transform.position.y + velocity.y > heightOfArea / 2) {
                velocity.y = -velocity.y;
            }
        } else {
            if (velocity.x < 0 && boid.transform.position.x + velocity.x < -widthOfArea / 2) {
                boid.transform.position = new Vector3(widthOfArea / 2, boid.transform.position.y);
            } else if (velocity.x > 0 && boid.transform.position.x + velocity.x > widthOfArea / 2) {
                boid.transform.position = new Vector3(-widthOfArea / 2, boid.transform.position.y);
            }
            if (velocity.y < 0 && boid.transform.position.y + velocity.y < -heightOfArea / 2) {
                boid.transform.position = new Vector3(boid.transform.position.x, heightOfArea / 2);
            } else if (velocity.y > 0 && boid.transform.position.y + velocity.y > heightOfArea / 2) {
                boid.transform.position = new Vector3(boid.transform.position.x, -heightOfArea / 2);
            }
        }
        return velocity;
    }

    int GetCloseObjects(Boid boid, out List<Transform> boids, out List<Transform> objects) {
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

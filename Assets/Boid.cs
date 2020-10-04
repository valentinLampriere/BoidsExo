using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.VFX;

public class Boid : MonoBehaviour {

    public enum boidState {
        FLOCKING,
        MOVABLE
    }

    public Collider2D boidCollider;
    private Rigidbody2D rb;

    private BoidManager manager;
    private boidState state;
    private bool isActive;

    private float mass;

    public void Start() {
        rb = GetComponent<Rigidbody2D>();
        boidCollider = GetComponent<Collider2D>();
        mass = rb.mass;
        state = boidState.FLOCKING;
        isActive = false;
    }
    public void Init(BoidManager bm) {
        manager = bm;
    }

    public Vector3 calcVelocity(Vector3 direction, float speed) {
        return direction * speed * Time.deltaTime;
    }

    Vector3 MoveCohesion(List<Transform> closeBoids) {
        Vector3 cohesionVelocity = Vector3.zero;
        if (closeBoids.Count == 0) return cohesionVelocity;
        foreach (Transform _boid in closeBoids) {
            cohesionVelocity += _boid.position;
        }

        if (manager.activeBoid != null) {
            Vector3 target = manager.activeBoid.transform.position;
            cohesionVelocity += target - cohesionVelocity;
        }

        cohesionVelocity /= closeBoids.Count;
        cohesionVelocity -= transform.position;
        return cohesionVelocity * manager.cohesionStrength;
    }
    Vector3 MoveSeparation(List<Transform> closeBoids) {
        Vector3 separationVelocity = Vector3.zero;
        int numClose = 0;
        if (closeBoids.Count == 0) return separationVelocity;
        foreach (Transform _boid in closeBoids) {
            float distance = Vector3.Distance(transform.position, _boid.position);
            if (distance <= manager.boidMinimumDistance) {
                separationVelocity += transform.position - _boid.position;
                numClose++;
            }
        }
        if (numClose > 0) {
            separationVelocity /= numClose;
        }
        return separationVelocity * manager.separationStrength * 2f;
    }
    Vector3 MoveAlignement(List<Transform> closeBoids) {
        Vector3 alignementVelocity = Vector3.zero;
        if (closeBoids.Count == 0) return alignementVelocity;
        foreach (Transform _boid in closeBoids) {
            alignementVelocity += _boid.up;
        }
        alignementVelocity /= closeBoids.Count;

        return alignementVelocity * manager.alignementStrength;
    }

    Vector3 MergeVelocities(Vector3 cohesionVelocity, Vector3 alignementVelocity, Vector3 separationVelocity) {
        Vector3 direction = (transform.up * 8f + cohesionVelocity + alignementVelocity + separationVelocity);
        return direction;
    }

    /*Vector3 ManageVelocity(Vector3 moveDirection, List<Transform> closeObjects) {
        Vector3 newVelocity = moveDirection;

        newVelocity += AvoidObstacles(closeObjects);
        newVelocity = calcVelocity(newVelocity, manager.boidSpeed);
        newVelocity = ManageFieldLimit(newVelocity);

        return newVelocity;
    }*/

    Vector3 AvoidObstacles(List<Transform> closeObjects) {
        Vector3 freeDirection;
        int i = 0;
        bool checkLeft = false;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up, manager.boidVisionDistance, LayerMask.GetMask("Obstacles"));

        if (closeObjects.Count == 0 || hit.collider == null)
            return Vector3.zero;

        while (IsObstacleInDirection(transform.position, transform.up, i, checkLeft, out freeDirection)) {
            if (i >= 8)
                return -transform.up;
            if (checkLeft) i++;
            checkLeft = !checkLeft;
        }

        return freeDirection;
    }
    Vector3 ManageFieldLimit(Vector3 velocity) {
        if (!manager.goThroughtWalls) {
            if (velocity.x < 0 && transform.position.x + velocity.x < -manager.widthOfArea / 2 || velocity.x > 0 && transform.position.x + velocity.x > manager.widthOfArea / 2) {
                velocity.x = -velocity.x;
            }
            if (velocity.y < 0 && transform.position.y + velocity.y < -manager.heightOfArea / 2 || velocity.y > 0 && transform.position.y + velocity.y > manager.heightOfArea / 2) {
                velocity.y = -velocity.y;
            }
        } else {
            if (velocity.x < 0 && transform.position.x + velocity.x < -manager.widthOfArea / 2) {
                transform.position = new Vector3(manager.widthOfArea / 2, transform.position.y);
            }
            else if (velocity.x > 0 && transform.position.x + velocity.x > manager.widthOfArea / 2) {
                transform.position = new Vector3(-manager.widthOfArea / 2, transform.position.y);
            }
            if (velocity.y < 0 && transform.position.y + velocity.y < -manager.heightOfArea / 2) {
                transform.position = new Vector3(transform.position.x, manager.heightOfArea / 2);
            }
            else if (velocity.y > 0 && transform.position.y + velocity.y > manager.heightOfArea / 2) {
                transform.position = new Vector3(transform.position.x, -manager.heightOfArea / 2);
            }
        }
        return velocity;
    }
    bool IsObstacleInDirection(Vector3 origin, Vector3 direction, int offset, bool checkLeft, out Vector3 freeDirection) {
        float cos = Mathf.Cos(Mathf.PI / 8f * offset);
        float sin = Mathf.Sin(Mathf.PI / 8f * offset);
        freeDirection = Vector3.zero;
        if (checkLeft) {
            cos = Mathf.Cos(-Mathf.PI / 8f * offset);
            sin = Mathf.Sin(-Mathf.PI / 8f * offset);
        }

        float dX = direction.x * cos - direction.y * sin;
        float dY = direction.x * sin + direction.y * cos;

        Vector3 rayDirection = new Vector3(dX, dY);
        RaycastHit2D hit = Physics2D.Raycast(origin, rayDirection, manager.boidVisionDistance, LayerMask.GetMask("Obstacles"));
        if (hit.collider == null) {
            Debug.DrawLine(origin, origin + direction, Color.red);
            freeDirection = rayDirection;
            return false;
        }
        return true;
    }

    public void Move() {
        List<Transform> closeBoids;
        List<Transform> closeObjects;
        manager.GetCloseObjects(this, out closeBoids, out closeObjects);

        Vector3 cohesionVelocity = MoveCohesion(closeBoids);
        Vector3 alignementVelocity = MoveAlignement(closeBoids);
        Vector3 separationVelocity = MoveSeparation(closeBoids);

        Vector3 moveDirection = MergeVelocities(cohesionVelocity, alignementVelocity, separationVelocity);
        Vector3 velocity = moveDirection * manager.boidSpeed * Time.fixedDeltaTime;
        velocity += AvoidObstacles(closeObjects);
        
        //Vector3 velocity = ManageVelocity(moveDirection, closeObjects);
        velocity = ManageFieldLimit(velocity);

        transform.up = velocity;

        //rb.MovePosition(transform.position + velocity);

        transform.position += velocity;
    }

    public void MoveToCursor() {
        Vector3 cursorPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //Vector3 direction = (cursorPosition - transform.position);

        Vector3 direction = Vector3.MoveTowards(transform.position, new Vector3(cursorPosition.x, cursorPosition.y, 0), Time.deltaTime);
        transform.up = new Vector3((direction - transform.position).x, (direction - transform.position).y, 0);
        rb.MovePosition(Vector3.MoveTowards(transform.position, direction, Time.deltaTime));

        //transform.position += new Vector3(direction.x, direction.y, 0);
    }

    public void FixedUpdate() {
        if (state == boidState.FLOCKING) {
            Move();
        } else if (state == boidState.MOVABLE) {
            MoveToCursor();
        }
    }

    public void SetState(boidState state) {
        if (state == boidState.FLOCKING) {
            transform.localScale = new Vector3(1, 1, 0);
            rb.mass = mass;
        } else if (state == boidState.MOVABLE) {
            transform.localScale = new Vector3(1.5f, 1.5f, 0);
            rb.mass = 0f;
        }
        this.state = state;
    }
}

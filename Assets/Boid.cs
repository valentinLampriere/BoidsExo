using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.VFX;

public class Boid : MonoBehaviour {

    public enum boidState {
        FLOCKING,
        MOVABLE,
        FIRED
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
            if (velocity.x < 0 && transform.position.x + velocity.x < -manager.widthOfArea / 2 - 5 || velocity.x > 0 && transform.position.x + velocity.x > manager.widthOfArea / 2 - 5) {
                velocity.x = -velocity.x;
            }
            if (velocity.y < 0 && transform.position.y + velocity.y < -manager.heightOfArea / 2 || velocity.y > 0 && transform.position.y + velocity.y > manager.heightOfArea / 2) {
                velocity.y = -velocity.y;
            }
        } else {
            if (velocity.x < 0 && transform.position.x + velocity.x < -manager.widthOfArea / 2 - 5) {
                transform.position = new Vector3(manager.widthOfArea / 2 - 5, transform.position.y);
            }
            else if (velocity.x > 0 && transform.position.x + velocity.x > manager.widthOfArea / 2 - 5) {
                transform.position = new Vector3(-manager.widthOfArea / 2 - 5, transform.position.y);
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

    private bool IsCloseActiveBoid(Boid b) {
        return manager.activeBoid != null && Vector3.Distance(manager.activeBoid.transform.position, b.transform.position) * 2<= manager.boidVisionDistance && b.name != manager.activeBoid.name;
    }

    public void Move() {
        List<Transform> closeBoids;
        List<Transform> closeObjects;
        Vector3 velocity;
        manager.GetCloseObjects(this, out closeBoids, out closeObjects);

        Vector3 cohesionVelocity = MoveCohesion(closeBoids);
        Vector3 alignementVelocity = MoveAlignement(closeBoids);
        Vector3 separationVelocity = MoveSeparation(closeBoids);

        Vector3 moveDirection = MergeVelocities(cohesionVelocity, alignementVelocity, separationVelocity);

        velocity = moveDirection;


        velocity += AvoidObstacles(closeObjects);

        velocity = ManageFieldLimit(velocity * manager.boidSpeed * Time.fixedDeltaTime);


        if (IsCloseActiveBoid(this)) {
            transform.up = manager.activeBoid.transform.up;
        } else
            transform.up = velocity;
        transform.position += velocity;
    }

    public void MoveToCursor() {
        Vector3 cursorPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        rb.MovePosition(Vector3.MoveTowards(transform.position, new Vector3(cursorPosition.x, cursorPosition.y, 0), manager.boidSpeed * 8 * Time.fixedDeltaTime));
        transform.up = new Vector3(cursorPosition.x, cursorPosition.y, 0) - transform.position;
    }

    public void MoveAhead() {
        rb.AddForce(transform.up * 6);
        //transform.position += transform.up;
    }

    public void FixedUpdate() {
        if (state == boidState.FLOCKING) {
            Move();
        } else if (state == boidState.MOVABLE) {
            MoveToCursor();
        } else if (state == boidState.FIRED) {
            MoveAhead();
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

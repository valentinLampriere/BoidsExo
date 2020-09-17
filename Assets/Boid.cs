using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Boid : MonoBehaviour {
    private float angle;
    private float speed;
    private float visionDistance

    private Vector3 velocity;
    private Rect space;

    Rigidbody2D rb;

    public void Init(Rect space, float visionDistance) {
        this.space = space;
        this.visionDistance = visionDistance;
    }

    public void Start() {
        float xMin = space.x;
        float xMax = space.x + space.width;
        float yMin = space.y;
        float yMax = space.y + space.height;

        rb = gameObject.GetComponent<Rigidbody2D>();
        gameObject.transform.position = new Vector2(Random.Range(xMin, xMax), Random.Range(yMin, yMax));
        velocity = new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));
        speed = Random.Range(3f, 5f);
    }

    private bool isVectorInSpace(Vector3 newPos) {
        if (newPos.x > space.x + space.width || newPos.x < space.x)
            return false;
        if (newPos.y > space.y + space.height || newPos.y < space.y)
            return false;
        return true;
    }

    private Vector3 appearOppositeSide(Vector3 newPos) {
        Vector3 oppositePos = newPos;
        if (newPos.x > space.x + space.width) {
            oppositePos.x = space.x;
        } else if (newPos.x < space.x) {
            oppositePos.x = space.x + space.width;
        }
        if (newPos.y > space.y + space.height) {
            oppositePos.y = space.y;
        } else if (newPos.y < space.y) {
            oppositePos.y = space.y + space.width;
        }
        return oppositePos;
    }

    public void FixedUpdate() {
        Vector3 newPos = gameObject.transform.position + velocity * speed * Time.deltaTime;
        List<Boid> closeBoids = new List<Boid>();
        if (!isVectorInSpace(newPos)) {
            newPos = appearOppositeSide(newPos);
        }
        foreach (GameObject objBoid in GameObject.FindGameObjectsWithTag("Boid")) {
            Boid b = objBoid.GetComponent<Boid>();
            if (b != null && objBoid != this) {
                float dist = Vector3.Distance(objBoid.transform.position, rb.position);
                if (dist <= visionDistance) {
                    closeBoids.Add(b);
                }
            }
        }
        rb.MovePosition(newPos);
    }
}

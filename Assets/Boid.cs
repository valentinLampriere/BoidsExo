﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Boid : MonoBehaviour {
    private float angle;
    private float speed;
    private float visionDistance;
    private float minDistance;

    private Vector3 velocity;
    private Rect space;

    Rigidbody2D rb;

    public void Init(Rect space, float speed, float visionDistance, float minDistance) {
        this.space = space;
        this.speed = speed;
        this.visionDistance = visionDistance;
        this.minDistance = minDistance;
    }

    public void Start() {
        float xMin = space.x;
        float xMax = space.x + space.width;
        float yMin = space.y;
        float yMax = space.y + space.height;

        rb = gameObject.GetComponent<Rigidbody2D>();
        gameObject.transform.position = new Vector2(Random.Range(xMin, xMax), Random.Range(yMin, yMax));
        velocity = new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));
    }

    private void MoveCloser(List<Boid> closeBoids) {
        float avgX = 0;
        float avgY = 0;
        foreach (Boid b in closeBoids) {
            Rigidbody2D b_rb = b.GetComponent<Rigidbody2D>();
            if (b == this || b_rb == null) continue;
            avgX += rb.position.x - b_rb.position.x;
            avgY += rb.position.y - b_rb.position.y;
        }
        avgX /= closeBoids.Count;
        avgY /= closeBoids.Count;

        velocity.x -= (avgX / 10);
        velocity.y -= (avgY / 10);
    }
    
    private void MoveAway(List<Boid> closeBoids) {
        float distanceX = 0;
        float distanceY = 0;
        int numClose = 0;
        foreach(Boid b in closeBoids) {
            Rigidbody2D b_rb = b.GetComponent<Rigidbody2D>();
            if (b == this || b_rb == null) continue;
            float distance = Vector3.Distance(rb.position, b_rb.position);
            if (distance < minDistance) {
                numClose++;
                float xDiff = rb.position.x - b_rb.position.x;
                float yDiff = rb.position.y - b_rb.position.y;
                if (xDiff >= 0)
                    xDiff = Mathf.Sqrt(minDistance) - xDiff;
                else
                    xDiff = -Mathf.Sqrt(minDistance) - xDiff;
                if (yDiff >= 0)
                    yDiff = Mathf.Sqrt(minDistance) - yDiff;
                else
                    yDiff = -Mathf.Sqrt(minDistance) - yDiff;
                distanceX += xDiff;
                distanceY += yDiff;
                if (numClose == 0) return;
                velocity.x -= distanceX / 5;
                velocity.y -= distanceY / 5;
            }
        }
    }

    public void FixedUpdate() {
        Vector3 newPos = gameObject.transform.position + velocity * speed * Time.deltaTime;
        List<Boid> closeBoids = new List<Boid>();

        foreach (GameObject objBoid in GameObject.FindGameObjectsWithTag("Boid")) {
            Boid b = objBoid.GetComponent<Boid>();
            if (b != null && objBoid != this) {
                float dist = Vector3.Distance(objBoid.transform.position, rb.position);
                if (dist <= visionDistance) {
                    closeBoids.Add(b);
                }
            }
        }

        MoveCloser(closeBoids);
        MoveAway(closeBoids);

        float border = space.width / 30;
        if (newPos.x < space.x + border && velocity.x < 0 || newPos.x > space.x + space.width - border && velocity.x > 0) {
            velocity.x = -velocity.x * Random.Range(0, 1f);
            newPos = gameObject.transform.position + velocity * speed * Time.deltaTime;
        } else if (newPos.y < space.y + border && velocity.y < 0 || newPos.y > space.y + space.height - border && velocity.y > 0) {
            velocity.y = -velocity.y * Random.Range(0, 1f);
            newPos = gameObject.transform.position + velocity * speed * Time.deltaTime;
        }
        
        rb.MovePosition(newPos);
    }
}

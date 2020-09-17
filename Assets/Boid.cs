using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Boid {
    public float angle;
    public float speed;

    private Vector2 position;
    private Vector2 velocity;

    public Boid(Rect space) {

        float xMin = space.x - space.width / 2;
        float xMax = space.x + space.width / 2;
        float yMin = space.y - space.height / 2;
        float yMax = space.y + space.height / 2;

        position = new Vector2(Random.Range(xMin, xMax), Random.Range(yMin, yMax));
        velocity = new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f));
        speed = Random.Range(1f, 5f);
    }

    public void Move() {

        position = position + velocity;
    }
}

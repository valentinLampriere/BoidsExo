﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour
{
    public float widthOfArea = 20;
    public float heightOfArea = 10;
    public int amountOfBoids = 10;

    public GameObject boid;

    // Start is called before the first frame update
    void Start() {
        Vector3 position = gameObject.transform.position;
        Rect space = new Rect(position.x - widthOfArea / 2, position.y - heightOfArea / 2, widthOfArea, heightOfArea);
        for (int i = 0; i < amountOfBoids; i++) {
            GameObject objBoid = Instantiate(boid);
            Boid b = objBoid.GetComponent<Boid>();
            if (b != null)
                b.SetSpace(space);
        }
    }
}
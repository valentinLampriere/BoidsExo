using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour
{
    [Range(5f, 20f)]
    public float widthOfArea = 20;
    [Range(3.3f, 10f)]
    public float heightOfArea = 10;
    [Range(1, 30)]
    public int amountOfBoids = 10;
    [Range(1f, 5f)]
    public float boidVisionDistance = 5f;
    [Range(0f, 3f)]
    public float boidMinimumDistance = 2f;
    [Range(0.0f, 3.0f)]
    public float boidSpeed = 1f;

    public GameObject boid;

    // Start is called before the first frame update
    void Start() {
        Vector3 position = gameObject.transform.position;
        for (int i = 0; i < amountOfBoids; i++) {
            GameObject objBoid = Instantiate(boid);
            Boid b = objBoid.GetComponent<Boid>();
            objBoid.name = i.ToString();
            if (b != null)
                b.manager = this;
            else
                Destroy(objBoid);
        }
    }
}

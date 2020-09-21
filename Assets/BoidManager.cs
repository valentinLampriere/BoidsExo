using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour
{
    public float widthOfArea = 20;
    public float heightOfArea = 10;
    public int amountOfBoids = 10;
    public float boidVisionDistance = 5f;
    public float boidMinimumDistance = 2f;
    public float boidSpeed = 1f;

    //public Rect space = new Rect(-10, -5, 20, 10);

    public GameObject boid;

    // Start is called before the first frame update
    void Start() {
        Vector3 position = gameObject.transform.position;
        //space = new Rect(position.x - widthOfArea / 2, position.y - heightOfArea / 2, widthOfArea, heightOfArea);
        for (int i = 0; i < amountOfBoids; i++) {
            GameObject objBoid = Instantiate(boid);
            Boid b = objBoid.GetComponent<Boid>();
            if (b != null)
                b.manager = this;
            //b.Init(space, boidSpeed, boidVisionDistance, boidMinimumDistance);
            else
                Destroy(objBoid);
        }
    }
}

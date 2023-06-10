using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public List<Boid> boids = new List<Boid>();
    public GameObject food;
    public GameObject hunter;   

    public float NPCEnergy;

    public float xPos;
    public float zPos;

    public static GameManager instance;

    //Pesos
    [Range(0, 3)]
    public float weightSeparation = 1;
    [Range(0, 3)]
    public float weightCohesion = 1;
    [Range(0, 3)]
    public float weightAlignment = 1;
    [Range(0, 3)]
    public float weightArrive = 1;
    [Range(0, 3)]
    public float weightEvade = 1;

    public float width;
    public float height;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {       
        food.transform.position = new Vector3(Random.Range(-width + 1, width - 1), 0, Random.Range(-height + 1, height - 1));
    }

    private void Update()
    {
        NPCEnergy = Mathf.Clamp(NPCEnergy, 0, 10);       
    }

    //Teletransportador de comida

    public void FoodDrop()
    {
        if (food != null)
        {
            xPos = Random.Range(-width + 1, width - 1);
            zPos = Random.Range(-height + 1, height - 1);
            food.transform.position = new Vector3(xPos, 0, zPos);
        }
    }

    //Añadir el boid a la lista
    public void AddToList(Boid b)
    {
        if (!boids.Contains(b))
        {
            boids.Add(b);
        }
    }
       

    //Limites del mapa

    public Vector3 ApplyBound(Vector3 objectPosition)
    {
        if (objectPosition.x > width)
            objectPosition.x = -width;
        if (objectPosition.x < -width)
            objectPosition.x = width;

        if (objectPosition.y != 0)
            objectPosition.y = 0;

        if (objectPosition.z > height)
            objectPosition.z = -height;
        if (objectPosition.z < -height)
            objectPosition.z = height;

        return objectPosition;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Vector3 topLeft = new Vector3(-width, 0, height);
        Vector3 topRight = new Vector3(width, 0, height);
        Vector3 botRight = new Vector3(width, 0, -height);
        Vector3 botLeft = new Vector3(-width, 0, -height);

        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, botRight);
        Gizmos.DrawLine(botRight, botLeft);
        Gizmos.DrawLine(botLeft, topLeft);
    }


}

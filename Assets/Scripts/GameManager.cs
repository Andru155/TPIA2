using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [NonSerialized] public List<Boid> boids = new List<Boid>();
    public GameObject food;
    public HunterIA2 hunter;   

    public float NPCEnergy;

    public float xPos;
    public float zPos;

    public static GameManager instance;

    public Boid Sample;

    public int quantityToSpawn=5;

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

    public int width;
    public int height;

   public SpatialGrid grid;

    public Transform wpFather;
    private void Awake()
    {
        instance = this;
        grid = GetComponent<SpatialGrid>();
       
        hunter = Instantiate(hunter, GetRandomLocation(),Quaternion.identity);
        hunter.wpFather = wpFather;
        food = Instantiate(food, GetRandomLocation(), Quaternion.identity);
        food.GetComponent<GridEntity>().onGrid = true;
        for (int i = 0; i < quantityToSpawn; i++)
        {
           var a = Instantiate(Sample, GetRandomLocation(), Quaternion.identity);
            Debug.Log("spawneo boid");
        }
        StartCoroutine(CheckList());
    }

    public Vector3 GetRandomLocation()
    {
        float unitsup = 20f;
        float xPoint = Random.Range(-width, width + 1);
        float zpoint = Random.Range(-width, width + 1);
        Vector3 point= new Vector3(xPoint, unitsup, zpoint);
        if (Physics.Raycast(point,Vector3.down,out RaycastHit hit))
        {
            return hit.point;
        }
        else
        {
            return GetRandomLocation();
        }       

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

    public void FoodDrop() => StartCoroutine(FoodSpawn());
  
    IEnumerator FoodSpawn()
    {
        food.transform.position = new Vector3(999, 999, 999);
        yield return new WaitForSeconds(5f);
        xPos = Random.Range(-width + 1, width - 1);
        zPos = Random.Range(-height + 1, height - 1);
        food.transform.position = new Vector3(xPos, 0, zPos);
    }

    //Añadir el boid a la lista
    public void AddToList(Boid b)
    {
        if (!boids.Contains(b))
        {
            boids.Add(b);
        }
    }

    public void RemoveFromList(Boid b)
    {
        if (boids.Contains(b))
        {
            boids.Remove(b);
        }
    }
       
    IEnumerator CheckList()
    {
        while (true)
        {
            for (int i = 0; i < 30; i++)
            {
                yield return null;
            }
            if (boids.Count<quantityToSpawn)
            {
                for (int i = 0; i < quantityToSpawn - boids.Count; i++)
                {
                    var a = Instantiate(Sample, GetRandomLocation(), Quaternion.identity);
                    Debug.Log("spawneo boid");
                    yield return new WaitForSeconds(2f);
                   
                }
            }
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

   [SerializeField] bool change;
    private void OnValidate()
    {
        
    }
}

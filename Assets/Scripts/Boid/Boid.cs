using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Boid : MonoBehaviour
{

    public float maxSpeed;
    public float maxForce;

    public float viewRadius;
    public float separationRadius;

    [SerializeField] Queries _OnRadius;

    public float evadeRadius;
    Vector3 _distToHunter;
    GameObject _hunter;

    NPC _npc;

    GameObject _food;
    Vector3 _distToFood;

    GridEntity myEntity;

    public float collisionRadius;
    public float goToFoodRadius;

    public Vector3 _velocity; 

    private void Start()
    {
        myEntity = transform.GetComponent<GridEntity>();

        _npc = new NPC();
        _hunter = GameManager.instance.hunter.gameObject;
        _food = GameManager.instance.food;
        Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized * maxForce;
        AddForce(randomDir);

        // lo igualo al radio de vision (el mas grande)
        _OnRadius.radius = viewRadius;
        _OnRadius.isBox = false;

        GameManager.instance.AddToList(this);
    }

    private void Update()
    {
        // guardar todos los gameobjects menos a mi mismo y luego preguntar si hay algo?

        _OnRadius.selected = _OnRadius.Query().Where(entity => entity.gameObject != this);

        if (_OnRadius.selected.Any())
        {
            Debug.Log("T");
        }

        var boids = _OnRadius.selected.Select(entity => entity.GetComponent<Boid>()).Where(x=> x!=null);
        var food = _OnRadius.selected.SkipWhile(entity => !entity.gameObject.CompareTag("Food")).Select(y=>y.gameObject).FirstOrDefault(x=>x = null);

        _distToFood = _food.transform.position - transform.position;
        _distToHunter = _hunter.transform.position - transform.position;

        if (_OnRadius.selected.Any())
        {
            //nunca entra aca
            Debug.Log("Hay algo en mi radio");
            GetNearestFood(food);

            if (_distToHunter.magnitude <= evadeRadius)
            {
                AddForce(Evade(GameManager.instance.hunter.gameObject, _npc) * GameManager.instance.weightEvade);
            }
            else
            {
                AddForce(Separation(boids) * GameManager.instance.weightSeparation);
                AddForce(Cohesion(boids) * GameManager.instance.weightCohesion);
                AddForce(Alignment(boids) * GameManager.instance.weightAlignment);
                Debug.Log(" Fuera del radio ");
            }
        }

        transform.position += _velocity * Time.deltaTime;
        myEntity.OnMove(_velocity);
        transform.position = GameManager.instance.ApplyBound(transform.position);
    }

    //FLOCKING

    //IA2-P1
    void GetNearestFood(GameObject food)
    {
        //var foodOnRadius = _OnRadius.selected.Where(entity => entity.gameObject.CompareTag("Food")).First(x=> x= null);

        if (food != null)
        {
            Debug.Log("Dentro del radio de comida");
            AddForce(Arrive(GameManager.instance.food) * GameManager.instance.weightArrive);

            if (_distToFood.magnitude <= collisionRadius)
                GameManager.instance.FoodDrop();
        }
    }

    //IA2-P1
    Vector3 Alignment(IEnumerable<Boid> boids)
    {
        //una lista de boids, excluyendo a mi mismo
       // var nearbyBoids = _OnRadius.selected
       //.Where(entity => entity.gameObject != this)
       //.Select(entity => entity.GetComponent<Boid>())
       //.ToList();

        //si no hay ninguno devuelvo 0 
        if (!boids.Any())
            return Vector3.zero;

        //la velocidad deseada sera la suma de las velocidades de los demas boids
        Vector3 desired = boids
           .Select(boid => boid._velocity)
           .Aggregate(Vector3.zero, (current, velocity) => current + velocity);

        //hago el promedio 
        desired /= boids.Count();
        desired.Normalize();
        desired *= maxForce;

        return CalculatedSteering(desired);


        /*
        Vector3 desired = Vector3.zero;
        int count = 0;

        foreach (var item in GameManager.instance.boids)
        {
            if (item == this)
                continue;

            Vector3 dist = item.transform.position - transform.position;

            if (dist.magnitude <= viewRadius)
            {
                desired += item._velocity;
                count++;
            }
        }

        if (count <= 0)
            return desired;

        desired /= count;

        desired.Normalize();
        desired *= maxForce;

        return CalculatedSteering(desired);
        */
    }

    public void Kill()
    {
        GameManager.instance.RemoveFromList(this);
        Destroy(this);
    }
    //IA2-P1
    Vector3 Cohesion(IEnumerable<Boid> boids)
    {
        //una lista de boids, excluyendome
      //  var nearbyBoids = _OnRadius.selected
      //.Where(entity => entity.gameObject != this)
      //.Select(entity => entity.GetComponent<Boid>())
      //.ToList();

        //si no hay ninguno devuelvo 0 
        if (!boids.Any())
            return Vector3.zero;

        //sumo las posiciones de los boids

        Vector3 desired = boids
            .Select(boid => boid.transform.position)
            .Aggregate(Vector3.zero, (current, position) => current + position);

        //hago el promedio 
        desired /= boids.Count();
        desired.Normalize();
        desired *= maxForce;

        return CalculatedSteering(desired);

        /*
        Vector3 desired = Vector3.zero;
        int count = 0;

        foreach (var item in GameManager.instance.boids)
        {
            if (item == this)
                continue;

            Vector3 dist = item.transform.position - transform.position;

            if (dist.magnitude <= viewRadius)
            {
                desired += item.transform.position;
                count++;
            }
        }

        if (count <= 0)
            return desired;

        desired /= count;
        desired -= transform.position;

        desired.Normalize();
        desired *= maxForce;

        return CalculatedSteering(desired);
        */
    }

    //IA2-P1
    Vector3 Separation(IEnumerable<Boid> boids)
    {
        Vector3 desired = Vector3.zero;

        var separationBoids = boids.Where(boid => Vector3.Distance(transform.position, boid.transform.position) <= separationRadius);

        //si no hay ninguno devuelvo 0 
        if (!separationBoids.Any())
            return Vector3.zero;

        //saco la distancia de cada boid
        Vector3 distance = separationBoids
            .Select(boid => transform.position - boid.transform.position)
            .Aggregate(Vector3.zero, (current, distance) => current + distance);

        //esto me olvide pq era lo dejo porlas
        //desired = -desired;

        desired.Normalize();
        desired *= maxForce;

        return CalculatedSteering(desired);


        /*
        Vector3 desired = Vector3.zero;

        foreach (var item in GameManager.instance.boids)
        {
            Vector3 dist = item.transform.position - transform.position;

            if (dist.magnitude <= separationRadius)
                desired += dist;
        }

        if (desired == Vector3.zero)
            return desired;

        desired = -desired;

        desired.Normalize();
        desired *= maxForce;

        return CalculatedSteering(desired);
        */
    }

    // MOVERSE HACIA LA COMIDA 

    Vector3 Arrive(GameObject actualTarget)
    {
        Vector3 desired = actualTarget.transform.position - transform.position;
        desired.Normalize();
        desired *= maxForce;

        return CalculatedSteering(desired);
    }

    //EVADIR EL NPC

    Vector3 Evade(GameObject h, NPC hunter)
    {
        Vector3 finalPos = h.transform.position + hunter.Velocity * Time.deltaTime;
        Vector3 desired = transform.position - finalPos;
        desired.Normalize();
        desired *= maxForce;

        return CalculatedSteering(desired);
    }


    Vector3 CalculatedSteering(Vector3 desired)
    {
        return Vector3.ClampMagnitude(desired - _velocity, maxSpeed);
    }

    void AddForce(Vector3 force)
    {
        _velocity = Vector3.ClampMagnitude(_velocity + force, maxForce);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, separationRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, goToFoodRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, collisionRadius);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, evadeRadius);
    }
    private void OnValidate()
    {
        // lo igualo al radio de vision (el mas grande)
        _OnRadius.radius = viewRadius;
        _OnRadius.isBox = false;
    }
}

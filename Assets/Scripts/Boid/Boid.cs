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

    [SerializeField] HunterIA2 _hunterIA;


    Vector3 _distToHunter;
    Vector3 _distToFood;

   public GridEntity myEntity;

    public float collisionRadius;
    public float goToFoodRadius;

    public Vector3 _velocity;

    private void Awake()
    {
        myEntity = transform.GetComponent<GridEntity>();

        Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized * maxForce;
        AddForce(randomDir);

        // lo igualo al radio de vision (el mas grande)
        _OnRadius.radius = viewRadius;
        _OnRadius.isBox = false;

        
    }
    private void Start()
    {
        
    }

    private void Update()
    {
        // guardar todos los gameobjects menos a mi mismo y luego preguntar si hay algo?
         //IA2-P1
        _OnRadius.selected = _OnRadius.Query().Where(entity => entity.gameObject != this);

      

        var boids = _OnRadius.selected.Select(entity => entity.GetComponent<Boid>()).Where(x => x != null);
       
      

        var food = _OnRadius.selected.SkipWhile(entity => !entity.gameObject.CompareTag("Food")).Select(y => y.gameObject).FirstOrDefault(x => x = null);


        if (_OnRadius.selected.Any())
        {

            Debug.Log("Hay algo en mi radio BOID");
            if (food != null)
            {
                Debug.Log("BOID: Hay comida en mi radio");
                _distToFood = food.transform.position - transform.position;
                GetNearestFood(food);
                return;
            }

            if (_hunterIA != null)
            {

                Debug.Log("BOID:El hunter es distinto de null");
               _distToHunter = _hunterIA.transform.position - transform.position;

                if (_distToHunter.magnitude <= evadeRadius)
                {
                    Debug.Log("BOID: el hunter esta en mi radio de evasion");
                    AddForce(Evade(_hunterIA) * GameManager.instance.weightEvade);
                    return;
                }
                else if (boids.Any())
                {
                    List<Boid> list = boids.ToList();
                    Debug.Log(list.Count);
                    Vector3 actualForce = Vector3.zero;
                    actualForce += Separation(list) * GameManager.instance.weightSeparation;
                    actualForce += Cohesion(list) * GameManager.instance.weightCohesion;
                    actualForce += Alignment(list) * GameManager.instance.weightAlignment;
                    AddForce(actualForce);
                    //Debug.Log(" BOID: Fuera del radio del hunter ,hago flocking con "+ boids.Count());
                    return;
                }
            }
        }

        Debug.Log("Boid:A�ado fuerza hacia adelante");
            AddForce(transform.forward * maxForce);
        

        

       
    }

    //FLOCKING

    //IA2-P1
    void GetNearestFood(GameObject food)
    {
        //var foodOnRadius = _OnRadius.selected.Where(entity => entity.gameObject.CompareTag("Food")).First(x=> x= null);

        Debug.Log("BOID: Dentro del radio de comida");
        AddForce(Arrive(food) * GameManager.instance.weightArrive);

        if (_distToFood.magnitude <= collisionRadius)
        {
            Debug.Log("BOID: la distancia de un boid con la comida es menor al radio de collision. Dberia pasar el FoodDrop");
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
        
        Destroy(this.gameObject);
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
        desired-=transform.position;
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
      

        var separationBoids = boids.Where(boid => Vector3.Distance(transform.position, boid.transform.position) <= separationRadius);

        

        //si no hay ninguno devuelvo 0 
        if (!separationBoids.Any())
            return Vector3.zero;

        //saco la distancia de cada boid
        Vector3 distance = separationBoids
            .Select(boid => boid.transform.position-transform.position)
            .Aggregate(Vector3.zero, (current, distance) => current + distance);
        distance = - distance;

        if (distance == Vector3.zero) return Vector3.zero;
       
        distance.Normalize();
        distance *= maxForce;
        Debug.Log("distance>0");
        return CalculatedSteering(distance);
        //esto me olvide pq era lo dejo porlas
        //desired = -desired;




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

    Vector3 Evade(HunterIA2 hunter)
    {

        Vector3 finalPos = hunter.gameObject.transform.position + hunter.Velocity * Time.deltaTime;
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
        transform.position += _velocity * Time.deltaTime;
        transform.forward = _velocity.normalized;
        transform.position = GameManager.instance.ApplyBound(transform.position);
        myEntity.OnMove(_velocity);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        if (!GameManager.instance.BoidGizmos) return;
       
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
        if (Application.isPlaying) return;
        // lo igualo al radio de vision (el mas grande)
        _OnRadius.radius = viewRadius;
        _OnRadius.isBox = false;
    }
}

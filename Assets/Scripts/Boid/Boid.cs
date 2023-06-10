using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{  

    public float maxSpeed;
    public float maxForce;

    public float viewRadius;
    public float separationRadius;

    public float evadeRadius;
    Vector3 _distToHunter;
    GameObject _hunter;    

    NPC _npc;

    GameObject _target;
    Vector3 _distToFood;
    public float collisionRadius;
    public float goToFoodRadius;

    public Vector3 _velocity;


    private void Start()
    {        
        _npc = new NPC();
        _hunter = GameManager.instance.hunter;
        _target = GameManager.instance.food;
        Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized * maxForce;
        AddForce(randomDir);

        GameManager.instance.AddToList(this);
    }

    private void Update()
    {
        _distToFood = _target.transform.position - transform.position;
        _distToHunter = _hunter.transform.position - transform.position;

        if (_distToFood.magnitude <= goToFoodRadius)
        {
            Debug.Log(" Dentro del radio de comida ");
            AddForce(Arrive(_target) * GameManager.instance.weightArrive);

            if (_distToFood.magnitude <= collisionRadius)
                GameManager.instance.FoodDrop();
        }
        else if (_distToHunter.magnitude <= evadeRadius)
        {
            AddForce(Evade(GameManager.instance.hunter, _npc) * GameManager.instance.weightEvade);
        }
        else
        {
            AddForce(Separation() * GameManager.instance.weightSeparation);
            AddForce(Cohesion() * GameManager.instance.weightCohesion);
            AddForce(Alignment() * GameManager.instance.weightAlignment);
            Debug.Log(" Fuera del radio ");
        }


        transform.position += _velocity * Time.deltaTime;
        transform.forward = _velocity;
        transform.position = GameManager.instance.ApplyBound(transform.position);
    }

    //FLOCKING

    Vector3 Alignment()
    {
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
    }

    Vector3 Cohesion()
    {
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
    }

    Vector3 Separation()
    {
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
}

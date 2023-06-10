using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    public float maxVelocity, minRadius, speed, steeringVelocity;
    Vector3 _velocity;
    public Transform[] waypoints;
    int _nextPos;
    Transform _chaseTarget;
    FSM _fsm;
    Boid _b;

    private void Start()
    {        
        _b = GetComponent<Boid>();
        _fsm = new FSM();
        _fsm.CreateState("Idle", new Idle(_fsm));
        _fsm.CreateState("Chase", new Chase(_fsm, _chaseTarget, maxVelocity, _velocity, transform, minRadius, steeringVelocity, _b));
        _fsm.CreateState("Patrol", new Patrol(_fsm, transform, waypoints, _nextPos, speed, _chaseTarget, minRadius));

        //Para pasar de idle a patrol, deben pasar 10 segundos hasta que se llene la energía.
        _fsm.ChangeState("Idle");

    }

    void Update()
    {
        _b = Target(minRadius);

        if (_b!= null)
        {
           _chaseTarget = _b.gameObject.transform;            
        }
        else
        {
            _chaseTarget = null;
        }

        _fsm.Execute();
        transform.position += _velocity * Time.deltaTime;
        transform.position = GameManager.instance.ApplyBound(transform.position);
    }

    //Método para asignar el target según el boid más cercano

    public Boid Target(float minRadius)
    {
        int index = -1;
        Vector3 nearestBoid = GameManager.instance.boids[0].transform.position - this.transform.position;

        for (int i = 0; i < GameManager.instance.boids.Count; i++)
        {

            Vector3 distance = GameManager.instance.boids[i].transform.position - this.transform.position;

            if (GameManager.instance.boids[i] != null)
            {
                if (distance.magnitude <= minRadius)
                {
                    if (nearestBoid.magnitude >= distance.magnitude)
                    {
                        index = i;
                        nearestBoid = distance;
                    }
                }
            }
        }

        if (index >= 0)
        {
            return GameManager.instance.boids[index];
        }
        else
        {
            return null;
        }
    }

    public Vector3 Velocity
    {
        get { return _velocity; }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, minRadius);

        if(_b != null)
        Gizmos.DrawWireSphere(_b.transform.position, minRadius);
    }



}

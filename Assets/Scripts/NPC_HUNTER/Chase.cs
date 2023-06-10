using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chase : IState
{
    FSM _fsm;
    Boid _b;
    float _maxVelocity, _minRadius, _steeringVelocity;
    Transform _target;
    Transform _transform;
    Vector3 _velocity;

    public Chase(FSM fsm, Transform target, float maxVelocity, Vector3 velocity, Transform transform, float minRadius, float steeringVelocity, Boid boid)
    {
        _b = boid;
        _fsm = fsm;
        _target = target;
        _maxVelocity = maxVelocity;
        _velocity = velocity;
        _transform = transform;
        _minRadius = minRadius;
        _steeringVelocity = steeringVelocity;
    }
    public void OnEnter()
    {       
        Debug.Log("Entro perseguir");     
    }

    public void OnUpdate()
    {
        GameManager.instance.NPCEnergy -= Time.deltaTime;        
        
        if (_target != null && GameManager.instance.NPCEnergy > 0)        
            AddForce(Pursuit(_target));        
        else
            ChangeState();              

    }

    void ChangeState()
    {
        if (GameManager.instance.NPCEnergy <= 0)
            _fsm.ChangeState("Idle");
        else if (_target == null && GameManager.instance.NPCEnergy > 0)
            _fsm.ChangeState("Patrol");
    }

    public void OnExit()
    {       
        Debug.Log("Termino perseguir");
    }


    Vector3 Pursuit(Transform actualTarget)
    {
        
        Vector3 finalPos = actualTarget.position + _b._velocity * Time.deltaTime;

        Vector3 desired = finalPos - _transform.position;

        desired.Normalize();

        desired *= _maxVelocity;

        return CalculatedSteering(desired);
    }

    Vector3 CalculatedSteering(Vector3 desired)
    {
        return Vector3.ClampMagnitude(desired - _velocity, _steeringVelocity);
    }

    public void AddForce(Vector3 force)
    {
        _velocity = Vector3.ClampMagnitude(_velocity + force, _maxVelocity);
    }
}

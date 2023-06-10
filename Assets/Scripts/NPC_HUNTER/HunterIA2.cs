using IA2;
using System.Collections.Generic;
using UnityEngine;

public class HunterIA2 : MonoBehaviour
{
    #region FiniteStateMachine
    public enum HunterStates
    {
        IDLE,PATROL,CHASE
    }
    public EventFSM<HunterStates> _fsm;

    State<HunterStates> idle;
    State<HunterStates> patrol;
    State<HunterStates> chase;
    #endregion

 
    Transform[] _waypoints;
    Boid target;

    Vector3 _velocity;

    int _nextPos;
    float speed,maxSpeed,steeringForce;
    float minDetectRadius;

    private void Awake()
    {
        SetIdle(); SetPatrol(); SetChase();

        _fsm = new EventFSM<HunterStates>(idle);
    }

    void SetIdle()
    {
        //q copado es esto, ni tuve q poner add
        var idleTransitions = new Dictionary<HunterStates, Transition<HunterStates>>
        {
            { HunterStates.PATROL, new Transition<HunterStates>(HunterStates.PATROL, patrol) },
            { HunterStates.CHASE, new Transition<HunterStates>(HunterStates.CHASE, chase) }
        };

        idle = new State<HunterStates>("Idle").Configure(idleTransitions);

        idle.OnUpdate += () => GameManager.instance.NPCEnergy += Time.deltaTime;

        idle.OnLateUpdate += () =>
        {
            if (GameManager.instance.NPCEnergy >= 10)
            {
                if (target == null)
                    _fsm.SendInput(HunterStates.PATROL);
                else if (target != null)
                    _fsm.SendInput(HunterStates.PATROL);
            }
        };
    }

    #region PatrolSet
    void SetPatrol()
    {
        var patrolTransitions = new Dictionary<HunterStates, Transition<HunterStates>>
        {
            { HunterStates.IDLE,  new Transition<HunterStates>(HunterStates.IDLE, idle) },
            { HunterStates.CHASE, new Transition<HunterStates>(HunterStates.CHASE, chase) }
        };

        patrol = new State<HunterStates>("Patrol").Configure(patrolTransitions);

        patrol.OnUpdate += PatrolDir; 

        patrol.OnLateUpdate += ChangeStateFromPatrol;
    }


    public void PatrolDir()
    {
        var dir = _waypoints[_nextPos].position - transform.position;
        transform.position += dir.normalized * speed * Time.deltaTime;
        transform.forward = dir;

        if (dir.magnitude <= 0.2f)
        {
            _nextPos++;

            if (_nextPos >= _waypoints.Length)
                _nextPos = 0;
        }
    }

    void ChangeStateFromPatrol()
    {
        if (GameManager.instance.NPCEnergy <= 0)
            _fsm.SendInput(HunterStates.IDLE);
        else if (target != null)
        {

            var dist = target.transform.position - transform.position;

            if (dist.magnitude <= minDetectRadius && GameManager.instance.NPCEnergy >= 0)
            {
                _fsm.SendInput(HunterStates.CHASE);
            }
        }

    }
    #endregion

    void SetChase()
    {
        var chaseTransitions = new Dictionary<HunterStates, Transition<HunterStates>>
        {
            { HunterStates.IDLE, new Transition<HunterStates>(HunterStates.IDLE, idle) },
            { HunterStates.PATROL, new Transition<HunterStates>(HunterStates.PATROL, patrol) }
        }; 

        chase = new State<HunterStates>("Chase").Configure(chaseTransitions);

        chase.OnUpdate += () => GameManager.instance.NPCEnergy -= Time.deltaTime;

        chase.OnFixedUpdate += () => AddForce(Pursuit(target));

        chase.OnLateUpdate += () =>
        {
            if (GameManager.instance.NPCEnergy <= 0)
                _fsm.SendInput(HunterStates.IDLE);
            else if (target == null && GameManager.instance.NPCEnergy > 0)
                _fsm.SendInput(HunterStates.PATROL);
        };

    }

    void Update()
    {
        _fsm.Update();
    }

    private void LateUpdate()
    {
        _fsm.LateUpdate();
    }

    private void FixedUpdate()
    {
        _fsm.FixedUpdate();
    }

    #region Movement
    Vector3 Pursuit(Boid actualTarget)
    {

        Vector3 finalPos = actualTarget.transform.position + actualTarget._velocity * Time.fixedDeltaTime;

        Vector3 desired = finalPos - transform.position;

        desired.Normalize();

        desired *= maxSpeed;

        return CalculatedSteering(desired);
    }

    Vector3 CalculatedSteering(Vector3 desired)
    {
        return Vector3.ClampMagnitude(desired - _velocity, steeringForce);
    }

    public void AddForce(Vector3 force)
    {
        _velocity = Vector3.ClampMagnitude(_velocity + force, maxSpeed);
    }
    #endregion
}

using IA2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[RequireComponent(typeof(Queries))]
[RequireComponent(typeof(GridEntity))]
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


    public Transform wpFather;
    public Transform[] _waypoints;
    public Boid target;

    public Vector3 _velocity;

    public int _nextPos = 0;
    public float speed,maxSpeed,steeringForce;
    [SerializeField]float lossRadius;

    Queries myRadius;

    private void Awake()
    {
        myRadius= GetComponent<Queries>();              

    }

    private void Start()
    {
        _waypoints = wpFather.GetComponentsInChildren<Transform>();
        SetPatrol(); SetChase(); SetIdle();
        _fsm = new EventFSM<HunterStates>(idle);
    }

    void SetIdle()
    {
        idle = new State<HunterStates>("Idle");

        StateConfigurer.Create(idle)
        .SetTransition(HunterStates.PATROL, patrol)
        .SetTransition(HunterStates.CHASE, chase)
        .Done();

        
        idle.OnEnter += (x) => Debug.Log("entre a idle");

        idle.OnUpdate += () => GameManager.instance.NPCEnergy += Time.deltaTime;

        idle.OnLateUpdate += () =>
        {
            if (GameManager.instance.NPCEnergy >= 10)
            {
                if (target == null)
                    _fsm.SendInput(HunterStates.PATROL);
                else if (target != null)
                    _fsm.SendInput(HunterStates.CHASE);
            }
        };

        idle.OnEnter += (x) => Debug.Log("sali de idle");
        
    }

    #region PatrolSet
    void SetPatrol()
    {
        var patrolTransitions = new Dictionary<HunterStates, Transition<HunterStates>>
        {
            { HunterStates.IDLE,  new Transition<HunterStates>(HunterStates.IDLE, idle) },
            { HunterStates.CHASE, new Transition<HunterStates>(HunterStates.CHASE, chase) }
        };

        patrol = new State<HunterStates>("Patrol");

        StateConfigurer.Create(patrol)
            .SetTransition(HunterStates.IDLE, idle)
            .SetTransition(HunterStates.CHASE,chase)
            .Done();

        patrol.OnUpdate += GetNearestTarget;

        patrol.OnFixedUpdate += PatrolDir; 

        patrol.OnLateUpdate += ChangeStateFromPatrol;
    }

    
    public void PatrolDir()
    {
        var dir = _waypoints[_nextPos].position - transform.position;
        transform.position += dir.normalized * speed * Time.fixedDeltaTime;
        transform.forward = dir;

        if (dir.magnitude <= 3)
        {
            _nextPos++;
            Debug.Log("paso al siguiente wp");

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

            if (dist.magnitude <= myRadius.radius + lossRadius && GameManager.instance.NPCEnergy >= 0)
            {
                _fsm.SendInput(HunterStates.CHASE);
            }
        }

    }
    #endregion

    void SetChase()
    {
        chase = new State<HunterStates>("Chase");

        StateConfigurer.Create(chase)
            .SetTransition(HunterStates.IDLE, idle)
            .SetTransition(HunterStates.PATROL, patrol)
            .Done();

        chase.OnEnter += (x) => { Debug.Log("entro a chase"); };


        chase.OnUpdate += () => GameManager.instance.NPCEnergy -= Time.deltaTime;

        chase.OnFixedUpdate += () => AddForce(Pursuit(target));


        chase.OnLateUpdate += GetNearestTarget;

        chase.OnLateUpdate += () =>
        {
            if (GameManager.instance.NPCEnergy <= 0)
                _fsm.SendInput(HunterStates.IDLE);
            else if (target == null && GameManager.instance.NPCEnergy > 0)
                _fsm.SendInput(HunterStates.PATROL);
        };

        chase.OnExit += (x) => { Debug.Log("salgo de chase, voy a "+x); };

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


    //IA2-P1
    void GetNearestTarget()
    {
        Debug.Log(myRadius.selected);
        if (myRadius.selected.Any())
        {
            target = myRadius.selected
            .Select(x => x.GetComponent<Boid>())
            .Where(x => x != null)
            .OrderBy(x => Vector3.Distance(x.transform.position, transform.position))
            .FirstOrDefault(null);
         
        }
        else
        {
            target = null;
        }

    }
    #endregion
}

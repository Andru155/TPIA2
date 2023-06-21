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

    //IA2-P3
    public GridEntity entity;

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
        myRadius = GetComponent<Queries>();
        entity = GetComponent<GridEntity>();

    }

    private void Start()
    {
        _waypoints = wpFather.GetComponentsInChildren<Transform>();
        SetPatrol(); SetChase(); SetIdle();

        StateConfigurer.Create(idle)
       .SetTransition(HunterStates.PATROL, patrol)
       .SetTransition(HunterStates.CHASE, chase)
       .Done();

        StateConfigurer.Create(patrol)
          .SetTransition(HunterStates.IDLE, idle)
          .SetTransition(HunterStates.CHASE, chase)
          .Done();

        StateConfigurer.Create(chase)
           .SetTransition(HunterStates.IDLE, idle)
           .SetTransition(HunterStates.PATROL, patrol)
           .Done();

        _fsm = new EventFSM<HunterStates>(idle);
    }

    void SetIdle()
    {
        idle = new State<HunterStates>("Idle");

       

        
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
        

        patrol = new State<HunterStates>("Patrol");

      

        patrol.OnUpdate += () =>
        {
            IEnumerable<Boid> boids = myRadius.Query()
            .Select(x=>x.GetComponent<Boid>())
            .Where(x=>x!=null);
            if (boids.Any())
            {
                GetNearestTarget(boids);
            }
           
        };

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
                Debug.Log("entro a chase");
                _fsm.SendInput(HunterStates.CHASE);
            }
        }

    }
    #endregion

    void SetChase()
    {
        chase = new State<HunterStates>("Chase");

       

        chase.OnEnter += (x) => { Debug.Log("entro a chase"); };


        chase.OnUpdate += () => GameManager.instance.NPCEnergy -= Time.deltaTime;

        chase.OnFixedUpdate += () => AddForce(Pursuit(target));
        chase.OnFixedUpdate += () => 
        {
            if (Vector3.Distance(target.transform.position,transform.position)<1f)
            {
                target.Kill();
                target = null;
            }         
                     
        };


        chase.OnLateUpdate += () =>
        {

            IEnumerable<Boid> boids = myRadius.Query()
            .Select(x => x.GetComponent<Boid>())
            .Where(x => x != null);
            if (boids.Count()>0)
            {
                GetNearestTarget(boids);
                
            }

            if (target != null)
            {
                if (Vector3.Distance(target.transform.position,transform.position) <= myRadius.radius)
                {
                    return;
                }
            }

            if (GameManager.instance.NPCEnergy > 0)
            {
                _fsm.SendInput(HunterStates.PATROL);
            }
            else if (GameManager.instance.NPCEnergy <= 0)
            {
                _fsm.SendInput(HunterStates.IDLE);
            }


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
        transform.position += _velocity * Time.deltaTime;
        transform.forward = _velocity;
        entity.OnMove(_velocity);
    }


    //IA2-P1
    void GetNearestTarget(IEnumerable<Boid> boids)
    {
      


        if (boids.Any())
        {

            target = boids                  
            .OrderBy(x => Vector3.Distance(x.transform.position, transform.position))
            .First();
         
        }
        else
        {
            target = null;
        }

    }
    #endregion

    public Vector3 Velocity
    {
        get { return _velocity; }
    }
}

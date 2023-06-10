using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patrol : IState
{
    Transform _transform;
    Transform _target;    
    NPC npc;
    float _speed, _minRadius;
    FSM _fsm;
    

    Transform[] _waypoints;
    int _nextPos;

    public Patrol(FSM fsm, Transform transform, Transform[] waypoints, int nextPos, float speed, Transform target, float minRadius)
    {        
        _fsm = fsm;
        _transform = transform;
        _waypoints = waypoints;
        _nextPos = nextPos;
        _speed = speed;
        _target = target;
        _minRadius = minRadius;
    }

    public void OnEnter()
    {        
        Debug.Log("Empiezo patrulla");      
    }

    public void OnUpdate()
    {        
        GameManager.instance.NPCEnergy -= Time.deltaTime;
        PatrolDir();       
        
        ChangeState();

    }    

    public void OnExit()
    {        
        Debug.Log("Termino patrulla");
    }

    void ChangeState()
    {
        if (GameManager.instance.NPCEnergy <= 0)
            _fsm.ChangeState("Idle");     
        else if(_target != null)
        {            
            
            var dist = _target.transform.position - _transform.position;

            if(dist.magnitude <= npc.minRadius && GameManager.instance.NPCEnergy >= 0)
            {
                _fsm.ChangeState("Chase");
            }
        }
       
    }    

    public void PatrolDir()
    {
        var dir = _waypoints[_nextPos].position - _transform.position;
        _transform.position += dir.normalized * _speed * Time.deltaTime;
        _transform.forward = dir;

        if (dir.magnitude <= 0.2f)
        {
            _nextPos++;

            if (_nextPos >= _waypoints.Length)
                _nextPos = 0;
        }
    }
}

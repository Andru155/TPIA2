using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Idle : IState
{
    FSM _fsm;
    Transform target;

    public Idle(FSM fsm)
    {
        _fsm = fsm;
    }

    public void OnEnter()
    {
        Debug.Log("Empiezo idle");
    }

    public void OnUpdate()
    {
        GameManager.instance.NPCEnergy += Time.deltaTime;

        if (GameManager.instance.NPCEnergy >= 10)
        {
            if (target == null)
                _fsm.ChangeState("Patrol");
            else if (target != null)
                _fsm.ChangeState("Chase");
        }


    }

    public void OnExit()
    {
        Debug.Log("Termino idle");
    }
}

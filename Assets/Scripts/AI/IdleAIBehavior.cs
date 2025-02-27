﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class IdleAIBehavior : StateMachineBehaviour
{
    Player player;
    Enemy self;
    NavMeshAgent agent;
    float time = 0f;

    float atkSpeed;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        self = animator.GetComponent<Enemy>();
        agent = animator.GetComponent<NavMeshAgent>();
        agent.isStopped = true;
        player = GameManager.Instance.PlayerObject;
        atkSpeed = animator.GetFloat("atkSpeed");
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!self.gameEnd)
        {
            time += Time.deltaTime;
            float distance = Vector3.Distance(animator.transform.position, player.transform.position);
            if ((distance <= self.detectDistance || self.hordeMode) && GameManager.Instance.enabled)
            {
                if (distance > agent.stoppingDistance) animator.Play("Run");
                else if (time >= atkSpeed)
                {
                    animator.Play("Attack");
                    time = 0;
                }
            }
        }
       
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}

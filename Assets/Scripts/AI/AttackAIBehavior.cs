using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AttackAIBehavior : StateMachineBehaviour
{
    bool damage = false;
    float disengageDist;
    Enemy self;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        damage = false;
        self = animator.GetComponent<Enemy>();
        disengageDist = animator.GetComponent<NavMeshAgent>().stoppingDistance;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
        if(!damage && stateInfo.normalizedTime * animator.GetInteger("totalFrame") > animator.GetInteger("damageFrame"))
        {
            if (true)//Vector3.Distance(animator.transform.position, GameManager.Instance.PlayerObject.transform.position) <= disengageDist)
            {
                if(self.type == EnemyType.Mob)
                {
                    SoundManager.PlaySound(self.audioSource, SoundManager.SoundType.GoblinAttack);
                    SoundManager.PlaySound(self.audioSource, SoundManager.SoundType.GoblinLaugh);
                }
                else if(self.type == EnemyType.Boss)
                {
                    SoundManager.PlaySound(self.audioSource, SoundManager.SoundType.TrollAttack);
                }
                GameManager.Instance.PlayerObject.Damage(animator.GetFloat("Damage"));
            }
                
            damage = true;
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
    }

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

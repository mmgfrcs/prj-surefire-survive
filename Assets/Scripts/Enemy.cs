using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.GameFoundation;

public class Enemy : Entity
{
    public EnemyType type;
    public float detectDistance;
    [SerializeField] internal AudioSource audioSource;

    GameManager gm;
    NavMeshAgent agent;
    Animator anim;
    bool attacking = false;
    GameItem enemyItem;

    internal float distance = 0;
    internal bool firstAtk = true;
    internal bool hordeMode = false;
    internal bool gameEnd = false;
    

    // Start is called before the first frame update
    void Start()
    {
        gm = GameManager.Instance;
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        GameItemDefinition def = GameFoundationSettings.database.gameItemCatalog.GetGameItemDefinition(type.ToString().ToLower());
        enemyItem = new GameItem(def);
        CurrentHealth = enemyItem.GetStatFloat(GameManager.DEF_HEALTH);

        anim.SetFloat("Damage", enemyItem.GetStatFloat(GameManager.DEF_DAMAGE));
        anim.SetFloat("atkSpeed", enemyItem.GetStatFloat(GameManager.DEF_ATKSPEED));
        agent.speed = enemyItem.GetStatFloat(GameManager.DEF_MOVESPEED);
        gm.OnGameEnd += OnGameEnd;
    }

    private void OnGameEnd()
    {
        gameEnd = true;
    }

    // Update is called once per frame
    void Update()
    {
        distance = Vector3.Distance(transform.position, gm.PlayerObject.transform.position);
        hordeMode = gm.IsHordeMode;
    }

    protected override void Die()
    {
        base.Die();
        if(type == EnemyType.Boss) SoundManager.PlaySound(GetComponent<AudioSource>(), SoundManager.SoundType.TrollRoar);
        anim.Play("Death");
        StartCoroutine(DieDecompose(transform.position.y));
        GetComponent<Collider>().enabled = false;
        agent.enabled = false;
        GameManager.Instance.EnemyDie(this);
    }

    private IEnumerator DieDecompose(float yPos)
    {
        yield return new WaitForSeconds(5f);
        while (transform.position.y >= yPos - 5)
        {
            transform.Translate(Vector3.up * -Time.deltaTime);
            yield return null;
        }

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        gm.OnGameEnd -= OnGameEnd;
    }

    //private void OnGUI()
    //{
    //    Vector2 toVector = GameManager.Instance.PlayerObject.transform.localPosition - anim.transform.localPosition;
    //    float angleToTarget = Vector2.Angle(anim.transform.forward, toVector);
    //    GUI.Label(new Rect(32, 32, 100, 40), angleToTarget.ToString("n0"));
    //}


}

public enum EnemyType
{
    Mob, Flight, Stealth, Poison, Stun, Boss
}
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

    protected GameManager gm;
    protected NavMeshAgent agent;
    protected Animator anim;
    protected GameItem enemyItem;

    internal float distance = 0;
    internal bool firstAtk = true;
    internal bool hordeMode = false;
    internal bool gameEnd = false;

    Dictionary<EnemyType, string> gameDefEntries = new Dictionary<EnemyType, string>()
    {
        {EnemyType.Mob, "mob" }, {EnemyType.FastMob, "mobFast"}, {EnemyType.EliteMob, "mobElite"},
        {EnemyType.Turret, "turret"}, {EnemyType.Boss, "boss"}
    };

    // Start is called before the first frame update
    protected virtual void Start()
    {
        GetGameFoundationStats();
        InitializeAgentAndAnimator();
        gm = GameManager.Instance;

        gm.OnGameEnd += OnGameEnd;
    }

    protected virtual void GetGameFoundationStats()
    {
        GameItemDefinition def = GameFoundationSettings.database.gameItemCatalog.GetGameItemDefinition(gameDefEntries[type]);
        enemyItem = new GameItem(def);
        CurrentHealth = enemyItem.GetStatFloat(GameManager.DEF_HEALTH);
    }

    protected virtual void InitializeAgentAndAnimator()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        anim.SetFloat("Damage", enemyItem.GetStatFloat(GameManager.DEF_DAMAGE));
        anim.SetFloat("atkSpeed", enemyItem.GetStatFloat(GameManager.DEF_ATKSPEED));
        if (type == EnemyType.Mob) agent.speed = enemyItem.GetStatFloat(GameManager.DEF_MOVESPEED) * Random.Range(1f, 1.2f);
        else agent.speed = enemyItem.GetStatFloat(GameManager.DEF_MOVESPEED);
    }

    private void OnGameEnd()
    {
        gameEnd = true;
    }

    // Update is called once per frame
    protected virtual void Update()
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

    protected virtual IEnumerator DieDecompose(float yPos)
    {
        yield return new WaitForSeconds(5f);
        while (transform.position.y >= yPos - 5)
        {
            transform.Translate(Vector3.up * -Time.deltaTime);
            yield return null;
        }

        Destroy(gameObject);
    }

    protected virtual void OnDestroy()
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
    Mob, FastMob, EliteMob, Turret, Boss
}
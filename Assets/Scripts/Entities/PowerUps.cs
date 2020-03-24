using System.Collections;
using UnityEngine.Networking;
using UnityEngine;

public class PowerUps : MonoBehaviour {

    public float hpBonus = 100, hpRegen = 1f, regenDuration = 1f;
    public float shieldBonus = 0, spawnTime = 30;
    bool taken = false;
    Collider coll;
    MeshRenderer mr;
    private void Start()
    {
        coll = GetComponent<Collider>();
        mr = GetComponent<MeshRenderer>();
    }
}

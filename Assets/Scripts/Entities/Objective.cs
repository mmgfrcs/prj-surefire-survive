using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Objective : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            SoundManager.PlaySound(SoundManager.SoundType.KeyCollect);
            GameManager.Instance.CompleteObjective(gameObject);
            Destroy(gameObject);
        }

    }
}

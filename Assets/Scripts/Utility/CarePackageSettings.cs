using System;
using System.Net.Mail;
using UnityEngine;

[Serializable]
public class CarePackageSettings
{
    public int deliveryAmount = 1;
    public GameObject packagePrefab;
    public CarePackageDelivery[] deliveries;
    public CarePackageDelivery CurrentDelivery { get { return currentDelivery < deliveries.Length ? deliveries[currentDelivery] : null; } }

    int currentDelivery = 0;

    public CarePackageDelivery GetNextDelivery()
    {
        currentDelivery++;
        return CurrentDelivery;
    }
}

[Serializable]
public class CarePackageDelivery
{
    public float time;
    //public bool forceHordeModeOnTake;
    //public float hordeModeTime;
    //public bool forceSpawnEnemy;
    //public int amountToSpawn;
    //public Vector3[] enemySpawnLocationOffset;
    public Transform deliveryLocation;
}
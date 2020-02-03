using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawn : MonoBehaviour
{
    public ChestType defaultChest;
    public ItemSpawnRateTable spawnRateTable;
    
}

[System.Serializable]
public struct ItemSpawnRateTable
{
    [Tooltip("Does not need to add up to 1 or 100")]
    public AnimationCurve rifleAmmo, handgunAmmo, assortedAmmo, bigHealth, smallHealth, grenade;
}

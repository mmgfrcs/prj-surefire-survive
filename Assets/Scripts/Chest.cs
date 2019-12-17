using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.GameFoundation;

public class Chest : MonoBehaviour
{
    [SerializeField] OpenChest chestOpener;
    [SerializeField] ChestType type;

    public ChestType Type { get { return type; } }
    public GameItem Item { get; private set; }

    private string[] chestDefIds = new string[] { "rifleAmmoChest", "handgunAmmoChest", "bothAmmoChest", "bigPotionChest", "smallPotionChest", "grenadeChest" };

    public IEnumerator OpenChest()
    {
        chestOpener.opening = true;
        yield return new WaitForSeconds(1f / chestOpener.speed);
    }

    public void Start()
    {
        GameItemDefinition def = GameFoundationSettings.database.gameItemCatalog.GetGameItemDefinition(chestDefIds[(int)type]);
        Item = new GameItem(def);
    }
}

public enum ChestType
{
    RifleAmmo, HandgunAmmo, AssortedAmmo, BigPotion, SmallPotion, Grenade
}
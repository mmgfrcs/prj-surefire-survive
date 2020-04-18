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
    public bool IsBusy { get { return chestOpener.factor > 0 && chestOpener.factor < 1; } }

    private string[] chestDefIds = new string[] { "rifleAmmoChest", "handgunAmmoChest", "bothAmmoChest", "bigPotionChest", "smallPotionChest", "grenadeChest", "medkitChest" };

    public IEnumerator OpenChest()
    {
        chestOpener.closing = false;
        chestOpener.opening = true;
        SoundManager.PlaySound(GetComponent<AudioSource>(), SoundManager.SoundType.ChestOpen);
        yield return new WaitForSeconds(1f / chestOpener.speed);
    }

    public IEnumerator CloseChest()
    {
        chestOpener.opening = false;
        chestOpener.closing = true;
        SoundManager.PlaySound(GetComponent<AudioSource>(), SoundManager.SoundType.ChestOpen);
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
    RifleAmmo, HandgunAmmo, AssortedAmmo, BigPotion, SmallPotion, Grenade, Medkit
}
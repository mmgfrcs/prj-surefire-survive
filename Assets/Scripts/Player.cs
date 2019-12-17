using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using FPSControllerLPFP;
using UnityEngine.GameFoundation;
using TMPro;
using System;

public class Player : Entity {


    public AutomaticGunScriptLPFP autoGunScript;
    public HandgunScriptLPFP handgunScript;

    [Header("Weapon Switching")]
    public GameObject autoGunArms;
    public GameObject handgunArms;
    public Animator autoGunAnimator;
    public Animator handgunAnimator;
    public Image gunImage;
    public Sprite autoGunSprite, handgunSprite;
    FpsControllerLPFP controller;
    bool autogunMode = true;

    [Header("UI"), Range(0, 1)]
    public float criticalHPRatio;
    public Color normalHPColor, criticalHPColor;
    public Slider hpBar;
    public Image hpBarFill;
    public Slider staminaBar;
    public TextMeshProUGUI healthText;
    public Image crosshair;
    public GameObject chestText;

    public bool CanRun { get; private set; }
    public bool CanShoot { get; private set; } = true;
    
    GameManager gameManager;
    GameItem playerItem;
    float stamina;
    float criticalHealth;
    float staminaRecharge = 0;

    private void Start()
    {
        gameManager = GameManager.Instance;
        GameItemDefinition playerDef = GameFoundationSettings.database.gameItemCatalog.GetGameItemDefinition("player");
        playerItem = new GameItem(playerDef, "mainPlayer");
        CurrentHealth = playerItem.GetStatFloat(GameManager.DEF_HEALTH);
        stamina = playerItem.GetStatFloat(GameManager.DEF_STAMINA);
        controller = GetComponent<FpsControllerLPFP>();

        criticalHealth = CurrentHealth * criticalHPRatio;
        hpBar.maxValue = CurrentHealth;
        staminaBar.maxValue = stamina;
        hpBar.value = CurrentHealth;
        staminaBar.value = stamina;
    }

    protected override void Die()
    {
        //Game Over
        Time.timeScale = 0;
    }

    private void Update()
    {
        UpdateUI();
        ProcessInput();
        Raycast();
    }

    private void Raycast()
    {
        crosshair.color = Color.white;
        chestText.SetActive(false);
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, 3))
        {
            Chest c = hit.collider.GetComponent<Chest>();
            if (c != null)
            {
                CanShoot = false;
                crosshair.color = new Color(1, 1, 0);
                chestText.SetActive(true);
                if (Input.GetKeyDown(KeyCode.F)) StartCoroutine(OpenChest(c));
            }
            
        }
        else CanShoot = true;
    }

    private IEnumerator OpenChest(Chest c)
    {
        yield return c.OpenChest();
        switch(c.Type)
        {
            case ChestType.RifleAmmo:
                {
                    FillRifleAmmo(c);
                    break;
                }
            case ChestType.HandgunAmmo:
                {
                    FillHandgunAmmo(c);
                    break;
                }
            case ChestType.AssortedAmmo:
                {
                    FillRifleAmmo(c);
                    FillHandgunAmmo(c);
                    break;
                }
            case ChestType.BigPotion:
                {
                    gameManager.GetBigPotion();
                    gameManager.Announce($"Big Potion Added!");
                    break;
                }
            case ChestType.SmallPotion:
                {
                    gameManager.GetSmallPotion();
                    gameManager.Announce($"Small Potion Added!");
                    break;
                }
            case ChestType.Grenade:
                {
                    gameManager.GetGrenade();
                    gameManager.Announce($"Grenade Added!");
                    break;
                }
        }
        Destroy(c);
    }

    void FillRifleAmmo(Chest c)
    {
        int ammo = c.Item.GetStatInt(GameManager.DEF_AK47AMMO);
        gameManager.Announce($"+{ammo} AK47 Ammo");
        autoGunScript.AddMagazine(ammo);
    }

    void FillHandgunAmmo(Chest c)
    {
        int ammo = c.Item.GetStatInt(GameManager.DEF_HANDGUNAMMO);
        gameManager.Announce($"+{ammo} Glock Ammo");
        handgunScript.AddMagazine(ammo);
    }

    private void ProcessInput()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (autogunMode)
            {
                autoGunArms.SetActive(false);
                handgunArms.SetActive(true);
                controller.ChangeArms(handgunArms.transform);
                handgunAnimator.Play("Draw");
                gunImage.sprite = handgunSprite;
                autogunMode = false;
            }
            else
            {
                handgunArms.SetActive(false);
                autoGunArms.SetActive(true);
                controller.ChangeArms(autoGunArms.transform);
                autoGunAnimator.Play("Draw");
                gunImage.sprite = autoGunSprite;
                autogunMode = true;
            }
        }
        if(Input.GetKey(KeyCode.LeftShift))
        {
            staminaRecharge = 0;
            stamina = Mathf.Clamp(stamina - 50 * Time.deltaTime, 0, playerItem.GetStatFloat(GameManager.DEF_STAMINA));
        }
        else
        {
            staminaRecharge += Time.deltaTime;
            if(staminaRecharge > 1) stamina = Mathf.Clamp(stamina + 25 * Time.deltaTime, 0, playerItem.GetStatFloat(GameManager.DEF_STAMINA));
        }
        if (stamina <= 0) CanRun = false;
        else CanRun = true;

        if (Input.GetKeyDown(KeyCode.Alpha3) && gameManager.BigPotionAvailable)
        {
            GameItem item = new GameItem(GameFoundationSettings.database.gameItemCatalog.GetGameItemDefinition("bigHP"));
            gameManager.UseBigPotion();
            HealHP(item.GetStatFloat(GameManager.DEF_HEAL));
        }

        if (Input.GetKeyDown(KeyCode.Alpha4) && gameManager.SmallPotionAvailable)
        {
            GameItem item = new GameItem(GameFoundationSettings.database.gameItemCatalog.GetGameItemDefinition("smallHP"));
            gameManager.UseSmallPotion();
            HealHP(item.GetStatFloat(GameManager.DEF_HEAL));
        }

        if (Input.GetKeyDown(KeyCode.G) && gameManager.GrenadeAvailable)
        {
            gameManager.UseGrenade();
        }

        if (Input.GetMouseButton(1)) crosshair.gameObject.SetActive(false);
        else crosshair.gameObject.SetActive(true);
    }

    private void UpdateUI()
    {
        hpBar.value = CurrentHealth;
        staminaBar.value = stamina;
        healthText.text = $"{Mathf.Round(CurrentHealth)} <size=24>{playerItem.GetStatFloat(GameManager.DEF_HEALTH)}</size>";
        if (CurrentHealth <= criticalHealth) hpBarFill.color = criticalHPColor;
        else hpBarFill.color = normalHPColor;
    }

    void HealHP(float amount)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, playerItem.GetStatFloat(GameManager.DEF_HEALTH));
    }
}
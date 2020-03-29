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
    internal FpsControllerLPFP controller;
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

    [Header("Debug - UI")]
    public Slider hpBarDebug;
    public TextMeshProUGUI healthTextDebug, currentAmmoDebugText, currentMagazineDebugText, gunNameDebugText;
    public Slider staminaBarDebug;
    public Image hpBarDebugFill;
    public Image gunImageDebug;

    public bool CanRun { get; private set; }
    public bool CanShoot { get; private set; } = true;
    public bool IsRifleEquipped { get { return autoGunArms.activeInHierarchy; } }
    public float CurrentStamina { get { return stamina;  } }

    List<Chest> chestsOpened = new List<Chest>();
    
    GameManager gameManager;
    internal GameItem playerItem;
    float stamina;
    float criticalHealth;
    float staminaRecharge = 0;
    bool end = false;

    Coroutine regenCoroutine;

    //Stats
    internal float SmallPotHealAmount { get; private set; }
    internal float SmallPotHealDuration { get; private set; }
    internal float SmallPotHealMaxDuration { get; private set; }

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
        hpBarDebug.maxValue = CurrentHealth;
        staminaBarDebug.maxValue = stamina;
        hpBarDebug.value = CurrentHealth;
        staminaBarDebug.value = stamina;
        gameManager.OnGameEnd += GameManager_OnGameEnd;
        SwitchWeapon();
        SwitchWeapon();
    }

    private void GameManager_OnGameEnd()
    {
        end = true;
    }

    protected override void Die()
    {
        //Game Over
        gameManager.PlayerDie();
    }

    public override void Damage(float amount)
    {
        if(!end)
            base.Damage(amount);
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
            if (c != null && !c.IsBusy)
            {
                CanShoot = false;
                crosshair.color = new Color(1, 1, 0);
                chestText.SetActive(true);
                if (Input.GetKeyDown(KeyCode.F))
                {
                    if(!chestsOpened.Contains(c)) StartCoroutine(OpenChest(c));
                }
            }
            
        }
        else CanShoot = true;
    }

    private IEnumerator OpenChest(Chest c)
    {
        
        yield return c.OpenChest();
        if (TryOpenChest(c))
        {
            chestsOpened.Add(c);
            Destroy(c);
        }
        else yield return c.CloseChest();
    }

    bool TryOpenChest(Chest c)
    {
        switch (c.Type)
        {
            case ChestType.RifleAmmo:
                {
                    FillRifleAmmo(c);
                    return true;
                }
            case ChestType.HandgunAmmo:
                {
                    FillHandgunAmmo(c);
                    return true;
                }
            case ChestType.AssortedAmmo:
                {
                    FillRifleAmmo(c);
                    FillHandgunAmmo(c);
                    return true;
                }
            case ChestType.BigPotion:
                {
                    return gameManager.GetBigPotion();
                }
            case ChestType.SmallPotion:
                {
                    return gameManager.GetSmallPotion();
                }
            case ChestType.Grenade:
                {
                    return gameManager.GetGrenade();
                }
        }
        Debug.LogError($"Chest Type Error: Type {c.Type} not found");
        return false;
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
            SwitchWeapon();
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
            RegenHP(item.GetStatFloat(GameManager.DEF_HEAL), item.GetStatFloat("regenAmount"));
        }

        if (Input.GetKeyDown(KeyCode.G) && gameManager.GrenadeAvailable)
        {
            gameManager.UseGrenade();
        }

        if (Input.GetMouseButton(1)) crosshair.gameObject.SetActive(false);
        else crosshair.gameObject.SetActive(true);
    }

    private void SwitchWeapon()
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

    private void UpdateUI()
    {
        hpBar.value = CurrentHealth;
        staminaBar.value = stamina;
        hpBarDebug.value = CurrentHealth;
        staminaBarDebug.value = stamina;
        gunNameDebugText.text = autoGunScript.currentWeaponText.text;
        currentAmmoDebugText.text = autoGunScript.currentAmmoText.text;
        currentMagazineDebugText.text = autoGunScript.totalAmmoText.text;
        gunImageDebug.sprite = gunImage.sprite;

        healthTextDebug.text = $"{Mathf.Round(CurrentHealth)} <size=24>{playerItem.GetStatFloat(GameManager.DEF_HEALTH)}</size>";
        healthText.text = $"{Mathf.Round(CurrentHealth)} <size=24>{playerItem.GetStatFloat(GameManager.DEF_HEALTH)}</size>";
        if (CurrentHealth <= criticalHealth)
        {
            hpBarFill.color = criticalHPColor;
            hpBarDebugFill.color = criticalHPColor;
        }
        else
        {
            hpBarFill.color = normalHPColor;
            hpBarDebugFill.color = normalHPColor;
        }
    }

    void HealHP(float amount)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, playerItem.GetStatFloat(GameManager.DEF_HEALTH));
    }

    void RegenHP(float amount, float time)
    {
        SmallPotHealAmount += amount;
        SmallPotHealMaxDuration += time;
        SmallPotHealDuration += time;
        if (regenCoroutine == null) regenCoroutine = StartCoroutine(Regenerate());
    }

    IEnumerator Regenerate()
    {
        while(SmallPotHealDuration > 0)
        {
            gameManager.smallPotionIcon.SetBarValue(SmallPotHealDuration / SmallPotHealMaxDuration);
            float tickHeal = SmallPotHealAmount * Time.deltaTime / SmallPotHealDuration;
            HealHP(tickHeal);
            SmallPotHealAmount -= tickHeal;
            SmallPotHealDuration -= Time.deltaTime;
            yield return null;
        }

        SmallPotHealAmount = 0;
        SmallPotHealDuration = 0;
        SmallPotHealMaxDuration = 0;
        gameManager.smallPotionIcon.DisableBar();
        regenCoroutine = null;
    }
}
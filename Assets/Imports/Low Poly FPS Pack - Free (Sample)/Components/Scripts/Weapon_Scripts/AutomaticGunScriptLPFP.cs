﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

// ----- Low Poly FPS Pack Free Version -----
public class AutomaticGunScriptLPFP : MonoBehaviour, IGun {

    //Game Manager
    GameManager gm;

	//Animator component attached to weapon
	Animator anim;

    [Header("Game")]
    public float bulletDamage;

	[Header("Gun Camera")]
	//Main gun camera
	public Camera gunCamera;

	[Header("Gun Camera Options")]
	//How fast the camera field of view changes when aiming 
	[Tooltip("How fast the camera field of view changes when aiming.")]
	public float fovSpeed = 15.0f;
	//Default camera field of view
	[Tooltip("Default value for camera field of view (40 is recommended).")]
	public float defaultFov = 40.0f;

	public float aimFov = 25.0f;

	[Header("UI Weapon Name")]
	[Tooltip("Name of the current weapon, shown in the game UI.")]
	public string weaponName;
	private string storedWeaponName;

	[Header("Weapon Sway")]
	//Enables weapon sway
	[Tooltip("Toggle weapon sway.")]
	public bool weaponSway;

	public float swayAmount = 0.02f;
	public float maxSwayAmount = 0.06f;
	public float swaySmoothValue = 4.0f;

	private Vector3 initialSwayPosition;

	//Used for fire rate
	private float lastFired;
	[Header("Weapon Settings")]
	//How fast the weapon fires, higher value means faster rate of fire
	[Tooltip("How fast the weapon fires, higher value means faster rate of fire.")]
	public float fireRate;
	//Eanbles auto reloading when out of ammo
	[Tooltip("Enables auto reloading when out of ammo.")]
	public bool autoReload;
	//Delay between shooting last bullet and reloading
	public float autoReloadDelay;
	//Check if reloading
	private bool isReloading;

	//Holstering weapon
	private bool hasBeenHolstered = false;
	//If weapon is holstered
	private bool holstered;
	//Check if running
	private bool isRunning;
	//Check if aiming
	private bool isAiming;
	//Check if walking
	private bool isWalking;
	//Check if inspecting weapon
	private bool isInspecting;

	//How much ammo is currently left
	private int currentAmmo;
	//Totalt amount of ammo
	[Tooltip("How much ammo the weapon should have.")]
	public int ammo;
	//Check if out of ammo
	private bool outOfAmmo;
    //How much magazine is currently left
    private int currentMags;
    [Tooltip("How much ammo the weapon should have in total.")]
    public int magazine;
    //Check if out of magazine
    private bool outOfMags;

	[Header("Bullet Settings")]
	//Bullet
	[Tooltip("How much force is applied to the bullet when shooting.")]
	public float bulletForce = 400.0f;
	[Tooltip("How long after reloading that the bullet model becomes visible " +
		"again, only used for out of ammo reload animations.")]
	public float showBulletInMagDelay = 0.6f;
	[Tooltip("The bullet model inside the mag, not used for all weapons.")]
	public SkinnedMeshRenderer bulletInMagRenderer;

	[Header("Grenade Settings")]
	public float grenadeSpawnDelay = 0.35f;

	[Header("Muzzleflash Settings")]
	public bool randomMuzzleflash = false;
	//min should always bee 1
	private int minRandomValue = 1;

	[Range(2, 25)]
	public int maxRandomValue = 5;

	private int randomMuzzleflashValue;

	public bool enableMuzzleflash = true;
	public ParticleSystem muzzleParticles;
	public bool enableSparks = true;
	public ParticleSystem sparkParticles;
	public int minSparkEmission = 1;
	public int maxSparkEmission = 7;

	[Header("Muzzleflash Light Settings")]
	public Light muzzleflashLight;
	public float lightDuration = 0.02f;

	[Header("Audio Source")]
	//Main audio source
	public AudioSource mainAudioSource;
	//Audio source used for shoot sound
	public AudioSource shootAudioSource;

	[Header("UI Components")]
	public TextMeshProUGUI currentWeaponText;
	public TextMeshProUGUI currentAmmoText;
	public TextMeshProUGUI totalAmmoText;

	[System.Serializable]
	public class prefabs
	{  
		[Header("Prefabs")]
		public Transform bulletPrefab;
		public Transform casingPrefab;
		public Transform grenadePrefab;
	}
	public prefabs Prefabs;
	
	[System.Serializable]
	public class spawnpoints
	{  
		[Header("Spawnpoints")]
		//Array holding casing spawn points 
		//(some weapons use more than one casing spawn)
		//Casing spawn point array
		public Transform casingSpawnPoint;
		//Bullet prefab spawn from this point
		public Transform bulletSpawnPoint;

		public Transform grenadeSpawnPoint;
	}
	public spawnpoints Spawnpoints;

	[System.Serializable]
	public class soundClips
	{
		public AudioClip shootSound;
		public AudioClip takeOutSound;
		public AudioClip holsterSound;
		public AudioClip reloadSoundOutOfAmmo;
		public AudioClip reloadSoundAmmoLeft;
		public AudioClip aimSound;
	}
	public soundClips SoundClips;

	private bool soundHasPlayed = false;

    public int CurrentAmmo => currentAmmo;

    public int CurrentMagazine => currentMags;

    private void Awake () {
		
		//Set the animator component
		anim = GetComponent<Animator>();
		//Set current ammo to total ammo value
		currentAmmo = ammo;
        currentMags = magazine;

		muzzleflashLight.enabled = false;
	}

	private void Start () {
		
		//Save the weapon name
		storedWeaponName = weaponName;
		//Get weapon name from string to text
		currentWeaponText.text = weaponName;
		//Set total ammo text from total ammo int
		totalAmmoText.text = currentMags.ToString();
        gm = GameManager.Instance;
        //Weapon sway
        initialSwayPosition = transform.localPosition;

		//Set the shoot sound to audio source
		shootAudioSource.clip = SoundClips.shootSound;
	}

	private void LateUpdate () {
        currentWeaponText.text = weaponName;
        //Weapon sway
        if (weaponSway == true) 
		{
			float movementX = -Input.GetAxis ("Mouse X") * swayAmount;
			float movementY = -Input.GetAxis ("Mouse Y") * swayAmount;
			//Clamp movement to min and max values
			movementX = Mathf.Clamp 
				(movementX, -maxSwayAmount, maxSwayAmount);
			movementY = Mathf.Clamp 
				(movementY, -maxSwayAmount, maxSwayAmount);
			//Lerp local pos
			Vector3 finalSwayPosition = new Vector3 
				(movementX, movementY, 0);
			transform.localPosition = Vector3.Lerp 
				(transform.localPosition, finalSwayPosition + 
					initialSwayPosition, Time.deltaTime * swaySmoothValue);
		}
	}
	
	private void Update () {

		//Aiming
		//Toggle camera FOV when right click is held down
		if(Input.GetButton("Aim") && !isReloading && !isRunning && !isInspecting) 
		{
			
			isAiming = true;
			//Start aiming
			anim.SetBool ("Aim", true);

			//When right click is released
			gunCamera.fieldOfView = Mathf.Lerp(gunCamera.fieldOfView,
				aimFov,fovSpeed * Time.deltaTime);

			if (!soundHasPlayed) 
			{
				mainAudioSource.clip = SoundClips.aimSound;
				mainAudioSource.Play ();
	
				soundHasPlayed = true;
			}
		} 
		else 
		{
			//When right click is released
			gunCamera.fieldOfView = Mathf.Lerp(gunCamera.fieldOfView,
				defaultFov,fovSpeed * Time.deltaTime);

			isAiming = false;
			//Stop aiming
			anim.SetBool ("Aim", false);
				
			soundHasPlayed = false;
		}
		//Aiming end

		//If randomize muzzleflash is true, genereate random int values
		if (randomMuzzleflash == true) 
		{
			randomMuzzleflashValue = Random.Range (minRandomValue, maxRandomValue);
		}

		//Set current ammo text from ammo int
		currentAmmoText.text = currentAmmo.ToString ();
        totalAmmoText.text = currentMags.ToString();
		if (currentAmmo == 0) currentAmmoText.color = new Color(0.7f, 0.7f, 0.7f, 0.2f);
		else currentAmmoText.color = Color.white;
		if (currentMags == 0) totalAmmoText.color = new Color(0.7f, 0.7f, 0.7f, 0.2f);
		else totalAmmoText.color = Color.white;
		//Continosuly check which animation 
		//is currently playing
		AnimationCheck ();

		////Play knife attack 1 animation when Q key is pressed
		//if (Input.GetKeyDown (KeyCode.Q) && !isInspecting) 
		//{
		//	anim.Play ("Knife Attack 1", 0, 0f);
		//}
		////Play knife attack 2 animation when F key is pressed
		//if (Input.GetKeyDown (KeyCode.F) && !isInspecting) 
		//{
		//	anim.Play ("Knife Attack 2", 0, 0f);
		//}
			
		//Throw grenade when pressing G key
		if (Input.GetButtonDown ("Grenade") && !isInspecting && gm.GrenadeAvailable) 
		{
			StartCoroutine (GrenadeSpawnDelay ());
			//Play grenade throw animation
			anim.Play("GrenadeThrow", 0, 0.0f);
		}


			
		//AUtomatic fire
		//Left click hold 
		if (Input.GetButton ("Fire") && !outOfAmmo && !isReloading && !isInspecting && !isRunning && gm.PlayerObject.CanShoot) 
		{
			//Shoot automatic
			if (Time.time - lastFired > 1 / fireRate) 
			{
				lastFired = Time.time;

				//Remove 1 bullet from ammo
				currentAmmo -= 1;

				shootAudioSource.clip = SoundClips.shootSound;
				shootAudioSource.Play ();

				if (!isAiming) //if not aiming
				{
					anim.Play ("Fire", 0, 0f);
					//If random muzzle is false
					if (!randomMuzzleflash && 
						enableMuzzleflash == true) 
					{
						muzzleParticles.Emit (1);
						//Light flash start
						StartCoroutine(MuzzleFlashLight());
					} 
					else if (randomMuzzleflash == true)
					{
						//Only emit if random value is 1
						if (randomMuzzleflashValue == 1) 
						{
							if (enableSparks == true) 
							{
								//Emit random amount of spark particles
								sparkParticles.Emit (Random.Range (minSparkEmission, maxSparkEmission));
							}
							if (enableMuzzleflash == true) 
							{
								muzzleParticles.Emit (1);
								//Light flash start
								StartCoroutine (MuzzleFlashLight ());
							}
						}
					}
				} 
				else //if aiming
				{
					
					anim.Play ("Aim Fire", 0, 0f);

					//If random muzzle is false
					if (!randomMuzzleflash) {
						muzzleParticles.Emit (1);
					//If random muzzle is true
					} 
					else if (randomMuzzleflash == true) 
					{
						//Only emit if random value is 1
						if (randomMuzzleflashValue == 1) 
						{
							if (enableSparks == true) 
							{
								//Emit random amount of spark particles
								sparkParticles.Emit (Random.Range (minSparkEmission, maxSparkEmission));
							}
							if (enableMuzzleflash == true) 
							{
								muzzleParticles.Emit (1);
								//Light flash start
								StartCoroutine (MuzzleFlashLight ());
							}
						}
					}
				}

				//Spawn bullet from bullet spawnpoint
				var bullet = (Transform)Instantiate (
					Prefabs.bulletPrefab,
					Spawnpoints.bulletSpawnPoint.transform.position,
					Spawnpoints.bulletSpawnPoint.transform.rotation);

                bullet.GetComponent<BulletScript>().damage = bulletDamage;

				//Add velocity to the bullet
				bullet.GetComponent<Rigidbody>().velocity = 
					bullet.transform.forward * bulletForce;
				
				//Spawn casing prefab at spawnpoint
				Instantiate (Prefabs.casingPrefab, 
					Spawnpoints.casingSpawnPoint.transform.position, 
					Spawnpoints.casingSpawnPoint.transform.rotation);

                //If out of ammo
                if (currentAmmo == 0)
                {
                    //Show out of ammo text
                    //Toggle bool
                    outOfAmmo = true;
                    //Auto reload if true
                    if (autoReload == true && !isReloading && !outOfMags)
                    {
                        StartCoroutine(Reload(autoReloadDelay));
                    }
                }
            }
		}

		//Inspect weapon when T key is pressed
		if (Input.GetKeyDown (KeyCode.T)) 
		{
			anim.SetTrigger ("Inspect");
		}

		//Toggle weapon holster when E key is pressed
		//if (Input.GetKeyDown (KeyCode.E) && !hasBeenHolstered) 
		//{
		//	holstered = true;

		//	mainAudioSource.clip = SoundClips.holsterSound;
		//	mainAudioSource.Play();

		//	hasBeenHolstered = true;
		//} 
		//else if (Input.GetKeyDown (KeyCode.E) && hasBeenHolstered) 
		//{
		//	holstered = false;

		//	mainAudioSource.clip = SoundClips.takeOutSound;
		//	mainAudioSource.Play ();

		//	hasBeenHolstered = false;
		//}
		//Holster anim toggle
		if (holstered == true) 
		{
			anim.SetBool ("Holster", true);
		} 
		else 
		{
			anim.SetBool ("Holster", false);
		}

		//Reload 
		if (Input.GetButtonDown ("Reload") && !isReloading && !isInspecting && !outOfMags) 
		{
            //Reload
            StartCoroutine(Reload());
		}

		//Walking when pressing down WASD keys
		if (Input.GetAxisRaw ("Horizontal") > 0 && !isRunning ||
			Input.GetAxisRaw("Vertical") > 0 && !isRunning) 
		{
			anim.SetBool ("Walk", true);
		} else {
			anim.SetBool ("Walk", false);
		}

		////Running when pressing down W and Left Shift key
		//if ((Input.GetKey (KeyCode.W) && Input.GetKey (KeyCode.LeftShift)) && gm.PlayerObject.CanRun) 
		//{
		//	isRunning = true;
		//} else {
		//	isRunning = false;
		//}
		
		//Run anim toggle
		if (isRunning == true) 
		{
			anim.SetBool ("Run", true);
		} 
		else 
		{
			anim.SetBool ("Run", false);
		}
	}

	private IEnumerator GrenadeSpawnDelay () {
		
		//Wait for set amount of time before spawning grenade
		yield return new WaitForSeconds (grenadeSpawnDelay);
		//Spawn grenade prefab at spawnpoint
		Instantiate(Prefabs.grenadePrefab, 
			Spawnpoints.grenadeSpawnPoint.transform.position, 
			Spawnpoints.grenadeSpawnPoint.transform.rotation);
	}

	//Reload
	private IEnumerator Reload (float delay = 0) {
        yield return new WaitForSeconds(delay);
        if (outOfAmmo == true) 
		{
			//Play diff anim if out of ammo
			anim.Play ("Reload Out Of Ammo", 0, 0f);

			mainAudioSource.clip = SoundClips.reloadSoundOutOfAmmo;
			mainAudioSource.Play ();

			//If out of ammo, hide the bullet renderer in the mag
			//Do not show if bullet renderer is not assigned in inspector
			if (bulletInMagRenderer != null) 
			{
				bulletInMagRenderer.GetComponent
				<SkinnedMeshRenderer> ().enabled = false;
				//Start show bullet delay
				StartCoroutine (ShowBulletInMag ());
			}
		} 
		else 
		{
			//Play diff anim if ammo left
			anim.Play ("Reload Ammo Left", 0, 0f);

			mainAudioSource.clip = SoundClips.reloadSoundAmmoLeft;
			mainAudioSource.Play ();

			//If reloading when ammo left, show bullet in mag
			//Do not show if bullet renderer is not assigned in inspector
			if (bulletInMagRenderer != null) 
			{
				bulletInMagRenderer.GetComponent
				<SkinnedMeshRenderer> ().enabled = true;
			}
		}

        yield return new WaitForSeconds(outOfAmmo ? 3f : 2f);
        //Restore ammo when reloading
        int magsRed = ammo - currentAmmo;
        if(currentMags - magsRed >= 0)
        {
            currentAmmo = ammo;
            currentMags -= magsRed;
        }
        else
        {
            currentAmmo += currentMags;
            currentMags = 0;
            outOfMags = true;
        }
		
		outOfAmmo = false;
	}

	//Enable bullet in mag renderer after set amount of time
	private IEnumerator ShowBulletInMag () {
		
		//Wait set amount of time before showing bullet in mag
		yield return new WaitForSeconds (showBulletInMagDelay);
		bulletInMagRenderer.GetComponent<SkinnedMeshRenderer> ().enabled = true;
	}

	//Show light when shooting, then disable after set amount of time
	private IEnumerator MuzzleFlashLight () {
		
		muzzleflashLight.enabled = true;
		yield return new WaitForSeconds (lightDuration);
		muzzleflashLight.enabled = false;
	}

	//Check current animation playing
	private void AnimationCheck () {
		
		//Check if reloading
		//Check both animations
		if (anim.GetCurrentAnimatorStateInfo (0).IsName ("Reload Out Of Ammo") || 
			anim.GetCurrentAnimatorStateInfo (0).IsName ("Reload Ammo Left")) 
		{
			isReloading = true;
		} 
		else 
		{
			isReloading = false;
		}

		//Check if inspecting weapon
		if (anim.GetCurrentAnimatorStateInfo (0).IsName ("Inspect")) 
		{
			isInspecting = true;
		} 
		else 
		{
			isInspecting = false;
		}
	}

    public void AddMagazine(int amount)
    {
        currentMags += amount;
        outOfMags = false;
    }
}
// ----- Low Poly FPS Pack Free Version -----
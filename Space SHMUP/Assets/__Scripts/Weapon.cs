using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is an enum of the various possible weapon types.
/// It also includes a "shield" type to allow a shield PowerUp.
/// Items marked [NI] below are Not Implemented in ths book.
/// </summary>
 
public enum eWeaponType
{
    none,       // The default / no weapon
    blaster,    // A simple blaster
    spread,     // Multiple shots simultaneously
    phaser,     // [NI] Shots that move in waves
    missile,    // [NI] Homing missiles

    laser,     // [NI] Damage over time
    shield,      // Raise shieldLevel
    swivel  // [NI] acts like blaster but shoots nearest enemy with lower damage
}

/// <summary>
/// The WeaponDefinition class allows you to set the propreties
///     of a specific weapon in the Inspector. The Main class has
///     an array of WeaponDefinitions that makes this possible.
/// </summary>

[System.Serializable]
public class WeaponDefinition
{
    public eWeaponType type = eWeaponType.none;
    [Tooltip("Letter to show on the PowerUp Cube")]
    public string letter;
    [Tooltip("Color of PowerUp Cube")]
    public Color powerUpColor = Color.white;
    [Tooltip("Prefab of Weapon model tht is attached to the Player Ship")]
    public GameObject weaponModelPrefab;
    [Tooltip("Prefab of projectile that is fired")]
    public GameObject projectilePrefab;
    [Tooltip("Color of the Projectile that is fired")]
    public Color projectileColor = Color.white;
    [Tooltip("Damage caused when a single Projectile hits an Enemy")]
    public float damageOnHit = 0;
    [Tooltip("Damage caused per second by the Laser [NI]")]
    public float damagerPerSec = 0;
    [Tooltip("Seconds to delay between shots")]
    public float delayBetweenShots = 0;
    [Tooltip("Velocity of individual Projectiles")]
    public float velocity = 50f;
}

public class Weapon : MonoBehaviour
{
    static public Transform PROJECTILE_ANCHOR;

    [Header("Dynamic")]
    [SerializeField]
    [Tooltip("Setting this manually while playing does not work properly.")]
    private eWeaponType _type = eWeaponType.none;
    public WeaponDefinition def;
    public float nextShotTime; // Time the Weapon will fire next

    private GameObject weaponModel;
    private Transform shotPointTrans;

    private bool laserActive = false;
    private LineRenderer lineRenderer; // For visualizing the laser

    private void Start()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.startColor = Color.red;

        // Set up PROJECTILE_ANCHOR if it has not already been done
        if (PROJECTILE_ANCHOR == null)
        {
            GameObject go = new GameObject("_ProjectileAnchor");
            PROJECTILE_ANCHOR = go.transform;
        }

        shotPointTrans = transform.GetChild(0);

        // Call SetType() for the default _type set in the Inspector
        SetType(_type);

        // Find the fireEvent of a Hero Component in the parent hierarchy
        Hero hero = GetComponentInParent<Hero>();
        if (hero != null) hero.fireEvent += Fire;
    }

    public eWeaponType type
    {
        get { return (_type); }
        set { SetType(value); }
    }

    public void SetType(eWeaponType wt)
    {
        _type = wt;
        if(type == eWeaponType.none)
        {
            this.gameObject.SetActive(false);
            if (laserActive)
            {
                laserActive = false;
                if (lineRenderer != null) lineRenderer.enabled = false;
            }
            return;
        }
        else
        {
            this.gameObject.SetActive(true);
        }
        // Get the WeaponDefinition from this type from Main
        def = Main.GET_WEAPON_DEFINITION(_type);
        // Destroy any old model and then attach a model for this weapon
        if (weaponModel != null) Destroy(weaponModel);
        weaponModel = Instantiate<GameObject>(def.weaponModelPrefab, transform);
        weaponModel.transform.localPosition = Vector3.zero;
        weaponModel.transform.localScale = Vector3.one;

        nextShotTime = 0; // You can fire immediately after _type is set.
    }

    private void Fire()
    {
        // If this.gameObject is inactive, return
        if (!gameObject.activeInHierarchy) return;
        // If it hasn't been enough time between shots, return
        if (Time.time < nextShotTime) return;

        ProjectileHero p;
        Vector3 vel = Vector3.up * def.velocity;

        switch (type)
        {
            case eWeaponType.blaster:
                p = MakeProjectile();
                p.vel = vel;
                break;

            case eWeaponType.spread:
                p = MakeProjectile();
                p.vel = vel;
                p = MakeProjectile();
                p.transform.rotation = Quaternion.AngleAxis(10, Vector3.back);
                p.vel = p.transform.rotation * vel;
                p = MakeProjectile();
                p.transform.rotation = Quaternion.AngleAxis(-10, Vector3.back);
                p.vel = p.transform.rotation * vel;
                break;

            // Shoots two projectiles that move in a sin wave pattern, similar to the movement of Enemy_1
            case eWeaponType.phaser:
                //p = MakeProjectile();
                //p.transform.rotation = Quaternion.AngleAxis(5, Vector3.back);
                //p.vel = p.transform.rotation * vel;
                //p = MakeProjectile();
                //p.transform.rotation = Quaternion.AngleAxis(-5, Vector3.back);
                //p.vel = p.transform.rotation * vel;
                //break;

                float frequency = 20f; // Higher frequency for faster oscillation
                float amplitude = 100f;  // Horizontal amplitude of the wave

                // Create the first projectile
                p = MakeProjectile();
                p.vel = vel; // Forward velocity
                StartCoroutine(MoveInSineWave(p, frequency, amplitude, 1)); // 1 for positive direction

                // Create the second projectile
                p = MakeProjectile();
                p.vel = vel; // Forward velocity
                StartCoroutine(MoveInSineWave(p, frequency, amplitude, -1)); // -1 for negative direction
                break;

            // Instead of doing all of its damage at once, the laser does continuous damage over time
            case eWeaponType.laser:
                if (!laserActive)
                {
                    laserActive = true;
                    StartCoroutine(FireLaser());
                }
                break;

            // Missile has a lock-on mechanic that could track enemies and always hit
            case eWeaponType.missile:
                p = MakeProjectile();
                p.vel = vel;
                break;

            // Like the blaster but actually shoots at the nearest enemy. However, the damage would be really low
            case eWeaponType.swivel:
                p = MakeProjectile();
                p.vel = vel;
                break;
        }

    }

    private ProjectileHero MakeProjectile()
    {
        GameObject go;
        go = Instantiate<GameObject>(def.projectilePrefab, PROJECTILE_ANCHOR);
        ProjectileHero p = go.GetComponent<ProjectileHero>();

        Vector3 pos = shotPointTrans.position;
        pos.z = 0;
        p.transform.position = pos;

        p.type = type;
        nextShotTime = Time.time + def.delayBetweenShots;
        return (p);
    }


    private IEnumerator MoveInSineWave(ProjectileHero projectile, float frequency, float amplitude, int direction)
    {
        float birthTime = Time.time; // Track when the projectile was created

        while (projectile != null && projectile.gameObject.activeInHierarchy)
        {
            // Calculate time elapsed since the projectile's creation
            float age = Time.time - birthTime;

            // Compute the sine wave offset
            float offsetX = Mathf.Sin(age * frequency) * amplitude * direction;

            // Update the projectile's position
            Vector3 pos = projectile.transform.position;
            pos.x += offsetX * Time.deltaTime;
            projectile.transform.position = pos;

            yield return null; // Wait for the next frame
        }
    }


    private IEnumerator FireLaser()
    {
        lineRenderer.enabled = true;

        while (laserActive)
        {
            if (Input.GetAxis("Jump") != 1)
            {
                Debug.Log("Space released");
                break;
            }
            else if (Input.touchCount < 2 && Input.touchCount > 0)
            {
                Debug.Log("Touch removed");
                break;
            }

            // Start the laser at the weapon's position
            lineRenderer.SetPosition(0, shotPointTrans.position);

            // Always fire the laser straight up
            RaycastHit hit;
            int layerMask = ~LayerMask.GetMask("ProjectileHero", "PowerUp"); // Exclude projectiles and powerups
            if (Physics.Raycast(shotPointTrans.position, Vector3.up, out hit, 100f, layerMask))
            {
                lineRenderer.SetPosition(1, hit.point);

                // Deal damage to enemy
                Enemy e = hit.collider.GetComponent<Enemy>();
                if (e != null)
                {
                    e.TakeDamage(def.damagerPerSec * Time.deltaTime);

                    // Trigger blink effect
                    BlinkColorOnHit blink = e.GetComponent<BlinkColorOnHit>();
                    if (blink != null)
                    {
                        blink.HitByLaser();
                    }
                }
            }
            else
            {
                lineRenderer.SetPosition(1, shotPointTrans.position + Vector3.up * 100f);
            }

            yield return null; // Wait until the next frame
        }

        // Disable the laser when no longer active
        lineRenderer.enabled = false;
        laserActive = false;
    }













}

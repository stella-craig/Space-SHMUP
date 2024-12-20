using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoundsCheck))]

public class ProjectileHero : MonoBehaviour
{
    private BoundsCheck bndCheck;
    private Renderer rend;

    [Header("Dynamic")]
    public Rigidbody rigid;
    [SerializeField]
    private eWeaponType _type;

    [Header("Missile Attributes")]
    public bool homingEnabled = false;
    public Transform target; // The current target for the missile
    public float homingStrength = 5f; // Determines how quickly the missile adjusts its direction

    // This pubic property masks the private field _type
    public eWeaponType type
    {
        get { return (_type); }
        set { SetType(value); }
    }

    void Awake()
    {
        bndCheck = GetComponent<BoundsCheck>();
        rend = GetComponent<Renderer>();
        rigid = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if(homingEnabled)
    {
            if (target == null || !target.gameObject.activeInHierarchy)
            {
                // Disable homing if the target is no longer valid
                homingEnabled = false;
            }
            else
            {
                // Continue homing toward the target
                Vector3 direction = (target.position - transform.position).normalized;
                Vector3 newVelocity = Vector3.Lerp(rigid.velocity, direction * rigid.velocity.magnitude, Time.deltaTime * homingStrength);
                rigid.velocity = newVelocity;

                float angle = Mathf.Atan2(newVelocity.y, newVelocity.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90));
            }
        }

        if (bndCheck.LocIs(BoundsCheck.eScreenLocs.offUp))
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Sets the _type private field and colors this projectile to watch the
    ///     WeaponDefinition.
    /// </summary>
    /// <param name="eType">Te eWeaponType to use.</param>
    public void SetType (eWeaponType eType)
    {
        _type = eType;
        WeaponDefinition def = Main.GET_WEAPON_DEFINITION(_type);
        rend.material.color = def.projectileColor;
    }

    /// <summary>
    /// Allows Weapon to easily set the velocity of this ProjectileHero
    /// </summary>
    public Vector3 vel
    {
        get { return rigid.velocity; }
        set { rigid.velocity = value; }
    }


}

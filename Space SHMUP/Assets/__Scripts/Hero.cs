using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Hero : MonoBehaviour
{
    public static Hero S {  get; private set; } // Singleton property

    [Header("Inscribed")]
    // These fields control the movement of the ship
    public float speed = 30;
    public float rollMult = -45;
    public float pitchMult = 30;

    [Header("Dynamic")][Range(0,4)]
    pubic float shieldLevel = 1;

    void Awake()
    {
        if (S == null)
        {
            S = this; // Set the Singleton only if it's null
        } else
        {
            Debug.LogError("Hero.Awake() - Attempted to assign second Hero.S!");
        }
    }

    void Update()
    {
        // Pull in information from the Input class
        float hAxis = Input.GetAxis("Horizontal");
        float vAxis = Input.GetAxis("Vertical");

        // Change transform.position based on the axes
    }

}

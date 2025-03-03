using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class Key : MonoBehaviour
{
    public float rotationSpeedX = 30f;
    public float rotationSpeedY = 60f;

    void Update()
    {
        transform.Rotate(rotationSpeedX * Time.deltaTime, rotationSpeedY * Time.deltaTime, 0f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            CollectKey();
        }
    }

    void CollectKey()
    {
        // Destroy the key itself
        Destroy(gameObject);

        // Destroy all objects with the "Bars" tag
        GameObject[] bars = GameObject.FindGameObjectsWithTag("Bars");
        foreach (GameObject bar in bars)
        {
            Destroy(bar);
        }

        Prisoner.keyCollected = true; // Set the static flag in Prisoner script
    }
}
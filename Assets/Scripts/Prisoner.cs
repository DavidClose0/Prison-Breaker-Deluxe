using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Prisoner : MonoBehaviour
{
    public GameObject player; // Reference to the Player GameObject
    public float rotationSpeed = 5f; // Speed of rotation towards the player
    public static bool keyCollected = false; // Static flag to track key collection

    private NavMeshAgent navMeshAgent;
    private GameObject endTarget; // GameObject with the "End" tag

    void Start()
    {
        keyCollected = false;

        navMeshAgent = GetComponent<NavMeshAgent>();

        player = GameObject.FindGameObjectWithTag("Player");
        endTarget = GameObject.FindGameObjectWithTag("End");
    }

    void Update()
    {
        if (!keyCollected)
        {
            // Default state: Face the player
            FacePlayer();
            if (navMeshAgent != null && navMeshAgent.enabled)
            {
                navMeshAgent.isStopped = true; // Stop NavMeshAgent if it's moving
            }
        }
        else
        {
            // Key collected state: Move to the "End" target
            MoveToEnd();
            if (navMeshAgent != null && navMeshAgent.enabled)
            {
                navMeshAgent.isStopped = false; // Allow NavMeshAgent to move
            }
        }
    }

    void FacePlayer()
    {
        if (player != null)
        {
            // Calculate direction to player
            Vector3 directionToPlayer = player.transform.position - transform.position;
            directionToPlayer.y = 0; // Keep rotation horizontal

            if (directionToPlayer != Vector3.zero) // Avoid errors if prisoner and player are at the same position
            {
                // Create the rotation to look at the player
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);

                // Smoothly rotate towards the player using Slerp
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    void MoveToEnd()
    {
        if (endTarget != null && navMeshAgent != null && navMeshAgent.enabled)
        {
            navMeshAgent.destination = endTarget.transform.position;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("End"))
        {
            Destroy(gameObject); // Destroy the prisoner
        }
    }
}
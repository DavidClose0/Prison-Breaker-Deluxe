using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Guard : MonoBehaviour
{
    public GameObject[] path;
    public GameObject player;
    public Material pursueMaterial;
    public Color pursueSpotlightColor = Color.red;
    public float detectionDistance = 10f;
    public float detectionAngle = 45f; // Increased for easier testing, revert to 15f if needed
    public float patrolSpeed = 1.5f;
    public float pursueSpeed = 3.5f;
    public float returnToPatrolDelay = 5f; // Delay before returning to patrol

    private NavMeshAgent navMeshAgent;
    private Light spotLight;
    private int currentPathIndex = 0;
    private bool isPursuingTarget = false; // Renamed from isChasingPlayer
    private float timeSinceLastSeenTarget = 0f; // Renamed from timeSinceLastSeenPlayer
    private Material originalMaterial;
    private Color originalSpotlightColor;
    private GameManager gameManager; // Reference to GameManager
    private GameObject currentTarget; // Track the current target being pursued

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        spotLight = GetComponentInChildren<Light>();
        originalMaterial = GetComponent<MeshRenderer>().material;
        originalSpotlightColor = spotLight.color;
        gameManager = FindFirstObjectByType<GameManager>(); // Find GameManager in the scene

        if (gameManager == null)
        {
            Debug.LogError("GameManager not found in the scene. Make sure you have a GameManager GameObject with GameManager script attached.");
        }

        if (path.Length > 0)
        {
            navMeshAgent.speed = patrolSpeed;
            navMeshAgent.destination = path[currentPathIndex].transform.position;
        }
        else
        {
            Debug.LogWarning("Path is not assigned to the Guard.");
        }
    }

    void Update()
    {
        if (player == null) return;

        if (!isPursuingTarget) // Only look for new targets if not already pursuing
        {
            GameObject detectedTarget = DetectTarget();
            if (detectedTarget != null)
            {
                StartPursuingTarget(detectedTarget);
            }
        }

        if (isPursuingTarget)
        {
            if (currentTarget != null)
            {
                navMeshAgent.destination = currentTarget.transform.position; // Update chase target every frame
            }
            timeSinceLastSeenTarget = 0f; // Reset timer as target is still in sight

            if (!IsTargetVisible())
            {
                // Target is out of sight, start timer to return to patrol
                timeSinceLastSeenTarget += Time.deltaTime;
                if (timeSinceLastSeenTarget >= returnToPatrolDelay)
                {
                    StopPursuingTarget();
                }
            }
        }
        else
        {
            // Patrolling logic
            if (path.Length > 0 && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance && !navMeshAgent.pathPending)
            {
                currentPathIndex = (currentPathIndex + 1) % path.Length;
                navMeshAgent.destination = path[currentPathIndex].transform.position;
            }
        }
    }

    GameObject DetectTarget()
    {
        // Check for Player
        Vector3 directionToPlayer = player.transform.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;
        float angleToPlayer = Vector3.Angle(directionToPlayer, transform.forward);

        if (distanceToPlayer <= detectionDistance && angleToPlayer <= detectionAngle)
        {
            return player;
        }

        // Check for Prisoners if they can escape
        if (Prisoner.keyCollected)
        {
            GameObject[] prisoners = GameObject.FindGameObjectsWithTag("Prisoner");
            foreach (GameObject prisoner in prisoners)
            {
                Vector3 directionToPrisoner = prisoner.transform.position - transform.position;
                float distanceToPrisoner = directionToPrisoner.magnitude;
                float angleToPrisoner = Vector3.Angle(directionToPrisoner, transform.forward);

                if (distanceToPrisoner <= detectionDistance && angleToPrisoner <= detectionAngle)
                {
                    return prisoner;
                }
            }
        }

        return null; // No target detected
    }

    bool IsTargetVisible()
    {
        if (currentTarget == null) return false;

        Vector3 directionToTarget = currentTarget.transform.position - transform.position;
        float distanceToTarget = directionToTarget.magnitude;
        float angleToTarget = Vector3.Angle(directionToTarget, transform.forward);

        return (distanceToTarget <= detectionDistance && angleToTarget <= detectionAngle);
    }


    void StartPursuingTarget(GameObject target)
    {
        isPursuingTarget = true;
        currentTarget = target;
        navMeshAgent.speed = pursueSpeed;
        GetComponent<MeshRenderer>().material = pursueMaterial;
        spotLight.color = pursueSpotlightColor;
    }

    void StopPursuingTarget()
    {
        isPursuingTarget = false;
        currentTarget = null;
        timeSinceLastSeenTarget = 0f;
        navMeshAgent.speed = patrolSpeed;
        GetComponent<MeshRenderer>().material = originalMaterial;
        spotLight.color = originalSpotlightColor;
        if (path.Length > 0)
        {
            navMeshAgent.destination = path[currentPathIndex].transform.position; // Resume patrol from current waypoint
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (gameManager != null)
            {
                gameManager.EndGame();
            }
            else
            {
                Debug.LogError("GameManager reference is null in Guard script. Ensure GameManager is properly set up.");
            }
        }
        else if (collision.gameObject.CompareTag("Prisoner"))
        {
            Destroy(collision.gameObject); // Destroy the prisoner
            StopPursuingTarget(); // Return to patrol after destroying prisoner
        }
        else if (collision.gameObject.CompareTag("End"))
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("End"))
        {
            Destroy(gameObject); // Destroy the guard
        }
    }

    // Optional: For debugging purposes, draw the detection angle in Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 rightRayDirection = Quaternion.AngleAxis(detectionAngle, Vector3.up) * transform.forward * detectionDistance;
        Vector3 leftRayDirection = Quaternion.AngleAxis(-detectionAngle, Vector3.up) * transform.forward * detectionDistance;

        Gizmos.DrawRay(transform.position, transform.forward * detectionDistance);
        Gizmos.DrawRay(transform.position, rightRayDirection);
        Gizmos.DrawRay(transform.position, leftRayDirection);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionDistance);
    }
}
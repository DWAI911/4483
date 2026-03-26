using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// The main enemy AI that hunts the player.
/// Unity 2022.3.62f1 compatible.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class AIChaser : MonoBehaviour
{
    public enum AIState
    {
        Patrol,
        Investigating,
        Chasing,
        Searching,
        Stunned
    }

    [Header("Movement Settings")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4.5f;
    [SerializeField] private float investigateSpeed = 3f;

    [Header("Detection Settings")]
    [SerializeField] private float hearingRange = 30f;
    [SerializeField] private float visionRange = 15f;
    [SerializeField] private float visionAngle = 90f;
    [SerializeField] private LayerMask visionObstructionMask;
    [SerializeField] private Transform eyePosition;

    [Header("Patrol Settings")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float waitAtPatrolPoint = 3f;

    [Header("Chase Settings")]
    [SerializeField] private float losePlayerTime = 5f;
    [SerializeField] private float searchRadius = 10f;

    [Header("Stun Settings")]
    [SerializeField] private float stunDuration = 3f;
    [SerializeField] private GameObject stunEffect;

    [Header("Audio")]
    [SerializeField] private AudioClip footstepSound;
    [SerializeField] private float footstepInterval = 0.5f;

    // Components
    private NavMeshAgent agent;
    private AudioSource audioSource;

    // State
    private AIState currentState = AIState.Patrol;
    private int currentPatrolIndex = 0;
    private float patrolWaitTimer = 0f;
    private Vector3 lastKnownPlayerPosition;
    private float losePlayerTimer = 0f;
    private float stunTimer = 0f;
    private float footstepTimer = 0f;

    // References
    private PlayerController player;
    private PlayerInteract playerInteract;

    // Events
    public event System.Action<AIState> OnStateChanged;
    public static event System.Action OnPlayerCaught;

    public AIState CurrentState => currentState;
    public bool IsStunned => currentState == AIState.Stunned;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Start()
    {
        player = FindObjectOfType<PlayerController>();
        playerInteract = player?.GetComponent<PlayerInteract>();
        
        if (player != null)
        {
            player.OnNoiseGenerated += OnPlayerNoise;
        }

        SetState(AIState.Patrol);
    }

    private void OnDestroy()
    {
        if (player != null)
        {
            player.OnNoiseGenerated -= OnPlayerNoise;
        }
    }

    private void Update()
    {
        switch (currentState)
        {
            case AIState.Patrol:
                UpdatePatrol();
                break;
            case AIState.Investigating:
                UpdateInvestigate();
                break;
            case AIState.Chasing:
                UpdateChase();
                break;
            case AIState.Searching:
                UpdateSearch();
                break;
            case AIState.Stunned:
                UpdateStunned();
                break;
        }

        UpdateFootsteps();
    }

    #region State Updates
    private void UpdatePatrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        if (patrolWaitTimer > 0)
        {
            patrolWaitTimer -= Time.deltaTime;
            return;
        }

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            patrolWaitTimer = waitAtPatrolPoint;
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }

        if (CheckForPlayer())
        {
            SetState(AIState.Chasing);
        }
    }

    private void UpdateInvestigate()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            SetState(AIState.Searching);
        }

        if (CheckForPlayer())
        {
            SetState(AIState.Chasing);
        }
    }

    private void UpdateChase()
    {
        if (player != null)
        {
            agent.SetDestination(player.Position);
            lastKnownPlayerPosition = player.Position;
        }

        if (CanSeePlayer())
        {
            losePlayerTimer = 0f;
        }
        else
        {
            losePlayerTimer += Time.deltaTime;
            
            if (losePlayerTimer >= losePlayerTime)
            {
                SetState(AIState.Searching);
            }
        }
    }

    private void UpdateSearch()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            Vector3 randomPoint = lastKnownPlayerPosition + Random.insideUnitSphere * searchRadius;
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, searchRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }

        if (CheckForPlayer())
        {
            SetState(AIState.Chasing);
        }
    }

    private void UpdateStunned()
    {
        stunTimer -= Time.deltaTime;
        
        if (stunTimer <= 0)
        {
            SetState(AIState.Searching);
            if (stunEffect != null)
            {
                stunEffect.SetActive(false);
            }
        }
    }
    #endregion

    #region Detection
    private bool CheckForPlayer()
    {
        if (playerInteract != null && playerInteract.IsHiding) return false;
        return CanSeePlayer();
    }

    private bool CanSeePlayer()
    {
        if (player == null) return false;
        if (eyePosition == null) return false;

        float distanceToPlayer = Vector3.Distance(eyePosition.position, player.Position);
        
        if (distanceToPlayer > visionRange) return false;

        Vector3 directionToPlayer = (player.Position - eyePosition.position).normalized;
        float angle = Vector3.Angle(eyePosition.forward, directionToPlayer);
        
        if (angle > visionAngle * 0.5f) return false;

        if (Physics.Linecast(eyePosition.position, player.Position, visionObstructionMask))
        {
            return false;
        }

        return true;
    }

    private void OnPlayerNoise(float noiseRadius)
    {
        if (currentState == AIState.Stunned) return;
        if (playerInteract != null && playerInteract.IsHiding) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.Position);
        
        if (distanceToPlayer <= hearingRange + noiseRadius)
        {
            Vector3 noiseOrigin = player.Position;

            if (currentState == AIState.Patrol)
            {
                lastKnownPlayerPosition = noiseOrigin;
                SetState(AIState.Investigating);
                agent.SetDestination(noiseOrigin);
            }
            else if (currentState == AIState.Investigating || currentState == AIState.Searching)
            {
                if (distanceToPlayer < hearingRange * 0.5f)
                {
                    SetState(AIState.Chasing);
                }
                else
                {
                    agent.SetDestination(noiseOrigin);
                }
            }
        }
    }
    #endregion

    #region State Management
    private void SetState(AIState newState)
    {
        if (currentState == newState) return;

        currentState = newState;
        OnStateChanged?.Invoke(newState);

        switch (currentState)
        {
            case AIState.Patrol:
                agent.speed = patrolSpeed;
                break;
            case AIState.Investigating:
                agent.speed = investigateSpeed;
                break;
            case AIState.Chasing:
                agent.speed = chaseSpeed;
                break;
            case AIState.Searching:
                agent.speed = investigateSpeed;
                break;
            case AIState.Stunned:
                agent.speed = 0f;
                agent.isStopped = true;
                break;
        }

        if (currentState != AIState.Stunned)
        {
            agent.isStopped = false;
        }
    }
    #endregion

    #region Stun
    public void Stun(float duration)
    {
        stunTimer = duration;
        SetState(AIState.Stunned);
        
        if (stunEffect != null)
        {
            stunEffect.SetActive(true);
        }
    }
    #endregion

    #region Audio
    private void UpdateFootsteps()
    {
        if (currentState == AIState.Stunned || !agent.hasPath) return;

        footstepTimer -= Time.deltaTime;
        
        if (footstepTimer <= 0)
        {
            footstepTimer = footstepInterval / (agent.speed / patrolSpeed);
            
            if (footstepSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(footstepSound, 0.5f);
            }
        }
    }
    #endregion

    #region Catch Player
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && currentState != AIState.Stunned)
        {
            OnPlayerCaught?.Invoke();
            Debug.Log("CAUGHT THE PLAYER!");
        }
    }
    #endregion

    #region Gizmos
    private void OnDrawGizmosSelected()
    {
        if (eyePosition != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 leftBoundary = Quaternion.Euler(0, -visionAngle * 0.5f, 0) * eyePosition.forward;
            Vector3 rightBoundary = Quaternion.Euler(0, visionAngle * 0.5f, 0) * eyePosition.forward;
            
            Gizmos.DrawRay(eyePosition.position, leftBoundary * visionRange);
            Gizmos.DrawRay(eyePosition.position, rightBoundary * visionRange);
            Gizmos.DrawRay(eyePosition.position, eyePosition.forward * visionRange);
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, hearingRange);

        if (patrolPoints != null)
        {
            Gizmos.color = Color.green;
            foreach (Transform point in patrolPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawSphere(point.position, 0.3f);
                }
            }
        }
    }
    #endregion
}

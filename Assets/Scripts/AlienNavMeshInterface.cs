using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AlienNavMeshInterface : MonoBehaviour {

    public NavMeshObstacle nmo;
    public float startDistanceFromPlayer;
    private SmallAlienHealth health;
    public GameObject SmallAlienNavmeshAgentPrefab;
    public NavMeshAgent _SmallAlienAgent;
    public float stoppingDistFromTarget = 3f;
    public float attackDistance = 2f;
    private Animator anims;
    public bool usingNavmeshAgent;
    private bool trackingTarget = false;
    private GameObject trackedTarget = null;
    private Vector3 lastTargetPos;
    public GameObject target;
    SmallAlienAI ai;
    SmallAlienPhysicsManager AlienPhysics;
    private bool isInRangeOfTarget;
    private bool walking;
    private bool reachedTarget = false;
    private float walkSpeed;
    public float walkAcceleration = 2f;
    
    public float forwardRaycastFrequency = 1f;
    public float forwardRaycastLength = 2f;
    public AnimationCurve SlowingFalloff;
    public LayerMask forwardRaycastMask;
    private bool lookingForward;
    public bool ObjectInWay = false;
    public float NoJumpThresholdDist = 2f;

    public Vector3 currentAttackPosition;
    public bool attacking;
    public bool isInAttackPosition = false;

	// Adding for dummy to work
	public bool attackingPlayer = false;

    public void ToggleNavmeshAgent (bool useNavMesh)
    {
        if (useNavMesh)
        {
            lookingForward = true;
            usingNavmeshAgent = true;
            _SmallAlienAgent.enabled = true;
            Invoke("FindAttackPosition", 1f);
            _SmallAlienAgent.transform.rotation = Quaternion.LookRotation(transform.forward);
        }
        else if (!useNavMesh)
        {
            lookingForward = false;
            usingNavmeshAgent = false;
            //_SmallAlienAgent.enabled = false;
            //_SmallAlienAgent.gameObject.SetActive(false);
        }
    }


   
    


	// Use this for initialization
	void Start ()
    {
        nmo = transform.FindChild("SmallAlienMesh").GetComponent<NavMeshObstacle>();
        health = gameObject.GetComponent<SmallAlienHealth>();
        AlienPhysics = gameObject.GetComponent<SmallAlienPhysicsManager>();
        ai = gameObject.GetComponent<SmallAlienAI>();
        walkSpeed = 1f;
        GameObject SmallAlienNavmeshAgent = (GameObject)Instantiate(SmallAlienNavmeshAgentPrefab, transform.position, transform.rotation);
        _SmallAlienAgent = SmallAlienNavmeshAgent.GetComponent<NavMeshAgent>();
        anims = gameObject.GetComponent<Animator>();
        ToggleNavmeshAgent(true);

        target = GameObject.Find("PlayerSpawnLocation");
        startDistanceFromPlayer = Vector3.Distance(transform.position, target.transform.position);
        FindAttackPosition();

		// Added by Matt
		attackingPlayer = true;

        /*
        //debug target
       target = GameObject.Find("debugNavmeshtarget");
        SetNavMeshAgentDestination(target.transform.position, true, target);
        */    
	}
	
    public void SetNavMeshAgentDestination(Vector3 target, bool track = false, GameObject targetGO = null)
    {
        NavMeshHit hit;
        Debug.Log("SetDestination");
        if (NavMesh.SamplePosition(target, out hit, 20, NavMesh.AllAreas))
        {
            _SmallAlienAgent.SetDestination(hit.position);
            lastTargetPos = hit.position;
        }

        if (track && targetGO != null)
        {
            trackingTarget = true;
            trackedTarget = targetGO;
        }
        else
        {
            StopTrackingTarget();   
        }
    }

    void StopTrackingTarget()
    {
        trackingTarget = false;
        trackedTarget = null;
    }

    void FollowAnimation()
    {
        if (usingNavmeshAgent && !isInRangeOfTarget && !attacking)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(_SmallAlienAgent.transform.forward), Time.deltaTime * 30f);
        }
        transform.Translate(anims.deltaPosition, Space.World);


    }

    private void OnAnimatorMove()
    {
        
        if (usingNavmeshAgent)
        {
            //Debug.Log((anims.deltaPosition * Time.deltaTime).magnitude);
            //_SmallAlienAgent.speed = (anims.deltaPosition / Time.deltaTime).magnitude;

            //_SmallAlienAgent.transform.Translate(anims.deltaPosition * Time.deltaTime);

        }
        else if (_SmallAlienAgent != null)
        {
            //_SmallAlienAgent.transform.position = transform.position;
        }
    }
    
    private void TrackTarget()
    {
        if(Vector3.Distance(trackedTarget.transform.position, lastTargetPos) > 0.25f)
        {
            SetNavMeshAgentDestination(trackedTarget.transform.position, true, trackedTarget);
            lastTargetPos = trackedTarget.transform.position;
        }
    }



    bool InRangeOfTarget()
    {
		// Vector3.Distance(lastTargetPos, transform.position)
		if(Vector3.Distance(target.transform.position, transform.position) < stoppingDistFromTarget)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    void UpdateWalkSpeed()
    {
        float speed = Mathf.Lerp(anims.GetFloat("Speed"), walkSpeed, Time.deltaTime * walkAcceleration);
        anims.SetFloat("Speed", speed);
    }


    void ReachedTarget()
    {
        walkSpeed = 0;
        anims.SetTrigger("Attacking");
        anims.SetTrigger("JumpForward");
        //ai.SetState(SmallAlienAI.States.Attacking);
    }

    public bool CheckIfReachedAttackPosition()
    {
        if (Vector3.Distance(transform.position, target.transform.position) <= attackDistance)
        {
            isInAttackPosition = true;
            currentAttackPosition = transform.position;
            //lastTargetPos = transform.position;
            return true;
        }
        else
        {
            isInAttackPosition = false;
            return false;
        }
    }

    public bool FindAttackPosition()
    {
        Vector3 foundPosition;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(target.transform.position, out hit, 4.0f, NavMesh.AllAreas))
        {
            foundPosition = hit.position;
        }
        else
        {
            foundPosition = Vector3.zero;
        }

      


        SetNavMeshAgentDestination(foundPosition);
        currentAttackPosition = foundPosition;

        if (!ObjectInWay)
        {
            return true;
        }
        else if(foundPosition == Vector3.zero)
        {
            Debug.Log("false");
            return false;
        }
        else
        {
            return false;
        }
    }


    void JumpToAttackPosition()
    {
        anims.SetTrigger("JumpForwardDefault");
    }


    // Update is called once per frame
    void Update ()
    {

        if (ObjectInWay && !attacking)
        {
            walkSpeed = 0.76f;
        }
        else if(!attacking)
        {
            walkSpeed = 1;
        }

        _SmallAlienAgent.transform.position = transform.position;

        if(Mathf.Abs(anims.GetFloat("Speed") - walkSpeed) > 0.01f)
        {
            UpdateWalkSpeed();
        }
        else
        {
            anims.SetFloat("Speed", walkSpeed);
        }


        if (!AlienPhysics.reactingToPhysics && !health.dead)
        {

            FollowAnimation();

            if (!InRangeOfTarget())
            {
                isInRangeOfTarget = false;
                if (usingNavmeshAgent)
                {
                    walking = true;
                }

                if (trackingTarget && usingNavmeshAgent)
                {
                    TrackTarget();
                }
            }
            else if (isInRangeOfTarget == false)
            {
                isInRangeOfTarget = true;
                if (!ObjectInWay)
                {
                    _SmallAlienAgent.transform.rotation = Quaternion.LookRotation(target.transform.position - transform.position);
                    JumpToAttackPosition();
                    nmo.enabled = true;
                }
            }

            if (attacking)
            {
                if (CheckIfReachedAttackPosition())
                {
                    isInAttackPosition = true;
                }
                else
                {
                    isInAttackPosition = false;

                }

                
                    Vector3 lookRotation = (Vector3.Scale(target.transform.position, new Vector3(1, 0, 1)) - transform.position);
                    lookRotation.Set(lookRotation.x, 0, lookRotation.z);
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookRotation), Time.deltaTime * 3f);
                
            }
            else
            {
                isInAttackPosition = false;
            }


        }

        

        

        if (isInRangeOfTarget && !ObjectInWay)
        {
            Vector3 lookRotation = (target.transform.position - transform.position);
            lookRotation.Set(lookRotation.x, 0, lookRotation.z);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookRotation), Time.deltaTime);
        }

        

        if (isInRangeOfTarget && !attacking)
        {
            if (CheckIfReachedAttackPosition())
            {
                attacking = true;
                anims.SetTrigger("StartAttacking");
                anims.SetBool("Attacking", true);
                nmo.enabled = true;
                walkSpeed = 0;
                ai.SetState(SmallAlienAI.States.Attacking);
            }
            else
            {
                
            }
        }
    }



}
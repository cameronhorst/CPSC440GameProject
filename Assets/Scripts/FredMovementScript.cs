using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FredMovementScript : MonoBehaviour {

    public Transform[] Waypoints;
    private FredWaypoint[] FredWaypoints;
    public float distToReachWaypoint = 3f;
    private Animator FredAnims;
    private enum State {Wandering, Fetching};
    private State currentState;
    private FredWaypoint currentWaypoint;
    private bool readyToMove;
    private bool busy;
    List<FredWaypoint> reachedWaypoints = new List<FredWaypoint>();

    public float AccelerationTime = 3;
    public float maxDistPerSec = 2;
    public AnimationCurve AccelerationCurve;
    public float TurnTime = 3;
    public float turnSpeed = 3;
    public float StoppingTime = 1;
    FredWaypoint lastWaypoint = null;
    private bool interupted = false;
    public float scanStopDistance = 1.5f;
    public Animator ScannerAnims;
    public List<GemGroupScript> GemsToCollect = new List<GemGroupScript>();
    private bool fetchingGems;
    private float currentSpeedMovingBetweenWaypoints = 0;
    private FredWaypoint currentTargetWaypoint;
    public float turningDistFromWaypoint = 4f;
    public Transform gemCollector;
    public float gemPickUpTime = 3f;
    public float PickUpInForce = 3f;
    public float PickUpUpForce = 12f;
    public GameObject TractorBeam;
    public PlayerManager playerManager;


    // Use this for initialization
    void Start ()
    {
        TractorBeam.SetActive(false);
        FredAnims = gameObject.GetComponent<Animator>();
        FredWaypoints = new FredWaypoint[Waypoints.Length];
        for(int i = 0; i < Waypoints.Length; i++)
        {
            FredWaypoints[i] = Waypoints[i].GetComponent<FredWaypoint>();
        }

        currentState = State.Wandering;
        readyToMove = true;
        StartCoroutine(Wandering());
	}
	
    public void ScanGem(GemGroupScript gemGroup)
    {
        GemsToCollect.Add(gemGroup);
        if (!fetchingGems)
        {
            fetchingGems = true;
            StopAllCoroutines();
            StartCoroutine(Fetching());
            ScannerAnims.SetTrigger("StopScanning");
        }
    }

    IEnumerator Fetching()
    {
        while(GemsToCollect.Count > 0)
        {
            GemGroupScript currentTargetGem;

            //Choose gem to get
            if(GemsToCollect.Count == 1)
            {
                currentTargetGem = GemsToCollect[0];
            }
            else
            {
                float bestDistance = 1000;
                GemGroupScript bestGemGroup = null;
                for (int i = 0; i < GemsToCollect.Count; i++)
                {
                    float distance = Vector3.Distance(transform.position, GemsToCollect[i].transform.position);
                    if(distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestGemGroup = GemsToCollect[i];
                    }       
                }

                if(bestGemGroup == null)
                {

                    yield break;
                }
                else
                {
                    currentTargetGem = bestGemGroup;
                }

            }

            //Found Gem, Going to get it
            //If was already moving, keep momentum

            FredWaypoint nextWaypoint;
            float speed = 0;
            float startMoveTime = Time.time;
            float distanceToNextWaypoint = 100;
            float startTurnTime = Time.time;
            float startSpeed = 0;
            if (currentTargetWaypoint != null && currentSpeedMovingBetweenWaypoints > 0)
            {
                float startSlowTime = Time.time;
                while (Time.time < startSlowTime + 1.5f)
                {
                    float currentSpeed = Mathf.Lerp(currentSpeedMovingBetweenWaypoints, 0, (Time.time - startSlowTime) / 1.5f);
                    transform.Translate(transform.forward * currentSpeed * Time.deltaTime, Space.World);


                    yield return null;
                }
                nextWaypoint = null;
                currentTargetWaypoint = null;
                currentSpeedMovingBetweenWaypoints = 0;
            }
            else if(currentTargetGem.GemLocationWaypoint != currentWaypoint && currentWaypoint != null)
            {
                nextWaypoint = currentWaypoint.ConnectedWaypointClosestTo(currentTargetGem.GemLocationWaypoint);
            }
            else
            {
                //at target waypoint
                nextWaypoint = null;
            }

            Vector3 _waypointTarget = currentTargetGem.GemLocationWaypoint.transform.position;
            Vector3 _gemTarget = currentTargetGem.transform.position;
            _gemTarget.Set(_gemTarget.x, transform.position.y, _gemTarget.z);

            //if not already at target waypoint
            if (nextWaypoint != null)
            {
                //TurnBeforeMoving
                startTurnTime = Time.time;
                Quaternion startRotation = transform.rotation;
                while (Time.time < startTurnTime + (TurnTime / 2f))
                {
                    transform.rotation = Quaternion.Lerp(startRotation, Quaternion.LookRotation(nextWaypoint.transform.position - transform.position), (Time.time - startTurnTime) / (TurnTime / 2f));
                    yield return null;
                }

                startMoveTime = Time.time;
                FredWaypoint _lastwaypoint = currentWaypoint;
                startRotation = transform.rotation;

                //while still finding target waypoint
                while (nextWaypoint != currentTargetGem.GemLocationWaypoint)
                {
                    if(Time.time < startTurnTime + TurnTime)
                    {
                        transform.rotation = Quaternion.Lerp(startRotation, Quaternion.LookRotation(nextWaypoint.transform.position - transform.position), (Time.time - startTurnTime)/TurnTime);
                    }
                    speed = Mathf.Lerp(0, maxDistPerSec, AccelerationCurve.Evaluate((Time.time - startMoveTime) / AccelerationTime));
                    transform.Translate(transform.forward * speed * Time.deltaTime, Space.World);
                    distanceToNextWaypoint = Vector3.Distance(transform.position, nextWaypoint.transform.position);
                    if(distanceToNextWaypoint < turningDistFromWaypoint)
                    {
                        startTurnTime = Time.time;
                        startRotation = transform.rotation;
                        currentWaypoint = nextWaypoint;
                        nextWaypoint = currentWaypoint.ConnectedWaypointClosestTo(currentTargetGem.GemLocationWaypoint, _lastwaypoint);
                        _lastwaypoint = currentWaypoint;
                    }

                    yield return null;
                }

                //found target waypoint, are at waypoint before it
                startSpeed = speed;
                startTurnTime = Time.time;
                Vector3 _turnTarget = _waypointTarget;
                startRotation = transform.rotation;
                while (true)
                {
                    if (Time.time < startTurnTime + .75f)
                    {
                        transform.rotation = Quaternion.Lerp(startRotation, Quaternion.LookRotation(_gemTarget - transform.position), (Time.time - startTurnTime) / .75f);
                    }
                    speed = Mathf.Lerp(startSpeed, maxDistPerSec, AccelerationCurve.Evaluate((Time.time - startMoveTime) / AccelerationTime));
                    transform.Translate(transform.forward * speed * Time.deltaTime, Space.World);
                    if(Vector3.Distance(transform.position, _gemTarget) < 3)
                    {
                        break;
                    }

                    yield return null;
                }

                //Slow and Stop
                float startSlowTime = Time.time;
                FredAnims.enabled = true;
                FredAnims.SetBool("Flying", false);
                while (Time.time < startSlowTime + StoppingTime)
                {
                    float currentSpeed = Mathf.Lerp(speed, 0, (Time.time - startSlowTime) / StoppingTime);
                    transform.Translate(transform.forward * currentSpeed * Time.deltaTime, Space.World);


                    yield return null;
                }
            }
            else
            {
                //just turn and move to gems
                //turn
                startTurnTime = Time.time;
                Quaternion startRotation = transform.rotation;
                while (Time.time < startTurnTime + (TurnTime / 2f))
                {
                    transform.rotation = Quaternion.Lerp(startRotation, Quaternion.LookRotation(_gemTarget - transform.position), (Time.time - startTurnTime) / (TurnTime / 2f));
                    yield return null;
                }

                float distanceToGems = Vector3.Distance(transform.position, _gemTarget);
                //Move To Gems
                if (distanceToGems > scanStopDistance)
                {
                    Vector3 moveDirection = transform.forward;
                    float startAccelerationTime = Time.time;

                    //start moving to scan pos
                    while (distanceToGems > scanStopDistance)
                    {
                        float currentSpeed = Mathf.Lerp(0, maxDistPerSec, AccelerationCurve.Evaluate((Time.time - startAccelerationTime) / AccelerationTime));
                        speed = currentSpeed;
                        transform.Translate(transform.forward * currentSpeed * Time.deltaTime, Space.World);
                        distanceToGems = Vector3.Distance(transform.position, _gemTarget);
                        yield return null;
                    }

                    //reached target, slow down and stop
                    float startSlowTime = Time.time;
                    FredAnims.enabled = true;
                    FredAnims.SetBool("Flying", false);
                    while (Time.time < startSlowTime + .3f)
                    {
                        float currentSpeed = Mathf.Lerp(speed, 0, (Time.time - startSlowTime) / .3f);
                        transform.Translate(moveDirection * currentSpeed * Time.deltaTime, Space.World);


                        yield return null;
                    }

                }
            }

            currentWaypoint = currentTargetGem.GemLocationWaypoint;

            //Collect Gems
            GemsToCollect.Remove(currentTargetGem);

            float startCollectTime = Time.time;
            TractorBeam.SetActive(true);
            currentTargetGem.FadeOutIcon();
            while(Time.time < startCollectTime + gemPickUpTime)
            {
                for(int i = 0; i < currentTargetGem.spawnedGems.Length; i++)
                {
                    Vector3 InForce = gemCollector.position - currentTargetGem.spawnedGems[i].transform.position;
                    InForce.Set(InForce.x, 0, InForce.z);
                    InForce.Normalize();
                    Vector3 force = (InForce * PickUpInForce) + (Vector3.up * PickUpUpForce);
                    currentTargetGem.spawnedGems[i].GetComponent<Rigidbody>().AddForce(force, ForceMode.Acceleration);
                }
                yield return null;
            }
            TractorBeam.SetActive(false);
            playerManager.ScanData(true);
            //Destroy(currentTargetGem);

            if (GemsToCollect.Count == 0)
            {
                break;
            }

            yield return null;
        }

        fetchingGems = false;
        yield return new WaitForSeconds(1f);
        currentState = State.Wandering;
        readyToMove = true;
        ScannerAnims.ResetTrigger("StopScanning");
        StartCoroutine(Wandering());

    }

    IEnumerator Wandering()
    {

        //If don't have a current waypoint
        if(currentWaypoint == null)
        {
            FredWaypoint bestWaypoint = null;

            //if you have a last waypoint, go to that
            if (lastWaypoint != null)
            {
                bestWaypoint = lastWaypoint;
            }
            else
            {
                //find the closest waypoint and move to that
                float bestDist = 1000;
                foreach (Transform _waypoint in Waypoints)
                {
                    float dist = Vector3.Distance(_waypoint.position, transform.position);
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        bestWaypoint = _waypoint.GetComponent<FredWaypoint>();
                    }
                }
            }
            
            if(bestWaypoint != null)
            {
                //Move to best Waypoint
                StartCoroutine(MovingToWaypoint(bestWaypoint));
                yield break;
            }
            
        }
        else
        {
            //you already have a waypoint, so you're on track and ready to move

            if (!reachedWaypoints.Contains(currentWaypoint))
            {
                reachedWaypoints.Add(currentWaypoint);
            }
        }

        //Wandering Update Loop 
        while (true)
        {
            //If ready to move to next wandering waypoint
            if (readyToMove)
            {
                readyToMove = false;
                //find new waypoint by choosing from the waypoints connected to the current waypoint.  Don't choose lastWaypoint, and weight reached waypoints as less weight.
                //find weight for each waypoint, and then add to total weight
                int[] weightedWaypoints = new int[currentWaypoint.connectedWaypoints.Length];
                int weightedTotal = 0;
                for (int i = 0; i < weightedWaypoints.Length; i++)
                {
                    FredWaypoint _potentialWaypoint = currentWaypoint.connectedWaypoints[i];

                    if( lastWaypoint != null && _potentialWaypoint == lastWaypoint)
                    {
                        //you were just there, low weight
                        weightedWaypoints[i] = 3;
                    }
                    else if (reachedWaypoints.Contains(_potentialWaypoint))
                    {
                        //you've been there, average weight
                        weightedWaypoints[i] = 4;
                    }
                    else
                    {
                        //new place, highest weight
                        weightedWaypoints[i] = 6;
                    }

                    weightedTotal = weightedTotal + weightedWaypoints[i];

                }

                int randomWeight = Random.Range(0, weightedTotal);
                FredWaypoint targetWaypoint = null;


                //loop through weights until total is greater than randomweight, then select that waypoint
                int counter = 0;
                for(int i = 0; targetWaypoint == null; i++)
                {
                    counter++;
                    if(i == weightedWaypoints.Length)
                    {
                        i = 0;
                    }
                    int currentWeight = weightedWaypoints[i] + counter;
                    if(currentWeight > randomWeight)
                    {
                        targetWaypoint = currentWaypoint.connectedWaypoints[i];
                        break;
                    }
                }

                if(targetWaypoint != null)
                {
                    //StartMovingToWaypoint and set readyToMove to false
                    StartCoroutine(MovingToWaypoint(targetWaypoint));
                    readyToMove = false;
                    yield break;
                }
                else
                {
                    Debug.Log("no good points");
                    StartCoroutine(MovingToWaypoint(currentWaypoint.connectedWaypoints[Random.Range(0, currentWaypoint.connectedWaypoints.Length -1)]));
                    readyToMove = false;
                    yield break;
                }

            }


            yield return new WaitForSeconds(1);
        }



        yield break;
    }


    IEnumerator MovingToWaypoint(FredWaypoint targetWaypoint)
    {

        if(currentState == State.Wandering)
        {
            float startTurnTime = Time.time;
            //turn to look towards destination
            while (Time.time < startTurnTime + TurnTime)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(targetWaypoint.transform.position - transform.position), Time.deltaTime * turnSpeed);
                yield return null;
            }

            //looking at target, start moving
            Vector3 moveDirection = transform.forward;
            float startAccelerationTime = Time.time;
            float distance = 1000;
            float speed = 0;
            FredAnims.SetBool("Flying", true);
            FredAnims.enabled = false;
            currentTargetWaypoint = targetWaypoint;
            while (distance > distToReachWaypoint)
            {
                float currentSpeed = Mathf.Lerp(0, maxDistPerSec, AccelerationCurve.Evaluate((Time.time - startAccelerationTime) / AccelerationTime));
                speed = currentSpeed;
                currentSpeedMovingBetweenWaypoints = speed;
                transform.Translate(transform.forward * currentSpeed * Time.deltaTime, Space.World);
                distance = Vector3.Distance(transform.position, targetWaypoint.transform.position);
                yield return null;
            }

            //reached target, slow down and stop
            float startSlowTime = Time.time;
            FredAnims.enabled = true;
            FredAnims.SetBool("Flying", false);
            while (Time.time < startSlowTime + StoppingTime)
            {
                float currentSpeed = Mathf.Lerp(speed, 0, (Time.time - startSlowTime) / StoppingTime);
                transform.Translate(moveDirection * currentSpeed * Time.deltaTime, Space.World);
                currentSpeedMovingBetweenWaypoints = currentSpeed;

                yield return null;
            }

            //reached waypoint, set as current, add to reached waypoints
            currentSpeedMovingBetweenWaypoints = 0;
            currentTargetWaypoint = null;
            if(currentWaypoint != null)
            {
                lastWaypoint = currentWaypoint;
            }
            currentWaypoint = targetWaypoint;
            if (!reachedWaypoints.Contains(currentWaypoint))
            {
                reachedWaypoints.Add(currentWaypoint);
            }
            readyToMove = false;
            StartCoroutine(Scanning());
            //yield return new WaitForSeconds(2f);
            //readyToMove = true;
        }
        else if(currentState == State.Fetching)
        {

        }

        yield break;
    }

   


    IEnumerator Scanning()
    {
        ScannableObject[] objectsToScan = currentWaypoint.ObjectsToScan();

        //loop for as many objects to scan
        for(int i = 0; i < objectsToScan.Length; i++)
        {

            if (objectsToScan[i] == null)
            {
                continue;
            }
            //get transform of current scan object
            Vector3 scanPos;
            if(objectsToScan[i].ScanPoint != null)
            {
                scanPos = objectsToScan[i].ScanPoint.position;
            }
            else
            {
                scanPos = objectsToScan[i].transform.position;
            }

            //set scan transform to equal fred's flying height
            scanPos.Set(scanPos.x, transform.position.y, scanPos.z);


            //turn To look at scan pos
            float startTurnTime = Time.time;

            while (Time.time < startTurnTime + TurnTime)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(scanPos - transform.position), Time.deltaTime * turnSpeed);
                yield return null;
            }


            //if not close enough to scan pos, move towards it
            float distance = Vector3.Distance(transform.position, scanPos);
            if(distance > scanStopDistance)
            {
                Vector3 moveDirection = transform.forward;
                float startAccelerationTime = Time.time;
                float speed = 0;
                
                //start moving to scan pos
                while (distance > scanStopDistance)
                {
                    float currentSpeed = Mathf.Lerp(0, maxDistPerSec/2f, AccelerationCurve.Evaluate((Time.time - startAccelerationTime) / AccelerationTime));
                    speed = currentSpeed;
                    transform.Translate(transform.forward * currentSpeed * Time.deltaTime, Space.World);
                    distance = Vector3.Distance(transform.position, scanPos);
                    yield return null;
                }

                //reached target, slow down and stop
                float startSlowTime = Time.time;
                FredAnims.enabled = true;
                FredAnims.SetBool("Flying", false);
                while (Time.time < startSlowTime + .3f)
                {
                    float currentSpeed = Mathf.Lerp(speed, 0, (Time.time - startSlowTime) / .3f);
                    transform.Translate(moveDirection * currentSpeed * Time.deltaTime, Space.World);


                    yield return null;
                }

            }


            //look at rocks
            if(objectsToScan[i].ScanPoint != null)
            {
                float startTurnScanTime = Time.time;
                Vector3 dir = objectsToScan[i].transform.position;
                dir.Set(dir.x, transform.position.y, dir.z);
                dir = dir - transform.position;

                while (Time.time < startTurnScanTime + .6f)
                {
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * turnSpeed);
                    yield return null;
                }
            }
            


            //scan target
            ScannerAnims.SetTrigger("Scan");
            playerManager.ScanData();

            yield return new WaitForSeconds(4);
            if (objectsToScan[i].containsGems)
            {
                //trigger found gems particles over rocks
                
                objectsToScan[i].GetComponent<BreakableRock>().ScanRock();
            }


            //delay before next iteration
            yield return new WaitForSeconds(3);
        }




        readyToMove = true;
        StartCoroutine(Wandering());

        yield break;
    }



	// Update is called once per frame
	void Update () {
		
	}
}

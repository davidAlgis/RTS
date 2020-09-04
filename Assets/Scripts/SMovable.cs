using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using UnityEngine.AI;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(NavMeshAgent))]

public class SMovable : SObject
{
    //protected bool m_stopCurrentAction = false;
    protected UnityEngine.AI.NavMeshAgent m_agent;
    public const float m_timeMaxToReachDest = 1000.0f;
    public const float m_timeMaxToFollowTarget = 30.0f;
    [SerializeField]
    protected float m_speedAttack;
    [SerializeField]
    protected uint m_powerAttack;
    [SerializeField]
    protected float m_distanceMaxAttack;
    [SerializeField]
    protected float m_radiusAttack;
    private Squad m_belongsToSquad;

    public NavMeshAgent Agent { get => m_agent; set => m_agent = value; }
    public Squad BelongsToSquad { get => m_belongsToSquad;}

    protected override void Awake()
    {
        base.Awake();
        m_agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
    }

    public void setBelongsSquad(Squad squad)
    {
        m_belongsToSquad = squad;
        squad.SquadSMovable.Add(this);
    }


    public void moveToFloor()
    {
        StartCoroutine(moveToFloorCoroutine());
    }

    protected IEnumerator moveToFloorCoroutine()
    {
        while(BelongsToSquad.DirectionIsSet == false)
            yield return new WaitForSeconds(0.5f);

        lookForANewDestination(BelongsToSquad.Goal);
    }

    public void lookForANewDestination(Vector3 originalDestination)
    {
        StartCoroutine(lookForANewDestinationCoroutine(originalDestination));
    }

    protected IEnumerator lookForANewDestinationCoroutine(Vector3 originalDestination)
    { 
        while (Vector3.Distance(transform.position, originalDestination) > 20)
            yield return new WaitForSeconds(0.1f);

        

        BelongsToSquad.getDestination(this);

    }

    public override void onClick(RaycastHit rayHit)
    {
        StopAllCoroutines();
        Vector3 dest = rayHit.point;
        Agent.destination = dest;


        if (m_belongsToSquad.SquadSMovable[0] == this)
            m_belongsToSquad.defineGoalAndSquadLeader(dest);

        moveToFloor();

        if (rayHit.collider.TryGetComponent(out SObject sobject))
        {
            //we update the destination to the nearer destination points.
            if (sobject.PointsDestinationNavMesh != null)
                for(int i=0; i<sobject.PointsDestinationNavMesh.Count; i++)
                {
                    Vector3 point = sobject.PointsDestinationNavMesh[i].first;
                    if (Vector3.Distance(transform.position, dest) > Vector3.Distance(transform.position, point) && sobject.PointsDestinationNavMesh[i].second == false)
                    {
                        dest = point;
                        sobject.PointsDestinationNavMesh[i].second = true;
                    }
                }
            //TODO: case with animals which are neutral but can be attacked
            //if the sobject belongs to ennemy we attack it
            if (sobject.BelongsTo is PlayerEnnemy)
            {
                StartCoroutine(actionAttack(sobject));
                //if it's an ennemy that can move we follow him 
                if (sobject is SMovable)
                    StartCoroutine(followSObject((SMovable)sobject));
            }
        }
        
        //Agent.destination = dest;
    }

    protected IEnumerator actionAttack(SObject target)
    {
        while (target.Health > 0)
        {
            Vector3 dest = transform.position;
            for (int i = 0; i < target.PointsDestinationNavMesh.Count; i++)
            {
                Vector3 point = target.PointsDestinationNavMesh[i].first;
                if (Vector3.Distance(transform.position, dest) > Vector3.Distance(transform.position, point) && target.PointsDestinationNavMesh[i].second == false)
                {
                    target.PointsDestinationNavMesh[i].second = true;
                    dest = point;
                }
            }

            if (m_distanceMaxAttack > Vector3.Distance(transform.position, dest))
                if (target.damage(m_powerAttack))
                {
                    print("stop all coroutine");

                    StopAllCoroutines();
                }
            //TODO : here just change target if there is an ennemy nearby
            if (target == null)
                yield break;

            yield return new WaitForSeconds(m_speedAttack);

        }
    }

    protected IEnumerator followSObject(SMovable target)
    {
        Vector3 saveOriginPosition = transform.position;
        int i = 0;
        const float timeIteration = 0.1f;
        
        if (target.PointsDestinationNavMesh == null)
        {
            Debug.LogError("Unable to follow the target, because the points destination nav mesh of " + target.gameObject.name + " has not been defined");
            yield break;
        }

        while (target.Health > 0)
        {
            while (Utilities.navMeshHaveReachDestination(m_agent) == true && target.Health > 0)
            {
                yield return new WaitForSeconds(timeIteration);
            }

            Vector3 dest = target.transform.position;
            if (i * timeIteration > m_timeMaxToReachDest)
            {
                Debug.LogWarning("The agent " + ID + " didn't suceed to follow his target");
                Agent.destination = dest;
                yield break;
            }

            for (int j = 0; j < target.PointsDestinationNavMesh.Count; j++)
            {
                Vector3 point = target.PointsDestinationNavMesh[j].first;
                if (Vector3.Distance(transform.position, dest) > Vector3.Distance(transform.position, point))
                    dest = point;
            }

            Agent.destination = dest;

            if (target == null)
                yield break;

            yield return new WaitForSeconds(timeIteration);
            i++;
        }
    }

    public Pair<float, Vector3> calculatePathLength(Vector3 destination)
    {
        // Create a path and set it based on a target position.
        NavMeshPath path = new NavMeshPath();

        NavMesh.CalculatePath(transform.position, destination, NavMesh.AllAreas, path);

        // Create an array of points which is the length of the number of corners in the path + 2.
        Vector3[] allWayPoints = new Vector3[path.corners.Length + 2];

        // The first point is the enemy's position.
        allWayPoints[0] = transform.position;

        // The last point is the target position.
        allWayPoints[allWayPoints.Length - 1] = destination;

        // The points inbetween are the corners of the path.
        for (int i = 0; i < path.corners.Length; i++)
        {
            allWayPoints[i + 1] = path.corners[i];
        }

        // Create a float to store the path length that is by default 0.
        float pathLength = 0;

        // Increment the path length by an amount equal to the distance between each waypoint and the next.
        for (int i = 0; i < allWayPoints.Length - 1; i++)
        {
            pathLength += Vector3.Distance(allWayPoints[i], allWayPoints[i + 1]);
        }

        return new Pair<float, Vector3>(pathLength, path.corners[path.corners.Length-2]);
    }
}

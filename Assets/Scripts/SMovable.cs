using System.Collections;
using System.Collections.Generic;
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

    public NavMeshAgent Agent { get => m_agent; set => m_agent = value; }


    protected override void Awake()
    {
        base.Awake();
        m_agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
    }

    public override void onClick(RaycastHit rayHit)
    {
        StopAllCoroutines();
        Vector3 dest = rayHit.point;
        if(rayHit.collider.TryGetComponent(out SObject sobject))
        {
            //we update the destination to the nearer destination points.
            if (sobject.PointsDestinationNavMesh != null)
                foreach (Vector3 point in sobject.PointsDestinationNavMesh)
                    if (Vector3.Distance(transform.position, dest) > Vector3.Distance(transform.position, point))
                        dest = point;
            //TODO: case with animals which are neutral but can be attacked
            //if the sobject belongs to ennemy we attack it
            if(sobject.BelongsTo is PlayerEnnemy)
            {
                StartCoroutine(actionAttack(sobject));
                //if it's an ennemy that can move we follow him 
                if (sobject is SMovable)
                    StartCoroutine(followSObject((SMovable)sobject));
            }
        }
        
        Agent.destination = dest;
    }

    protected IEnumerator actionAttack(SObject target)
    {
        while (target.Health > 0)
        {
            Vector3 dest = transform.position;
            foreach (Vector3 point in target.PointsDestinationNavMesh)
                if (Vector3.Distance(transform.position, target.transform.position) > Vector3.Distance(transform.position, point))
                    dest = point;

            if (m_distanceMaxAttack > Vector3.Distance(transform.position, dest))
                if (target.damage(m_powerAttack))
                    StopAllCoroutines();
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
            
            foreach (Vector3 point in target.PointsDestinationNavMesh)
                if (Vector3.Distance(transform.position, target.transform.position) > Vector3.Distance(transform.position, point))
                    dest = point;

            Agent.destination = dest;

            if (target == null)
                yield break;

            yield return new WaitForSeconds(timeIteration);
            i++;
        }
    }
}

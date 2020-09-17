using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using UnityEngine.AI;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(NavMeshAgent))]

public class SMovable : SObject
{
    protected UnityEngine.AI.NavMeshAgent m_agent;
    [SerializeField]
    protected const float m_timeMaxToReachDest = 1000.0f;
    [SerializeField]
    protected const float timeMaxToFollowTarget = 30.0f;
    #region attack_Attributes
    [SerializeField]
    protected float m_speedAttack;
    [SerializeField]
    protected uint m_powerAttack;
    [SerializeField]
    protected float m_distanceMaxAttack;
    [SerializeField]
    protected float m_radiusAttack;
    #endregion
    protected Squad m_belongsToSquad;
    [SerializeField]
    protected bool m_isActive;

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

    public override void onClick(RaycastHit rayHit)
    {
        base.onClick(rayHit);
        m_isActive = true;
        moveTo(rayHit);
        attack(rayHit);
    }

    #region movement

    public void moveTo(RaycastHit rayHit)
    {
        Vector3 dest = rayHit.point;

        if (rayHit.collider.gameObject.layer == LayerMask.NameToLayer("Floor"))
        {
            Agent.destination = dest;
            moveToSquadFloor(dest);
        }
        else if(rayHit.collider.gameObject.layer == LayerMask.NameToLayer("Selectable"))
        {
            
            SObject sobject = SObject.getSobjectFromSelectionField(rayHit.collider.gameObject);
            //if it's a smovable we redefine his destination points
            if (sobject is SMovable)
                sobject.definePointsDestination();
            moveToSquadSobject(sobject);
        }

    }

    public void moveToSquadFloor(Vector3 dest)
    {
        if (m_belongsToSquad.SquadSMovable[0] == this)
            m_belongsToSquad.defineGoalAndSquadLeader(dest);
        
        StartCoroutine(moveToFloorCoroutine());
    }

    protected IEnumerator moveToFloorCoroutine()
    {
        while(BelongsToSquad.DirectionIsSet == false)
            yield return new WaitForSeconds(0.5f);

        while (Vector3.Distance(transform.position, BelongsToSquad.Goal) > 20)
            yield return new WaitForSeconds(0.1f);

        BelongsToSquad.getDestinationFloor(this);
    }

    public bool moveOneToSObject(SObject target)
    {

        Vector3 dest = new Vector3(0.0f, Mathf.Infinity, 0.0f);
        if (target.PointsDestinationNavMesh != null)
            for (int i = 1; i < target.PointsDestinationNavMesh.Count; i++)
            {
                Vector3 point = target.PointsDestinationNavMesh[i];
                
                if (Vector3.Distance(transform.position, dest) > Vector3.Distance(transform.position, point))
                {
                    bool positionIsAvailable = Utilities.isPositionAvailable(target.PointsDestinationNavMesh[i], m_radius);
                    if (positionIsAvailable)
                        dest = point;
                }
            }

        moveToSquadSObjectLoadCoroutine(target);
        if (dest == new Vector3(0.0f, Mathf.Infinity, 0.0f))
        {
            Debug.LogWarning("Didn't find any destination on target " + target.ID + " for " + ID + " stay at his initial position");

            return false;
        }
        else
        {
            Agent.destination = dest;
            return true;
        }
    }

    public void moveToSquadSobject(SObject target)
    {
        //we execute the code only once by the squad leader.
        if (m_belongsToSquad.SquadSMovable[0] == this)
            m_belongsToSquad.getDestinationSObject(target);
    }

    public void moveToSquadSObjectLoadCoroutine(SObject sobject)
    {
        StartCoroutine(moveToSObjectCoroutine(sobject));
    }

    public IEnumerator moveToSObjectCoroutine(SObject sobject)
    {
        while(Vector3.Distance(transform.position, Agent.destination) < 3.0f*m_radius)
        {
            yield return new WaitForSeconds(0.5f);
        }

        /*If the position isn't available, we look if another position is 
         * available. Else the smovable stop moving.*/
        if (Utilities.isPositionAvailable(Agent.destination, m_radius) == false)
            if (moveOneToSObject(sobject) == false)
                Agent.destination = transform.position;

    }

    #endregion

    #region attack

    protected void attack(RaycastHit rayHit)
    {
        if (rayHit.collider.gameObject.layer == LayerMask.NameToLayer("Selectable"))
        {

            SObject sobject = SObject.getSobjectFromSelectionField(rayHit.collider.gameObject);

            //TODO: case with animals which are neutral but can be attacked
            //if the sobject belongs to ennemy we attack it
            if (sobject.BelongsTo is PlayerEnnemy)
            {
                
                beginAttack(sobject);
                //m_belongsToSquad.attackSquad(sobject);
            }
        }
    }

    public void beginAttack(SObject sobject)
    {
        print(ID + " begin attack");

        //if it's an ennemy that can move we follow him 
        actionAttack(sobject);
        if(sobject is SMovable)
        {
            SMovable ennemy = (SMovable)sobject;
            ennemy.isUnderAttack();
        }
    }

    public void isUnderAttack()
    {
        StartCoroutine(isUnderAttackCoroutine());
    }

    protected IEnumerator isUnderAttackCoroutine()
    {
        print(ID + " is under attack");
        List<SObject> ennemies = getEnnemiesNearBy(m_fieldOfView);
        int nbrIteration = 0;

        while (ennemies.Count == 0)
        {
            print(ennemies.Count);
            ennemies = getEnnemiesNearBy(m_fieldOfView);

            nbrIteration++;

            if (nbrIteration > 100)
            {
                print("the ennemy didn't came");
                yield break;
            }

            yield return new WaitForSeconds(0.5f);
        }

        Pair<List<SMovable>, List<SMovable>> allies = getActiveInactiveAlliesNearBy(m_fieldOfView);
        //If we are near the mother house everyone attack.
        if (isMotherHouseNearBy(m_fieldOfView))
        {
            if (allies.second != null)
                foreach (SMovable smovable in allies.second)
                {
                    float minDist = Vector3.Distance(ennemies[0].transform.position, smovable.transform.position);
                    SObject nearerEnnemy = ennemies[0];

                    foreach(SObject ennemy in ennemies)
                    {
                        if (Vector3.Distance(ennemy.transform.position, smovable.transform.position) < minDist)
                            nearerEnnemy = ennemy;
                    }
                    smovable.actionAttack(nearerEnnemy);
                }
            print("Is near mother house");
            yield break;
        }

        //if they outnumbered the attackers they attack, else the run away to the mother house
        if (allies.first.Count + allies.second.Count >= ennemies.Count) 
        {

            foreach (SMovable smovable in allies.second)
                smovable.actionAttack(ennemies[0]);
            
        }
        else
        {
            foreach (SMovable smovable in allies.second)
                smovable.moveOneToSObject(smovable.BelongsTo.MotherHouse);

            foreach (SMovable smovable in allies.first)
                smovable.moveOneToSObject(smovable.BelongsTo.MotherHouse);
        }

    }

    public void actionAttack(SObject target)
    {
        print(ID + " action attack");
        StartCoroutine(actionAttackCoroutine(target));
    }

    protected IEnumerator actionAttackCoroutine(SObject target)
    {
        int nbrIteration = 0;
        while (target.Health > 0)
        {
            //If he can shoot he try.
            if (m_distanceMaxAttack > Vector3.Distance(transform.position, target.transform.position) - (target.Radius + m_radius))
            {
                m_agent.destination = transform.position;
                if (target.damage(m_powerAttack))
                {

                    //is dead
                    //we look for a ennemies nearby and we attack it
                    List<SObject> ennemies = getEnnemiesNearBy(m_fieldOfView);
                    if (ennemies.Count > 0)
                    {
                        actionAttack(ennemies[0]);
                        yield break;
                    }
                    else
                    {
                        m_isActive = false;
                        StopAllCoroutines();
                    }
                }
            }
            else
                m_agent.destination = target.transform.position;

            nbrIteration++;
            //after 100*m_speedAttack the smovable give up and stay at his position
            if (nbrIteration > 100)
            {
                m_agent.destination = transform.position;
                Debug.LogWarning("Unable to kill " + target.ID);
                yield break;

            }

            yield return new WaitForSeconds(m_speedAttack);
        }
        m_isActive = false;
    }
    

    #endregion

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

        if(path.corners.Length > 2)
            return new Pair<float, Vector3>(pathLength, path.corners[path.corners.Length-2]);
        else
            return new Pair<float, Vector3>(pathLength, transform.position);
    }

    public List<SObject> getEnnemiesNearBy(float radius)
    {
        int layerMask = ~(LayerMask.GetMask("Floor") | LayerMask.GetMask("Selectable"));

        var hitColliders = Physics.OverlapSphere(transform.position, radius, layerMask);
        List<SObject> ennemies = new List<SObject>();

        //if hitcolliders.length = 1 it means that it only detect himself.
        if (hitColliders.Length > 1)
        {
            foreach(Collider collider in hitColliders)
            {
                if(collider.TryGetComponent(out SObject sobject))
                    if (sobject.BelongsTo != m_belongsTo && sobject.IsNeutral == false)
                        ennemies.Add(sobject);
                
            }
        }

        nearerToFirstOne(ennemies); 
        return ennemies;
    }

    public bool isMotherHouseNearBy(float radius)
    {
        if(m_belongsTo != null)
        {
            float substractRad = m_belongsTo.MotherHouse.Radius + m_radius;
            if ((Vector3.Distance(m_belongsTo.MotherHouse.transform.position, transform.position) - substractRad) < radius)
                return true;
            else
                return false;
        }
        return false;
    }

    //get the number of allies nearby and the list of inactive allies.
    public Pair<List<SMovable>, List<SMovable>> getActiveInactiveAlliesNearBy(float radius)
    {
        int layerMask = ~(LayerMask.GetMask("Floor") | LayerMask.GetMask("Selectable"));

        var hitColliders = Physics.OverlapSphere(transform.position, radius, layerMask);
        List<SMovable> alliesInactive = new List<SMovable>();
        List<SMovable> alliesActive = new List<SMovable>();

        //if hitcolliders.length = 1 it means that it only detect himself.
        if (hitColliders.Length > 1)
        {
            foreach (Collider collider in hitColliders)
            {
                if (collider.TryGetComponent(out SMovable smovable))
                {
                    if (smovable.BelongsTo == m_belongsTo)
                    {
                        if (smovable.m_isActive)
                            alliesActive.Add(smovable);
                        else
                            alliesInactive.Add(smovable);
                    }

                }

            }
        }

        return new Pair<List<SMovable>, List<SMovable>>(alliesActive, alliesInactive);
    }
}

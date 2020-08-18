using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHarvester : SUnit
{
    //time for taking 10 unit;
    private float m_speedHarvest = 10.0f;

    public void test()
    {
        print("a");
    }
    public override void onClick(RaycastHit rayHit)
    {
        base.onClick(rayHit);
        if(rayHit.collider.TryGetComponent(out SRessources sressources))
        {

            print("gonna to harvest " + gameObject.name + ID);
            StartCoroutine(actionGetRessourcesCoroutine(sressources));
        }

        if ((rayHit.collider.TryGetComponent(out SBuilding sbuilding)))
        {
            if(sbuilding.BelongsTo != this.BelongsTo)
            {

                print("gonna to attack " + gameObject.name + ID);
                StartCoroutine(actionGetRessourcesCoroutine(sressources));

            }
        }

    }

    IEnumerator actionGetRessourcesCoroutine(SRessources sressources)
    {
        //yield return new WaitForSeconds(0.5f);
        int i = 0;
        while(Utilities.navMeshHaveReachDestination(m_agent) == false)
        {

            if (i * 0.5f > m_timeMaxToReachDest)
            {
                Debug.LogWarning("The agent " + ID + " didn't suceed to reach his destination");
                yield break;
            }
                
            yield return new WaitForSeconds(0.5f);
            i++;
        }

        //add ressources until the ressources is empty.
        while (sressources.Contains > 0)
        {

            yield return new WaitForSeconds(m_speedHarvest);
            uint nbrOfRessources = sressources.getRessources(10);
            print("get " + nbrOfRessources + " ressources");
            if (IsNeutral == false)
            {
                switch(sressources.Type)
                {
                    case RessourcesType.wood:
                        print("add wood");
                        m_belongsTo.Wood += nbrOfRessources;
                        break;
                    case RessourcesType.food:
                        m_belongsTo.Food += nbrOfRessources;
                        break;
                    case RessourcesType.gold:
                        m_belongsTo.Gold += nbrOfRessources;
                        break;
                    case RessourcesType.rock:
                        m_belongsTo.Rock += nbrOfRessources;
                        break;

                }
            } 
        }



    }

    public void beginCreateBuilding(GameObject buildingGO)
    {
        SBuilding sbuilding;
        if (buildingGO.TryGetComponent(out sbuilding) == false)
        {
            Debug.LogWarning("The harvester try to create something which is not a building");
            return;
        }

        sbuilding.BelongsTo = GameManager.Instance.CurrentPlayer;

        RaycastHit rayHit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out rayHit, Mathf.Infinity))
            UIManager.Instance.BuildingCreated = Instantiate(buildingGO, rayHit.point, Quaternion.identity);
        
        
        if (UIManager.Instance.BuildingCreated.TryGetComponent(out Rigidbody rb))
            rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;


        if (m_belongsTo is PlayerHuman)
        {
            PlayerHuman p = (PlayerHuman)m_belongsTo;
            p.BeginCreation = buildingGO;
        }

    }




}

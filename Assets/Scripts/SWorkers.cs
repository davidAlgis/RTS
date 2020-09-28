using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SWorkers : SUnit
{
    //time for taking 10 units of resources;
    [SerializeField]
    private float m_speedHarvest = 10.0f;
    //Factor of speed of construction, reduce it if you want to increase the speed
    private float m_speedConstruction = 1.0f;

    private SBuilding m_buildingUnderConstruction = null;

    public override void onClick(RaycastHit rayHit)
    {
        base.onClick(rayHit);

        //if the builder was building a construction, we reduce the number of builder on this construction
        if (m_buildingUnderConstruction != null)
        {
            m_buildingUnderConstruction.BuilderOnConstruction--;
            m_buildingUnderConstruction = null;
        }

        clickOnConstruction(rayHit);
        harvest(rayHit);
    }

    public override void onClick(SObject sobject)
    {
        base.onClick(sobject);
        print("on click sobject");
        //if the builder was building a construction, we reduce the number of builder on this construction
        if (m_buildingUnderConstruction != null)
        {
            m_buildingUnderConstruction.BuilderOnConstruction--;
            m_buildingUnderConstruction = null;
        }

        if (sobject is SResources)
        {
            print("gonna to harvest " + ID);
            SResources sresources = (SResources)sobject;
            actionGetResources(sresources);
            if(m_belongsToSquad != null)
                m_belongsToSquad.harvestSquad(sresources);
            print("get resources");
        }

        if (sobject is SBuilding)
        {
            SBuilding sbuilding = (SBuilding)sobject;
            if (sbuilding.BelongsTo != m_belongsTo)
            {

                print("gonna to attack " + ID);
                //TODO add attack here
            }
            else
            {
                //if the building is in construction we continue the construction
                if (sbuilding.IsInConstruction)
                    continueConstructBuilding(sbuilding.gameObject);
            }
        }
    }

    #region harvest
    public void harvest(RaycastHit rayHit)
    {
        if (rayHit.collider.gameObject.layer == LayerMask.NameToLayer("Selectable"))
        {
            SObject sobject = SObject.getSobjectFromSelectionField(rayHit.collider.gameObject);

            if (sobject is SResources)
            {
                print("gonna to harvest " + ID);
                SResources sresources = (SResources)sobject;
                actionGetResources(sresources);
                if(m_belongsToSquad != null)
                    m_belongsToSquad.harvestSquad(sresources);
            }
        }
    }

    public void actionGetResources(SResources sresources)
    {

        StartCoroutine(actionGetResourcesCoroutine(sresources));
    }

    private IEnumerator actionGetResourcesCoroutine(SResources sresources)
    {
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

        print("nav mesh true");
        //add resources until the resources is empty.
        while (sresources.Contains != new Resources())
        {
            print("try to get resources with " + ID);
            yield return new WaitForSeconds(m_speedHarvest);
            Resources resources = sresources.getResources(Constant.RESOURCESTOGET, m_belongsTo);
        }
    }

    public List<SResources> getSResourcesNearBy(float radius, SResources addNotThisOne = null)
    {
        int layerMask = ~(LayerMask.GetMask("Floor") | LayerMask.GetMask("Selectable"));

        var hitColliders = Physics.OverlapSphere(transform.position, radius, layerMask);
        List<SResources> resources = new List<SResources>();

        if (hitColliders.Length > 0)
            foreach (Collider collider in hitColliders)
                if (collider.TryGetComponent(out SResources sresources))
                    if(sresources != addNotThisOne)
                        resources.Add(sresources);

        nearerToFirstOne(resources);

        return resources;
    }
    #endregion

    #region buildingCreation

    //called when the player click on the button of a workers to make a building
    public void beginCreateBuilding()
    {

        GameObject buildingGO = m_currentButtonCreation.go;

        SBuilding sbuilding;
        if (buildingGO.TryGetComponent(out sbuilding) == false)
        {
            Debug.LogWarning("The worker " + ID + " try to create something which is not a building");
            return;
        }

        if (m_belongsTo is PlayerHuman == false)
        {
            Debug.LogWarning("Unable to begin construction on a player non human with worker " + ID);
            return;
        }

        sbuilding.BelongsTo = m_belongsTo;

        if (m_belongsTo.canBuySobject(sbuilding) == false)
            return;

        //define the initial position of the building with raycast on floor
        LayerMask mask = LayerMask.GetMask("Floor");

        RaycastHit rayHit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out rayHit, Mathf.Infinity, mask))
        {
            /* !!! When we call this function, the update function
             * PlayerHuman handle the rest. */
            ((PlayerHuman)m_belongsTo).initConstructionBuilding(buildingGO, rayHit.point);
        }
        else
            Debug.LogWarning("Try to create a building outside of the map.");
    }

    //called when the player validate the position of construction 
    public void beginConstructBuilding(GameObject buildingGO)
    {
        StopAllCoroutines();
        buildingGO.isStatic = true;

        //add cloth on it 
        if(DebugTool.tryFindGOChildren(buildingGO, "Cloth", out GameObject clothGO, LogType.Error))
        {
            clothGO.SetActive(true);
        }


        buildingGO.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        Vector3 dest = buildingGO.transform.position;
        if (buildingGO.TryGetComponent(out SBuilding sbuilding))
        {
            sbuilding.defineSelectionField();
            //reset the material
            if (buildingGO.TryGetComponent(out MeshRenderer meshRenderer))
            {

                //meshRenderer.material = sbuilding.InitMaterial;
                meshRenderer.materials = sbuilding.InitMaterials;
                for (int i = 0; i < meshRenderer.materials.Length; i++)
                {
                    meshRenderer.materials[i] = sbuilding.InitMaterials[i];
                }
            }
            else
                Debug.LogWarning("Unable to find the material component of " + buildingGO.name);

            //we update the destination of the whole squad
            moveToSquadSobject(sbuilding);
        }

        //we construct the building for the whole squad
        m_belongsToSquad.constructBuilding(buildingGO);
    }

    public void constructBuilding(GameObject buildingGO)
    {
        StartCoroutine(constructBuildingCoroutine(buildingGO));
    }

    private IEnumerator constructBuildingCoroutine(GameObject buildingGO)
    {
        #region constructBuildingInit
        
        SBuilding sbuilging;
        if (buildingGO.TryGetComponent(out sbuilging) == false)
        {
            Debug.LogWarning("Unable to construct the " + gameObject.name + " it doesn't have any SBuilding component");
            yield break;
        }

        float duration = sbuilging.DurationCreation;
       
        #endregion

        #region reachBuilding
        int i = 0;
        //wait until the agent reach destination
        while (Utilities.navMeshHaveReachDestination(m_agent) == false)
        {
            if (i * 0.5f > m_timeMaxToReachDest)
            {
                Debug.LogWarning("The agent " + ID + " didn't suceed to reach his destination");
                yield break;
            }

            yield return new WaitForSeconds(0.5f);
            i++;
        }
        #endregion

        #region constructBuildingLoop

        m_buildingUnderConstruction = sbuilging;
        sbuilging.BuilderOnConstruction++;
        const float timeIteration = 0.1f;

        while(sbuilging.StateOfConstruction < 1.0f)
        {
            sbuilging.StateOfConstruction += (timeIteration / duration)* m_speedConstruction;
            yield return new WaitForSeconds(timeIteration);
        }
        #endregion

        #region end_construction
        //change mesh to the full form off the building
        //buildingGO.GetComponent<MeshFilter>().sharedMesh = sbuilging.InitMeshBuilding;
        sbuilging.enabled = true;
        
        //the building isn't in construction anymore
        sbuilging.IsInConstruction = false;

        IsActive = false;

        //add cloth on it 
        if (DebugTool.tryFindGOChildren(buildingGO, "Cloth", out GameObject clothGO, LogType.Error))
        {
            clothGO.SetActive(false);
        }
        #endregion
    }

    private void clickOnConstruction(RaycastHit rayHit)
    {
        if (rayHit.collider.gameObject.layer == LayerMask.NameToLayer("Selectable"))
        {
            SObject sobject = SObject.getSobjectFromSelectionField(rayHit.collider.gameObject);

            if (sobject is SBuilding)
            {
                SBuilding sbuilding = (SBuilding)sobject;
                if (sbuilding.BelongsTo != m_belongsTo)
                {

                    print("gonna to attack " + ID);
                    //TODO add attack here
                }
                else
                {
                    //if the building is in construction we continue the construction
                    if (sbuilding.IsInConstruction)
                        continueConstructBuilding(sbuilding.gameObject);
                }
            }

        }

    }

    public void continueConstructBuilding(GameObject buildingGO)
    {
        Vector3 dest = buildingGO.transform.position;
        if (buildingGO.TryGetComponent(out SBuilding sbuilding))
            moveToSquadSobject(sbuilding);

        m_belongsToSquad.constructBuilding(buildingGO);
    }
    #endregion


}

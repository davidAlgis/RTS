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
        List<SResources> resourcesNearBy;
        //add resources until the resources is empty.
        while (sresources.Contains != new Resources())
        {

            yield return new WaitForSeconds(m_speedHarvest);
            bool isEmpty = sresources.Contains - Constant.RESOURCESTOGET == new Resources();

            Resources resources = sresources.getResources(Constant.RESOURCESTOGET, m_belongsTo);
            
            if(isEmpty)
            {
                print(ID + " resources is empty");
                resourcesNearBy = getSResourcesNearBy(m_fieldOfView, sresources);

                if (resourcesNearBy.Count > 0)
                {
                    print("move to " + resourcesNearBy[0].gameObject.name);
                    moveOneToSObject(resourcesNearBy[0]);
                    actionGetResources(resourcesNearBy[0]);
                    yield break;
                }
                else
                {
                    m_isActive = false;
                    yield break;
                }
            }

            if(sresources == null)
            {
                resourcesNearBy = getSResourcesNearBy(m_fieldOfView);

                if (resourcesNearBy.Count > 0)
                {
                    moveOneToSObject(resourcesNearBy[0]);
                    actionGetResources(resourcesNearBy[0]);
                    yield break;
                }
                else
                {
                    m_isActive = false;
                    yield break;
                }
            }
        }


        resourcesNearBy = getSResourcesNearBy(m_fieldOfView, sresources);

        print(ID + " out of loop look for resources");
        if (resourcesNearBy.Count > 0)
        {
            print("move to " + resourcesNearBy[0].gameObject.name);
            moveOneToSObject(resourcesNearBy[0]);
            actionGetResources(resourcesNearBy[0]);
            yield break;
        }
        else
        {
            m_isActive = false;
            yield break;
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
    public void clickOnConstruction(RaycastHit rayHit)
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

    public void beginCreateBuilding(GameObject buildingGO)
    {
        if (m_belongsTo is PlayerHuman)
        {
            PlayerHuman p = (PlayerHuman)m_belongsTo;
            p.BeginCreation = buildingGO;
        }

        SBuilding sbuilding;
        if (buildingGO.TryGetComponent(out sbuilding) == false)
        {
            Debug.LogWarning("The harvester try to create something which is not a building");
            return;
        }



        sbuilding.BelongsTo = GameManager.Instance.CurrentPlayer;

        if (m_belongsTo.canBuySobject(sbuilding) == false)
            return;

        UIManager.Instance.SbuildingCreated = sbuilding;
        sbuilding.IsInConstruction = true;

        //define the initial position of the building with raycast on floor
        LayerMask mask = LayerMask.GetMask("Floor");

        RaycastHit rayHit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out rayHit, Mathf.Infinity, mask))
        {
            Vector3 origPos = rayHit.point;
            //when we change this value, the update function of UIManager and the update of PlayerHuman handle the rest.
            UIManager.Instance.BuildingCreated = Instantiate(buildingGO, origPos, Quaternion.identity);

            
            //translate the building above floor
            if (UIManager.Instance.BuildingCreated.TryGetComponent(out Collider collider))
            {
                origPos = new Vector3(origPos.x, origPos.y + collider.bounds.extents.y, origPos.z);
                UIManager.Instance.BuildingCreated.transform.position = origPos;
            }
            else
                Debug.LogWarning("Unable to find any collider component on "+ UIManager.Instance.BuildingCreated.name + ". The building could be beneath floor");
        }

        //define the usual material of construction
        if (UIManager.Instance.BuildingCreated.TryGetComponent(out MeshRenderer meshRenderer))
            meshRenderer.material = GameManager.Instance.MatBuildingCreationAvailable;
        else
            Debug.LogWarning("Unable to find the material component of " + UIManager.Instance.BuildingCreated.name);

        if (UIManager.Instance.BuildingCreated.TryGetComponent(out Rigidbody rb))
            rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;

        
        
    }

    public void beginConstructBuilding(GameObject buildingGO)
    {
        StopAllCoroutines();
        buildingGO.isStatic = true;
        buildingGO.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        Vector3 dest = buildingGO.transform.position;
        if (buildingGO.TryGetComponent(out SBuilding sbuilding))
        {
            //reset the material
            if (buildingGO.TryGetComponent(out MeshRenderer meshRenderer))
                meshRenderer.material = sbuilding.InitMaterial;
            else
                Debug.LogWarning("Unable to find the material component of " + buildingGO.name);

            //we update the destination of the whole squad
            moveToSquadSobject(sbuilding);
        }

        //we construct the building for the whole squad
        m_belongsToSquad.constructBuilding(buildingGO);
    }

    public void continueConstructBuilding(GameObject buildingGO)
    {
        Vector3 dest = buildingGO.transform.position;
        if (buildingGO.TryGetComponent(out SBuilding sbuilding))
            moveToSquadSobject(sbuilding);
        
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

        int nbrOfStepOfConstruction = sbuilging.StepConstructionMesh.Length;
        float duration = sbuilging.DurationCreation;
        Mesh[] stepConstruction = sbuilging.StepConstructionMesh;

        //we change the mesh to the first step of construction if it's the first workers on construction
        if(sbuilging.StateOfConstruction == 0.0f)
        {
            if (stepConstruction != null)
            {
                if (stepConstruction[0] != null)
                {
                    buildingGO.GetComponent<MeshFilter>().sharedMesh = stepConstruction[0];
                    sbuilging.IndexOfConstructionMesh = 1;
                }
                else
                    Debug.LogWarning("The mesh component of " + gameObject.name + " as not been set");
            }
        }
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
            //change mesh if construction state is above a given threshold
            if (sbuilging.StateOfConstruction > (float)sbuilging.IndexOfConstructionMesh / (float)nbrOfStepOfConstruction)
            {
                if (sbuilging.IndexOfConstructionMesh > stepConstruction.Length)
                {
                    Debug.LogWarning("try to access outside of the allocated memory");
                    break;
                }

                Mesh mesh = stepConstruction[sbuilging.IndexOfConstructionMesh];
                if (mesh == null)
                {
                    mesh = buildingGO.GetComponent<MeshFilter>().mesh;
                    Debug.LogWarning("The mesh component of " + gameObject.name + " as not been set");
                }
                //change the mesh to the next step
                buildingGO.GetComponent<MeshFilter>().sharedMesh = mesh;
                sbuilging.IndexOfConstructionMesh++; 
            }
            sbuilging.StateOfConstruction += (timeIteration / duration)* m_speedConstruction;
            yield return new WaitForSeconds(timeIteration);
        }


        //change mesh to the full form off the building
        buildingGO.GetComponent<MeshFilter>().sharedMesh = sbuilging.InitMeshBuilding;
        sbuilging.enabled = true;
        //the building isn't in construction anymore
        sbuilging.IsInConstruction = false;
        #endregion

        m_isActive = false;
    }

    #endregion


}

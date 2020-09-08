using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SWorkers : SUnit
{
    //time for taking 10 units of ressources;
    private float m_speedHarvest = 10.0f;
    //Factor of speed of construction, reduce it if you want to increase the speed
    private float m_speedConstruction = 1.0f;
    private SBuilding m_buildingUnderConstruction = null;
    public override void onClick(RaycastHit rayHit)
    {
        base.onClick(rayHit);

        //if the builder was building a construction, we reduce the number of builder on this construction
        if(m_buildingUnderConstruction != null)
        {
            m_buildingUnderConstruction.BuilderOnConstruction--;
            m_buildingUnderConstruction = null;
        }

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
            else
            {
                //if the building is in construction we continue the construction
                if (sbuilding.IsInConstruction)
                    continueConstructBuilding(sbuilding.gameObject);
            }
        }
    }

    IEnumerator actionGetRessourcesCoroutine(SRessources sressources)
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
        sbuilding.IsInConstruction = true;
        //define the initial position of the building with raycast on floor
        LayerMask mask = LayerMask.GetMask("Floor");

        RaycastHit rayHit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out rayHit, Mathf.Infinity, mask))
        {
            Vector3 origPos = rayHit.point;
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
            meshRenderer.material = GameManager.Instance.MatBuildingCreation;
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

            //we update the destination to the nearer destination points.
            moveOneToSObject(sbuilding);
        }

        StartCoroutine(constructBuildingCoroutine(buildingGO));
    }

    public void continueConstructBuilding(GameObject buildingGO)
    {
        Vector3 dest = buildingGO.transform.position;
        if (buildingGO.TryGetComponent(out SBuilding sbuilding))
        {
            if (moveOneToSObject(sbuilding) == false)
            {
                Debug.Log("The building " + buildingGO.name + " is already in construction");
                return;

            }
        }

        StartCoroutine(constructBuildingCoroutine(buildingGO));
    }

    IEnumerator constructBuildingCoroutine(GameObject buildingGO)
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
            print(gameObject.name + "is building");
            sbuilging.StateOfConstruction += (timeIteration / duration)* m_speedConstruction;
            yield return new WaitForSeconds(timeIteration);
        }


        //change mesh to the full form off the building
        buildingGO.GetComponent<MeshFilter>().sharedMesh = sbuilging.InitMeshBuilding;
        sbuilging.enabled = true;
        //the building isn't in construction anymore
        sbuilging.IsInConstruction = false;
        #endregion
    }

}

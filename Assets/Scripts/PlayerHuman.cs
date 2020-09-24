using Packages.Rider.Editor.PostProcessors;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerHuman : Player
{
    private GameObject m_buildingInCreationGO = null;
    private SBuilding m_sbuildingInCreation;
    private bool m_constructionIsAvailable = true;

    private Squad m_currentSquad;
    private List<SObject> m_currentSelection = new List<SObject>();
    private SObject m_lastSelection;
    private bool m_enableSelection;
    private LayerMask m_layerMaskFloor;

    #region multi-selection attributes
    private bool m_mouseIsHold;
    private BoxCollider m_boxForSelection;
    private Vector3 m_extremityBoxSelection1;
    private Vector3 m_extremityBoxSelection2;
    private Vector3 m_extremitySelectionUI1;
    #endregion


    #region getter_setter
    public bool MouseIsHold { get => m_mouseIsHold; set => m_mouseIsHold = value; }
    #endregion

    protected override void Awake()
    {
        base.Awake();
        #region init multi-selection
        /*multi-selection :
         First we create an object with a box collider and a script which will add the 
         SObject in the box to the selection.
         The coordinate of the box collider will be updated in update loop when the player
         hold the mouse.*/
        m_enableSelection = true;
        m_mouseIsHold = false;
        GameObject cubeForDetection = new GameObject();
        cubeForDetection.name = "BoxForMultiSelection";
        AddToSelection addToSelection = cubeForDetection.AddComponent<AddToSelection>();
        addToSelection.Player = this;
        m_boxForSelection = cubeForDetection.AddComponent<BoxCollider>();
        m_boxForSelection.isTrigger = true;
        Rigidbody rb = cubeForDetection.AddComponent<Rigidbody>();
        rb.useGravity = false;
        #endregion

        m_layerMaskFloor = LayerMask.GetMask("Floor");
    }

    void Update()
    {
        m_enableSelection = selectIsEnable();

        if (m_enableSelection)
        {
            select();
            multiSelection();

            if (Input.GetMouseButtonDown(1))
            {
                RaycastHit rayHit;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out rayHit, Mathf.Infinity))
                    actionCurrentSelection(rayHit);

            }
        }

        //When this member variables is set from initConstructionBuilding we called this function
        if (m_buildingInCreationGO != null)
        {
            plotBuildingOnMouse();
            beginConstruction();
        }

    }

    #region selection_tool

    private bool selectIsEnable()
    {

        if (m_buildingInCreationGO != null)
            return false;
        if (UIManager.Instance.IsPointerOverUIElement())
            return false;

        return true; 
    }

    private void select()
    {
        if (Input.GetMouseButtonDown(0))
        {
            cleanCurrentSelection();
            RaycastHit rayHit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out rayHit, Mathf.Infinity, GameManager.Instance.LayerSelection))
                if (rayHit.collider.TryGetComponent(out SObject selectableObject)) 
                    addToCurrentSelection(selectableObject);

        }
    }

    private void multiSelection()
    {
        //mouse is held down first
        if (Input.GetMouseButton(0) && m_mouseIsHold == false && Input.GetMouseButtonDown(0))
        {
            m_mouseIsHold = true;
            m_extremitySelectionUI1 = UIManager.Instance.convertMousePositionToCanvasPosition(Input.mousePosition);
            RaycastHit rayHit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out rayHit, Mathf.Infinity, m_layerMaskFloor))
            {
                Vector3 point = rayHit.point;
                m_extremityBoxSelection1 = new Vector3(point.x, point.y - 10.0f, point.z);
                m_extremityBoxSelection2 = new Vector3(point.x, point.y + 10.0f, point.z);
                Vector3 center, size;
                Utilities.convertPropertiesBoxCollider(m_extremityBoxSelection1, m_extremityBoxSelection2, out center, out size);
                m_boxForSelection.center = center;
                m_boxForSelection.size = size;
            }
        }
        

        //mouse is held down
        if (Input.GetMouseButton(0) && m_mouseIsHold)
        {
            UIManager.Instance.plotSelector(m_extremitySelectionUI1, UIManager.Instance.convertMousePositionToCanvasPosition(Input.mousePosition));

            RaycastHit rayHit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out rayHit, Mathf.Infinity, m_layerMaskFloor))
            {
                m_boxForSelection.enabled = true;
                Vector3 point = rayHit.point;
                m_extremityBoxSelection2 = new Vector3(point.x, point.y + 10.0f, point.z);
                Vector3 center, size;
                Utilities.convertPropertiesBoxCollider(m_extremityBoxSelection1, m_extremityBoxSelection2, out center, out size);
                m_boxForSelection.center = center;
                m_boxForSelection.size = size;
            }

        }

        //mouse is released
        if (Input.GetMouseButton(0) == false && m_mouseIsHold)
        {
            m_mouseIsHold = false;
            UIManager.Instance.unPlotSelector();

            RaycastHit rayHit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out rayHit, Mathf.Infinity, m_layerMaskFloor))
            {
                Vector3 point = rayHit.point;

                m_extremityBoxSelection2 = new Vector3(point.x, point.y + 10.0f, point.z);
                /*Utilities.instantiateSphereAtPosition(m_extremityBoxSelection1);
                Utilities.instantiateSphereAtPosition(m_extremityBoxSelection2);*/
                Vector3 center, size;
                Utilities.convertPropertiesBoxCollider(m_extremityBoxSelection1, m_extremityBoxSelection2, out center, out size);
                m_boxForSelection.center = center;
                m_boxForSelection.size = size;
                //reset the box collider
                m_boxForSelection.center = Vector3.zero;
                m_boxForSelection.size = Vector3.zero;
                m_boxForSelection.enabled = false;
                
            }
            updateUIForSelection();
        }
    }

    public bool needUpdateUI()
    {
        if (m_currentSelection.Count == 0)
            return false;


        System.Type sobjectRef = m_currentSelection[0].GetType();

        if (m_currentSelection[0].BelongsTo == null || m_currentSelection[0].BelongsTo is PlayerEnnemy)
            return false;

        System.Type playerRef = m_currentSelection[0].BelongsTo.GetType();

        for(int i = 1; i< m_currentSelection.Count; i++)
        {
            
            if (m_currentSelection[i].GetType() != sobjectRef || m_currentSelection[i].BelongsTo.GetType() != playerRef)
                return false;

            if (m_currentSelection[i] is SWorkers == false)
                return false;

            if (m_currentSelection[0] is SWorkers == false)
                return false;
        }

        return true;
    }

    public void updateUIForSelection()
    {
        //To set the ID Sobject
        if (m_currentSelection.Count > 0)
        {
            UIManager.Instance.enableDisableIDSObject();
            UIManager.Instance.setIDSObject(m_currentSelection[0]);
        }


        //To set the UI creation button
        if (needUpdateUI())
            m_currentSelection[0].updateUI();
        else
            UIManager.Instance.setCreationButton(null, new List<CreationImprovement>());

    }

    public void removeFromCurrentSelection(SObject selectableObject)
    {
        if(m_currentSelection.Contains(selectableObject))
        {
            selectableObject.unSelect();
            m_currentSelection.Remove(selectableObject);
        }
    }

    private void cleanCurrentSelection()
    {

        if(m_currentSelection != null)
            foreach(SObject selectableObject in m_currentSelection)
            {
                //check if it has been destroyed 
                if(selectableObject != null)
                    selectableObject.unSelect();
            }

        m_currentSquad = null;
        UIManager.Instance.enableDisableIDSObject(false);
        m_currentSelection.Clear();
    }

    public void addToCurrentSelection(SObject sobject)
    {
        if (sobject.BelongsTo == this)
        {
            if (m_currentSquad == null)
                m_currentSquad = new Squad();

            if (sobject is SMovable)
            {
                SMovable smovable = (SMovable)sobject;
                smovable.setBelongsSquad(m_currentSquad);
            }
        }    

        sobject.defineColorSObject();
        sobject.isSelect();
        m_currentSelection.Add(sobject);
    }

    #endregion


    #region construction_tool
    public void initConstructionBuilding(GameObject buildingGO, Vector3 initPosition)
    {
        m_buildingInCreationGO = (GameObject)Instantiate(buildingGO, initPosition, Quaternion.identity); ;

        //it's not necessary to make any check, because they were made before in beginCreateBuilding()
        m_sbuildingInCreation = m_buildingInCreationGO.GetComponent<SBuilding>();
        m_sbuildingInCreation.IsInConstruction = true;
        if (m_buildingInCreationGO.TryGetComponent(out Rigidbody rb))
            rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;

    }

    private void plotBuildingOnMouse()
    {
        LayerMask mask = LayerMask.GetMask("Floor");
        RaycastHit rayHit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out rayHit, Mathf.Infinity, mask))
        {
            Vector3 point = rayHit.point;
            //Is the construction possible ?
            if (Utilities.isPositionAvailable2(m_buildingInCreationGO, point, m_sbuildingInCreation.Radius * 0.5f))
            {
                m_constructionIsAvailable = true;
                if (m_buildingInCreationGO.TryGetComponent(out MeshRenderer meshRenderer))
                {
                    /*Here you have to create an array to define meshRenderer.materials, 
                      Indeed, if you take a foreach(mat in meshRenderer.materials), nothing 
                      will change, the component of meshRenderer.materials are not references
                      therefore you must change the whole array once. 
                      https://answers.unity.com/questions/124794/how-to-replace-materials-in-the-materials-array.html*/

                    //TODO could be optimize by using an attributes
                    Material[] mats = new Material[meshRenderer.materials.Length];
                    for (int i = 0; i < meshRenderer.materials.Length; i++)
                        mats[i] = GameManager.Instance.MatBuildingCreationAvailable;

                    meshRenderer.materials = mats;
                }
                else
                    Debug.LogWarning("Unable to find the material component of " + m_sbuildingInCreation.ID);
            }
            else
            {
                m_constructionIsAvailable = false;
                if (m_buildingInCreationGO.TryGetComponent(out MeshRenderer meshRenderer))
                {
                    //TODO could be optimize by using an attributes
                    Material[] mats = new Material[meshRenderer.materials.Length];
                    for (int i = 0; i < meshRenderer.materials.Length; i++)
                        mats[i] = GameManager.Instance.MatBuildingCreationNotAvailable;

                    meshRenderer.materials = mats;
                }
                else
                    Debug.LogWarning("Unable to find the material component of " + m_sbuildingInCreation.ID);
            }

            m_buildingInCreationGO.transform.position = new Vector3(point.x, m_buildingInCreationGO.transform.position.y, point.z);
        }
    }

    private void beginConstruction()
    {
        if (Input.GetMouseButton(0) && m_constructionIsAvailable)
        {
            /*this line must be before the next one, indeed if we call constructBuilding() 
             * it will try to reach the destinationPoints which are set to 0 without this line*/
            m_sbuildingInCreation.definePointsDestination();
            //TODO : change the 0 index for m_currentSelection
            if (m_currentSelection != null)
                if (m_currentSelection[0].TryGetComponent(out SWorkers sworkers))
                    sworkers.beginConstructBuilding(m_buildingInCreationGO); //UIManager.Instance.BuildingCreated);

            m_buildingInCreationGO = null;
        }
    }
    #endregion

    private void actionCurrentSelection(RaycastHit rayHit)
    {
        if (m_currentSelection != null)
        {
            if (m_currentSquad != null)
                m_currentSquad.DirectionIsSet = false;

            foreach (SObject sobject in m_currentSelection)
                if (sobject.BelongsTo == this)
                {
                    sobject.onClick(rayHit);
                }
        }
    }

    public void removeFromCurrentSquad(SMovable smovable)
    {
        if (m_currentSquad.SquadSMovable.Contains(smovable))
            m_currentSquad.SquadSMovable.Remove(smovable);

    }
}

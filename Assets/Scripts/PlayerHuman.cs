using Packages.Rider.Editor.PostProcessors;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerHuman : Player
{
    private GameObject m_beginCreation = null;

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

    #region getter
    public bool MouseIsHold { get => m_mouseIsHold; set => m_mouseIsHold = value; }
    public GameObject BeginCreation { get => m_beginCreation; set => m_beginCreation = value; }
    public bool EnableSelection { get => m_enableSelection; set => m_enableSelection = value; }
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
            

        if (m_beginCreation != null)
        {
            if (Input.GetMouseButton(0))
            {
                /*this line must be before the next one, indeed if we call constructBuilding() 
                 * it will try to reach the destinationPoints which are set to 0 without this line*/
                if (UIManager.Instance.BuildingCreated.TryGetComponent(out SObject sobject))
                    sobject.definePointsDestination();

                //TODO : change the 0 index for m_currentSelection
                if (m_currentSelection != null)
                    if (m_currentSelection[0].TryGetComponent(out SWorkers sworkers))
                        sworkers.beginConstructBuilding(UIManager.Instance.BuildingCreated);

                UIManager.Instance.BuildingCreated = null;
                m_beginCreation = null;
                //m_enableSelection = true;
            }
        }

    }

    #region selection_tool

    private bool selectIsEnable()
    {

        if (m_beginCreation != null)
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
        }
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
                selectableObject.unSelect();
            
        
        m_currentSelection.Clear();
    }

    public void addToCurrentSelection(SObject sObject)
    {
        if (sObject.BelongsTo == this)
        {
            //if it belongs to the player we update the UI
            sObject.updateUI();
            sObject.setColorCursor(Color.blue);
        }    
        else if (sObject.BelongsTo is PlayerEnnemy)
            sObject.setColorCursor(Color.red);
        else if (sObject.IsNeutral == true)
            sObject.setColorCursor(Color.grey);
        else
            Debug.LogWarning("Unknown belonging for " + sObject.ID + " cannot set color of cursor");

        sObject.isSelect();
        m_currentSelection.Add(sObject);

        
        //print("selectable = " + selectableObject.gameObject.name);
    }
    #endregion

    private void actionCurrentSelection(RaycastHit rayHit)
    {
        if (m_currentSelection != null)
            foreach (SObject sobject in m_currentSelection)
                if(sobject.BelongsTo == this)
                    sobject.onClick(rayHit);
    }


}

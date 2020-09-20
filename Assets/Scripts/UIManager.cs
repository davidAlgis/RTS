using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager m_instance;

    [SerializeField]
    private Canvas m_mainUICanvas = default;

    private GameObject[] m_creationButtonsGO = new GameObject[18];
    private Image[] m_creationButtonsImage = new Image[18];
    private GameObject m_queueButtonsPanel;
    private List<GameObject> m_creationQueueGO = new List<GameObject>();
    private GameObject m_buildingCreated = null;
    private SBuilding m_sbuildingCreated;
    private GameObject m_defaultLineRendererGO;
    [SerializeField]
    private RawImage m_selectionImage;
    private Image m_sobjectRepresentation;
    private Text m_sobjectName;
    private Text m_sobjectHealth;
    private Text[] m_textResources;
    private SObject m_updateIDPanelWithSobject;
    public static UIManager Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = GameObject.Find("Manager").GetComponent<UIManager>();
            }
            return m_instance;
        }
    }

    public GameObject BuildingCreated { get => m_buildingCreated; set => m_buildingCreated = value; }
    public GameObject DefaultLineRendererGO { get => m_defaultLineRendererGO; set => m_defaultLineRendererGO = value; }
    public SBuilding SbuildingCreated { get => m_sbuildingCreated; set => m_sbuildingCreated = value; }

    private void Awake()
    {
        if (m_mainUICanvas == null)
        {
            Debug.LogError("Unable to find the main UI");
            return;
        }

        if (DebugTool.tryFindGOChildren(m_mainUICanvas.gameObject, "MultiSelectionImage", out GameObject multiSelectionGO))
            if (multiSelectionGO.TryGetComponent(out m_selectionImage) == false)
                Debug.LogWarning("Unable to find any spriteRenderer component on " + multiSelectionGO.name);
            else
                m_selectionImage.enabled = false;

        DebugTool.tryFindGOChildren(m_mainUICanvas.gameObject, "Panel/CreationGridButton", out GameObject gridButtonCreationGO);

        for(int i = 0; i< 18; i++)
        {
            DebugTool.tryFindGOChildren(gridButtonCreationGO, "CreationButton (" + i + ")", out m_creationButtonsGO[i]);
            DebugTool.tryFindGOChildren(m_creationButtonsGO[i], "CreationButtonImage (" + i + ")", out GameObject tempGO);
            if(tempGO.TryGetComponent(out m_creationButtonsImage[i]) == false)
                Debug.LogWarning("Unable to find any image component on " + tempGO.name);
        }
            


        DebugTool.tryFindGOChildren(m_mainUICanvas.gameObject, "Panel/Ressources", out GameObject verticalLayoutRessourcesGO);

        const uint ressourcesCount = GameManager.m_ressourcesCount;
        m_textResources = new Text[ressourcesCount];

        for (uint i=0; i< ressourcesCount; i++)
            if (DebugTool.tryFindGOChildren(verticalLayoutRessourcesGO, "RessourcesButton (" + i + ")/RessourcesButtonText (" + i + ")", out GameObject textGO))
                if (textGO.TryGetComponent(out m_textResources[i]) == false)
                    Debug.LogWarning("Unable to find any text component in " + textGO.name);

        DebugTool.tryFindGOChildren(m_mainUICanvas.gameObject, "Panel/CreationQueueButton", out m_queueButtonsPanel);

        m_defaultLineRendererGO = UnityEngine.Resources.Load("DefaultLineRenderer", typeof(GameObject)) as GameObject;

        if (m_defaultLineRendererGO == null)
            Debug.LogWarning("Unable to load the DefaultLineRenderer");

        GameObject IDSobjectGO;
        if (DebugTool.tryFindGOChildren(m_mainUICanvas.gameObject, "Panel/IDSobject", out IDSobjectGO))
        {
            DebugTool.tryFindGOChildren(IDSobjectGO, "SObjectName", out GameObject sobjectNameGO);
            if(sobjectNameGO.TryGetComponent(out m_sobjectName) == false)
                Debug.LogWarning("Unable to find any text component in " + sobjectNameGO.name);

            DebugTool.tryFindGOChildren(IDSobjectGO, "SObjectHealth", out GameObject sobjectHealthGO);
            if (sobjectHealthGO.TryGetComponent(out m_sobjectHealth) == false)
                Debug.LogWarning("Unable to find any text component in " + sobjectHealthGO.name);

            DebugTool.tryFindGOChildren(IDSobjectGO, "SObjectRepresentation", out GameObject sobjectRepresentationGO);
            if (sobjectRepresentationGO.TryGetComponent(out m_sobjectRepresentation) == false)
                Debug.LogWarning("Unable to find any text component in " + sobjectRepresentationGO.name);
        }


        enableDisableIDSObject(false);
        m_updateIDPanelWithSobject = null;
    }

    public void plotSelector(Vector3 extremity1, Vector3 extremity2)
    {
        m_selectionImage.enabled = true;
        RectTransform rectTransform = m_selectionImage.gameObject.GetComponent<RectTransform>();
        Vector3 center = 0.5f * (extremity1 + extremity2);
        rectTransform.localPosition = center;
        rectTransform.sizeDelta = new Vector2(Mathf.Abs(extremity2.x - extremity1.x), Mathf.Abs(extremity2.y - extremity1.y));
    }

    public void unPlotSelector()
    {
        m_selectionImage.enabled = false;
    }

    public Vector2 convertMousePositionToCanvasPosition(Vector3 mousePosition)
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(m_mainUICanvas.transform as RectTransform, mousePosition, m_mainUICanvas.worldCamera, out pos);
        return pos;
    }

    public void setCreationButton(SObject sobject, List<CreationImprovement> creationButtons, bool isInteractable = true)
    {
        if(creationButtons!= null)
        {
            int index = 0;
            foreach(CreationImprovement creationButton in creationButtons)
            {
                if(index > 17)
                {
                    Debug.LogError("Index out of range to set creation button");
                    return;
                }

                if (m_creationButtonsGO[index].TryGetComponent(out Button button))
                {
                    button.onClick.RemoveAllListeners();

                    if (sobject is SBuilding)
                    {
                        SBuilding sbuilding = (SBuilding)sobject;
                        button.onClick.AddListener(() => sbuilding.addToQueue(creationButton));
                    }
                    else if(sobject is SWorkers)
                    {
                        button.onClick.AddListener(() => creationButton.method?.Invoke());

                    }
                    else
                        button.onClick.AddListener(() => creationButton.method?.Invoke());

                    //if he click on button we must send the informations of the buttons to the sobject
                    button.onClick.AddListener(() => sobject.setCurrentButtonCreation(creationButton));

                    
                    button.interactable = isInteractable;
                }



                m_creationButtonsImage[index].enabled = true;
                m_creationButtonsImage[index].sprite = creationButton.spriteButton;

                index++;
            }

            //the others buttons are not interactable
            for(int i = index; i < 18; i++)
            {
                m_creationButtonsImage[i].enabled = false;

                if (m_creationButtonsGO[i].TryGetComponent(out Button button))
                {
                    button.onClick.RemoveAllListeners();
                    button.interactable = false;
                }
                    
            }
        }
    }

    public void setIDSObject(SObject sobject)
    {
        if(sobject.Representation != null)
            m_sobjectRepresentation.sprite = sobject.Representation;
        
        if (sobject.name == "")
            m_sobjectName.text = "Unkwown Name";
        else
            m_sobjectName.text = sobject.Name;

        if (sobject is SResources)
        {
            SResources sresources = (SResources)sobject;
            m_sobjectHealth.text = sresources.Contains + " / " + sresources.ContainsInitial;
        }
        else
            m_sobjectHealth.text = sobject.Health + " / " + sobject.TotalHealth;

        m_updateIDPanelWithSobject = sobject;
    }

    public void enableDisableIDSObject(bool enable = true)
    {
        m_sobjectRepresentation.enabled = enable;
        m_sobjectName.enabled = enable;
        m_sobjectHealth.enabled = enable;
        if (enable == false)
            m_updateIDPanelWithSobject = null;
    }

    public void setDefaultCreationButton()
    {
        for (int i = 0; i < 18; i++)
        {
            if (m_creationButtonsGO[i].TryGetComponent(out Button button))
            {
                button.onClick.RemoveAllListeners();
                button.interactable = false;
            }

            m_creationButtonsImage[i].enabled = false;
        }
    }

    public void addQueueButton(CreationImprovement creationImprovement)
    {

        GameObject button = new GameObject();
        button.transform.SetParent(m_queueButtonsPanel.transform,false);
        button.AddComponent<RectTransform>();
        button.AddComponent<Button>();
        Image image = button.AddComponent<Image>();
        image.sprite = creationImprovement.spriteButton;
        m_creationQueueGO.Add(button);
    }

    public void dequeueButton(int index)
    {

        GameObject go = m_creationQueueGO[index];
        m_creationQueueGO.RemoveAt(index);
        Destroy(go);
    }

    public void setQueueButton(Queue<CreationImprovement> queueCreationImprovement)
    {
        if (m_creationQueueGO != null)
            foreach (GameObject creationQueue in m_creationQueueGO)
                Destroy(creationQueue);
        
        m_creationQueueGO.Clear();
        
        if(queueCreationImprovement != null)
            foreach(CreationImprovement creationImprovement in queueCreationImprovement)
                addQueueButton(creationImprovement);
            
    }

    public void updateRessourcesCount(Player player)
    {
        m_textResources[0].text = "Food : " + player.Resources.food;
        m_textResources[1].text = "Wood : " + player.Resources.wood;
        m_textResources[2].text = "Gold : " + player.Resources.gold;
        m_textResources[3].text = "Rock : " + player.Resources.rock;

    }

    public bool IsPointerOverUIElement()
    {
        var eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }

    public void blinkSelectionField(SObject sobject)
    {
        sobject.defineColorSObject();
        StartCoroutine(blinkSelectionFieldCoroutine(sobject));
    }

    private IEnumerator blinkSelectionFieldCoroutine(SObject sobject)
    {
        
        sobject.LineRenderer.enabled = true;
        yield return new WaitForSeconds(0.2f);
        sobject.LineRenderer.enabled = false;
        yield return new WaitForSeconds(0.2f);
        sobject.LineRenderer.enabled = true;
        yield return new WaitForSeconds(0.2f);
        sobject.LineRenderer.enabled = false;
    }

    public void Update()
    {
        updateRessourcesCount(GameManager.Instance.CurrentPlayer);


        #region construction 
        if (m_buildingCreated != null)
        {
            LayerMask mask = LayerMask.GetMask("Floor");
            LayerMask maskSelectable = LayerMask.GetMask("Selectable");

            RaycastHit rayHit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out rayHit, Mathf.Infinity, mask))
            {

                Vector3 point = rayHit.point;
                //Is the construction possible ?
                if (Utilities.isPositionAvailable2(m_buildingCreated, point, m_sbuildingCreated.Radius*0.5f))
                {
                    GameManager.Instance.CurrentPlayer.ConstructionIsAvailable = true;
                    if (UIManager.Instance.BuildingCreated.TryGetComponent(out MeshRenderer meshRenderer))
                        meshRenderer.material = GameManager.Instance.MatBuildingCreationAvailable;
                    else
                        Debug.LogWarning("Unable to find the material component of " + UIManager.Instance.BuildingCreated.name);
                }
                else
                {
                    GameManager.Instance.CurrentPlayer.ConstructionIsAvailable = false;
                    if (UIManager.Instance.BuildingCreated.TryGetComponent(out MeshRenderer meshRenderer))
                        meshRenderer.material = GameManager.Instance.MatBuildingCreationNotAvailable;
                    else
                        Debug.LogWarning("Unable to find the material component of " + UIManager.Instance.BuildingCreated.name);
                }

                m_buildingCreated.transform.position = new Vector3(point.x, m_buildingCreated.transform.position.y, point.z) ;
            }
            
        }
        #endregion

        #region IDPanel

        if(m_updateIDPanelWithSobject != null)
        {
            if(m_updateIDPanelWithSobject is SResources)
            {
                SResources sresources = (SResources)m_updateIDPanelWithSobject;
                m_sobjectHealth.text = sresources.Contains + " / " + sresources.ContainsInitial;
            }
            else
                m_sobjectHealth.text = m_updateIDPanelWithSobject.Health + " / " + m_updateIDPanelWithSobject.TotalHealth;

        }
        #endregion

    }
}

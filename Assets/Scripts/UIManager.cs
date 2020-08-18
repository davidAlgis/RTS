﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager m_instance;

    [SerializeField]
    private Canvas m_mainUICanvas;

    GameObject[] m_creationButtonsGO = new GameObject[18];
    Image[] m_creationButtonsImage = new Image[18];
    GameObject m_queueButtonsPanel;
    List<GameObject> m_creationQueueGO = new List<GameObject>();

    [SerializeField]
    private RawImage m_selectionImage;

    Text[] m_textRessources;

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
        m_textRessources = new Text[ressourcesCount];

        for (uint i=0; i< ressourcesCount; i++)
            if (DebugTool.tryFindGOChildren(verticalLayoutRessourcesGO, "RessourcesButton (" + i + ")/RessourcesButtonText (" + i + ")", out GameObject textGO))
                if (textGO.TryGetComponent(out m_textRessources[i]) == false)
                    Debug.LogWarning("Unable to find any text component in " + textGO.name);

        DebugTool.tryFindGOChildren(m_mainUICanvas.gameObject, "Panel/CreationQueueButton", out m_queueButtonsPanel);


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

    public void setCreationButton(List<CreationImprovement> creationButtons, SBuilding sbuilding = null)
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

                    if (sbuilding != null)
                        button.onClick.AddListener(() => sbuilding.addToQueue(creationButton));
                    else
                        button.onClick.AddListener(() => creationButton.method?.Invoke());

                    button.interactable = true;
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
                    button.interactable = false;
            }
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
        m_textRessources[0].text = "Food : " + player.Food.ToString();
        m_textRessources[1].text = "Wood : " + player.Wood.ToString();
        m_textRessources[2].text = "Gold : " + player.Gold.ToString();
        m_textRessources[3].text = "Rock : " + player.Rock.ToString();

    }

}

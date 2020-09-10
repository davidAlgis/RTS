using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private PlayerHuman m_currentPlayer;

    private static GameManager m_instance;
    private Neutral m_neutral = new Neutral();
    public const uint m_ressourcesCount = 4;
    [SerializeField]
    private Material m_matBuildingCreationAvailable;
    [SerializeField]
    private Material m_matBuildingCreationNotAvailable;
    private LayerMask m_layerSelection;
    public static GameManager Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = GameObject.Find("Manager").GetComponent<GameManager>();
            }
            return m_instance;
        }
    }

    public Neutral Neutral { get => m_neutral; set => m_neutral = value; }
    public PlayerHuman CurrentPlayer { get => m_currentPlayer; set => m_currentPlayer = value; }
    public Material MatBuildingCreationAvailable { get => m_matBuildingCreationAvailable; set => m_matBuildingCreationAvailable = value; }
    public LayerMask LayerSelection { get => m_layerSelection; set => m_layerSelection = value; }
    public Material MatBuildingCreationNotAvailable { get => m_matBuildingCreationNotAvailable; set => m_matBuildingCreationNotAvailable = value; }

    public void Awake()
    {
        m_layerSelection = LayerMask.GetMask("Selectable");
    }

    public void Update()
    {
        
        UIManager.Instance.updateRessourcesCount(m_currentPlayer);
    }

}

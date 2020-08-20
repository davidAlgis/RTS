using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private Player m_currentPlayer;

    private static GameManager m_instance;
    private Neutral m_neutral = new Neutral();
    public const uint m_ressourcesCount = 4;
    [SerializeField]
    private Material m_matBuildingCreation;
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
    public Player CurrentPlayer { get => m_currentPlayer; set => m_currentPlayer = value; }
    public Material MatBuildingCreation { get => m_matBuildingCreation; set => m_matBuildingCreation = value; }

    public void Update()
    {
        UIManager.Instance.updateRessourcesCount(m_currentPlayer);
    }
}

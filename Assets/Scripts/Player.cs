using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private uint m_wood;
    [SerializeField]
    private uint m_rock;
    [SerializeField]
    private uint m_gold;
    [SerializeField]
    private uint m_food;
    private uint m_nbrSObject = 0;
    public uint Wood { get => m_wood; set => m_wood = value; }
    public uint Rock { get => m_rock; set => m_rock = value; }
    public uint Gold { get => m_gold; set => m_gold = value; }
    public uint Food { get => m_food; set => m_food = value; }
    public uint NbrSObject { get => m_nbrSObject; set => m_nbrSObject = value; }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    protected virtual void Awake()
    {
        initRessources();

    }

    protected virtual void initRessources()
    {
        m_food = 100;
        m_gold = 100;
        m_wood = 100;
        m_rock = 100;

    }
}

public enum RessourcesType
{
    wood,
    gold,
    rock,
    food
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private Resources m_resources = new Resources();

    private uint m_nbrSObject = 0;
    [SerializeField]
    private SBuilding m_motherHouse;

    public uint NbrSObject { get => m_nbrSObject; set => m_nbrSObject = value; }
    public Resources Resources { get => m_resources; set => m_resources = value; }
    public SBuilding MotherHouse { get => m_motherHouse; set => m_motherHouse = value; }


    protected virtual void Awake(){}

    public bool canBuySobject(SObject sobject)
    {
        if (m_resources < sobject.CostResources)
        {
            print("Player has not enough money to buy the " + sobject.ID);
            return false;
        }
        else
        {
            m_resources -= sobject.CostResources;
            return true;
        }
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SResources : SUnmovable
{
    [SerializeField]
    private Resources m_contains;

    public Resources Contains { get => m_contains; set => m_contains = value; }

    /*Here we suppose that m_contains is a multiple of nbr.
     Therefore when a sworkers get resources it'll always 
     return resources(nbr) */
    public Resources getResources(uint nbr)
    {
        Resources result = Resources.realSubstract(m_contains, new Resources(nbr));
        m_contains -= nbr;

        //If the resources is empty we destroy the gameobject.
        if(m_contains == new Resources())
            Destroy(gameObject);
        

        return result;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SRessources : SUnmovable
{
    [SerializeField]
    private uint m_contains;
    [SerializeField]
    private RessourcesType m_type;

    public uint Contains { get => m_contains; set => m_contains = value; }
    public RessourcesType Type { get => m_type; set => m_type = value; }

    public uint getRessources(uint nbr)
    {
        if(Contains - nbr >=0)
        {
            Contains -= nbr;
            return nbr;
        }
        else
        {
            Destroy(gameObject);
            return Contains;
        }
    }
}

﻿using UnityEngine;

public class SResources : SUnmovable
{
    [SerializeField]
    private Resources m_contains;
    private Resources m_containsInitial;

    private int m_stepHarvest;
    [SerializeField]
    private Mesh[] m_stepHarvestMesh = new Mesh[4];
    private int m_lengthStepHarvest;

    public Resources Contains { get => m_contains; }
    public Resources ContainsInitial { get => m_containsInitial; set => m_containsInitial = value; }

    protected override void Awake()
    {
        base.Awake();
        m_containsInitial = new Resources(m_contains.wood, m_contains.gold, m_contains.rock, m_contains.food);
        m_lengthStepHarvest = m_stepHarvestMesh.Length;
        m_stepHarvest = m_lengthStepHarvest;
    }

    /*Here we suppose that m_contains is a multiple of nbr.
     Therefore when a sworkers get resources it'll always 
     return resources(nbr) */
    public Resources getResources(uint nbr, Player belongTo)
    {
        if (m_contains == new Resources())
        {
            Destroy(gameObject);
            Debug.Log("The resources is empty");
            return new Resources();
        }

        Resources result = Resources.realSubstract(m_contains, new Resources(nbr));
        m_contains -= nbr;
        belongTo.Resources += result;

        if (m_contains <= m_containsInitial * ((float)m_stepHarvest / (float)m_lengthStepHarvest))
        {
            if (m_lengthStepHarvest - m_stepHarvest >= m_stepHarvestMesh.Length)
            {
                Debug.LogWarning("try to access outside of the allocated memory");
                return result;
            }

            Mesh mesh = m_stepHarvestMesh[m_lengthStepHarvest - m_stepHarvest];

            if (mesh == null)
            {
                mesh = GetComponent<MeshFilter>().mesh;
                Debug.LogWarning("The m_stepHarvestMesh of " + ID + " as not been set");
                return result;

            }

            /*If the ressources is a tree we have to change the shader of the leaf 
              and set it as the bark shader, indeed if we dont do this the texture 
              trunk will have some leaves on it.*/
            if (TryGetComponent(out Tree tree) && m_stepHarvest == m_lengthStepHarvest - 1)
            {
                Material[] materials = GetComponent<Renderer>().materials;

                if (materials.LongLength < 2)
                    Debug.LogWarning("Thz size of shader on the tree is anormal.");
                else
                    materials[1].shader = materials[0].shader;
                
            }

            //change the mesh to the next step
            GetComponent<MeshFilter>().sharedMesh = mesh;


            m_stepHarvest--;

        }

        //If the resources is empty we destroy the gameobject.
        if (m_contains == new Resources())
            Debug.Log("The resources is empty");


        return result;
    }
}

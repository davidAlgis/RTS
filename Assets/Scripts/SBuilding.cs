using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class SBuilding : SUnmovable
{

    private Queue<CreationImprovement> m_queueCreation = new Queue<CreationImprovement>();
    private bool m_creationOnGoing = false;
    [SerializeField]
    private Mesh[] m_stepConstructionMesh = new Mesh[4];
    [SerializeField]
    private bool m_isInConstruction;
    /*state of construction is between 0 and 1 
     * 0 means the begin of construction and 1 the end*/
    [SerializeField]
    private float m_stateOfConstruction = 0.0f;
    private uint m_indexOfConstructionMesh = 0;
    private Material m_initMaterial;
    private Mesh m_initMeshBuilding;
    private uint m_builderOnConstruction = 0;

    public Queue<CreationImprovement> QueueCreation { get => m_queueCreation; set => m_queueCreation = value; }
    public Mesh[] StepConstructionMesh { get => m_stepConstructionMesh; set => m_stepConstructionMesh = value; }
    public bool IsInConstruction { get => m_isInConstruction; set => m_isInConstruction = value; }
    public Material InitMaterial { get => m_initMaterial; }
    public float StateOfConstruction { get => m_stateOfConstruction; set => m_stateOfConstruction = value; }
    public uint IndexOfConstructionMesh { get => m_indexOfConstructionMesh; set => m_indexOfConstructionMesh = value; }
    public Mesh InitMeshBuilding { get => m_initMeshBuilding; set => m_initMeshBuilding = value; }
    public uint BuilderOnConstruction { get => m_builderOnConstruction; set => m_builderOnConstruction = value; }

    protected override void Awake()
    {
        base.Awake();
        if (m_stepConstructionMesh != null)
            foreach (Mesh meshFilter in m_stepConstructionMesh)
                if (meshFilter == null)
                    Debug.LogWarning("The mesh of the step of construction of " + gameObject.name + " have not been defined");

        if (TryGetComponent(out MeshRenderer meshRenderer))
            m_initMaterial = meshRenderer.material;
        else
            Debug.LogWarning("Unable to find the material component of " + gameObject.name);

        //if we doesn't find any mesh
        if (TryGetComponent(out MeshFilter meshFilterInit) == false)
        {
            Debug.LogWarning("Unable to find any MeshFilter component on " + gameObject.name);
            return;
        }
        //save the mesh of the full building
        m_initMeshBuilding = meshFilterInit.mesh;
    }

    public void addToQueue(CreationImprovement buttonImage)
    {
        m_queueCreation.Enqueue(buttonImage);
        UIManager.Instance.addQueueButton(buttonImage);
        if (m_creationOnGoing == false)
            StartCoroutine(treatQueue());
    }

    public void dequeue()
    {
        m_queueCreation.Dequeue();
        UIManager.Instance.dequeueButton(0);
    }

    public void createUnit(GameObject sunitGO)
    {

        if (sunitGO.TryGetComponent(out SUnit sunit) == false)
        {
            Debug.LogError("Building try to create something else than a sunit");
            return;
        }

        sunit.BelongsTo = GameManager.Instance.CurrentPlayer;
        Instantiate(sunitGO, PointsDestinationNavMesh[3].first, Quaternion.identity);
    }

    IEnumerator treatQueue()
    {
        m_creationOnGoing = true;
        while (m_queueCreation.Count > 0)
        {
            yield return new WaitForSeconds(m_queueCreation.Peek().sobject.DurationCreation);
            m_queueCreation.Peek().method.Invoke();
            dequeue();
        }
        m_creationOnGoing = false;
    }

    public override void updateUI(bool isInteractable = true)
    {
        if(isInteractable && m_isInConstruction == false)
        {
            UIManager.Instance.setCreationButton(this, m_buttonCreation);
            UIManager.Instance.setQueueButton(m_queueCreation);
        }
        else
        {
            UIManager.Instance.setCreationButton(this, m_buttonCreation, false);
            UIManager.Instance.setQueueButton(null);
        }
    }

}

public enum CreationType
{
    improvement, 
    unit
}

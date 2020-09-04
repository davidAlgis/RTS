using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class SObject : MonoBehaviour
{
    private GameObject m_cursorGO;
    private Renderer m_cursorRenderer;
    private LineRenderer m_lineRenderer;
    [SerializeField]
    protected Player m_belongsTo;
    protected string m_name;
    [SerializeField]
    private bool m_isNeutral = false;
    private string m_ID;
    /*the radius of the smaller circle which contains the object.
    this circle is used to define the destination of the nav mesh.
    To avoid agent which cannot reach is destination*/
    [SerializeField]
    protected float m_radius = 0f;
    private List<Pair<Vector3, bool>> m_pointsDestionationNavMesh = new List<Pair<Vector3, bool>>();
    [SerializeField]
    private FieldSelection m_fieldType;
    [SerializeField]
    protected uint m_nbrUnitMaxOnObject = 4;
    protected uint m_nbrUnitOnObject = 0;
    [SerializeField]
    protected List<CreationImprovement> m_buttonCreation = new List<CreationImprovement>();
    protected CreationImprovement m_currentButtonCreation;
    [SerializeField]
    private uint m_health = 100;
    [SerializeField]
    private float m_durationCreation = 0.0f;

    #region getter
    public Player BelongsTo { get => m_belongsTo; set => m_belongsTo = value; }
    public List<Pair<Vector3, bool>> PointsDestinationNavMesh { get => m_pointsDestionationNavMesh; set => m_pointsDestionationNavMesh = value; }
    public bool IsNeutral { get => m_isNeutral; set => m_isNeutral = value; }
    public string ID { get => m_ID; set => m_ID = value; }
    public uint Health { get => m_health; }
    public float DurationCreation { get => m_durationCreation; set => m_durationCreation = value; }
    public float Radius { get => m_radius; set => m_radius = value; }
    #endregion



    protected virtual void Awake()
    {
        if (checkCoherency() == false)
            Debug.LogWarning(gameObject.name + " isn't coherent");

        definePointsDestination();
        
        updateNbrOfSobject();
        defineID();

        if (DebugTool.tryFindGOChildren(gameObject, "Cursor", out m_cursorGO, LogType.Error) == false)
            return;

        if (m_cursorGO.TryGetComponent(out m_cursorRenderer) == false)
            Debug.LogWarning("Unable to find any renderer component on cursor of " + gameObject.name);
        m_cursorGO.SetActive(false);
    }

    public void Start()
    {
        /*this function has to be executed in start, because 
         * it have to wait the load ressources of UIManager in his awake*/
        defineSelectionField(); 
    }

    public void defineSelectionField()
    {
        if(UIManager.Instance.DefaultLineRendererGO == null)
        {
            Debug.LogWarning("Unable to instantiate UIManager.Instance.DefaultLineRendererGO in " + gameObject.name);
            return; 
        }

        GameObject selectionFieldGO = Instantiate(UIManager.Instance.DefaultLineRendererGO);
        //selectionFieldGO.transform.position = Vector3.zero;
        selectionFieldGO.transform.position = transform.position;// new Vector3(transform.position.x, 0.1f, transform.position.z);// new Vector3(0.0f , 0.2f, 0.0f);

        selectionFieldGO.transform.parent = gameObject.transform;
        selectionFieldGO.name = gameObject.name + "SelectionField";

        selectionFieldGO.layer = 9;//GameManager.Instance.LayerSelection;
        if (selectionFieldGO.TryGetComponent(out m_lineRenderer) == false)
        {
            Debug.LogWarning("Unable to find the lineRenderer component " + selectionFieldGO.name);
            return;
        }

        
        switch (m_fieldType)
        {
            case FieldSelection.circle:
                Utilities.drawCircleContour(m_lineRenderer, transform.position, m_radius);
                CapsuleCollider capsuleCollider = selectionFieldGO.AddComponent<CapsuleCollider>();
                capsuleCollider.isTrigger = true;
                capsuleCollider.radius = m_radius;
                break;
            case FieldSelection.square:

                Utilities.drawSquareContour(m_lineRenderer, transform.position, m_radius);
                BoxCollider boxCollider = selectionFieldGO.AddComponent<BoxCollider>();
                boxCollider.isTrigger = true;
                float sizeRadius = m_radius * Mathf.Sqrt(2.0f);
                boxCollider.size = new Vector3(sizeRadius, sizeRadius, sizeRadius);
                break;
            default:
                Debug.LogError("Unknown field type for " + gameObject.name);
                break;
        }
        m_lineRenderer.enabled = false;
    }

    public void setCurrentButtonCreation(CreationImprovement creationImprovement)
    {
        m_currentButtonCreation = creationImprovement;
    }

    private uint updateNbrOfSobject()
    {
        if (m_isNeutral)
            return GameManager.Instance.Neutral.NbrSObject++;
        else
            return m_belongsTo.NbrSObject++;
    }

    protected virtual bool checkCoherency()
    {
        bool coherency = true;
        if(m_isNeutral && m_belongsTo != null)
        {
            Debug.LogWarning(gameObject.name + " is neutral but belongs to " + m_belongsTo.gameObject.name);
            Debug.LogWarning("We set it to neutral by default");
            m_belongsTo = null;
            coherency = false;
        }

        if (m_isNeutral == false && m_belongsTo == null)
        {
            Debug.LogWarning(gameObject.name + " isn't neutral and doesn't belongs any Player");
            Debug.LogWarning("We set it to neutral by default");
            m_isNeutral = true;
            coherency = false;
        }


        if (m_radius == 0.0f)
            Debug.LogWarning("The radius of " + gameObject.name + " is set to 0");
        
        return coherency;
    }

    public void setColorCursor(Color color)
    {
        m_lineRenderer.startColor = color;
        m_lineRenderer.endColor = color;
        m_cursorRenderer.material.SetColor("_Color", color);
    }

    private void defineID()
    {
        if (m_isNeutral)
            m_ID = m_name + "_Neutral_" + GameManager.Instance.Neutral.NbrSObject;
        else
            m_ID = m_name + "_" + m_belongsTo.gameObject + "_" + m_belongsTo.NbrSObject; 
            
    }

    public void definePointsDestination()
    {
        float left, up, right, down, height;
        left = transform.position.x - m_radius / Mathf.Sqrt(2);
        right = transform.position.x + m_radius / Mathf.Sqrt(2);
        up = transform.position.z + m_radius / Mathf.Sqrt(2);
        down = transform.position.z - m_radius / Mathf.Sqrt(2);
        height = transform.position.y;

        m_pointsDestionationNavMesh.Add(new Pair<Vector3, bool>(new Vector3(left, height, up), false));
        m_pointsDestionationNavMesh.Add(new Pair<Vector3, bool>(new Vector3(left, height, down), false));
        m_pointsDestionationNavMesh.Add(new Pair<Vector3, bool>(new Vector3(right, height, up), false));
        m_pointsDestionationNavMesh.Add(new Pair<Vector3, bool>(new Vector3(right, height, down), false));
        
        if(m_radius > 2.0f)
        {
            m_pointsDestionationNavMesh.Add(new Pair<Vector3, bool>(new Vector3(transform.position.x, height, up), false));
            m_pointsDestionationNavMesh.Add(new Pair<Vector3, bool>(new Vector3(transform.position.x, height, down), false));
            m_pointsDestionationNavMesh.Add(new Pair<Vector3, bool>(new Vector3(left, height, transform.position.z), false));
            m_pointsDestionationNavMesh.Add(new Pair<Vector3, bool>(new Vector3(right, height, transform.position.z), false));
        }
        /*
        m_pointsDestinationNavMesh[0] = new Vector3(left, height, up);
        m_pointsDestinationNavMesh[1] = new Vector3(left, height, down);
        m_pointsDestinationNavMesh[2] = new Vector3(right, height, up);
        m_pointsDestinationNavMesh[3] = new Vector3(right, height, down);
        */
    }

    public void isSelect()
    {
        //add cursor on above the selectedObject      
        m_lineRenderer.enabled = true;
        m_cursorGO.SetActive(true);
    }

    public void unSelect()
    {
        m_lineRenderer.enabled = false;
        m_cursorGO.SetActive(false);
    }

    public virtual void onClick(RaycastHit rayHit)
    {

    }

    public bool damage(uint damage)
    {
        if (m_health - damage > 0)
        {
            m_health -= damage;
            return false;
        }
        else
        {
            Destroy(gameObject);
            return true; 
        }
            
    }

    public virtual void updateUI(bool isInteractable = true)
    {
        if(isInteractable)
            UIManager.Instance.setCreationButton(this, m_buttonCreation);
        else
            UIManager.Instance.setCreationButton(this, m_buttonCreation, false);
    }
}

public enum FieldSelection
{
    square, 
    circle
}

public enum Agent
{
    player, 
    ennemy,
    neutral
}

[System.Serializable]
public struct CreationImprovement
{
    public Sprite spriteButton;
    //public float duration;
    public SObject sobject;
    //the methods used here can only have less than 2 parameters.
    public UnityEvent method;

}
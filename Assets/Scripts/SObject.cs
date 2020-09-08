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
    private List<Vector3> m_pointsDestionationNavMesh = new List<Vector3>();
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
    public List<Vector3> PointsDestinationNavMesh { get => m_pointsDestionationNavMesh; set => m_pointsDestionationNavMesh = value; }
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
        selectionFieldGO.layer = LayerMask.NameToLayer("Selectable");

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
        float greaterRadius = m_radius + 0.5f;
        float x;

        switch (m_fieldType)
        {
            case FieldSelection.circle:

                for(x = 0.0f; x<2*Mathf.PI; x+=2.0f)
                {
                    Vector3 pos = new Vector3(transform.position.x + greaterRadius * Mathf.Cos(x), 0.01f, transform.position.z + greaterRadius * Mathf.Sin(x));

                    //With the last element we check if it isn't too close of the first one.
                    if (x + 2.0f > 2 * Mathf.PI)
                        if (m_pointsDestionationNavMesh.Count > 1)
                            if (Vector3.Distance(pos, m_pointsDestionationNavMesh[0]) < 2.0f)
                                break;

                    m_pointsDestionationNavMesh.Add(pos);
                }

                break;
            case FieldSelection.square:

                //pythagore
                float length = Mathf.Sqrt(2 * greaterRadius * greaterRadius);
                //right
                for (x = 2.0f; x < length - 2.0f; x += 2.0f)
                {
                    Vector3 pos = new Vector3(transform.position.x + length / 2.0f, 0.01f, transform.position.z - length / 2.0f + x);
                    m_pointsDestionationNavMesh.Add(pos);
                }
                //up
                for (x = 0.0f; x < length; x += 2.0f)
                {
                    Vector3 pos = new Vector3(transform.position.x - length/2.0f + x, 0.01f, transform.position.z + length / 2.0f);
                    m_pointsDestionationNavMesh.Add(pos);
                }
                //left
                for (x = 2.0f; x < length - 2.0f; x += 2.0f)
                {
                    Vector3 pos = new Vector3(transform.position.x - length / 2.0f, 0.01f, transform.position.z - length / 2.0f + x);
                    m_pointsDestionationNavMesh.Add(pos);
                }
                //bottom
                for (x = 0.0f; x < length; x += 2.0f)
                {
                    Vector3 pos = new Vector3(transform.position.x - length / 2.0f + x, 0.01f, transform.position.z - length / 2.0f);
                    m_pointsDestionationNavMesh.Add(pos);
                }

                break;
            default:
                Debug.LogWarning("Unknown FieldSelectionType for " + gameObject.name);
                break;

        }

    }

    public static SObject getSobjectFromSelectionField(GameObject selectionFieldGO)
    {
        GameObject go = selectionFieldGO.transform.parent.gameObject;

        if (go.TryGetComponent(out SObject sobject) == false)
        {
            Debug.LogWarning("The selectionfield " + selectionFieldGO.name + " wasn't children of a sobject");
            return go.AddComponent(typeof(SObject)) as SObject;
        }
        return sobject;
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
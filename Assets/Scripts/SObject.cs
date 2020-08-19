using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SObject : MonoBehaviour
{
    private GameObject m_cursorGO;
    private Renderer m_cursorRenderer;
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
    private Vector3[] m_pointsDestinationNavMesh = new Vector3[4];
    [SerializeField]
    protected uint m_nbrUnitMaxOnObject = 4;
    protected uint m_nbrUnitOnObject = 0;
    [SerializeField]
    protected List<CreationImprovement> m_buttonCreation = new List<CreationImprovement>();
    //public UnityAction method;
    [SerializeField]
    private uint m_health = 100;

    #region getter
    public Player BelongsTo { get => m_belongsTo; set => m_belongsTo = value; }
    public Vector3[] PointsDestinationNavMesh { get => m_pointsDestinationNavMesh; set => m_pointsDestinationNavMesh = value; }
    public bool IsNeutral { get => m_isNeutral; set => m_isNeutral = value; }
    public string ID { get => m_ID; set => m_ID = value; }
    public uint Health { get => m_health; }
    #endregion


    protected virtual void Awake()
    {
        if (checkCoherency() == false)
            Debug.LogWarning(gameObject.name + " isn't coherent");

        updateNbrOfSobject();
        defineID();
        if (DebugTool.tryFindGOChildren(gameObject, "Cursor", out m_cursorGO, LogType.Error) == false)
            return;

        if (m_cursorGO.TryGetComponent(out m_cursorRenderer) == false)
            Debug.LogWarning("Unable to find any renderer component on cursor of " + gameObject.name);

        m_cursorGO.SetActive(false);
        definePointsDestination();
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

        m_pointsDestinationNavMesh[0] = new Vector3(left, height, up);
        m_pointsDestinationNavMesh[1] = new Vector3(left, height, down);
        m_pointsDestinationNavMesh[2] = new Vector3(right, height, up);
        m_pointsDestinationNavMesh[3] = new Vector3(right, height, down);

    }

    public void isSelect()
    {
        //add cursor on above the selectedObject        
        m_cursorGO.SetActive(true);
    }

    public void unSelect()
    {
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

    public virtual void updateUI()
    {
        UIManager.Instance.setCreationButton(m_buttonCreation);
    }
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
    public float duration;
    //the methods used here can only have less than 2 parameters.
    public UnityEvent method;

}
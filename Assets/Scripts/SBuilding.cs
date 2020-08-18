using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class SBuilding : SUnmovable
{

    private Queue<CreationImprovement> m_queueCreation = new Queue<CreationImprovement>();
    private bool m_creationOnGoing = false;

    public Queue<CreationImprovement> QueueCreation { get => m_queueCreation; set => m_queueCreation = value; }

    public void addToQueue(CreationImprovement buttonImage)
    {
        print("add to deque");
        m_queueCreation.Enqueue(buttonImage);
        UIManager.Instance.addQueueButton(buttonImage);
        if (m_creationOnGoing == false)
            StartCoroutine(treatQueue());
    }

    public void dequeue()
    {
        print("dequeu");
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
        Instantiate(sunitGO, PointsDestinationNavMesh[3], Quaternion.identity);
    }

    IEnumerator treatQueue()
    {
        m_creationOnGoing = true;
        while (m_queueCreation.Count > 0)
        {
            yield return new WaitForSeconds(m_queueCreation.Peek().duration);
            m_queueCreation.Peek().method.Invoke();
            dequeue();
        }
        m_creationOnGoing = false;
    }

    public override void updateUI()
    {
        
        UIManager.Instance.setCreationButton(m_buttonCreation, this);
        UIManager.Instance.setQueueButton(m_queueCreation);
    }

}

public enum CreationType
{
    improvement, 
    unit
}

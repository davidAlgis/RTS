using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddToSelection : MonoBehaviour
{
    private PlayerHuman m_player;

    public PlayerHuman Player { get => m_player; set => m_player = value; }


    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Selectable"))
            if (other.transform.parent.gameObject.TryGetComponent(out SObject selectableObject))
                m_player.addToCurrentSelection(selectableObject);

    }

    private void OnTriggerExit(Collider other)
    {
        if(m_player.MouseIsHold)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Selectable"))
                if (other.transform.parent.gameObject.TryGetComponent(out SObject selectableObject))
                    m_player.removeFromCurrentSelection(selectableObject);
        }
    }
}

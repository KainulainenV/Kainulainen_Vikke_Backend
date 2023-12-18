using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OfflineMessage : MonoBehaviour, ISelectable
{
    public Message messageData;
    public void Select()
    {
        // When selected, open selected user's threads
        Debug.Log("Selected: " + this.name);

        Destroy(gameObject);
    }

    
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageObject : MonoBehaviour, ISelectable
{
    public Message messageData;
    public void Select()
    {
        // When selected, open selected user's threads
        Debug.Log("Selected: " + this.name);

        // Destroy and remove this message from everywhere
        restClient.instance.deleteMsg(messageData.id);
        Destroy(gameObject);
    }
}

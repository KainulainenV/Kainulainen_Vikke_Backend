using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OfflineThread : MonoBehaviour, ISelectable
{
    public Thread threadData;
    public void Select()
    {
        // When selected, open selected user's threads
        Debug.Log("Selected: " + this.name);

        OfflineGame.instance.initMessages(threadData.id);

        UIManager.instance.threadsScreen.SetActive(false);
        UIManager.instance.messagesScreen.SetActive(true);
    }

    
}

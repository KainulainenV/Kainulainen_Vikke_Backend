using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OfflineUser : MonoBehaviour, ISelectable
{
    public User userData;
    public void Select()
    {
        // When selected, open selected user's threads
        Debug.Log("Selected: " + this.name);

        OfflineGame.instance.initThreads(userData.id);

        UIManager.instance.usersScreen.SetActive(false);
        UIManager.instance.threadsScreen.SetActive(true);
    }


}

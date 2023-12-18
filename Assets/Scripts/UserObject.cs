using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserObject : MonoBehaviour, ISelectable
{
    public User userData;

    public void Select()
    {
        // When selected, open selected user's threads
        Debug.Log("Selected: " + this.name);

        restClient.instance.threadsPoll(userData.id);

        UIManager.instance.usersScreen.SetActive(false);
        UIManager.instance.threadsScreen.SetActive(true);       
    }
   
}

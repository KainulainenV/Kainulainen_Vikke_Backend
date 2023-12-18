using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.UIElements.UxmlAttributeDescription;
using UnityEngine.Networking;
using System.Threading;
using UnityEditor.VersionControl;

public class OfflineGame : MonoBehaviour
{
    public static OfflineGame instance;

    public User[] offlineUsers;
    public Thread[] offlineThreads;
    public Message[] offlineMessages;

    public GameObject userPrefab;
    public GameObject messagePrefab;
    public GameObject threadPrefab;

    public GameObject usersScreen;
    public GameObject messagesScreen;
    public GameObject threadsScreen;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
    public void startOfflineGame()
    {
        UIManager.instance.OfflineUI.SetActive(true);
        initUsers();
    }

    public void initUsers()
    {
        int instantiated = 0;

        Vector3 position = new Vector3(-7.0f, 2.0f, 0.0f);
        float gap = 2;
        for (int i = 0; i < offlineUsers.Length; i++)
        {
            GameObject newObject = Instantiate(userPrefab, position, Quaternion.identity, usersScreen.transform);
            position.x += gap;
            newObject.name = offlineUsers[i].username;
            newObject.GetComponentInChildren<TextMeshProUGUI>().text = offlineUsers[i].username;

            newObject.GetComponent<OfflineUser>().userData = offlineUsers[i];

            instantiated++;
            if (instantiated % 9 == 0 && i != 0)
            {
                position.y -= gap;
                position.x = -7.0f;
            }
        }
    }

    public void initThreads(int selectedUser)
    {
        Vector3 position = new Vector3(-7.0f, 2.0f, 0.0f);
        float gap = 2;

        int instantiated = 0;
        for (int i = 0; i < offlineThreads.Length; i++)
        {
            // Instantiate selected user's threads if it's not hidden
            if (offlineThreads[i].author == selectedUser && offlineThreads[i].hidden == 0)
            {
                GameObject newObject = Instantiate(threadPrefab, position, Quaternion.identity, threadsScreen.transform);
                position.x += gap;
                newObject.name = offlineThreads[i].title;
                newObject.GetComponentInChildren<TextMeshProUGUI>().text = offlineThreads[i].title;

                newObject.GetComponent<OfflineThread>().threadData = offlineThreads[i];

                instantiated++;
                if (instantiated % 9 == 0 && i != 0)
                {
                    position.y -= gap;
                    position.x = -7.0f;
                }
            }
        }
    }

    public void initMessages(int selectedThread)
    {
        Vector3 position = new Vector3(-7.0f, 2.0f, 0.0f);
        float gap = 2;
        int instantiated = 0;

        for (int i = 0; i < offlineMessages.Length; i++)
        {
            // Instantiate selected thread's non-hidden messages
            if (offlineMessages[i].thread == selectedThread && offlineMessages[i].hidden == 0)
            {
                GameObject newObject = Instantiate(messagePrefab, position, Quaternion.identity, messagesScreen.transform);
                position.x += gap;
                newObject.name = offlineMessages[i].title;
                newObject.GetComponentInChildren<TextMeshProUGUI>().text = offlineMessages[i].title;

                newObject.GetComponent<OfflineMessage>().messageData = offlineMessages[i];

                instantiated++;
                if (instantiated % 9 == 0 && i != 0)
                {
                    position.y -= gap;
                    position.x = -7.0f;
                }
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject LoginScreen;

    public GameObject ErrorText;
    public GameObject AdminErrorText;
    public GameObject LoggingText;

    public GameObject usersScreen;
    public GameObject threadsScreen;
    public GameObject messagesScreen;

    public GameObject OfflineUI;

    private void Awake()
    {
        if(instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }


    public void authError()
    {
        ErrorText.SetActive(true);
        AdminErrorText.SetActive(false);
    }

    public void adminError()
    {
        ErrorText.SetActive(false);
        LoggingText.SetActive(false);
        AdminErrorText.SetActive(true);
    }

    public void loggingText()
    {
        ErrorText.SetActive(false);
        LoggingText.SetActive(true);
        AdminErrorText.SetActive(false);
    }
}

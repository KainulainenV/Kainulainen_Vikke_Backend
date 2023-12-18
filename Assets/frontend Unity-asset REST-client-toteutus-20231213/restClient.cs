using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class User
{
    public int id;
    public string username;
    public string password;
    public string firstname;
    public string lastname;
    public string created;
    public string lastseen;
    public int banned;
    public int isadmin;
   

    public static User CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<User>(jsonString);
    }

}

[System.Serializable]
public class Thread
{
    public int id;
    public string title;
    public int author;
    public string created;
    public int hidden;

    public static Thread CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<Thread>(jsonString);
    }
}

[System.Serializable]
public class Message
{
    public int id;
    public int thread;
    public string title;
    public string content;
    public int author;
    public int replyto;
    public string created;
    public string modified;
    public int hidden;

    public static Message CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<Message>(jsonString);
    }
}


public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items;
    }

    public static string ToJson<T>(T[] array)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper);
    }

    public static string ToJson<T>(T[] array, bool prettyPrint)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }
}


public class restClient : MonoBehaviour
{
    public static restClient instance;

    public TextMeshProUGUI UsernameInput;
    public TextMeshProUGUI PasswordInput;

    private string baseurl = "http://localhost:80/bb/api";
    private bool loggedIn = false;
    private bool usersInited = false;
    private bool threadsInited = false;
    private bool messagesInited = false;

    private string LoggedUser;
    
    public User[] users; // array representing copy of the database (as the backend server provides)
    public Thread[] threads;
    public Message[] messages;

    public GameObject usersScreen;
    public GameObject threadsScreen;
    public GameObject messagesScreen;

    public GameObject userPrefab;
    public GameObject threadPrefab;
    public GameObject messagePrefab;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(Login());
        StartCoroutine(TestServer());
        //StartCoroutine(PollThreads());

        // If login input is wrong, does not work (or shouldn't)
        StartCoroutine(PollUsers());
    }

    private string fixJson(string value)
    {
        value = "{\"Items\":" + value + "}";
        return value;
    }

    public void LogButton()
    {
        //string password = PasswordInput.text;
        //Debug.Log(password);
      
        // When getting string from input, it adds invisible special character, so we have to clean it
        string sanitizedUsername = UsernameInput.text.Replace("\u200B", "");
        string sanitizedPassword = PasswordInput.text.Replace("\u200B", "");
        StartCoroutine(Login(sanitizedUsername, sanitizedPassword));
        
    }

    // Test if server is online
    IEnumerator TestServer()
    {
        UnityWebRequest server = UnityWebRequest.Get("http://localhost:80/bb");
        yield return server.SendWebRequest();

        if (server.result != UnityWebRequest.Result.Success)
        {
            var text = server.downloadHandler.text;   
            Debug.Log(server.error);
            Debug.Log(text);
            OfflineGame.instance.startOfflineGame();
        }
        else
        {
            // If server is online, open Login screen
            Debug.Log("Server Online");
            Debug.Log("Open login screen");
            UIManager.instance.LoginScreen.SetActive(true);
        }
            
    }

    // perform one-time login at the beginning of a connection
    IEnumerator Login(string username, string password)
    {
        UnityWebRequest www = UnityWebRequest.Post(baseurl + "/login", "{ \"username\": \"" + username + "\", \"password\": \"" + password + "\" }", "application/json");
        Debug.Log(password);
        yield return www.SendWebRequest();


        if (www.result != UnityWebRequest.Result.Success)
        {
            var text = www.downloadHandler.text;
            Debug.Log(baseurl+ "/login");
            Debug.Log(www.error);
            Debug.Log(text);
            UIManager.instance.authError();
        }
        else
        {
            Debug.Log(www.responseCode);

            // If username or password is incorrect, responseCode 401
            if(www.responseCode == 200)
            {
                LoggedUser = username;

                // Check if user is admin
                Debug.Log("Login complete!");
                loggedIn = true;
                var text = www.downloadHandler.text;
                Debug.Log(text);

                // Logging in...
                UIManager.instance.loggingText();
            }
                       
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            StartCoroutine(DeleteUser());
        }
    }

    // TODO: Use same kind of method to "delete" messages
    // Bans user
    IEnumerator DeleteUser()
    {
        UnityWebRequest www = UnityWebRequest.Delete(baseurl + "/users/2");
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            var text = www.downloadHandler.text;
            Debug.Log(baseurl + "/users");
            Debug.Log(www.error);
            Debug.Log(text);
        }
        else
        {
            Debug.Log("User deleted");
        }
    }

    // perform asynchronnous polling of users information every X seconds after login succesfull
    IEnumerator PollUsers()
    {
        
        while(!loggedIn) yield return new WaitForSeconds(10); // wait for login to happen

        while (true){
            UnityWebRequest www = UnityWebRequest.Get(baseurl + "/users");
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                var text = www.downloadHandler.text;
                Debug.Log(baseurl+ "/users");
                Debug.Log(www.error);
                Debug.Log(text);
            }
            else
            {
                var text = www.downloadHandler.text; // Data in JSON form
                Debug.Log("users download complete: " + text);
                loggedIn = true;
                string jsonString = fixJson(text); // add Items: in front of the json array
                users = JsonHelper.FromJson<User>(jsonString); // convert json to User-array (public users) // overwrite data each update!
                // SEE :
                // https://stackoverflow.com/questions/36239705/serialize-and-deserialize-json-and-json-array-in-unity/36244111#36244111

                // Check if logged user is admin
                bool isAdmin = false;
                foreach (User user in users)
                {
                    if (LoggedUser.ToLower() == user.username.ToLower())
                        if (user.isadmin == 1)
                        {
                            isAdmin = true;
                            break;
                        }
                }

                if(!isAdmin)
                {
                    UIManager.instance.adminError();
                }
                else
                {
                    UIManager.instance.LoginScreen.SetActive(false);

                    int instantiated = 0;

                    if (!usersInited)
                    {
                        //GameObject userSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        Vector3 position = new Vector3(-7.0f, 2.0f, 0.0f);
                        float gap = 2;
                        for (int i = 0; i < users.Length; i++)
                        {
                            GameObject newObject = Instantiate(userPrefab, position, Quaternion.identity, usersScreen.transform);
                            position.x += gap;
                            newObject.name = users[i].username;
                            newObject.GetComponentInChildren<TextMeshProUGUI>().text = users[i].username;

                            newObject.GetComponent<UserObject>().userData = users[i];

                            instantiated++;
                            if (instantiated % 9 == 0 && i != 0)
                            {
                                position.y -= gap;
                                position.x = -7.0f;
                            }
                        }
                        usersInited = true;
                    }
                    else
                    {
                        // TODO: only update users, e.g. add new user or update changed properties of existing one, need to compare existing ones
                    }
                }
                
            }
            yield return new WaitForSeconds(60); // users may not update very often
        }
        
    }

    public void threadsPoll(int author)
    {
        StartCoroutine(PollThreads(author));
    }
    // perform asynchronnous polling of threads information every X seconds after login succesfull
    IEnumerator PollThreads(int selectedUser)
    {
        while (!loggedIn) yield return new WaitForSeconds(10); // wait for login to happen

        while (true)
        {
            UnityWebRequest www = UnityWebRequest.Get(baseurl + "/threads");
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                var text = www.downloadHandler.text;
                Debug.Log(baseurl + "/threads");
                Debug.Log(www.error);
                Debug.Log(text);
            }
            else
            {
                var text = www.downloadHandler.text;
                Debug.Log("threads download complete: " + text);
                loggedIn = true;
                // TODO: handle messages JSON somwhow
                string jsonString = fixJson(text);
                threads = JsonHelper.FromJson<Thread>(jsonString);
                
                if (!threadsInited)
                {
                    Vector3 position = new Vector3(-7.0f, 2.0f, 0.0f);
                    float gap = 2;

                    int instantiated = 0;
                    for (int i = 0; i < threads.Length; i++)
                    {
                        // Instantiate selected user's threads if it's not hidden
                        if (threads[i].author == selectedUser && threads[i].hidden == 0)
                        {
                            GameObject newObject = Instantiate(threadPrefab, position, Quaternion.identity, threadsScreen.transform);
                            position.x += gap;
                            newObject.name = threads[i].title;
                            newObject.GetComponentInChildren<TextMeshProUGUI>().text = threads[i].title;

                            newObject.GetComponent<ThreadObject>().threadData = threads[i];

                            instantiated++;
                            if (instantiated % 9 == 0 && i != 0)
                            {
                                position.y -= gap;
                                position.x = -7.0f;
                            }
                        }                       
                    }
                    threadsInited = true;
                }
                else
                {
                    // TODO: only update threads, e.g. add new thread or update changed properties of existing one, need to compare existing ones
                }

            }
            yield return new WaitForSeconds(10);
        }

    }

    public void messagesPoll(int thread)
    {
        StartCoroutine(PollMessages(thread));
    }
    IEnumerator PollMessages(int selectedThread)
    {

        while (!loggedIn) yield return new WaitForSeconds(10); // wait for login to happen

        while (true)
        {
            UnityWebRequest www = UnityWebRequest.Get(baseurl + "/messages");
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                var text = www.downloadHandler.text;
                Debug.Log(baseurl + "/messages");
                Debug.Log(www.error);
                Debug.Log(text);
            }
            else
            {
                var text = www.downloadHandler.text; // Data in JSON form
                Debug.Log("messages download complete: " + text);
                loggedIn = true;
                string jsonString = fixJson(text); // add Items: in front of the json array
                messages = JsonHelper.FromJson<Message>(jsonString); // convert json to User-array (public users) // overwrite data each update!
               
                if(!messagesInited)
                {
                    Vector3 position = new Vector3(-7.0f, 2.0f, 0.0f);
                    float gap = 2;

                    int instantiated = 0;

                    for (int i = 0; i < messages.Length; i++)
                    {
                        // Instantiate selected thread's non-hidden messages
                        if (messages[i].thread == selectedThread && messages[i].hidden == 0)
                        {
                            GameObject newObject = Instantiate(messagePrefab, position, Quaternion.identity, messagesScreen.transform);
                            position.x += gap;
                            newObject.name = messages[i].title;
                            newObject.GetComponentInChildren<TextMeshProUGUI>().text = messages[i].title;

                            newObject.GetComponent<MessageObject>().messageData = messages[i];

                            instantiated++;
                            if (instantiated % 9 == 0 && i != 0)
                            {
                                position.y -= gap;
                                position.x = -7.0f;
                            }
                        }
                    }
                    messagesInited = true;
                }
                else
                {
                    // TODO: only update messages, e.g. add new message or update changed properties of existing one, need to compare existing ones
                }


            }
            yield return new WaitForSeconds(30); // users may not update very often
        }

    }

    public void deleteMsg(int ID)
    {
        StartCoroutine(deleteMessage(ID));
    }
    IEnumerator deleteMessage(int msgID)
    {
        UnityWebRequest www = UnityWebRequest.Delete(baseurl + "/messages/" + msgID);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            var text = www.downloadHandler.text;
            //Debug.Log(baseurl + "/users");
            Debug.Log(www.error);
            Debug.Log(text);
        }
        else
        {
            Debug.Log("Message deleted");
        }
    }
}

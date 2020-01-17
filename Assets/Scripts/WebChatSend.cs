using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WebChatSend : MonoBehaviour
{
    public InputField inputField;
    
    // Start is called before the first frame update
    void Start()
    {
        inputField.text = "";
    }

    public void Send()
    {
        if(inputField.text.Length > 0)
        {
        GlobalEvents.PerformEvent(new OnlineEvents.ChatSend(){ msg=inputField.text});
        inputField.text = "";
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(InputManager.GetKey(InputManager.Action.Close))
        {
            inputField.text = "";
        }

        if(InputManager.GetKey(InputManager.Action.Confirm))
        {
            Send();
        }
    }
}
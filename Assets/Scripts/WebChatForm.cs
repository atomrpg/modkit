using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WebChatForm : MonoBehaviour
{
    public Text textForm;
    
    // Start is called before the first frame update
    void Start()
    {
        textForm.text = "";
        GlobalEvents.AddListener<OnlineEvents.ChatGet>(Recive);
    }

    public void Recive(OnlineEvents.ChatGet evnt)
    {
        textForm.text += string.Format("{0}: {1}\n", evnt.initiator, evnt.msg);
    }
}


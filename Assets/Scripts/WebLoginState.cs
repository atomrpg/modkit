using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class WebLoginState : MonoBehaviour
{
    public UnityEngine.UI.Text loginName;
    public UnityEngine.UI.Text password;
    public void Login()
    {
        StartCoroutine(TryLogin());
    }
    
    IEnumerator TryLogin()
    {
        WebRequest request = new WebRequest();
        yield return request.Do(ModEntryPoint.server + "login.php",
            new MultipartFormDataSection("login", loginName.text),
            new MultipartFormDataSection("pass", password.text)
            );

       if(request.Success)
       {
           var json = request.GetData();
           GlobalEvents.PerformEvent<OnlineEvents.Login>(new OnlineEvents.Login { 
               uid = json["id"].AsInt, room=json["room"].AsInt,
            x=json["x"].AsInt, y=json["y"].AsInt,
            data=json["data"],
            lastActionId = json["lastActionId"].AsInt});
       }
       else
       {
           //error handle
       }
    }
}

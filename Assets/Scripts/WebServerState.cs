using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WebServerState : MonoBehaviour
{
    [SerializeField]
    private Dropdown _dropDown = null;
    // Start is called before the first frame update
    void Start()
    {   
        var assembly = GetType().Assembly;
        string modName = assembly.GetName().Name;
        string dir = System.IO.Path.GetDirectoryName(assembly.Location);
        string[] serverList = null;
        if(System.IO.File.Exists(dir + System.IO.Path.DirectorySeparatorChar + "servers.cfg"))
        {
        System.IO.File.ReadAllLines(dir + System.IO.Path.DirectorySeparatorChar + "servers.cfg");
        }
        if(serverList != null && serverList.Length != 0)
        {
         _dropDown.AddOptions(new List<string>(serverList));
        }
        else
        {
            _dropDown.AddOptions(new List<string>(){ModEntryPoint.server});
        }
    }

    public void OnServerChanged(int index)
    {
        ModEntryPoint.server = _dropDown.options[index].text;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

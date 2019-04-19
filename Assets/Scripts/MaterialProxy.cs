using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialProxy : MonoBehaviour
{
    public string materialPath = "";

    void OnEnable()
    {
        GetComponent<Renderer>().material = ResourceManager.Load<Material>(materialPath, ResourceManager.EXT_MATERIAL);
    }
}

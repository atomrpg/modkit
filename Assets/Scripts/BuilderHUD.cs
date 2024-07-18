using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuilderHUD : MonoBehaviour
{
    [SerializeField]
    GameObject _item;

    [SerializeField]
    Transform _content;

    [SerializeField]
    ToggleGroup _toggleGroup;

    EntityProto _proto;
    // Start is called before the first frame update
    void Start()
    {
        var allInteractive = ResourceManager.LoadAll<InteractiveProto>("Entities/Interactive", ResourceManager.EXT_ASSET);

        foreach (var item in allInteractive)
        {
            GameObject.Instantiate(_item, _content).GetComponent<BuilderItemHUD>().Setup(item, this, _toggleGroup);
        }

        var allChest = ResourceManager.LoadAll<ChestProto>("Entities/Chest", ResourceManager.EXT_ASSET);

        foreach (var item in allChest)
        {
            GameObject.Instantiate(_item, _content).GetComponent<BuilderItemHUD>().Setup(item, this, _toggleGroup);
        }
    }

    public void Select(EntityProto proto)
    {
        _proto = proto;
    }

    public void Build()
    {
        ModEntryPoint.Current.Build(_proto);
    }
}

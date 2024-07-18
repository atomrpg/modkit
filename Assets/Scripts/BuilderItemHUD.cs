using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuilderItemHUD : MonoBehaviour
{
    EntityProto _proto;
    BuilderHUD _hud;

    public EntityProto EntityProto => _proto;

    [SerializeField]
    private Text _caption;


    [SerializeField]
    private Toggle _togle;


    public void Setup(EntityProto proto, BuilderHUD hud, ToggleGroup toggleGroup)
    {
        _proto = proto;
        _hud = hud;
        _caption.text = proto.Caption;
        _togle.group = toggleGroup;
    }

    public void Click()
    {
        _hud.Select(_proto);
    }
}

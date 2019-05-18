using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class EntityProxy : MonoBehaviour
{
    public GameObjectRef prefab = null;

    public EntityProto proto = null;

    private void OnEnable()
    {
        var goCopy = prefab.Get();

        GameObject go = null;

        go = Instantiate(goCopy, transform);
        go.transform.localPosition = Vector3.zero;
        go.name = goCopy.name;

        var oldEntity = go.GetComponent<EntityComponent>();

        EntityComponent newEntity = (EntityComponent) gameObject.AddComponent(oldEntity.GetType());
        newEntity.SetEntity(proto.CreateEntity());

        Destroy(oldEntity);

        { // BoxCollider
            var bc = go.GetComponent<BoxCollider>();
            if (bc != null)
            {
                var bcNew = gameObject.AddComponent<BoxCollider>();
                bcNew.center = bc.center;
                bcNew.size = bc.size;
                Destroy(bc);
            }
        }

        { // WeaponLink
            var wl = go.GetComponent<WeaponLink>();
            if (wl != null)
            {
                var wlNew = gameObject.AddComponent<WeaponLink>();
                wlNew.Flash = wl.Flash;
                wlNew.FlashSlot = wl.FlashSlot;
                wlNew.ReloadOn = wl.ReloadOn;
                wlNew.ReloadOff = wl.ReloadOff;
                Destroy(wlNew);
            }
        }

        foreach (var proxy in go.GetComponentsInChildren<ProxyComponent>())
        {
            proxy.link = newEntity;
        }
    }
}


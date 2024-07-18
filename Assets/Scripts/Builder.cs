using BehaviorEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Builder : PlayerHUD.WaitState
{
    GameObject _ui = null;
    GameObject _go = null;
    bool _down = false;

    public override void Begin()
    {
        _ui = GameObject.Instantiate(ResourceManager.Load<GameObject>("UI/Builder", ResourceManager.EXT_PREFAB), Game.World.HUD.m_Canvas.gameObject.transform);
    }

    public override void Update()
    {
        if (_go != null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                _down = true;
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float hitDist = 999;

            EntityComponent candidate = Game.World.Player.RayCast(ray, ref hitDist, true);
            Vector3 targetPoint = ray.GetPoint(hitDist);
            Node n = Pathfinder.Instance.FindNode(targetPoint);

            if (n != null)
            {
                if (n.walkable)
                {
                    if (Input.GetKey(KeyCode.LeftAlt))
                    {
                        _go.transform.position = n.GetPosition() + new Vector3(0, -0.1f, 0);
                    }
                    else
                    {
                        _go.transform.position = targetPoint;
                    }
                }
                else
                {
                    n = null;
                }
            }

            //_go.SetActive(n != null);

            if (Input.GetMouseButtonUp(0) && _down)
            {
                if (n != null)
                {
                }

                var po = _go.GetComponent<PathOverride>();
                if (po != null)
                {
                    po.Override();
                }

                Game.World.HUD.SetWaitState(null);
            }
        }
    }

    public void Build(EntityProto entityProto)
    {
        CloseUI();

        _go = GameObject.Instantiate(entityProto.Prefab);
    }

    void CloseUI()
    {
        if (_ui != null)
        {
            GameObject.Destroy(_ui);
            _ui = null;
        }
    }

    public override void End()
    {
        CloseUI();
    }
}

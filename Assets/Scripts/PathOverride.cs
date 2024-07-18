using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathOverride : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /*
    public void OverrideByCollider(Collider collider)
    {
        Bounds bounds = collider.bounds;
        Vector3 pos = new Vector3(bounds.min.x, 0f, bounds.min.z);
        Vector2Int cell = GetCell(pos);
        int num = Mathf.CeilToInt(Mathf.Abs((bounds.max.x - bounds.min.x) / CellSize));
        int num2 = Mathf.CeilToInt(Mathf.Abs((bounds.max.z - bounds.min.z) / CellSize));
        for (int i = cell.x; i < cell.x + num; i++)
        {
            for (int j = cell.y; j < cell.y + num2; j++)
            {
                if (i < 0 || j < 0 || i >= Width || j >= Height)
                {
                    continue;
                }

                int index = GetIndex(i, j);
                if (index >= 0 && index < Map.Length)
                {
                    Ray ray = new Ray(Map[index].GetPosition() - new Vector3(0f, 10f, 0f), Vector3.up * 100f);
                    if (collider.Raycast(ray, out var _, 1000f))
                    {
                        lockCells.Add(index);
                        Map[index].walkable = walkable;
                    }
                }
            }
        }
    }*/

    public void Override()
    {
        Pathfinder.Instance.DynamicRaycastUpdate(GetComponent<Collider>().bounds);
    }
}

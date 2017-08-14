using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MapMapper : MonoBehaviour {

    private const string BoxTag = "Box";

    public static MapMapper ins = null;

    private Dictionary<Vector3, Box> Map = null;

    #region MonoBehaviour
    void Awake()
    {
        ins = this;
        Map = InitMap();
    }
    #endregion

    /* 初始化地图数组 */
    Dictionary<Vector3, Box> InitMap()
    {
        var boxes = GameObject.FindGameObjectsWithTag(BoxTag);

        var map = new Dictionary<Vector3, Box>();

        foreach (var box in boxes)
        {
            map[box.transform.position] = box.GetComponent<Box>();
        }

        return (from b in map
                orderby b.Key.y ascending
                select b).ToDictionary(k => k.Key, v => v.Value);
    }

    void SortMap()
    {
        Map = (from b in Map
               orderby b.Key.y ascending
               select b).ToDictionary(k => k.Key, v => v.Value);
    }
    /* 获取地图原数组 */
    public Dictionary<Vector3, Box> GetMap()
    {
        return Map;
    }
    /* 指定位置是否存在Box */
    public bool IsExist(Vector3 position)
    {
        try
        {
            if (Map[position])
                return true;
            else return false;
        }
        catch (KeyNotFoundException e)
        {
            return false;
        }
    }
    /* 获取元素在地图中的索引 */
    public Vector3 GetBoxIndex(Box box)
    {
        return (from b in Map where b.Value == box select b.Key).ToArray()[0];
    }

    /* 移动地图中的元素到指定坐标 */
    public void MoveTo(Box box, Vector3 destination)
    {
        var index = GetBoxIndex(box);
        //Map[destination] = box;
        if (Map.ContainsKey(destination))
        {
            Debug.LogWarning(box.gameObject.name + " source:" + index);
            Debug.LogError(destination + " ContainInMap:" + Map[destination].gameObject.name);
        }
        Map.Remove(index);
        Map.Add(destination, box);

        SortMap();
    }
    #region DrawOnSceneView
    void OnDrawGizmos()
    {
        DrawBoxInArray();
        DrawBoxRealPosition();
    }
    void DrawBoxRealPosition()
    {
        if (Map == null)
            return;
        foreach (var box in Map)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(box.Value.transform.position, Vector3.one);
        }
    }

    void DrawBoxInArray()
    {
        if (Map == null)
            return;
        foreach (var box in Map)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(box.Key, Vector3.one);
        }
    }
    #endregion
}

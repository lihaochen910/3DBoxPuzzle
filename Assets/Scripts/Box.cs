using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HighlightingSystem;

public class Box : MonoBehaviour {

    #region Private variables
    private MapMapper _map;
    private readonly Vector3 _nilPosition = new Vector3(float.MinValue, float.MinValue, float.MinValue);
    //记录被推动后的坐标,用于被推动的源物体
    private Vector3 _moveEndPosition;

    private Highlighter _debugLight;
    #endregion

    #region Public variables
    //Box实例当前状态
    public BoxState State;
    //当Box处于Isolated状态时预计算出的下落停靠目标点
    //Linked状态时应该置于(-1,-1)
    private Vector3 _linkedPosition = new Vector3(-1, -1,-1);
    #endregion

    #region Shared variables
    /// <summary>
    /// 所有Box实例共享的下落速度
    /// </summary>
    protected static float DropSpeed = 2f;
    /// <summary>
    /// 所有Box实例共享的队列,用于从源物体推送同步下落事件
    /// </summary>
    protected static Queue<Box> To_be_synchronized_object = new Queue<Box>();
    /// <summary>
    /// 用于检索受影响的box,所有box实例共享此队列以节省内存
    /// </summary>
    protected static Queue<Box> Affected_Boxes = new Queue<Box>();

    protected static int ReciveCount = 0;
    #endregion

    public enum BoxState
    {
        /// <summary>
        /// 连接状态
        /// </summary>
        Linked,
        /// <summary>
        /// 孤立状态
        /// </summary>
        Isolated,
    }

    #region MonoBehaviour
    void Start()
    {
        _map = MapMapper.ins;
        State = BoxState.Linked;

        _debugLight = gameObject.AddComponent<Highlighter>();
    }
    #endregion

    #region AlphaCode
    /// <summary>
    /// 被推动动画结束后执行回调，逻辑入口
    /// </summary>
    /// <param name="moveEndPosition"></param>
    public void OnMoveEnd(Vector3 moveEndPosition)
    {
        ExecuteTimer.OnExecutePrepare();
        //保存被推动后的坐标
        _moveEndPosition = moveEndPosition;
        //递归检查
        SearchAffectedBoxes();
        //递归检查结束,所有box状态确定,下落坐标确定,可以下落
        pushDropEvent();

        ExecuteTimer.OnExecuteEnd();

        print("sendCount =" + ReciveCount);
        ReciveCount = 0;
    }

    /* 检查自身状态 */
    private void CheckSelf()
    {
        State = getCurrentState();
        if (State == BoxState.Isolated)
        {
            _linkedPosition = calcDropPosition();
            To_be_synchronized_object.Enqueue(this);
        }
    }
    /* 检索所有受影响box，入队 */
    private void SearchAffectedBoxes()
    {
        foreach (var box in GetAffectedBoxes(_map.GetBoxIndex(this)))
        {
            if (box)
                Affected_Boxes.Enqueue(box);
        }
        //原坐标保留后,使用被推动后的坐标更新此实例在数组中的位置
        DoMoveToInArray(_moveEndPosition);
        CheckSelf();

        while (Affected_Boxes.Count != 0)
        {
            var box = Affected_Boxes.Dequeue();

            box.ReciveAffect();

            foreach (var sub in GetAffectedBoxes(_map.GetBoxIndex(box)))
            {
                if (sub && !Affected_Boxes.Contains(sub))
                    Affected_Boxes.Enqueue(sub);
            }
        }
    }
    /* 受目标点影响的box,三维中共计5个位置 */
    public IEnumerable<Box> GetAffectedBoxes(Vector3 originPosition)
    {
        var map = _map.GetMap();
        //正上方
        if (map.ContainsKey(new Vector3(originPosition.x, originPosition.y + 1, originPosition.z)))
            yield return map[new Vector3(originPosition.x, originPosition.y + 1, originPosition.z)];
        //Z轴正方向
        if (map.ContainsKey(new Vector3(originPosition.x, originPosition.y + 1, originPosition.z + 1)))
            yield return map[new Vector3(originPosition.x, originPosition.y + 1, originPosition.z + 1)];
        //Z轴负方向
        if (map.ContainsKey(new Vector3(originPosition.x, originPosition.y + 1, originPosition.z - 1)))
            yield return map[new Vector3(originPosition.x, originPosition.y + 1, originPosition.z - 1)];
        //X轴正方向
        if (map.ContainsKey(new Vector3(originPosition.x + 1, originPosition.y + 1, originPosition.z)))
            yield return map[new Vector3(originPosition.x + 1, originPosition.y + 1, originPosition.z)];
        //X轴负方向
        if (map.ContainsKey(new Vector3(originPosition.x - 1, originPosition.y + 1, originPosition.z)))
            yield return map[new Vector3(originPosition.x - 1, originPosition.y + 1, originPosition.z)];
    }
    /* 目标点box在三维世界中的5个支撑物体 */
    public IEnumerable<Box> GetSupportBoxes(Vector3 originPosition)
    {
        var map = _map.GetMap();
        //正下方
        if (map.ContainsKey(new Vector3(originPosition.x, originPosition.y - 1, originPosition.z)))
            yield return map[new Vector3(originPosition.x, originPosition.y - 1, originPosition.z)];
        //Z轴正方向
        if (map.ContainsKey(new Vector3(originPosition.x, originPosition.y - 1, originPosition.z + 1)))
            yield return map[new Vector3(originPosition.x, originPosition.y - 1, originPosition.z + 1)];
        //Z轴负方向
        if (map.ContainsKey(new Vector3(originPosition.x, originPosition.y - 1, originPosition.z - 1)))
            yield return map[new Vector3(originPosition.x, originPosition.y - 1, originPosition.z - 1)];
        //X轴正方向
        if (map.ContainsKey(new Vector3(originPosition.x + 1, originPosition.y - 1, originPosition.z)))
            yield return map[new Vector3(originPosition.x + 1, originPosition.y - 1, originPosition.z)];
        //X轴负方向
        if (map.ContainsKey(new Vector3(originPosition.x - 1, originPosition.y - 1, originPosition.z)))
            yield return map[new Vector3(originPosition.x - 1, originPosition.y - 1, originPosition.z)];
    }
    /* 检查此Box实例的当前状态 */
    private BoxState getCurrentState()
    {
        var currentPositionInArray = transform.position;

        var boxes = new Box[5];
        var index = 0;
        foreach (var box in GetSupportBoxes(currentPositionInArray))
        {
            boxes[index] = box;
            index ++;
        }

        foreach (var box in boxes)
        {
            if (box && box.State == BoxState.Linked)
                return BoxState.Linked;
        }
        return BoxState.Isolated;
    }

    /* 当box处于孤立状态时(预下落状态),计算下落坐标 */
    private Vector3 calcDropPosition()
    {
        var currentPositionInArray = transform.position;

        var currentX = (int)currentPositionInArray.x;
        var currentY = (int)currentPositionInArray.y;
        var currentZ = (int)currentPositionInArray.z;

        var hitX = new int[] { currentX - 1, currentX, currentX + 1 };
        var hitZ = new int[] { currentZ - 1, currentZ, currentZ + 1 };
        //从当前行倒序遍历地图数组(世界方向向下)

        //使用List收集所有的停靠点,取y值最大的坐标点即为最终的停靠点
        List<Vector3> dropPositionList = new List<Vector3>();

        foreach (var box in _map.GetMap().Reverse())
        {
            //print(gameObject.name + " check " + box.Key + " " + box.Value.gameObject.name);
            if (box.Key.y < currentY && 
                 (
                    (box.Key.z == currentZ && IntArrayContainElement(hitX, (int)box.Key.x)) || 
                        (box.Key.x == currentX && IntArrayContainElement(hitZ, (int)box.Key.z))
                    )
                )
            {
                //print(gameObject.name + " add " + box + " 2 dropPositionList");
                //Debug.LogWarning(box.Value.gameObject.name+" checkInRange " + box.Value.State + " org:" + box.Key + " linked:" + box.Value._linkedPosition);
                if (box.Value.State == BoxState.Isolated)
                {
                    //Debug.LogWarning(gameObject.name + " use " + box.Value.gameObject.name + " _linkedPosition.y:" + box.Value._linkedPosition.y);
                    dropPositionList.Add(new Vector3(currentX, box.Value._linkedPosition.y + 1, currentZ));
                }
                else
                {
                    //Debug.LogWarning(gameObject.name + " use " + box.Value.gameObject.name + " .y:" + box.Key.y);
                    dropPositionList.Add(new Vector3(currentX, box.Key.y + 1, currentZ));
                }
            }
        }
        /* 查找最上方的停靠点 */
        if (dropPositionList.Count != 0)
        {
            return (from pos in dropPositionList
                    orderby pos.y descending
                    select pos).ToArray()[0];
        }
        Debug.LogError("dropPosition not found");
        //如果没有找到停靠点，则返回地图外最下方的坐标
        return _nilPosition;
    }
    /* 工具方法,检查静态int数组中是否包括指定int元素 */
    private bool IntArrayContainElement(int[] array, int v)
    {
        foreach (var arrayValue in array)
            if (arrayValue == v)
                return true;
        return false;
    }
    /* 推送同步下落事件 */
    private void pushDropEvent()
    {
        while (To_be_synchronized_object.Count != 0)
        {
            var box = To_be_synchronized_object.Dequeue();
            box.DoMoveToInArray(box._linkedPosition);
            box.DoDropTween();
        }
    }
    /* 执行下落动画 */
    private void DoDropTween()
    {
        //print(gameObject.name+" DoDropTween 2pos:"+_linkedPosition);
        iTween.MoveTo(gameObject,
            iTween.Hash("position", _linkedPosition, "easeType", "linear",
            "speed", DropSpeed, "oncomplete", "SwitchState"));
    }
    /* 下落动画完成后切换box状态 */
    private void SwitchState()
    {
        State = BoxState.Linked;
    }
    /* 在地图类中改变此实例的位置 */
    private void DoMoveToInArray(Vector3 position)
    {
        _map.MoveTo(this, position);
    }
    /* 受影响时被调用 */
    protected void ReciveAffect()
    {
        ReciveCount++;

        CheckSelf();

        StartCoroutine(DebugLight());
    }
    IEnumerator DebugLight()
    {
        _debugLight.ConstantOn(Color.white);
        yield return new WaitForSeconds(0.7f);
        _debugLight.ConstantOff();
    }
    #endregion
}

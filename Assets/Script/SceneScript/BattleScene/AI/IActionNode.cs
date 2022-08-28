using System.Collections.Generic;
using UnityEngine;

public abstract class IActionNode
{
    /// <summary>
    /// Ψһ���Ӳ������ȼ��������ظ����ᱻ���
    /// </summary>
    public float priority;
    //�����Ӳ���������֣��Ǳ��ֻ��������
    public string name;
    public IActionNode(float priority, string name = null)
    {
        this.priority = priority;
        this.name = name;
    }
    public abstract bool Run(GameObject activingRoleGO, List<GameObject> allRoleGO, GameObject[,] mapGridItems, List<GameObject> allCanMoveGridItems, ActionStrategySmart actionStrategySmart);
}

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ����Hp����15%
/// Ҫ���ǵ����ʵ��̫���ˣ����ƻ�����Ҫʹ��̰���㷨���
/// </summary>
public class ActionNodePlayerHPLower15Percent : IActionNode
{
    public ActionNodePlayerHPLower15Percent(float priority, string name = null) : base(priority, name)
    {
    }

    public override bool Run(GameObject activingRoleGO, List<GameObject> allRoleGO, GameObject[,] mapGridItems, ActionStrategySmart actionStrategySmart)
    {
        BaseRole activingRole = activingRoleGO.GetComponent<BaseRole>();
        if (activingRole.teamNum == TeamNum.TEAM_TWO) //���ˣ�Ŀ���������
        {
            List<(int, int)> obstacles = new List<(int, int)>();
            GameObject hanLiGO = null;
            foreach (GameObject roleGO in allRoleGO)
            {
                if (roleGO.tag.Equals("Player"))
                {
                    hanLiGO = roleGO;
                    continue;
                }
                if (roleGO == activingRoleGO) continue;
                BaseRole role = roleGO.GetComponent<BaseRole>();
                obstacles.Add((role.battleOriginPosX, role.battleOriginPosZ));
            }
            HanLiScriptInBattle hanLi = hanLiGO.GetComponent<HanLiScriptInBattle>();
            if (hanLi.hp / hanLi.maxHp <= 0.15f) //����Ѫ������15%���������ǣ�ȫ����������
            {
                Debug.LogWarning("����hp����15%����������ȫ������ģ��");
                bool isNeedMove;
                AStarPathUtil aStarPathUtil = new AStarPathUtil();
                (int, int) start = (activingRole.battleOriginPosX, activingRole.battleOriginPosZ);
                (int, int) end = (hanLi.battleOriginPosX, hanLi.battleOriginPosZ);
                aStarPathUtil.Reset(mapGridItems.GetLength(0), mapGridItems.GetLength(1), start, end, obstacles);
                List<AStarPathUtil.Node> nodes = aStarPathUtil.GetShortestPath(true);
                if (nodes.Count > activingRole.speed) //����̫Զ��ѡ������ƶ�����
                {
                    AStarPathUtil.Node maxSpeedNode = nodes[activingRole.speed - 1];
                    actionStrategySmart.SetMoveTargetGridItem(mapGridItems[maxSpeedNode.x, maxSpeedNode.y]);
                    isNeedMove = true;
                }
                else
                {
                    if (nodes.Count > 0) //Ѱ·�����һ��
                    {
                        AStarPathUtil.Node lastPathNode = nodes[nodes.Count - 1];
                        actionStrategySmart.SetMoveTargetGridItem(mapGridItems[lastPathNode.x, lastPathNode.y]);
                        isNeedMove = true;
                    }
                    else //ԭ��
                    {
                        actionStrategySmart.SetMoveTargetGridItem(mapGridItems[activingRole.battleOriginPosX, activingRole.battleOriginPosZ]);
                        isNeedMove = false;
                    }
                }

                //ѡ���˺���ߵ���ͨ��todo ��ʵҲ��һ��Ҫѡ�˺���ߵģ�mp���������£�Ҳ����ѡ������mp���Ľ�С�ģ��Ժ��ٸ�����һ��ɣ�
                SortedSet<Shentong> sortST = new SortedSet<Shentong>(new ShentongDamageSort());
                foreach (Shentong shentong in activingRole.shentongInBattle)
                {
                    if(shentong.damage > 0 && shentong.effType == ShentongEffType.Gong_Ji) sortST.Add(shentong);
                }

                //�ȼ�������Ƿ��㹻ʩ��
                activingRole.selectedShentong = null;
                foreach (Shentong st in sortST)
                {
                    //ѡ��һ���㹻mpʩչ����ͨ���˺������������ڹ�����Χ�ڵ� todo ...
                    //if (st.needMp <= activingRole.mp && (������Χ��)) 
                    //{
                    //    actionStrategySmart.SetSelectShentong(st);
                    //    break;
                    //}
                }

                //û�к��ʵ���ͨ���������������Ի��߹���������ɱ�ĵ��˻������mp̫�������Ȳ���mp(mp����30%���Ȳ���)��hp����20%�����Ȳ�hp
                if (activingRole.selectedShentong == null)
                {
                    if(activingRole.hp / activingRole.maxHp < 0.2f)
                    {
                        //todo ...
                    }
                    else if (activingRole.mp / activingRole.maxMp < 0.3f)
                    {
                        //��������,���Դ���Ʒ���䣬Ҳ���Դ�����Ȼ����
                        foreach (RoleItem item in activingRole.roleItems)
                        {
                            //����ǻָ�mp���ߣ���ʹ��
                            if (item.recoverMp > 0)
                            {
                                activingRole.selectRoleItem = item;
                                //actionStrategySmart.SetSelectRoleItem(item); //todo ʹ���������Ҫset null
                                activingRole.roleItems.Remove(item);
                                break;
                            }
                        }
                        if (activingRole.selectRoleItem == null) //û�лָ�mp�ĵ����ˣ�ֻ�ȿ�������Ϣ�ָ�mp
                        {
                            if (isNeedMove) //�������ƶ����Ϣ
                            {
                                actionStrategySmart.SetIsPass(true);
                            }
                            else //�Ѿ���������ߣ�ֱ�ӵ�Ϣ
                            {
                                actionStrategySmart.SetIsPass(true);
                            }
                        }
                    }
                    else
                    {
                        //todo ... �����������Ի��߹���������ɱ�ĵ���
                    }
                }
                else //�к��ʵĹ�����ͨ
                {
                    //todo ...
                    //actionStrategySmart.SetAttackMapGridItem();
                }

                return true;
            }
        }

        return false;
    }

    private class ShentongDamageSort : IComparer<Shentong>
    {
        public int Compare(Shentong x, Shentong y)
        {
            return x.damage - y.damage;
        }
    }

}

/// <summary>
/// ��������
/// </summary>
public class ActionNodeMaxTotalDamage : IActionNode
{
    public ActionNodeMaxTotalDamage(float priority, string name = null) : base(priority, name)
    {
    }

    public override bool Run(GameObject activingRoleGO, List<GameObject> allRoleGO, GameObject[,] mapGridItems, ActionStrategySmart actionStrategySmart)
    {
        throw new System.NotImplementedException();
    }
}

/// <summary>
/// ��������ĵ���
/// </summary>
public class ActionNodeAttackShortestDistance: IActionNode
{
    public ActionNodeAttackShortestDistance(float priority, string name = null) : base(priority, name)
    {
    }

    public override bool Run(GameObject activingRoleGO, List<GameObject> allRoleGO, GameObject[,] mapGridItems, ActionStrategySmart actionStrategySmart)
    {
        throw new System.NotImplementedException();
    }
}

/// <summary>
/// ̰�� ���
/// </summary>
public class ActionNodeGreedyAlgorithm : IActionNode
{
    public ActionNodeGreedyAlgorithm(float priority, string name = null) : base(priority, name)
    {
    }

    public override bool Run(GameObject activingRoleGO, List<GameObject> allRoleGO, GameObject[,] mapGridItems, ActionStrategySmart actionStrategySmart)
    {
        return true;
    }
}

public class ActionNodeMpNotEnough : IActionNode
{
    public ActionNodeMpNotEnough(float priority, string name = null) : base(priority, name)
    {
    }

    public override bool Run(GameObject activingRoleGO, 
        List<GameObject> allRoleGO, 
        GameObject[,] mapGridItems, 
        ActionStrategySmart actionStrategySmart)
    {
        BaseRole activingRole = activingRoleGO.GetComponent<BaseRole>();

        //��������Ǹ������Ƿ�NPCʹ�õ�
        if (activingRole.teamNum != TeamNum.TEAM_TWO) {
            Debug.LogError("����ڵ����ֻ������TeamNum.TEAM_TWO����");
            return false;
        }

        foreach(Shentong st in activingRole.shentongInBattle)
        {
            if(st.needMp <= activingRole.mp && st.effType == ShentongEffType.Gong_Ji)
            {
                //���㹻��mpʹ�ù�����ͨ
                return false;
            }
        }

        //�޹�����ͨ���ã�ʹ�õ��߻��ߵ�Ϣ����mp
        //�������Ѫ����20%���£��������ƶ�������Զ������
        HanLiScriptInBattle hanLiScriptInBattle = GetHanLi(allRoleGO);
        if(hanLiScriptInBattle.hp / hanLiScriptInBattle.maxHp < 0.2f) //�������ƶ�
        {
            Debug.LogWarning("����hp����20%������������");
            AStarPathUtil aStarPathUtil = new AStarPathUtil();
            (int, int) start = (activingRole.battleOriginPosX, activingRole.battleOriginPosZ);
            (int, int) end = (hanLiScriptInBattle.battleOriginPosX, hanLiScriptInBattle.battleOriginPosZ);
            aStarPathUtil.Reset(mapGridItems.GetLength(0), mapGridItems.GetLength(1), start, end, GetObstacles(allRoleGO, activingRoleGO, hanLiScriptInBattle.gameObject));
            List<AStarPathUtil.Node> nodes = aStarPathUtil.GetShortestPath(true);
            if (nodes.Count > activingRole.speed) //����̫Զ��ѡ������ƶ�����
            {
                AStarPathUtil.Node maxSpeedNode = nodes[activingRole.speed - 1];
                actionStrategySmart.SetMoveTargetGridItem(mapGridItems[maxSpeedNode.x, maxSpeedNode.y]);
            }
            else
            {
                if (nodes.Count > 0) //Ѱ·�����һ��
                {
                    AStarPathUtil.Node lastPathNode = nodes[nodes.Count - 1];
                    actionStrategySmart.SetMoveTargetGridItem(mapGridItems[lastPathNode.x, lastPathNode.y]);
                }
                else //����ԭ��
                {
                    actionStrategySmart.SetMoveTargetGridItem(mapGridItems[activingRole.battleOriginPosX, activingRole.battleOriginPosZ]);
                }
            }
        }
        else //Զ������
        {
            Debug.LogWarning("Զ�������ж�");
            int maxDistance = 0;
            GameObject targetMapGridItem = null;
            foreach(GameObject canMoveMapGridItem in activingRole.lastAllCanMoveGrids)
            {
                int distance = ManHaDunDistanceTrim(MapUtil.GetPositionFromGridItemGO(canMoveMapGridItem), hanLiScriptInBattle.battleOriginPosition);
                if (distance > maxDistance) 
                {
                    maxDistance = distance;
                    targetMapGridItem = canMoveMapGridItem;
                }
            }
            actionStrategySmart.SetMoveTargetGridItem(targetMapGridItem);
        }

        foreach(RoleItem item in activingRole.roleItems) //��ѡ���߻ָ�mp
        {
            if(item.recoverMp > 0)
            {
                activingRole.selectRoleItem = item;
                break;
            }
        }

        if(activingRole.selectRoleItem == null) //�޵��߿���->��Ϣ
        {
            actionStrategySmart.SetIsPass(true);
        }

        return true;
    }
}
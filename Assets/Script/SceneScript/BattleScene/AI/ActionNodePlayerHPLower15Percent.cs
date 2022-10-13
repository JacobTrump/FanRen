using System.Collections.Generic;
using Unity.VisualScripting;
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

    public override bool Run(GameObject activingRoleGO, List<GameObject> allRoleGO, GameObject[,] mapGridItems, ActionStrategyGeneral actionStrategyGeneral)
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
                    actionStrategyGeneral.SetMoveTargetGridItem(mapGridItems[maxSpeedNode.x, maxSpeedNode.y]);
                    isNeedMove = true;
                }
                else
                {
                    if (nodes.Count > 0) //Ѱ·�����һ��
                    {
                        AStarPathUtil.Node lastPathNode = nodes[nodes.Count - 1];
                        actionStrategyGeneral.SetMoveTargetGridItem(mapGridItems[lastPathNode.x, lastPathNode.y]);
                        isNeedMove = true;
                    }
                    else //ԭ��
                    {
                        actionStrategyGeneral.SetMoveTargetGridItem(mapGridItems[activingRole.battleOriginPosX, activingRole.battleOriginPosZ]);
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
                                actionStrategyGeneral.SetIsPass(true);
                            }
                            else //�Ѿ���������ߣ�ֱ�ӵ�Ϣ
                            {
                                actionStrategyGeneral.SetIsPass(true);
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
/// ̰�� ��� ����
/// </summary>
public class ActionNodeGreedyAlgorithm : IActionNode
{
    public ActionNodeGreedyAlgorithm(float priority, string name = null) : base(priority, name)
    {
    }

    public override bool Run(GameObject activingRoleGO, List<GameObject> allRoleGO, GameObject[,] mapGridItems, ActionStrategyGeneral actionStrategyGeneral)
    {
        Debug.Log("Run " + this.name);
        BaseRole currentRole = activingRoleGO.GetComponent<BaseRole>();

        //����λ��
        Dictionary<(int, int), GameObject> position_roleGO = new Dictionary<(int, int), GameObject>();
        foreach (GameObject roleGO in allRoleGO)
        {
            if (roleGO == null || roleGO.IsDestroyed()) continue;
            BaseRole role = roleGO.GetComponent<BaseRole>();
            if (role.teamNum != currentRole.teamNum)
            {
                position_roleGO.Add((role.battleOriginPosX, role.battleOriginPosZ), roleGO);
            }
        }

        Shentong[] roleST = currentRole.shentongInBattle;

        List<GameObject> canMoveMapGrids = currentRole.lastAllCanMoveGrids;

        int maxDamage = 0;

        foreach (GameObject canMoveMapGrid in canMoveMapGrids)
        {
            (int, int) rolePosition = MapUtil.GetPositionFromGridItemGO(canMoveMapGrid);
            foreach(Shentong st in roleST)
            {
                if (st == null) continue;
                if (st.rangeType == ShentongRangeType.Point)
                {
                    int minX = rolePosition.Item1 - st.unitDistance;
                    if (minX < 0) minX = 0;

                    int maxX = rolePosition.Item1 + st.unitDistance;
                    if (maxX > mapGridItems.GetLength(0)-1) maxX = mapGridItems.GetLength(0)-1;

                    int minY = rolePosition.Item2 - st.unitDistance;
                    if (minY < 0) minY = 0;

                    int maxY = rolePosition.Item2 + st.unitDistance;
                    if (maxY > mapGridItems.GetLength(1)-1) maxY = mapGridItems.GetLength(1) - 1;

                    for(int i=minX; i<= maxX; i++)
                    {
                        for (int j = minY; j <= maxY; j++)
                        {
                            if((ManHaDunDistanceTrim(rolePosition, (i, j))+1) <= st.unitDistance) //������Χ
                            {
                                GameObject enemyGO = position_roleGO.GetValueOrDefault((i, j), null);
                                if (enemyGO != null)//���������ж��˵ĵ���
                                {
                                    int damage = currentRole.CalculateDamage(enemyGO.GetComponent<BaseRole>(), st);
                                    if (damage > maxDamage)
                                    {
                                        maxDamage = damage;
                                        actionStrategyGeneral.SetMoveTargetGridItem(canMoveMapGrid); //�ƶ�����
                                        currentRole.selectedShentong = st;//ʹ��ʲô��ͨ
                                        actionStrategyGeneral.SetAttackMapGridItem(mapGridItems[i, j]);//������һ��
                                    }
                                }
                            }
                        }
                    }

                }
                else if (st.rangeType == ShentongRangeType.Line)
                {
                    int minX = rolePosition.Item1 - st.unitDistance;
                    if (minX < 0) minX = 0;

                    int maxX = rolePosition.Item1 + st.unitDistance;
                    if (maxX > mapGridItems.GetLength(0) - 1) maxX = mapGridItems.GetLength(0) - 1;

                    int minY = rolePosition.Item2 - st.unitDistance;
                    if (minY < 0) minY = 0;

                    int maxY = rolePosition.Item2 + st.unitDistance;
                    if (maxY > mapGridItems.GetLength(1) - 1) maxY = mapGridItems.GetLength(1) - 1;

                    int thisGroupTotalDamage = 0;
                    for(int i= minX; i< rolePosition.Item1; i++)
                    {
                        GameObject enemyGO = position_roleGO.GetValueOrDefault((i, rolePosition.Item2), null);
                        if(enemyGO != null)
                        {
                            thisGroupTotalDamage += currentRole.CalculateDamage(enemyGO.GetComponent<BaseRole>(), st);
                        }
                    }
                    if(thisGroupTotalDamage > maxDamage)
                    {
                        maxDamage = thisGroupTotalDamage;
                        actionStrategyGeneral.SetMoveTargetGridItem(canMoveMapGrid); //�ƶ�����
                        currentRole.selectedShentong = st;//ʹ��ʲô��ͨ
                        actionStrategyGeneral.SetAttackMapGridItem(mapGridItems[rolePosition.Item1-1, rolePosition.Item2]);//������һ��
                    }

                    thisGroupTotalDamage = 0;
                    for (int i = rolePosition.Item1+1; i <= maxX; i++)
                    {
                        GameObject enemyGO = position_roleGO.GetValueOrDefault((i, rolePosition.Item2), null);
                        if (enemyGO != null)
                        {
                            thisGroupTotalDamage += currentRole.CalculateDamage(enemyGO.GetComponent<BaseRole>(), st);
                        }
                    }
                    if (thisGroupTotalDamage > maxDamage)
                    {
                        maxDamage = thisGroupTotalDamage;
                        actionStrategyGeneral.SetMoveTargetGridItem(canMoveMapGrid); //�ƶ�����
                        currentRole.selectedShentong = st;//ʹ��ʲô��ͨ
                        actionStrategyGeneral.SetAttackMapGridItem(mapGridItems[rolePosition.Item1 + 1, rolePosition.Item2]);//������һ��
                    }

                    thisGroupTotalDamage = 0;
                    for (int i = minY; i < rolePosition.Item2; i++)
                    {
                        GameObject enemyGO = position_roleGO.GetValueOrDefault((rolePosition.Item1, i), null);
                        if (enemyGO != null)
                        {
                            thisGroupTotalDamage += currentRole.CalculateDamage(enemyGO.GetComponent<BaseRole>(), st);
                        }
                    }
                    if (thisGroupTotalDamage > maxDamage)
                    {
                        maxDamage = thisGroupTotalDamage;
                        actionStrategyGeneral.SetMoveTargetGridItem(canMoveMapGrid); //�ƶ�����
                        currentRole.selectedShentong = st;//ʹ��ʲô��ͨ
                        actionStrategyGeneral.SetAttackMapGridItem(mapGridItems[rolePosition.Item1, rolePosition.Item2 - 1]);//������һ��
                    }

                    thisGroupTotalDamage = 0;
                    for (int i = rolePosition.Item2 + 1; i <= maxY; i++)
                    {
                        GameObject enemyGO = position_roleGO.GetValueOrDefault((rolePosition.Item1, i), null);
                        if (enemyGO != null)
                        {
                            thisGroupTotalDamage += currentRole.CalculateDamage(enemyGO.GetComponent<BaseRole>(), st);
                        }
                    }
                    if (thisGroupTotalDamage > maxDamage)
                    {
                        maxDamage = thisGroupTotalDamage;
                        actionStrategyGeneral.SetMoveTargetGridItem(canMoveMapGrid); //�ƶ�����
                        currentRole.selectedShentong = st;//ʹ��ʲô��ͨ
                        actionStrategyGeneral.SetAttackMapGridItem(mapGridItems[rolePosition.Item1, rolePosition.Item2 + 1]);//������һ��
                    }

                }
                else if (st.rangeType == ShentongRangeType.Plane)
                {
                    int minX = rolePosition.Item1 - st.unitDistance;
                    if (minX < 0) minX = 0;

                    int maxX = rolePosition.Item1 + st.unitDistance;
                    if (maxX > mapGridItems.GetLength(0) - 1) maxX = mapGridItems.GetLength(0) - 1;

                    int minY = rolePosition.Item2 - st.unitDistance;
                    if (minY < 0) minY = 0;

                    int maxY = rolePosition.Item2 + st.unitDistance;
                    if (maxY > mapGridItems.GetLength(1) - 1) maxY = mapGridItems.GetLength(1) - 1;

                    int thisGroupTotalDamage = 0;

                    for (int i = minX; i <= maxX; i++)
                    {
                        for (int j = minY; j <= maxY; j++)
                        {
                            if ((ManHaDunDistanceTrim(rolePosition, (i, j)) + 1) <= st.unitDistance) //�������Χ
                            {
                                (int, int) attackCore = (i, j);

                                int minX2 = attackCore.Item1 - st.planeRadius;
                                if (minX2 < 0) minX2 = 0;

                                int maxX2 = attackCore.Item1 + st.planeRadius;
                                if (maxX2 > mapGridItems.GetLength(0) - 1) maxX2 = mapGridItems.GetLength(0) - 1;

                                int minY2 = attackCore.Item2 - st.planeRadius;
                                if (minY2 < 0) minY2 = 0;

                                int maxY2 = attackCore.Item2 + st.planeRadius;
                                if (maxY2 > mapGridItems.GetLength(1) - 1) maxY2 = mapGridItems.GetLength(1) - 1;

                                for (int m = minX2; m <= maxX2; m++)
                                {
                                    for (int n = minY2; n <= maxY2; n++)
                                    {
                                        if ((ManHaDunDistanceTrim(attackCore, (m, n)) + 1) <= st.planeRadius) //�˺���Χ
                                        {
                                            GameObject enemyGO = position_roleGO.GetValueOrDefault((m, n), null);
                                            if (enemyGO != null)//���������ж��˵ĵ���
                                            {
                                                thisGroupTotalDamage += currentRole.CalculateDamage(enemyGO.GetComponent<BaseRole>(), st);
                                            }
                                        }
                                    }
                                }

                                if(thisGroupTotalDamage > maxDamage)
                                {
                                    maxDamage = thisGroupTotalDamage;
                                    actionStrategyGeneral.SetMoveTargetGridItem(canMoveMapGrid); //�ƶ�����
                                    currentRole.selectedShentong = st;//ʹ��ʲô��ͨ
                                    actionStrategyGeneral.SetAttackMapGridItem(mapGridItems[i, j]);//������һ��
                                }
                                thisGroupTotalDamage = 0;

                            }
                        }
                    }
                }
            }
        }

        if(maxDamage > 0)
        {
            actionStrategyGeneral.SetIsPass(false);
            return true;
        }
        else
        {
            return false;
        }
        
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
        ActionStrategyGeneral actionStrategyGeneral)
    {
        Debug.Log("Run " + this.name);
        BaseRole activingRole = activingRoleGO.GetComponent<BaseRole>();

        //��������Ǹ������Ƿ�NPCʹ�õ�
        if (activingRole.teamNum != TeamNum.TEAM_TWO) {
            Debug.LogError("����ڵ����ֻ������TeamNum.TEAM_TWO����");
            return false;
        }

        HanLiScriptInBattle hanLiScriptInBattle = GetHanLi(allRoleGO);

        foreach (Shentong st in activingRole.shentongInBattle)
        {
            if (st == null) continue;
            //if ((((float)hanLiScriptInBattle.hp) / ((float)hanLiScriptInBattle.maxHp)) > 0.1f && st.shenTongId == 1) //����Ѫ����10%����ʱ�򣬲�Ҫ����ͨ����(ֻ����ͨ�����أ�)
            //{
            //    continue;
            //}
            if (st.needMp <= activingRole.mp && st.effType == ShentongEffType.Gong_Ji)
            {
                //���㹻��mpʹ�ù�����ͨ
                return false;
            }
        }

        //�޹�����ͨ���ã�ʹ�õ��߻��ߵ�Ϣ����mp
        //�������Ѫ����10%���£��������ƶ�������Զ������
        if(((float)hanLiScriptInBattle.hp / (float)hanLiScriptInBattle.maxHp) < 0.1f) //�������ƶ�
        {
            Debug.LogWarning("����hp����10%������������");
            AStarPathUtil aStarPathUtil = new AStarPathUtil();
            (int, int) start = (activingRole.battleOriginPosX, activingRole.battleOriginPosZ);
            (int, int) end = (hanLiScriptInBattle.battleOriginPosX, hanLiScriptInBattle.battleOriginPosZ);
            aStarPathUtil.Reset(mapGridItems.GetLength(0), mapGridItems.GetLength(1), start, end, GetObstacles(allRoleGO, activingRoleGO, hanLiScriptInBattle.gameObject));
            List<AStarPathUtil.Node> nodes = aStarPathUtil.GetShortestPath(true);
            if (nodes.Count > activingRole.speed) //����̫Զ��ѡ������ƶ�����
            {
                AStarPathUtil.Node maxSpeedNode = nodes[activingRole.speed - 1];
                actionStrategyGeneral.SetMoveTargetGridItem(mapGridItems[maxSpeedNode.x, maxSpeedNode.y]);
            }
            else
            {
                if (nodes.Count > 0) //Ѱ·�����һ��
                {
                    AStarPathUtil.Node lastPathNode = nodes[nodes.Count - 1];
                    actionStrategyGeneral.SetMoveTargetGridItem(mapGridItems[lastPathNode.x, lastPathNode.y]);
                }
                else //����ԭ��
                {
                    actionStrategyGeneral.SetMoveTargetGridItem(mapGridItems[activingRole.battleOriginPosX, activingRole.battleOriginPosZ]);
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
            actionStrategyGeneral.SetMoveTargetGridItem(targetMapGridItem);
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
            actionStrategyGeneral.SetIsPass(true);
        }

        Debug.Log("ִ����ActionNodeMpNotEnough");
        return true;
    }
}
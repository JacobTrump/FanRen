using System.Collections.Generic;
using UnityEngine;

public class ActionNodePlayerHPLower15Percent : IActionNode
{
    public ActionNodePlayerHPLower15Percent(float priority, string name = null) : base(priority, name)
    {
    }

    public override bool Run(GameObject activingRoleGO, List<GameObject> allRoleGO, GameObject[,] mapGridItems, List<GameObject> allCanMoveGridItems, ActionStrategySmart actionStrategySmart)
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
            HanLi hanLi = hanLiGO.GetComponent<HanLi>();
            if (hanLi.hp / hanLi.maxHp <= 0.15f) //����Ѫ������15%���������ǣ�ȫ����������
            {
                Debug.LogWarning("����hp����15%����������ȫ������ģ��");
                AStarPathUtil aStarPathUtil = new AStarPathUtil();
                (int, int) start = (activingRole.battleOriginPosX, activingRole.battleOriginPosZ);
                (int, int) end = (hanLi.battleOriginPosX, hanLi.battleOriginPosZ);
                aStarPathUtil.Reset(mapGridItems.GetLength(0), mapGridItems.GetLength(1), start, end, obstacles);
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
                    else //ԭ��
                    {
                        actionStrategySmart.SetMoveTargetGridItem(mapGridItems[activingRole.battleOriginPosX, activingRole.battleOriginPosZ]);
                    }
                }

                //ѡ���˺���ߵ���ͨ
                Shentong maxDamageShentong = activingRole.shentongInBattle[0];
                foreach (Shentong shentong in activingRole.shentongInBattle)
                {
                    if (shentong.effType == ShentongEffType.Gong_Ji)
                    {
                        if (shentong.damage > maxDamageShentong.damage)
                        {
                            maxDamageShentong = shentong;
                        }
                    }
                }

                //�ȼ�������Ƿ��㹻ʩ��
                if (activingRole.mp >= maxDamageShentong.needMp)
                {
                    actionStrategySmart.SetSelectShentong(maxDamageShentong);
                }
                else //todo ��������,���Դ���Ʒ���䣬Ҳ���Դ�����Ȼ����
                {
                    actionStrategySmart.SetSelectShentong(null);

                }

                return true;
            }
        }

        return false;
    }
}

public class ActionNodeMaxTotalDamage : IActionNode
{
    public ActionNodeMaxTotalDamage(float priority, string name = null) : base(priority, name)
    {
    }

    public override bool Run(GameObject activingRoleGO, List<GameObject> allRoleGO, GameObject[,] mapGridItems, List<GameObject> allCanMoveGridItems, ActionStrategySmart actionStrategySmart)
    {
        throw new System.NotImplementedException();
    }
}

public class ActionNodeAttackShortestDistance: IActionNode
{
    public ActionNodeAttackShortestDistance(float priority, string name = null) : base(priority, name)
    {
    }

    public override bool Run(GameObject activingRoleGO, List<GameObject> allRoleGO, GameObject[,] mapGridItems, List<GameObject> allCanMoveGridItems, ActionStrategySmart actionStrategySmart)
    {
        throw new System.NotImplementedException();
    }
}
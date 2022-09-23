using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �˲���ÿ��ֻ���ߵ������������λ��ʹ����ͨ����
/// </summary>
public class ActionStrategyGeneral : ActionStrategy
{

    //���غ��ߵ���һ��
    private GameObject moveTargetGridItem;
    //�����ĸ�����
    private GameObject attackMapGrid;
    //���غ��Ƿ����, �������ȣ������ж�
    private bool isPass = false;
    //ѡ����ʹ�õ���
    //private RoleItem selectRoleItem;

    /// <summary>
    /// �̳б��࣬��д���������NPCʵ���µ�ս�����ԣ�ս����ʼ����ʱ�򸳸���ɫ����
    /// 
    /// </summary>
    /// <param name="activingRoleGO">��ɫ�Ļغ�</param>
    /// <param name="allRole">ȫ����ɫս����Ϣ</param>
    /// <param name="mapGrids">ȫ����ͼ������Ϣ</param>
    public override void GenerateStrategy(GameObject activingRoleGO, List<GameObject> allRoleGO, GameObject[,] mapGridItems)
    {
        GameObject hanLiGO = null;
        List<(int, int)> obstacles = new List<(int, int)>();
        BaseRole activingRole = activingRoleGO.GetComponent<BaseRole>();
        foreach (GameObject item in allRoleGO)
        {
            if (item == null || !item.activeInHierarchy || !item.activeSelf) continue;
            if (item.tag.Equals("Player"))
            {
                hanLiGO = item;
                continue; //Ŀ�겻���ϰ���
            }
            if (item == activingRoleGO) continue; //�Լ������ϰ���
            BaseRole itemRole = item.GetComponent<BaseRole>();
            obstacles.Add((itemRole.battleOriginPosX, itemRole.battleOriginPosZ));
        }
        BaseRole hanLiRole = hanLiGO.GetComponent<BaseRole>();
        //BaseRole activingRole = activingRoleGO.GetComponent<BaseRole>();
        AStarPathUtil aStarPathUtil = new AStarPathUtil();
        aStarPathUtil.Reset(mapGridItems.GetLength(0), mapGridItems.GetLength(1), (activingRole.battleOriginPosX, activingRole.battleOriginPosZ), (hanLiRole.battleOriginPosX, hanLiRole.battleOriginPosZ), obstacles);
        List<AStarPathUtil.Node> nodes = aStarPathUtil.GetShortestPath(true);
        
        if(nodes != null)
        {
            //���ԣ�ֱ��������ߣ�����ͨ��������������Ϊ1

            //����֮��ĸ��� + 1
            int targetDistance = nodes.Count + 1;
            //���Թ�������Զ����
            int attackDistance = activingRole.speed + activingRole.shentongInBattle[0].unitDistance;
            if (targetDistance > attackDistance) //Ŀ����볬��(�ƶ�+����)���룬ѡ�����Ŀ��ƶ�����
            {
                AStarPathUtil.Node maxSpeedCanReachNode = nodes[activingRole.speed-1];
                this.moveTargetGridItem = mapGridItems[maxSpeedCanReachNode.x, maxSpeedCanReachNode.y];
                this.isPass = true;
            }
            else //Ŀ������ڹ�����Χ��
            {
                if (nodes.Count > 0)
                {
                    AStarPathUtil.Node targetNode = nodes[nodes.Count - 1]; //������ߵĸ��ӣ��������ʹ��������ͨ����ôӦ����һ����ѡ�ķ�Χ
                    this.moveTargetGridItem = mapGridItems[targetNode.x, targetNode.y];
                    this.isPass = false;
                }
                else if (nodes.Count == 0)
                {
                    this.moveTargetGridItem = mapGridItems[activingRole.battleOriginPosX, activingRole.battleOriginPosZ];
                    this.isPass = false;
                }
            }
        }
        else
        {
            //todo AIһ����ԣ���·���ߣ�����ԭ�أ���������������ʽ
            Debug.LogWarning("AIһ����ԣ���·���ߣ�����ԭ�أ���������������ʽ");
            this.moveTargetGridItem = mapGridItems[activingRole.battleOriginPosX, activingRole.battleOriginPosZ];
            this.isPass = true;
        }
        
        activingRole.selectedShentong = activingRole.shentongInBattle[0];
        this.attackMapGrid = mapGridItems[hanLiRole.battleOriginPosX, hanLiRole.battleOriginPosZ];
    }


    public override GameObject GetMoveTargetGridItem()
    {
        return this.moveTargetGridItem;
    }

    public void SetMoveTargetGridItem(GameObject moveTargetGridItem)
    {
        this.moveTargetGridItem = moveTargetGridItem;
    }

    public override GameObject GetAttackMapGridItem()
    {
        return this.attackMapGrid;
    }

    public void SetAttackMapGridItem(GameObject attackMapGrid)
    {
        this.attackMapGrid = attackMapGrid;
    }

    public override bool IsPass()
    {
        return this.isPass;
    }

    public void SetIsPass(bool isPass)
    {
        this.isPass = isPass;
    }

}
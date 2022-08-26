using System.Collections.Generic;
using UnityEngine;

public class ActionStrategyGeneral : ActionStrategy
{

    //���غ��ߵ���һ��
    protected GameObject moveTargetGridItem;
    //ѡ��ʲô��ͨ
    protected Shentong selectShentong;
    //�����ĸ�����
    protected GameObject attackMapGrid;
    //�ƶ����Ƿ�ֱ�Ӵ���
    protected bool isPassAfterMove = false;
    //���غ��Ƿ����, �������ȣ������ж�
    protected bool isPass = false;

    /// <summary>
    /// �̳б��࣬��д���������NPCʵ���µ�ս�����ԣ�ս����ʼ����ʱ�򸳸���ɫ����
    /// </summary>
    /// <param name="activingRoleGO">��ɫ�Ļغ�</param>
    /// <param name="allRole">ȫ����ɫս����Ϣ</param>
    /// <param name="mapGrids">ȫ����ͼ������Ϣ</param>
    public override void GenerateStrategy(GameObject activingRoleGO, List<GameObject> allRole, GameObject[,] mapGrids)
    {
        GameObject hanLiGO = null;
        List<(int, int)> obstacles = new List<(int, int)>();
        BaseRole activingRole = activingRoleGO.GetComponent<BaseRole>();
        foreach (GameObject item in allRole)
        {
            if (item == null || !item.activeInHierarchy || !item.activeSelf) continue;
            if (item.tag.Equals("Player"))
            {
                hanLiGO = item;
                continue; //Ŀ�겻���ϰ���
            }
            if (item == activingRoleGO) continue; //�Լ������ϰ���
            BaseRole itemRole = item.GetComponent<BaseRole>();
            //if (itemRole.teamNum == activingRole.teamNum) //todo �������ϰ����Ŀ�����Ҳ���ϰ���
            //{
            //    if (item == activingRoleGO) continue;
            //    obstacles.Add((itemRole.battleOriginPosX, itemRole.battleOriginPosZ));
            //}
            obstacles.Add((itemRole.battleOriginPosX, itemRole.battleOriginPosZ));
        }
        BaseRole hanLiRole = hanLiGO.GetComponent<BaseRole>();
        //BaseRole activingRole = activingRoleGO.GetComponent<BaseRole>();
        AStarPathUtil aStarPathUtil = new AStarPathUtil();
        aStarPathUtil.Reset(mapGrids.GetLength(0), mapGrids.GetLength(1), (activingRole.battleOriginPosX, activingRole.battleOriginPosZ), (hanLiRole.battleOriginPosX, hanLiRole.battleOriginPosZ), obstacles);
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
                this.moveTargetGridItem = mapGrids[maxSpeedCanReachNode.x, maxSpeedCanReachNode.y];
                this.isPassAfterMove = true;
            }
            else //Ŀ������ڹ�����Χ��
            {
                if (nodes.Count > 0)
                {
                    AStarPathUtil.Node targetNode = nodes[nodes.Count - 1]; //������ߵĸ��ӣ��������ʹ��������ͨ����ôӦ����һ����ѡ�ķ�Χ
                    this.moveTargetGridItem = mapGrids[targetNode.x, targetNode.y];
                    this.isPassAfterMove = false;
                }
                else if (nodes.Count == 0)
                {
                    this.moveTargetGridItem = mapGrids[activingRole.battleOriginPosX, activingRole.battleOriginPosZ];
                    this.isPassAfterMove = false;
                }
            }
        }
        else
        {
            //todo AIһ����ԣ���·���ߣ�����ԭ�أ���������������ʽ
            Debug.LogWarning("AIһ����ԣ���·���ߣ�����ԭ�أ���������������ʽ");
            this.moveTargetGridItem = mapGrids[activingRole.battleOriginPosX, activingRole.battleOriginPosZ];
            this.isPassAfterMove = true;
        }
        
        this.selectShentong = activingRole.shentongInBattle[0];
        this.attackMapGrid = mapGrids[hanLiRole.battleOriginPosX, hanLiRole.battleOriginPosZ];
    }


    public override GameObject GetMoveTargetGridItem()
    {
        return this.moveTargetGridItem;
    }

    public override Shentong GetSelectShentong()
    {
        return this.selectShentong;
    }

    public override GameObject GetAttackMapGridItem()
    {
        return this.attackMapGrid;
    }

    public override bool IsPassAfterMove()
    {
        return this.isPassAfterMove;
    }

    public override bool IsPass()
    {
        return this.isPass;
    }
}

public class ActionStrategySmart : ActionStrategyGeneral
{
    //����һ��ԭ����������
    //��ɫѪ��������10%���£�����һ�иɵ��� ���ȼ���ߣ�9999 (�������ƶ���һ·���п��Թ�����Ŀ���˳�ֹ���������������㣬�����Ȳ�������)
    //����Ѫ����������״̬�£��������ƶ�
    //һ����������ȹ�������ɹ���Ŀ�꣬������������ڣ����ȹ������ǣ�
    //����ѡ���˺��������������̷�Χ�ڵ���ͨ
    //������Χ���л���ĨɱĿ�������£���������ȣ���ǰ���ǰ���£��ᾡ����ͬʱ�������Ŀ��
    //����û�����������Ȳ�������������Ҫ
    public override void GenerateStrategy(GameObject activingRoleGO, List<GameObject> allRole, GameObject[,] mapGrids)
    {

    }

}
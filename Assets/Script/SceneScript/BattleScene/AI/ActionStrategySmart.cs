using System.Collections.Generic;
using UnityEngine;

public class ActionStrategySmart : ActionStrategyGeneral
{
    //����һ��ԭ����������
    //��ɫѪ��������10%���£�����һ�иɵ��� ���ȼ���ߣ�9999 (�������ƶ���һ·���п��Թ�����Ŀ���˳�ֹ���������������㣬�����Ȳ�������)
    //����Ѫ����������״̬�£��������ƶ�
    //һ����������ȹ�������ɹ���Ŀ�꣬������������ڣ����ȹ������ǣ�
    //����ѡ���˺��������������̷�Χ�ڵ���ͨ
    //������Χ���л���ĨɱĿ�������£���������ȣ���ǰ���ǰ���£��ᾡ����ͬʱ�������Ŀ��
    //����û�����������Ȳ�������������Ҫ
    public override void GenerateStrategy(GameObject activingRoleGO, List<GameObject> allRoleGO, GameObject[,] mapGridItems, List<GameObject> allCanMoveGridItems)
    {
        new ActionNodeManager(activingRoleGO, allRoleGO, mapGridItems, allCanMoveGridItems, this)
           // .AddActionNode(new ActionNodePlayerHPLower15Percent(99.0f, "ִ�в��ԣ�����hp_lower_15%"))
           // .AddActionNode(new ActionNodeMaxTotalDamage(98.0f, "ִ�в��ԣ����˺����"))
           // .AddActionNode(new ActionNodeAttackShortestDistance(97.0f, "ִ�в��ԣ���������ĵ���"))
           .AddActionNode(new ActionNodeMpNotEnough(99.0f, "�ж�mp"))
           .AddActionNode(new ActionNodeGreedyAlgorithm(98.0f, "ִ�в��ԣ�̰���㷨�����"))
           .Execute();
    }

}

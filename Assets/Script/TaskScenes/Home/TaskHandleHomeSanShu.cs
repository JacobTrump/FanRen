using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskHandleHomeSanShu : ITaskHandle
{

    public const int ROLE_ID = 6;

    /// <summary>
    /// todo ...... �Ի�Ӧ�ô������ݿ����excel�����ع�
    /// </summary>
    /// <param name="taskId"></param>
    /// <returns></returns>
    public override Queue<TalkContentItemModel> TriggerTaskTalkData(int taskId)
    {

        Queue<TalkContentItemModel> allTalkContent = new Queue<TalkContentItemModel>();

        TalkContentItemModel talkContentItemModel = new TalkContentItemModel
        {
            dfAvatar = "hanLi",
            dfName = "����",
            dfTalkContent = "�����"
        };
        allTalkContent.Enqueue(talkContentItemModel);

        talkContentItemModel = new TalkContentItemModel
        {
            dfAvatar = "sanShu",
            dfName = "����",
            dfTalkContent = "�����������˰�����磬ɩ�ӣ��������������ʵ��Ҫ�������������¶����ҹ����ľ�¥������ͨ�ľ�¥������ʵ���ڽ�����һ���ܴ�����ɣ������š������������ź�����֮�֣�ǰ���ã�����ʽ��Ϊ�������ŵ����ŵ��ӣ��ܹ��ƾ��ߵ�ʮ����ĺ�ͯȥ�μ��������������ŵ��ӵĿ��顣�����������һ�Σ��¸��¾�Ҫ��ʼ�ˡ�����ɩ������Ҳ֪��������û�ж�Ů��������ȥ��Ҳ�����ǼҵĶ����������ʣ������ѵã���֪�������������ѽ��"
        };
        allTalkContent.Enqueue(talkContentItemModel);

        talkContentItemModel = new TalkContentItemModel
        {
            dfAvatar = "hanFu",
            dfName = "����",
            dfTalkContent = "�������ɣ�"
        };
        allTalkContent.Enqueue(talkContentItemModel);

        talkContentItemModel = new TalkContentItemModel
        {
            dfAvatar = "sanShu",
            dfName = "����",
            dfTalkContent = "��磬����Ǹ��û���ѽ�����������ⷽԲ����������һ�����Ĵ����ɣ�ֻҪС���������ŵ��ӣ������Ժ�������ϰ�䣬�ԺȲ��ÿ�»�����һ�����ɢ�����㻨�ء�"
        };
        allTalkContent.Enqueue(talkContentItemModel);

        talkContentItemModel = new TalkContentItemModel
        {
            dfAvatar = "hanFu",
            dfName = "����",
            dfTalkContent = "ÿ����һ�������ӣ���"
        };
        allTalkContent.Enqueue(talkContentItemModel);

        talkContentItemModel = new TalkContentItemModel
        {
            dfAvatar = "sanShu",
            dfName = "����",
            dfTalkContent = "��ѽ�����Ҳμӿ�����ˣ���ʹδ����ѡ��Ҳ�л����Ϊ����һ����������Ա��ר���������Ŵ�����������⡣������ѵã������ɵúú����룬�����ϧ��"
        };
        allTalkContent.Enqueue(talkContentItemModel);

        talkContentItemModel = new TalkContentItemModel
        {
            dfAvatar = "hanFu",
            dfName = "����",
            dfTalkContent = "................................"
        };
        allTalkContent.Enqueue(talkContentItemModel);

        talkContentItemModel = new TalkContentItemModel
        {
            dfAvatar = "hanFu",
            dfName = "����",
            dfTalkContent = "................."
        };
        allTalkContent.Enqueue(talkContentItemModel);

        talkContentItemModel = new TalkContentItemModel
        {
            dfAvatar = "hanFu",
            dfName = "����",
            dfTalkContent = "......"
        };
        allTalkContent.Enqueue(talkContentItemModel);

        talkContentItemModel = new TalkContentItemModel
        {
            dfAvatar = "hanFu",
            dfName = "����",
            dfTalkContent = "�У��Ǿ��ö���Ӹ���ȥ�ɡ�"
        };
        allTalkContent.Enqueue(talkContentItemModel);

        talkContentItemModel = new TalkContentItemModel
        {
            dfAvatar = "sanShu",
            dfName = "����",
            dfTalkContent = "�ã��ǿ�̫���ˣ����⻹��Щ���ӣ���������š��������Ѷȣ�����Ҳ����Ҫ�����ʱ�����Ƕ��С������óԵģ���Ӧ�����飬��һ���º������С���ߣ����Ҿ��ȸ���ˣ�����ɩ������"
        };
        allTalkContent.Enqueue(talkContentItemModel);

        talkContentItemModel = new TalkContentItemModel
        {
            dfAvatar = "hanLi",
            dfName = "����",
            dfTalkContent = "��Ȼ����ո�˵�Ļ���ȫ���ף����Һ�����Խ���������Ǯ�ˡ�",
            isInner = true
        };
        allTalkContent.Enqueue(talkContentItemModel);

        return allTalkContent;
    }

    public override Queue<TalkContentItemModel> InProgressTaskTalkData(int taskId)
    {
        return TriggerTaskTalkData(taskId);
    }

    public override Queue<TalkContentItemModel> SubmitTaskTalkData(int taskId)
    {
        throw new System.NotImplementedException();
    }

    public override Queue<TalkContentItemModel> GeneralTalkData()
    {
        Queue<TalkContentItemModel> allTalkContent = new Queue<TalkContentItemModel>();
        TalkContentItemModel talkContentItemModel = new TalkContentItemModel
        {
            dfAvatar = "sanShu",
            dfName = "����",
            dfTalkContent = "�����������˰�"
        };
        allTalkContent.Enqueue(talkContentItemModel);
        return allTalkContent;
    }

    public override bool IsTriggerable(int taskId)
    {
        MyDBManager.GetInstance().ConnDB();
        if(taskId == 3) //�����嵽��ţ������
        {
            return MyDBManager.GetInstance().GetRoleTask(1).taskState == (int)FRTaskState.Finished; //�ռ��ɲ�������ɿɴ���
        }
        else
        {
            Debug.LogError("�߼����� TaskHandleHomeSanShu IsTriggerable taskId " + taskId);
            return false;
        }
    }

    public override bool IsSubmitable(int taskId)
    {
        throw new System.NotImplementedException();
    }

    public override void OnSubmitTaskComplete(int taskId)
    {

    }

    public override void OnTriggerTask(int taskId)
    {
        MyDBManager.GetInstance().ConnDB();
        MyDBManager.GetInstance().AddRoleTask(4);
        MyDBManager.GetInstance().AddRoleTask(5);
        UIUtil.NotifyTaskUIDatasetChanged();
    }

}

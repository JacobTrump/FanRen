using System.Collections;
using UnityEngine;

public class TaskHandleHomeSanShu : ITaskHandle
{

    public const int ROLE_ID = 6;

    public override Queue TriggerTaskTalkData(int taskId)
    {
        //������ ���ͺ�����𡿡��ͺ�ĸ���
        //MyDBManager.GetInstance().ConnDB();
        //MyDBManager.GetInstance().AddRoleTask(5);
        //MyDBManager.GetInstance().AddRoleTask(4);
        //UIUtil.NotifyTaskUIDatasetChanged();


        Queue allTalkContent = new Queue();
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
            dfTalkContent = "�����������˰�"
        };
        allTalkContent.Enqueue(talkContentItemModel);

        //todo ......

        return allTalkContent;
    }

    public override Queue InProgressTaskTalkData(int taskId)
    {
        return TriggerTaskTalkData(taskId);
    }

    public override Queue SubmitTaskTalkData(int taskId)
    {
        throw new System.NotImplementedException();
    }

    public override Queue GeneralTalkData()
    {
        Queue allTalkContent = new Queue();
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

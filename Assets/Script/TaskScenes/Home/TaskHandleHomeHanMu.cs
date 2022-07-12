using System.Collections;
using UnityEngine;

public class TaskHandleHomeHanMu : ITaskHandle
{

    public const int ROLE_ID = 4;

    public override Queue TriggerTaskTalkData(int taskId)
    {
        Debug.LogError("�߼����󣬲�Ӧ���д˴�ӡ TaskHandleHomeHanMu TriggerTaskTalkData " + taskId);
        return null;
    }

    public override Queue InProgressTaskTalkData(int taskId)
    {
        return SubmitTaskTalkData(taskId);
    }

    public override Queue SubmitTaskTalkData(int taskId)
    {
        Queue allTalkContent = new Queue();
        TalkContentItemModel talkContentItemModel = new TalkContentItemModel
        {
            dfAvatar = "hanMu",
            dfName = "��ĸ",
            dfTalkContent = "������������Ҫ��ע�����壬Ҫ�Ժ�˯��"
        };
        allTalkContent.Enqueue(talkContentItemModel);
        return allTalkContent;
    }

    public override Queue GeneralTalkData()
    {
        MyDBManager.GetInstance().ConnDB();
        MyDBManager.RoleTask roleTask = MyDBManager.GetInstance().GetRoleTask(4); //���ĸ����
        Queue allTalkContent = new Queue();
        if (roleTask.taskState == (int)FRTaskState.Untrigger)
        {
            TalkContentItemModel talkContentItemModel = new TalkContentItemModel
            {
                dfAvatar = "hanMu",
                dfName = "��ĸ",
                dfTalkContent = "��û����..."
            };
            allTalkContent.Enqueue(talkContentItemModel);
        }
        else if(roleTask.taskState == (int)FRTaskState.Finished)
        {
            TalkContentItemModel talkContentItemModel = new TalkContentItemModel
            {
                dfAvatar = "hanMu",
                dfName = "��ĸ",
                dfTalkContent = "������������Ҫ��ע�����壬Ҫ�Ժ�˯��"
            };
            allTalkContent.Enqueue(talkContentItemModel);
        }
        return allTalkContent;
    }

    public override bool IsTriggerable(int taskId)
    {
        Debug.LogError("�߼����󣬲�Ӧ���д˴�ӡ TaskHandleHomeHanMu IsTriggerable " + taskId);
        return false;
    }

    public override bool IsSubmitable(int taskId)
    {
        MyDBManager.GetInstance().ConnDB();
        return MyDBManager.GetInstance().GetRoleTask(4).taskState == (int)FRTaskState.InProgress;
    }

    public override void OnSubmitTaskComplete(int taskId)
    {
        Debug.Log("�ľ�+1");
    }

    public override void OnTriggerTask(int taskId)
    {
    }

}

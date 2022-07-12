using System.Collections;
using UnityEngine;

public class TaskHandleHomeHanFu : ITaskHandle
{

    public const int ROLE_ID = 3;

    public override Queue TriggerTaskTalkData(int taskId)
    {
        Queue allTalkContent = new Queue();
        TalkContentItemModel talkContentItemModel = new TalkContentItemModel
        {
            dfAvatar = "hanFu",
            dfName = "����",
            dfTalkContent = "������ȥ��ɽ��Щ�ɲ����"
        };
        allTalkContent.Enqueue(talkContentItemModel);

        talkContentItemModel = new TalkContentItemModel
        {
            dfAvatar = "hanLi",
            dfName = "����",
            dfTalkContent = "�ã������ȥ"
        };
        allTalkContent.Enqueue(talkContentItemModel);

        talkContentItemModel = new TalkContentItemModel
        {
            dfAvatar = "hanFu",
            dfName = "����",
            dfTalkContent = "�������������ܡ�"
        };
        allTalkContent.Enqueue(talkContentItemModel);
        return allTalkContent;
    }

    public override Queue InProgressTaskTalkData(int taskId)  //A NPC������B NPC�ύ�����
    {
        if(taskId == 1) //�ɲ����񣬴������ύͬ��
        {
            return TriggerTaskTalkData(taskId);
        }
        else if (taskId == 5) //������񣬴������ύ��ͬ��
        {
            Queue allTalkContent = new Queue();
            TalkContentItemModel talkContentItemModel = new TalkContentItemModel
            {
                dfAvatar = "hanFu",
                dfName = "����",
                dfTalkContent = "�������������������ˣ������"
            };
            allTalkContent.Enqueue(talkContentItemModel);
            return allTalkContent;
        }
        Debug.LogError("�߼����� TaskHandleHomeHanFu InProgressTaskTalkData taskId " + taskId);
        return null;
        //return TriggerTaskTalkData(taskId);
    }

    public override Queue SubmitTaskTalkData(int taskId)
    {
        Queue allTalkContent = new Queue();
        if (taskId == 1) //�ύ�ռ��ɲ�����
        {
            TalkContentItemModel talkContentItemModel = new TalkContentItemModel
            {
                dfAvatar = "hanFu",
                dfName = "����",
                dfTalkContent = "�������������������ˣ������"
            };
            allTalkContent.Enqueue(talkContentItemModel);
        }
        else if(taskId == 5) //�ύ�������
        {
            TalkContentItemModel talkContentItemModel = new TalkContentItemModel
            {
                dfAvatar = "hanFu",
                dfName = "����",
                dfTalkContent = "������������Ҫ��ʵ�����¶����ã��������������ִ"
            };
            allTalkContent.Enqueue(talkContentItemModel);
        }
        return allTalkContent;
    }

    public override Queue GeneralTalkData()
    {
        MyDBManager.GetInstance().ConnDB();
        MyDBManager.RoleTask roleTask = MyDBManager.GetInstance().GetRoleTask(1);
        MyDBManager.RoleTask roleTask2 = MyDBManager.GetInstance().GetRoleTask(5);

        Queue allTalkContent = new Queue();
        if (roleTask.taskState == (int)FRTaskState.Untrigger) //δ������
        {
            Debug.LogError("�߼����� TaskHandleHomeHanFu GeneralTalkData");
        }
        else if (roleTask.taskState == (int)FRTaskState.Finished && roleTask2.taskState == (int)FRTaskState.Untrigger)
        {
            TalkContentItemModel talkContentItemModel = new TalkContentItemModel
            {
                dfAvatar = "hanFu",
                dfName = "����",
                dfTalkContent = "�������������������ˣ������"
            };
            allTalkContent.Enqueue(talkContentItemModel);
        }
        else if (roleTask2.taskState == (int)FRTaskState.Finished)
        {
            TalkContentItemModel talkContentItemModel = new TalkContentItemModel
            {
                dfAvatar = "hanFu",
                dfName = "����",
                dfTalkContent = "������������Ҫ��ʵ�����¶����ã��������������ִ"
            };
            allTalkContent.Enqueue(talkContentItemModel);
        }

        return allTalkContent;
    }

    

    public bool IsCanSubmitTask()
    {
        MyDBManager.GetInstance().ConnDB();
        MyDBManager.RoleItem roleItem = MyDBManager.GetInstance().GetRoleItem(1); //1�Ǹɲ�
        return roleItem.itemCount >= 5;
    }

    public override bool IsTriggerable(int taskId)
    {
        MyDBManager.GetInstance().ConnDB();
        return MyDBManager.GetInstance().GetRoleTask(taskId).taskState == (int)FRTaskState.Untrigger;
    }

    public override bool IsSubmitable(int taskId)
    {
        MyDBManager.GetInstance().ConnDB();
        MyDBManager.RoleItem roleItem = MyDBManager.GetInstance().GetRoleItem(1); //1�Ǹɲ�
        return roleItem.itemCount >= 5;
    }

    public override void OnSubmitTaskComplete(int taskId)
    {
        Debug.Log("�ɲ�>=5���ľ�+1");
        MyDBManager.GetInstance().ConnDB();
        MyDBManager.RoleItem roleItem = MyDBManager.GetInstance().GetRoleItem(1);
        MyDBManager.GetInstance().DeleteItemInBag(1, 5, roleItem.itemCount);
    }

    public override void OnTriggerTask(int taskId)
    {
        Debug.Log("OnTriggerTask taskId " + taskId);
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskHandleHomeHanFu : ITaskHandle
{

    public const int ROLE_ID = 3;

    public override Queue<TalkContentItemModel> TriggerTaskTalkData(int taskId)
    {
        Queue<TalkContentItemModel> allTalkContent = new Queue<TalkContentItemModel>();
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

    public override Queue<TalkContentItemModel> InProgressTaskTalkData(int taskId)  //A NPC������B NPC�ύ�����
    {
        if(taskId == 1) //�ɲ����񣬴������ύͬ��
        {
            return TriggerTaskTalkData(taskId);
        }
        else if (taskId == 5) //������񣬴������ύ��ͬ��
        {
            Queue<TalkContentItemModel> allTalkContent = new Queue<TalkContentItemModel>();
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

    public override Queue<TalkContentItemModel> SubmitTaskTalkData(int taskId)
    {
        Queue<TalkContentItemModel> allTalkContent = new Queue<TalkContentItemModel>();
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

    public override Queue<TalkContentItemModel> GeneralTalkData()
    {
        MyDBManager.GetInstance().ConnDB();
        RoleTask roleTask = MyDBManager.GetInstance().GetRoleTask(1);
        RoleTask roleTask2 = MyDBManager.GetInstance().GetRoleTask(5);

        Queue<TalkContentItemModel> allTalkContent = new Queue<TalkContentItemModel>();
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

    public override bool IsTriggerable(int taskId)
    {
        MyDBManager.GetInstance().ConnDB();
        return MyDBManager.GetInstance().GetRoleTask(taskId).taskState == (int)FRTaskState.Untrigger;
    }

    public override bool IsSubmitable(int taskId)
    {
        if(taskId == 1)
        {
            MyDBManager.GetInstance().ConnDB();
            RoleItem roleItem = MyDBManager.GetInstance().GetRoleItemInBag(1); //1�Ǹɲ�
            return roleItem.itemCount >= 5;
        }
        else if(taskId == 5) //���
        {
            return true;
        }
        Debug.LogError("�߼�����TaskHandleHomeHanFu IsSubmitable taskId " + taskId);
        return false;
    }

    public override void OnSubmitTaskComplete(int taskId)
    {
        if(taskId == 1)
        {
            Debug.Log("�ɲ�>=5��-5�ɲ�");
            MyDBManager.GetInstance().ConnDB();
            RoleItem roleItem = MyDBManager.GetInstance().GetRoleItemInBag(1);
            MyDBManager.GetInstance().DeleteItemInBag(1, 5, roleItem.itemCount);
        }else if (taskId == 5)
        {
            Debug.Log("����ľ�+1");
        }
    }

    public override void OnTriggerTask(int taskId)
    {
        Debug.Log("OnTriggerTask taskId " + taskId);
    }

}

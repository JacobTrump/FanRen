using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//��ȡ3���콬������
public class TaskHandleHomeXiaoMei : ITaskHandle
{

    public const int ROLE_ID = 2;

    public override Queue<TalkContentItemModel> TriggerTaskTalkData(int taskId)
    {
        Debug.Log("TriggerTaskTalkData taskId : " + taskId);
        Queue<TalkContentItemModel> allTalkContent = new Queue<TalkContentItemModel>();
        TalkContentItemModel talkContentItemModel = new TalkContentItemModel
        {
            dfAvatar = "xiaoMei",
            dfName = "С��",
            dfTalkContent = "����磬����Ժ콬����"
        };
        allTalkContent.Enqueue(talkContentItemModel);
        talkContentItemModel = new TalkContentItemModel
        {
            dfAvatar = "hanLi",
            dfName = "����",
            dfTalkContent = "�ã����һ�����"
        };
        allTalkContent.Enqueue(talkContentItemModel);
        return allTalkContent;
    }

    public override Queue<TalkContentItemModel> InProgressTaskTalkData(int taskId)
    {
        Debug.Log("InProgressTaskTalkData taskId : " + taskId);
        return TriggerTaskTalkData(taskId);
    }

    public override Queue<TalkContentItemModel> SubmitTaskTalkData(int taskId)
    {
        Debug.Log("SubmitTaskTalkData taskId : " + taskId);
        Queue<TalkContentItemModel> allTalkContent = new Queue<TalkContentItemModel>();
        TalkContentItemModel talkContentItemModel = new TalkContentItemModel
        {
            dfAvatar = "hanLi",
            dfName = "����",
            dfTalkContent = "С���һ����ˣ������콬����"
        };
        allTalkContent.Enqueue(talkContentItemModel);
        talkContentItemModel = new TalkContentItemModel
        {
            dfAvatar = "xiaoMei",
            dfName = "С��",
            dfTalkContent = "лл����磡"
        };
        allTalkContent.Enqueue(talkContentItemModel);
        return allTalkContent;
    }

    public override Queue<TalkContentItemModel> GeneralTalkData()
    {
        Debug.Log("GeneralTalkData");
        MyDBManager.GetInstance().ConnDB();
        RoleTask roleTask = MyDBManager.GetInstance().GetRoleTask(2); //2��С�õ�Ψһ����id�����ĵ������ݿ⣩��֪
        Queue<TalkContentItemModel> allTalkContent = new Queue<TalkContentItemModel>();
        if (roleTask.taskState == (int)FRTaskState.Finished) //�����Ѿ����
        {
            TalkContentItemModel talkContentItemModel = new TalkContentItemModel
            {
                dfAvatar = "xiaoMei",
                dfName = "С��",
                dfTalkContent = "лл����磡"
            };
            allTalkContent.Enqueue(talkContentItemModel);
        }
        return allTalkContent;
    }

    public override bool IsTriggerable(int taskId)
    {
        Debug.Log("IsTriggerable");
        MyDBManager.GetInstance().ConnDB();
        return MyDBManager.GetInstance().GetRoleTask(taskId).taskState == (int)FRTaskState.Untrigger;
    }

    public override bool IsSubmitable(int taskId)
    {
        Debug.Log("IsSubmitable");
        MyDBManager.GetInstance().ConnDB();
        RoleItem roleItem = MyDBManager.GetInstance().GetRoleItemInBag(2); //2�Ǻ콬��  
        return roleItem.itemCount >= 3;
    }

    public override void OnSubmitTaskComplete(int taskId)
    {
        Debug.Log("�콬������-3���ľ�+1");
        MyDBManager.GetInstance().ConnDB();
        RoleItem roleItem = MyDBManager.GetInstance().GetRoleItemInBag(2);
        MyDBManager.GetInstance().DeleteItemInBag(2, 3, roleItem.itemCount);
    }

    public override void OnTriggerTask(int taskId)
    {
        Debug.Log("��Ҫ�ռ��콬������*3");
    }
}

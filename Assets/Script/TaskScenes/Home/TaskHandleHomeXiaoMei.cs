using System.Collections;
using UnityEngine;

//��ȡ3���콬������
public class TaskHandleHomeXiaoMei : ITaskHandle
{

    public const int ROLE_ID = 2;

    public override Queue TriggerTaskTalkData(int taskId)
    {
        Debug.Log("TriggerTaskTalkData taskId : " + taskId);
        Queue allTalkContent = new Queue();
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

    public override Queue InProgressTaskTalkData(int taskId)
    {
        Debug.Log("InProgressTaskTalkData taskId : " + taskId);
        return TriggerTaskTalkData(taskId);
    }

    public override Queue SubmitTaskTalkData(int taskId)
    {
        Debug.Log("SubmitTaskTalkData taskId : " + taskId);
        Queue allTalkContent = new Queue();
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

    public override Queue GeneralTalkData()
    {
        Debug.Log("GeneralTalkData");
        MyDBManager.GetInstance().ConnDB();
        MyDBManager.RoleTask roleTask = MyDBManager.GetInstance().GetRoleTask(2); //2��С�õ�Ψһ����id�����ĵ������ݿ⣩��֪
        Queue allTalkContent = new Queue();
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
        MyDBManager.RoleItem roleItem = MyDBManager.GetInstance().GetRoleItem(2); //2�Ǻ콬��  
        return roleItem.itemCount >= 3;
    }

    public override void OnSubmitTaskComplete(int taskId)
    {
        Debug.Log("�콬������-3���ľ�+1");
    }

    public override void OnTriggerTask(int taskId)
    {
        Debug.Log("��Ҫ�ռ��콬������*3");
    }
}

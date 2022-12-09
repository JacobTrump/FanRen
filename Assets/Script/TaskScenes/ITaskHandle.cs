
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ITaskHandle
{

    public abstract Queue<TalkContentItemModel> TriggerTaskTalkData(int taskId);
    public abstract Queue<TalkContentItemModel> InProgressTaskTalkData(int taskId);//һ���TriggerTaskTalkData��ͬ
    public abstract Queue<TalkContentItemModel> SubmitTaskTalkData(int taskId);

    //һ���ԶԻ���һ����������NPCʹ��
    public abstract Queue<TalkContentItemModel> GeneralTalkData();

    public abstract bool IsTriggerable(int taskId);//�����Ƿ�ɴ���

    public abstract bool IsSubmitable(int taskId);//�����Ƿ���ύ

    public abstract void OnSubmitTaskComplete(int taskId); //�ύ������ɻص�

    public abstract void OnTriggerTask(int taskId); //��������ʱ�ص�

    public class TaskHandleBuilder
    {
        public static ITaskHandle Build(int roleId)
        {
            switch (roleId)
            {
                case TaskHandleHomeXiaoMei.ROLE_ID: return new TaskHandleHomeXiaoMei();
                case TaskHandleHomeHanFu.ROLE_ID: return new TaskHandleHomeHanFu();
                case TaskHandleHomeHanMu.ROLE_ID: return new TaskHandleHomeHanMu();
                case TaskHandleHomeSanShu.ROLE_ID: return new TaskHandleHomeSanShu();
                case TaskHandleHomeHanZhu.ROLE_ID: return new TaskHandleHomeHanZhu();
            }

            Debug.LogError("unknow roleId : " + roleId);

            return null;
        }
    }

}

public class TalkContentItemModel
{
    public string dfAvatar;
    public string dfName;
    public string dfTalkContent;
    /// <summary>
    /// �Ƿ������Ķ���
    /// </summary>
    public bool isInner;
}


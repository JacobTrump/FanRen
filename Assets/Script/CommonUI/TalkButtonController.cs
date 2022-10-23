﻿using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TalkButtonController : MonoBehaviour
{

    CanvasGroup talkUI;
    CanvasGroup talkButton;

    GameObject talkUIGO;

    Image[] images;
    Text[] texts;

    // Start is called before the first frame update
    void Start()
    {
        talkButton = GameObject.FindGameObjectWithTag("DoTalkButton").GetComponent<CanvasGroup>();
        HideTalkButton();

        talkUIGO = GameObject.FindGameObjectWithTag("TalkUI");
        talkUI = talkUIGO.GetComponent<CanvasGroup>();
        HideTalkUI();

        images = talkUIGO.GetComponentsInChildren<Image>();
        texts = talkUIGO.GetComponentsInChildren<Text>();
    }

    public void ShowTalkButton(int roleId)
    {
        talkButton.alpha = 1;
        talkButton.interactable = true;
        talkButton.blocksRaycasts = true;

        //this.npcCommonScript = npcCommonScript;
        this.roleId = roleId;


    }

    //NPCCommonScript npcCommonScript;
    int roleId;
    Queue<TalkContentItemModel> allTalkContentOriginData;
    Queue<TalkContentItemModel> allTalkContentHandleData = new Queue<TalkContentItemModel>();

    private void HandleOriginTalkData()
    {
        //allTalkContentHandleData = new Queue();
        allTalkContentHandleData.Clear();
        while (this.allTalkContentOriginData.Count > 0)
        {

            TalkContentItemModel item = this.allTalkContentOriginData.Dequeue();

            //Debug.Log("while item : " + item.dfTalkContent);

            TextGenerator generator = new TextGenerator();//文本生成器
            Rect rect = texts[1].gameObject.GetComponent<RectTransform>().rect;
            TextGenerationSettings setting = texts[1].GetGenerationSettings(rect.size);//文本设置
            generator.Populate(item.dfTalkContent, setting);
            int textCharacterMaxCount = generator.characterCount;

            if (item.dfTalkContent.Length <= textCharacterMaxCount)
            {
                TalkContentItemModel tcim = new TalkContentItemModel();
                tcim.dfAvatar = item.dfAvatar;
                tcim.dfName = item.dfName;
                tcim.dfTalkContent = item.dfTalkContent;
                allTalkContentHandleData.Enqueue(tcim);
            }
            else
            {
                int forCount;
                if (item.dfTalkContent.Length % textCharacterMaxCount == 0)
                {
                    forCount = item.dfTalkContent.Length / textCharacterMaxCount;
                }
                else
                {
                    forCount = item.dfTalkContent.Length / textCharacterMaxCount + 1;
                }
                for (int i = 0; i < forCount; i++)
                {
                    //talkContents.Enqueue(dfTalkContent.Substring(0, textCharacterMaxCount < dfTalkContent.Length ? textCharacterMaxCount : dfTalkContent.Length));
                    TalkContentItemModel tcim = new()
                    {
                        dfAvatar = item.dfAvatar,
                        dfName = item.dfName,
                        dfTalkContent = item.dfTalkContent[..(textCharacterMaxCount < item.dfTalkContent.Length ? textCharacterMaxCount : item.dfTalkContent.Length)]
                    };
                    allTalkContentHandleData.Enqueue(tcim);

                    item.dfTalkContent = textCharacterMaxCount < item.dfTalkContent.Length ? item.dfTalkContent.Substring(textCharacterMaxCount) : "";
                }
            }
            //allTalkContentHandleData.Enqueue(tcim);
        }
    }

    public void HideTalkButton()
    {
        if (talkButton.alpha == 0) return;
        talkButton.alpha = 0;
        talkButton.interactable = false;
        talkButton.blocksRaycasts = false;
    }

    private bool IsTalkButtonShowing()
    {
        return talkButton.alpha == 1;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (IsTalkButtonShowing())
            {
                OnTalkButtonClick();
            }else if (IsTalkUIShowing())
            {
                //显示下一段话
                if(allTalkContentHandleData != null && allTalkContentHandleData.Count > 0)
                {
                    if (!isTextPlaying) {
                        SetNextUIContent();
                    }
                }
                else
                {
                    if (!isTextPlaying)
                    {
                        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>().StopMove(false);
                        HideTalkUI();
                    }
                }
            }
        }
    }

    public void OnTalkButtonClick()
    {
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>().StopMove(true);

        MyDBManager.GetInstance().ConnDB();

        List<RoleTask> leaderActorWithNPCSubmitTasks = MyDBManager.GetInstance().GetAllLeaderActorWithNPCSubmitTasks(this.roleId);

        ITaskHandle taskHandle = ITaskHandle.TaskHandleBuilder.Build(this.roleId);

        if(leaderActorWithNPCSubmitTasks.Count > 0) //该NPC有对应的提交任务
        {
            RoleTask inProgressTask = null;
            foreach (RoleTask task in leaderActorWithNPCSubmitTasks)
            {
                if(task.taskState == (int)FRTaskState.InProgress)
                {
                    inProgressTask = task;
                    break;
                }
            }
            if(inProgressTask != null) //有进行中的任务
            {
                Debug.Log("有进行中的submit任务");
                if (taskHandle.IsSubmitable(inProgressTask.taskId))
                {
                    Debug.Log("可以submit任务");
                    this.allTalkContentOriginData = taskHandle.SubmitTaskTalkData(inProgressTask.taskId);
                    //执行提交任务完成回调
                    taskHandle.OnSubmitTaskComplete(inProgressTask.taskId);
                    //更改任务状态
                    MyDBManager.GetInstance().UpdateRoleTaskState(inProgressTask.taskId, FRTaskState.Finished);
                    //刷新右边TaskUI
                    UIUtil.NotifyTaskUIDatasetChanged();
                }
                else
                {
                    Debug.Log("不可以submit任务");
                    this.allTalkContentOriginData = taskHandle.InProgressTaskTalkData(inProgressTask.taskId);
                }
            }
            else //没有正在进行中的任务，所以这次对话转为尝试接任务
            {
                Debug.Log("没有正在进行中的任务，所以这次对话转为尝试接任务");
                TryGetTask(taskHandle);
            }
        }
        else  //没有任何任务需要提交给此NPC，所以此次对话转为尝试接任务
        {
            TryGetTask(taskHandle);
        }
        
        HideTalkButton();
        ShowTalkUI();
    }

    private void TryGetTask(ITaskHandle taskHandle)
    {
        List<RoleTask> leaderActorWithNPCTriggerTasks = MyDBManager.GetInstance().GetAllLeaderActorWithNPCTriggerTasks(this.roleId);
        if (leaderActorWithNPCTriggerTasks.Count == 0) //该NPC没有可触发任务
        {
            this.allTalkContentOriginData = taskHandle.GeneralTalkData(); //无任务NPC的对话也有可能跟着进程发生改变
        }
        else //与该NPC有任务
        {
            RoleTask inProgressTask = null;
            foreach (RoleTask rt in leaderActorWithNPCTriggerTasks)
            {
                if (rt.taskState == (int)FRTaskState.InProgress) //进行中的任务(同一个NPC只会同时存在一个)
                {
                    inProgressTask = rt;
                    break;
                }
            }
            if (inProgressTask != null) //与该NPC有正在进行中的任务
            {
                this.allTalkContentOriginData = taskHandle.InProgressTaskTalkData(inProgressTask.taskId);
            }
            else //没有正在进行中的
            {
                RoleTask unTriggerTask = null;
                foreach (RoleTask rt in leaderActorWithNPCTriggerTasks)
                {
                    if (rt.taskState == (int)FRTaskState.Untrigger) //最靠前的未触发的任务
                    {
                        unTriggerTask = rt;
                        break;
                    }
                }
                if (unTriggerTask == null) //没有正在进行的任务，也没有未触发的任务，那就是所有任务都完成了
                {
                    this.allTalkContentOriginData = taskHandle.GeneralTalkData();
                }
                else //有未触发的任务
                {
                    if (taskHandle.IsTriggerable(unTriggerTask.taskId)) //可触发
                    {
                        this.allTalkContentOriginData = taskHandle.TriggerTaskTalkData(unTriggerTask.taskId);
                        taskHandle.OnTriggerTask(unTriggerTask.taskId);
                        //接任务
                        MyDBManager.GetInstance().AddRoleTask(unTriggerTask.taskId);
                        //刷新右边TaskUI
                        UIUtil.NotifyTaskUIDatasetChanged();
                    }
                    else //不可触发
                    {
                        this.allTalkContentOriginData = taskHandle.GeneralTalkData();
                    }
                }
            }
        }
    }

    public void ShowTalkUIWith(Queue<TalkContentItemModel> allTalkContentOriginData)
    {
        this.allTalkContentOriginData = allTalkContentOriginData;
        ShowTalkUI();
    }

    private void ShowTalkUI()
    {
        HandleOriginTalkData();

        talkUI.alpha = 1;
        talkUI.interactable = true;
        talkUI.blocksRaycasts = true;

        SetNextUIContent();
    }

    private void SetNextUIContent()
    {
        isTextPlaying = true;
        TalkContentItemModel tcim = (TalkContentItemModel)this.allTalkContentHandleData.Dequeue();
        //texts[1].DOComplete();
        texts[1].text = "";
        texts[1].DOText(tcim.dfTalkContent, 0.5f).OnComplete(OnTextPlayFinish).SetEase(Ease.Linear);
        images[1].sprite = Resources.Load<Sprite>("Images/Avatar/" + tcim.dfAvatar);
        texts[0].text = tcim.dfName;
    }

    bool isTextPlaying = false;

    private void OnTextPlayFinish()
    {
        isTextPlaying = false;
    }

    //Queue talkContents = new Queue();

    private void HideTalkUI()
    {
        talkUI.alpha = 0;
        talkUI.interactable = false;
        talkUI.blocksRaycasts = false;
    }

    public bool IsTalkUIShowing()
    {
        return talkUI.alpha == 1;
    }

}

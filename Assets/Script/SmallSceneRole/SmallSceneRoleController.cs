using cakeslice;
using System.Collections;
using UnityEngine;

public class SmallSceneRoleController : MonoBehaviour
{

    //�Ի��߼���ƣ���������Դ�������ݿ⣬
    //ÿ��NPC���������Լ��Ĺ����ߣ�ÿ�������߶���N��step��
    //��ÿ��step,NPC��������ͬ������˵�����߸���Ʒ�ȵȣ�
    //�и�index��ǵ�ǰ�����ĸ�step

    //Queue allTalkContent = new Queue();

    Outline outline;
    TalkButtonController talkButtonController;

    public int roleId;

    //public string defaultName = "";
    //public string defaultTalkContent = "";
    //public string defaultAvatar = "";

    // Start is called before the first frame update
    void Start()
    {
        outline = GetComponentInChildren<Outline>();
        if (outline == null) Debug.LogError("GetComponentInChildren<Outline>() is null");
        if(outline != null) outline.enabled = false;
        talkButtonController = GameObject.FindGameObjectWithTag("DoTalkButton").GetComponent<TalkButtonController>();

        //Debug.Log("here is " + gameObject.name);

        //TalkContentItemModel talkContentItemModel;

        //defaultName = gameObject.name;
        //switch (gameObject.name)
        //{
        //    case "С��":
        //        talkContentItemModel = new TalkContentItemModel
        //        {
        //            dfAvatar = "xiaoMei",
        //            dfName = gameObject.name,
        //            dfTalkContent = "����磬����Ժ콬����"
        //        };
        //        allTalkContent.Enqueue(talkContentItemModel);
        //        break;

        //    case "����":
        //        talkContentItemModel = new TalkContentItemModel
        //        {
        //            dfAvatar = "sanShu",
        //            dfName = gameObject.name,
        //            dfTalkContent = "С�������ڶ�����"
        //        };
        //        allTalkContent.Enqueue(talkContentItemModel);
        //        break;

        //    case "����":
        //        talkContentItemModel = new TalkContentItemModel
        //        {
        //            dfAvatar = "hanFu",
        //            dfName = "����",
        //            dfTalkContent = "������ȥ��ɽ��Щ�ɲ����"
        //        };
        //        allTalkContent.Enqueue(talkContentItemModel);

        //        talkContentItemModel = new TalkContentItemModel
        //        {
        //            dfAvatar = "hanLi",
        //            dfName = "����",
        //            dfTalkContent = "�ã������ȥ"
        //        };
        //        allTalkContent.Enqueue(talkContentItemModel);

        //        talkContentItemModel = new TalkContentItemModel
        //        {
        //            dfAvatar = "hanFu",
        //            dfName = "����",
        //            dfTalkContent = "�������������ܡ�"
        //        };
        //        allTalkContent.Enqueue(talkContentItemModel);
        //        break;

        //    case "��ĸ":
        //        talkContentItemModel = new TalkContentItemModel
        //        {
        //            dfAvatar = "hanMu",
        //            dfName = gameObject.name,
        //            dfTalkContent = "��û����..."
        //        };
        //        allTalkContent.Enqueue(talkContentItemModel);
        //        break;

        //    case "����":
        //        talkContentItemModel = new TalkContentItemModel
        //        {
        //            dfAvatar = "hanZhu",
        //            dfName = gameObject.name,
        //            dfTalkContent = "����..����..����˯��...��"
        //        };
        //        allTalkContent.Enqueue(talkContentItemModel);
        //        break;
        //}

    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log(this.gameObject.name + " SmallSceneRoleController OnCollisionEnter() " + collision.gameObject.name);
        if (collision != null && collision.gameObject.tag.Equals("Player"))
        {
            if (outline != null) outline.enabled = true;
            //Debug.Log(this.gameObject.name + ": ����������, " + allTalkContent.Count);
            talkButtonController.ShowTalkButton(this);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        //Debug.Log(this.gameObject.name + " SmallSceneRoleController OnCollisionExit() " + collision.gameObject.name);
        if (collision != null && collision.gameObject.tag.Equals("Player"))
        {
            if (outline != null) outline.enabled = false;
            //Debug.Log(this.gameObject.name + ": �����뿪��");
            talkButtonController.HideTalkButton();
        }
    }

}



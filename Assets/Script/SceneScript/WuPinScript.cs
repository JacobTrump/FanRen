using cakeslice;
using UnityEngine;

public class WuPinScript : BaseMono, IColliderWithCC
{

    Outline outline;
    CatchWupinButtonScript mCatchWupinButtonScript;

    public string wuPinName;
    public string wuPinCount = "1";

    public int itemId;
    //public FRItemType itemType;

    // Start is called before the first frame update
    void Start()
    {

        if (!ShowOrHideGameObjByUniquePrefenceKey()) return;

        outline = GetComponentInChildren<Outline>();
        outline.enabled = false;

        mCatchWupinButtonScript = GameObject.Find("CatchWupingCanvas").GetComponent<CatchWupinButtonScript>();
    }

    public void OnPlayerCollisionEnter(GameObject player)
    {
        if (player.tag.Equals("Player"))
        {
            if (outline != null) outline.enabled = true;
            //Debug.Log(this.gameObject.name + ": ����������");
            //talkButtonController.ShowTalkButton(allTalkContent.Clone() as Queue);

            //��ʾʰȡ��ť
            mCatchWupinButtonScript.ShowCatchButton(gameObject);

        }
    }

    public void OnPlayerCollisionExit(GameObject player)
    {
        if (player.tag.Equals("Player"))
        {
            if (outline != null) outline.enabled = false;
            //Debug.Log(this.gameObject.name + ": �����뿪��");
            //talkButtonController.HideTalkButton();

            //����ʰȡ��ť
            mCatchWupinButtonScript.HideCatchButton(gameObject);
        }
    }

}

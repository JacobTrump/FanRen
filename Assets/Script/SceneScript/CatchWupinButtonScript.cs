using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatchWupinButtonScript : MonoBehaviour
{

    public GameObject catchButtonGameObj;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyUp(KeyCode.Space))
        {
            if (catchButtonGameObj.activeInHierarchy)
            {
                OnCatchButtonClick();
            }
        }
    }

    public void OnCatchButtonClick()
    {
        GameObject catchWupin = this.wuPinGameObjs[this.wuPinGameObjs.Count - 1] as GameObject; //ͬʱ������߶��ڿ���ʰȡ�ķ�Χ�ڣ���ʰȡ���һ��������
        WuPinScript wuPinScript = catchWupin.GetComponent<WuPinScript>();

        Debug.Log("ʰȡ�� : " + wuPinScript.wuPinName);

        PlayerPrefs.SetInt(wuPinScript.uniquePrefenceKey, 1);  //�õ�������ֻ���ڳ�������ʾ1��

        MyDBManager.GetInstance().ConnDB();
        MyDBManager.GetInstance().AddItemToBag(wuPinScript.itemId, wuPinScript.itemType, 1); //������ӵ���

        //Destroy(catchWupin); ������ᱨ��
        catchWupin.SetActive(false);
        this.wuPinGameObjs.Remove(catchWupin);

        UIUtil.ShowTipsUI(wuPinScript.wuPinName + " +" + wuPinScript.wuPinCount);

        if(this.wuPinGameObjs.Count == 0) //ͬʱ1�����߶�����߶��ڿ���ʰȡ�ķ�Χ�ڣ�����ȫ��ʰȡ����
        {
            catchButtonGameObj.SetActive(false);
        }
    }

    ArrayList wuPinGameObjs = new ArrayList(); //ͬʱ1�����߶�����߶��ڿ���ʰȡ�ķ�Χ��

    public void ShowCatchButton(GameObject wuPinGameObj)
    {
        catchButtonGameObj.SetActive(true);
        if (wuPinGameObjs.Contains(wuPinGameObj))
        {
            wuPinGameObjs.Remove(wuPinGameObj);
        }
        wuPinGameObjs.Add(wuPinGameObj);
    }

    public void HideCatchButton(GameObject wuPinGameObj)
    {
        catchButtonGameObj.SetActive(false);
        wuPinGameObjs.Remove(wuPinGameObj);
    }

}

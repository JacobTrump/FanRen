using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeInitScript : MonoBehaviour
{

    public GameObject sanShu;

    // Start is called before the first frame update
    void Start()
    {
        MyDBManager.GetInstance().ConnDB();
        //�ռ��ɲ���������У��Ҵ��ڿ��ύδ�ύ״̬���Һ�����һ��ȥ��ţ������û����������ʾ����
        if ((MyDBManager.GetInstance().GetRoleItemInBag(1).itemCount >= 5 
            && MyDBManager.GetInstance().GetRoleTask(1).taskState == (int)FRTaskState.InProgress
            && MyDBManager.GetInstance().GetRoleTask(3).taskState == (int)FRTaskState.Untrigger) 
            || MyDBManager.GetInstance().GetRoleTask(3).taskState == (int)FRTaskState.InProgress)
        {
            sanShu.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}

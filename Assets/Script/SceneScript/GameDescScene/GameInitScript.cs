using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInitScript : MonoBehaviour
{

    private void Awake()
    {
        //if (PlayerPrefs.GetInt("isInited", 0) == 0)
        //{
        //    PlayerPrefs.SetInt("isInited", 1);

        //}
    }

}

//����״̬
public enum FRTaskState
{    
    InProgress = 1, //������
    Finished = 2, //���
    Fail = 3, //ʧ��
    Untrigger = 0 //��û�д���
}

//��Ʒ����
public enum FRItemType
{
    Fabao = 1,//����
    CaiLiao = 2,//����
    LingCao = 3,//���
    DanYao = 4,//��ҩ
    LingShou = 5,//����
    LingChong = 6,//���
    GongFa = 7,//����
    DanFang = 8,//����
    Other = 9,//����
    KuiLei = 10,//����
    TianDiLingWu = 11,//�������
    ShenTong = 12,//��ͨ
    Story = 13//����
}


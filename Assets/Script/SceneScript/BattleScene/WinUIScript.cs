using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinUIScript : MonoBehaviour
{

    public GameObject winTextGO;
    public GameObject winTextGO2;
    public GameObject gridLayout;
    public List<GameObject> imageViewGameObjs;
    public GameObject moreGainTextGameObj;

    public void ShowGainWupin()
    {
        if (RootBattleInit.enemyRoleIds != null)
        {
            MyDBManager dBManager = MyDBManager.GetInstance();
            dBManager.ConnDB();

            Dictionary<int, int> itemId_count = new Dictionary<int, int>();

            for (int i = 0; i < RootBattleInit.enemyRoleIds.Length; i++)
            {

                int count = RootBattleInit.countOfEnemyRole[i];//�ñ��ֵ�����
                int roleId = RootBattleInit.enemyRoleIds[i];//����id
                RoleInfo roleInfo = dBManager.GetRoleInfo(roleId);

                List<float> gainSuccPercentList = roleInfo.CanGetItemIdPercentList();
                List<int> itemIds = roleInfo.CanGetItemIdList();

                for (int j=0; j< gainSuccPercentList.Count; j++)
                {
                    float randomF = Random.Range(0f, 1f);
                    if (randomF <= gainSuccPercentList[j]) //�ж������Ʒ
                    {
                        int gainItemId = itemIds[j];
                        int c = itemId_count.GetValueOrDefault(gainItemId, 0);
                        if(c == 0)
                        {
                            itemId_count.TryAdd(gainItemId, 1 * count);//����ÿ����Ʒ������
                        }
                        else
                        {
                            itemId_count[gainItemId] = c + 1 * count;//����ÿ����Ʒ������
                        }
                    }
                    else
                    {
                        Debug.Log("û�л��ս��Ʒ -- ���� roleId " + roleId);
                    }
                }
            }

            Dictionary<int, int>.KeyCollection allKeys = itemId_count.Keys;
            if(allKeys.Count > 0) //��ս��Ʒ
            {
                gridLayout.SetActive(true);
                int k = 0;
                foreach (int itemId in allKeys) //k : 0-13
                {
                    RoleItem itemInfo = dBManager.GetItemDetailInfo(itemId);
                    Sprite itemImage = Resources.Load<Sprite>("Images/ItemImage/" + itemInfo.imageName);
                    //Debug.LogError("k " + k);
                    imageViewGameObjs[k].SetActive(true);
                    if (itemImage != null)
                    {
                        imageViewGameObjs[k].GetComponent<Image>().sprite = itemImage;
                    }
                    else
                    {
                        Debug.LogError("�õ���û��ͷ��ͼƬ����Ҫ��ȫ�� itemId " + itemId);
                        if (PlayerControl.IS_DEBUG)
                        {
                            imageViewGameObjs[k].GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/ItemImage/yuJian");
                        }
                    }
                    int itemCount = itemId_count[itemId];
                    string forText = itemInfo.itemName + (itemCount > 1 ? ("X" + itemCount) : "");
                    imageViewGameObjs[k].GetComponentInChildren<Text>().text = forText;

                    //���봢���
                    dBManager.AddItemToBag(itemId, itemCount);

                    k++;
                    if (k == 14)
                    {
                        moreGainTextGameObj.SetActive(true);
                        moreGainTextGameObj.GetComponent<Text>().text = "...��" + (allKeys.Count-14) + "��ս��Ʒ";
                        break;
                    }
                }
            }
            else
            {
                winTextGO2.GetComponent<Text>().text = "ս��Ʒ����";
                Debug.LogError("û��ս��Ʒ");
            }

        }
        else
        {
            Debug.LogError("RootBattleInit.enemyRoleIds is null, ����Ǵ�ս����������������Ա���־");
        }
    }

    public void ShowFailUI()
    {
        winTextGO.GetComponent<Text>().text = "ս��ʧ�ܣ���������";
        winTextGO2.SetActive(false);
        gridLayout.SetActive(false);
    }

}

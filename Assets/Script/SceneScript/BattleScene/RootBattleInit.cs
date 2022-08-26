using System.Collections.Generic;
using UnityEngine;

public class RootBattleInit : BaseMono
{

    public List<GameObject> roles;

    public static int[] enemyRoleIds; //�����ݿ��ѯ��ɫ����

    public static int[] countOfEnemyRole; //��Ӧ����

    //public static string[] enemyRolePrefabPath; //����Ԥ����·��

    public static string triggerToBattleGameObjUnionPreKey; //����ս���Ĵ�������ǣ����������¼����������Ժ�����ʾ��

    private void OnDestroy()
    {
        enemyRoleIds = null;
        countOfEnemyRole = null;
        //enemyRolePrefabPath = null;
        triggerToBattleGameObjUnionPreKey = null;

        //���ܻ��ڲ��ųɹ�����ʧ������
        MyAudioManager.GetInstance().StopSE();
    }

    // Start is called before the first frame update
    void Start()
    {
        //roles = new GameObject[enemyRoleIds.Length + 1 + (����?���ܣ����ޣ���棿)];

        

        if (enemyRoleIds == null) //for test
        {
            roles[0].SetActive(true);
            roles[1].SetActive(true);

            GameObject hanLiGameObj = roles[0];
            HanLi hanLiCS = hanLiGameObj.GetComponent<HanLi>();
            hanLiCS.Init();
            hanLiCS.InitRoleBattelePos(2, 7); //todo
            
            Enemy enemyCS = roles[1].GetComponent<Enemy>();
            RoleInfo enemyRoleInfo = MyDBManager.GetInstance().GetRoleInfo(7);
            enemyCS.Init(enemyRoleInfo, 1);
            enemyCS.InitRoleBattelePos(2, 22);
            enemyCS.SetActionStrategy(new ActionStrategyGeneral());

            GameObject.FindGameObjectWithTag("UI_Canvas").GetComponent<BattleUIControl>().Init(roles);
            GameObject.FindGameObjectWithTag("Terrain").GetComponent<BattleController>().Init(roles);

        }
        else
        {
            List<GameObject> roleList = new List<GameObject>();
            //roles = new GameObject[enemyRoleIds.Length + 1]; //todo
            GameObject hanLiPrefab = Resources.Load<GameObject>("Prefab/RolePrefab/HanLiBattle");
            GameObject hanLiGameObj = Instantiate(hanLiPrefab);
            HanLi hanLiCS = hanLiGameObj.GetComponent<HanLi>();
            hanLiCS.Init();
            hanLiCS.InitRoleBattelePos(5, 5); //todo
            //roles[0] = hanLiGameObj;
            roleList.Add(hanLiGameObj);

            MyDBManager.GetInstance().ConnDB();
            for (int i = 0; i < enemyRoleIds.Length; i++)
            {
                RoleInfo enemyRoleInfo = MyDBManager.GetInstance().GetRoleInfo(enemyRoleIds[i]);
                for (int j = 0; j < countOfEnemyRole[i]; j++)
                {
                    GameObject enemyRolePrefab = Resources.Load<GameObject>(enemyRoleInfo.battleModelPath);
                    GameObject enemyRoleGameObj = Instantiate(enemyRolePrefab);
                    Enemy enemyCS = enemyRoleGameObj.AddComponent<Enemy>();
                    enemyCS.Init(enemyRoleInfo, j+1);
                    enemyCS.InitRoleBattelePos(7 + j*2, 7 + j*2); //todo
                    enemyCS.SetActionStrategy(new ActionStrategyGeneral());
                    roleList.Add(enemyRoleGameObj);

                    if(enemyCS.roleId == 7) //��Ȯ
                    {
                        enemyRoleGameObj.transform.localScale = new Vector3(0.36f, 0.36f, 0.36f);
                    }
                }
            }

            roles = roleList;
            GameObject.FindGameObjectWithTag("UI_Canvas").GetComponent<BattleUIControl>().Init(roles);
            GameObject.FindGameObjectWithTag("Terrain").GetComponent<BattleController>().Init(roles);
        }

    }    

}

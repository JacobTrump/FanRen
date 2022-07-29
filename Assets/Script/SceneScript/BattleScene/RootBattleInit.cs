using System.Collections.Generic;
using UnityEngine;

public class RootBattleInit : BaseMono
{

    public GameObject[] roles;

    public static int[] enemyRoleIds; //�����ݿ��ѯ��ɫ����

    public static int[] countOfEnemyRole; //��Ӧ����

    public static string[] enemyRolePrefabPath; //����Ԥ����·��

    private void OnDestroy()
    {
        enemyRoleIds = null;
        countOfEnemyRole = null;
        enemyRolePrefabPath = null;
    }

    // Start is called before the first frame update
    void Start()
    {
        //roles = new GameObject[enemyRoleIds.Length + 1 + (����?���ܣ����ޣ���棿)];
        roles = new GameObject[enemyRoleIds.Length + 1]; //todo

        GameObject hanLiPrefab = Resources.Load<GameObject>("Prefab/RolePrefab/HanLi");
        GameObject hanLiGameObj = Instantiate(hanLiPrefab);
        HanLi hanLiCS = hanLiGameObj.GetComponent<HanLi>();
        hanLiCS.Init();
        hanLiCS.InitRoleBattelePos(5, 5); //todo
        roles[0] = hanLiGameObj;

        int index = 1;
        MyDBManager.GetInstance().ConnDB();
        for(int i=0; i< enemyRoleIds.Length; i++)
        {
            MyDBManager.RoleInfo enemyRoleInfo = MyDBManager.GetInstance().GetRoleInfo(enemyRoleIds[i]);
            List<Shentong> enemyRoleShentongs = MyDBManager.GetInstance().GetRoleActiveShentong(enemyRoleIds[i]);
            for (int j=0; j< countOfEnemyRole[i]; j++)
            {
                GameObject enemyRolePrefab = Resources.Load<GameObject>(enemyRolePrefabPath[i]);
                GameObject enemyRoleGameObj = Instantiate(enemyRolePrefab);
                Enemy enemyCS = enemyRoleGameObj.GetComponent<Enemy>();
                enemyCS.Init(enemyRoleInfo, enemyRoleShentongs.ToArray());
                enemyCS.InitRoleBattelePos(6 + i, 6 + i); //todo
                roles[index] = enemyRoleGameObj;
                index++;
            }
        }
        
        GameObject.FindGameObjectWithTag("UI_Canvas").GetComponent<BattleUIControl>().Init(roles);
        GameObject.FindGameObjectWithTag("Terrain").GetComponent<BattleController>().Init(roles);
    }    

}

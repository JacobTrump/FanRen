using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyTriggerToBattleScript : BaseMono, IColliderWithCC
{

    //���ݿ��н�ɫid
    public int[] roleId;
    //ϣ���ý�ɫ��ս��������
    public int[] countOfRoleId;

    //public string[] rolePrefabPath;

    private void Start()
    {
        ShowOrHideGameObjByUniquePrefenceKey();
    }

    public void OnPlayerCollisionEnter(GameObject player)
    {
        if (player.tag.Equals("Player"))
        {
            Debug.Log("���������ˣ���ʼս��");

            RootBattleInit.enemyRoleIds = roleId;
            RootBattleInit.countOfEnemyRole = countOfRoleId;
            //RootBattleInit.enemyRolePrefabPath = rolePrefabPath;
            RootBattleInit.triggerToBattleGameObjUnionPreKey = this.uniquePrefenceKey;

            SaveUtil.SaveGameObjLastState(player);

            SceneManager.LoadScene(2);
        }
    }

    public void OnPlayerCollisionExit(GameObject player)
    {
        if (player.tag.Equals("Player"))
        {
            Debug.Log("�����뿪��");
        }
    }

}

using UnityEngine;

public class EnemyTriggerToBattleScript : MonoBehaviour, IColliderWithCC
{

    public int[] roleId;

    public int[] countOfRoleId;

    public string[] rolePrefabPath;

    public void OnPlayerCollisionEnter(GameObject player)
    {
        if (player.tag.Equals("Player"))
        {
            Debug.Log("���������ˣ���ʼս��");
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

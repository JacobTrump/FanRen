using UnityEngine.UI;
using UnityEngine;

public class BaseRole : BaseMono
{
    //todo ������Ϻ���Ҫ���� battleOriginPosX = battleToPosX
    public int battleOriginPosX;
    //todo ������Ϻ���Ҫ���� battleOriginPosZ = battleToPosZ
    public int battleOriginPosZ;

    public int battleToPosX;
    public int battleToPosZ;

    //��ɫΨһid,���ݿ�id
    public int roleId;

    //�ĸ��Ŷ� 1�ҷ��� 2�з�
    public TeamNum teamNum;

    public int maxHp;
    public int hp;
    public int mp;
    public int gongJiLi;
    public int fangYuLi;
    public int speed;
    
    public Shentong[] shentongInBattle;

    public Shentong selectedShentong;

    //public GameObject sliderPrefab;

    public RoleInBattleStatus roleInBattleStatus = RoleInBattleStatus.Waiting;

    GameObject uiParent;
    GameObject damageTextPrefab;

    public GameObject hpGO = null;
    public GameObject sliderAvatarGO = null;

    private void OnDestroy()
    {
        Destroy(hpGO);
        Destroy(sliderAvatarGO);
    }

    public void InitRoleBattelePos(int startX, int startZ)
    {
        this.battleOriginPosX = startX;
        this.battleOriginPosZ = startZ;
        this.battleToPosX = this.battleOriginPosX;
        this.battleToPosZ = this.battleOriginPosZ;
        this.gameObject.transform.position = new Vector3(startX + 0.5f, 0, startZ + 0.5f);
    }

    public void InitRoleData(int hp, int maxHp, int mp, int gongJiLi, int fangYuLi, Shentong[] shentongInBattle, int speed, int roleId, TeamNum teamNum)
    {
        this.hp = hp;
        this.maxHp = maxHp;
        this.mp = mp;
        this.gongJiLi = gongJiLi;
        this.fangYuLi = fangYuLi;
        this.shentongInBattle = shentongInBattle;        
        this.speed = speed;
        this.roleId = roleId;
        this.teamNum = teamNum;
        gameObjectType = GameObjectType.Role;

        uiParent = GameObject.FindGameObjectWithTag("UI_Canvas");
        damageTextPrefab = Resources.Load<GameObject>("Prefab/TextDamage");
    }

    

    public int GetMoveDistanceInBattle()
    {
        //todo �ٶ�ת�����ƶ�����������ʽ����
        return this.speed;
    }

    public void OnSelectShentong(int index)
    {
        
        if (this.shentongInBattle[index].needMp < this.mp)
        {
            this.selectedShentong = this.shentongInBattle[index];
            
            BattleController battleController = GameObject.FindGameObjectWithTag("Terrain").GetComponent<BattleController>();
            battleController.OnRoleSelectedShentong(this.selectedShentong);
        }
        else
        {
            //UI��ʾ��������
        }
    }

    public void DoCancelShentong()
    {
        this.selectedShentong = null;
    }

    public string GetHpUIGameObjectName()
    {
        return "hp_" + this.name;
    }

    public Slider GetHpSlide()
    {
        return GameObject.Find(GetHpUIGameObjectName()).GetComponent<Slider>();
    }

    //�����Ƿ�����
    public bool DoAck(BaseRole enemy)
    {
        Debug.Log("DoAck");
        //��ʽ����
        int damage = this.gongJiLi + this.selectedShentong.damage - enemy.fangYuLi;

        //GameObject uiParent = GameObject.FindGameObjectWithTag("UI_Canvas");
        //GameObject damageTextPrefab = Resources.Load<GameObject>("Prefab/TextDamage");
        GameObject damageTextGO = Instantiate(this.damageTextPrefab, this.uiParent.transform);
        damageTextGO.GetComponent<Text>().text = "-" + damage;
        Vector2 tp2 = RectTransformUtility.WorldToScreenPoint(Camera.main, enemy.gameObject.transform.position);
        damageTextGO.GetComponent<RectTransform>().position = tp2;

        if (enemy.hp > damage)
        {
            Debug.Log("enemy ��Ѫ:" + damage);
            enemy.hp -= damage;
            this.mp -= this.selectedShentong.needMp;

            Slider enemySlide = enemy.GetHpSlide();
            enemySlide.maxValue = maxHp;
            enemySlide.minValue = 0;
            enemySlide.value = enemy.hp;

            return false;
        }
        else
        {
            GameObject.Destroy(enemy.gameObject);
            //die
            return true;
        }
    }

    public void DoShentong()
    {

    }











    // Update is called once per frame
    void Update()
    {

    }

    //ѡ������ͨ�ص�
    //private RoleSelectShentongListener roleSelectShentongListener;

    //public void SetRoleSelectShentongListener(RoleSelectShentongListener roleSelectShentongListener)
    //{
    //    this.roleSelectShentongListener = roleSelectShentongListener;
    //}

    //public interface RoleSelectShentongListener
    //{
    //    public void OnSelected(int index, Shentong shentong);
    //}



    //��ѡ�лص�
    //private RoleOnSelectedListener roleOnSelectedListener;

    //public void SetRoleOnSelectedListener(RoleOnSelectedListener roleOnSelectedListener)
    //{
    //    this.roleOnSelectedListener = roleOnSelectedListener;
    //}

    //public RoleOnSelectedListener GetRoleOnSelectedListener()
    //{
    //    return this.roleOnSelectedListener;
    //}

    //public interface RoleOnSelectedListener
    //{
    //    public void OnSelected();
    //}

}

public enum RoleInBattleStatus
{
    Activing = 1,
    Waiting = 2
}

public enum TeamNum
{
    Us = 1,
    Enemy = 2
}


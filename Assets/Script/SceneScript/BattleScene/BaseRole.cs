using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;

public class BaseRole : BaseMono
{
    //todo ������Ϻ���Ҫ���� battleOriginPosX = battleToPosX
    public int battleOriginPosX;
    //todo ������Ϻ���Ҫ���� battleOriginPosZ = battleToPosZ
    public int battleOriginPosZ;

    public int battleToPosX;
    public int battleToPosZ;

    //���˻����Ѿ�����Я������Ʒ
    public List<RoleItem> roleItems = new List<RoleItem>();

    public RoleItem selectRoleItem;

    public bool isDidUseShentongOrBagCurrentRound = false;

    public (int, int) battleOriginPosition
    {
        get {
            return (battleOriginPosX, battleOriginPosZ);
        }
    }

    public int BattleToPosXWillOriginPosXIfNone
    {
        set {
            battleToPosX = value;
        }
        get {
            if (battleToPosX == -1) return battleOriginPosX;
            return battleToPosX;
        }
    }

    public int BattleToPosZWillOriginPosZIfNone
    {
        set
        {
            battleToPosZ = value;
        }
        get
        {
            if (battleToPosZ == -1) return battleOriginPosZ;
            return battleToPosZ;
        }
    }

    //��ɫΨһid,���ݿ�id
    public int roleId;

    //�ĸ��Ŷ� 1�ҷ��� 2�з�
    public TeamNum teamNum;

    public int maxHp;
    public int maxMp;
    public int hp;
    public int mp;
    public int gongJiLi;
    public int fangYuLi;
    public int speed;
    public string roleName;
    public string roleAvatar;
    
    public Shentong[] shentongInBattle;

    public Shentong selectedShentong;

    //private Animator animator;




    public delegate void RoleHitAnimDelegate();

    private RoleHitAnimDelegate mRoleHitAnimDelegate;




    private RoleHitAnimListener mRoleHitAnimListener;

    public virtual void StartRoleHitAnim(RoleHitAnimDelegate roleHitAnimDelegate)
    {
        Debug.Log("StartHitAnim");
        mRoleHitAnimDelegate = roleHitAnimDelegate;
        GetComponent<Animator>().SetBool("isAttack", true);
    }

    public virtual void StartRoleHitAnim(RoleHitAnimListener roleHitAnimListener)
    {
        Debug.Log("StartHitAnim");
        mRoleHitAnimListener = roleHitAnimListener;
        GetComponent<Animator>().SetBool("isAttack", true);
    }

    //hit anim end call back
    public virtual void EndRoleHitAnim()
    {
        Debug.Log("EndHitAnim");
        GetComponent<Animator>().SetBool("isAttack", false);
        if (mRoleHitAnimListener != null) {
            mRoleHitAnimListener.OnEndRoleHitAnim();
            mRoleHitAnimListener = null;
        }
        if (mRoleHitAnimDelegate != null)
        {
            mRoleHitAnimDelegate();
            mRoleHitAnimDelegate = null;
        }
    }

    

    //״̬�����������¼������ص� 
    public void Hit()
    {
        EndRoleHitAnim();
    }

    private void Awake()
    {
        //animator = this.GetComponent<Animator>();
    }

    //public GameObject sliderPrefab;

    public RoleInBattleStatus roleInBattleStatus = RoleInBattleStatus.Waiting;

    //GameObject uiParent;
    //GameObject damageTextPrefab;

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
        this.battleToPosX = -1;
        this.battleToPosZ = -1;
        this.gameObject.transform.position = new Vector3(startX + 0.5f, 0, startZ + 0.5f);
    }

    public void InitRoleData(int hp, int maxHp, int mp, int maxMp, int gongJiLi, int fangYuLi, Shentong[] shentongInBattle, int speed, int roleId, TeamNum teamNum, string roleName, string gameObjName, string roleAvatar)
    {
        this.hp = hp;
        this.maxHp = maxHp;
        this.mp = mp;
        this.maxMp = maxMp;
        this.gongJiLi = gongJiLi;
        this.fangYuLi = fangYuLi;
        this.shentongInBattle = shentongInBattle;        
        this.speed = speed;
        this.roleId = roleId;
        this.teamNum = teamNum;
        this.roleName = roleName;
        this.name = gameObjName;
        this.roleAvatar = roleAvatar;

        gameObjectType = GameObjectType.Role;

        //uiParent = GameObject.FindGameObjectWithTag("UI_Canvas");
        //damageTextPrefab = Resources.Load<GameObject>("Prefab/TextDamage");
    }

    public void DoUseRoleItem()
    {
        isDidUseShentongOrBagCurrentRound = true;

        RecoverHPMP(this.selectRoleItem.recoverHp, this.selectRoleItem.recoverMp);

        if (roleId > 1) //�����ߴ�����߼�
        {
            if (this.selectRoleItem.itemCount == 1)
            {
                this.roleItems.Remove(this.selectRoleItem);
            }
            else if (this.selectRoleItem.itemCount > 1)
            {
                this.selectRoleItem.itemCount--;
            }
            else
            {
                Debug.LogError("DoUseRoleItem() �߼�����");
            }
        }
    }

    public void UpdateHP(int value)
    {
        this.hp -= value;

        if (this.hp < 0) this.hp = 0;
        if (this.hp > this.maxHp) this.hp = this.maxHp;

        Debug.Log(this.name + "����Ѫ�� maxHp " + this.maxHp + ", hp " + this.hp);
        Slider hpSlide = this.hpGO.GetComponent<Slider>();
        hpSlide.maxValue = this.maxHp;
        hpSlide.minValue = 0;
        hpSlide.value = this.hp;
    }

    public void UpdateMP(int value)
    {
        this.mp -= value;

        if (this.mp < 0) this.mp = 0;
        if (this.mp > this.maxMp) this.mp = this.maxHp;

        Debug.Log(this.name + "���������� maxMp " + this.maxMp + ", mp " + this.mp);
        Slider mpSlide = this.hpGO.transform.Find("MP_Slider").GetComponent<Slider>();
        mpSlide.maxValue = this.maxMp;
        mpSlide.minValue = 0;
        mpSlide.value = this.mp;
    }

    /// <summary>
    /// �޹�������ʹ�ô��������Ϣ�ɻָ�3% hp mp
    /// </summary>
    public void DoRest()
    {
        RecoverHPMPOnRest();
    }

    private void RecoverHPMPOnRest()
    {
        if (isDidUseShentongOrBagCurrentRound)
        {
            isDidUseShentongOrBagCurrentRound = false;
            return;
        }
        float rPercent = 0.03f;

        int rHp = (int)(this.maxHp * rPercent);
        if (rHp < 1) rHp = 1;

        int rMp = (int)(this.maxMp * rPercent);
        if (rMp < 1) rMp = 1;

        Debug.Log("��Ϣ�ָ�hp " + rHp + ", mp " + rMp);

        RecoverHPMP(rHp, rMp);
    }

    private void RecoverHPMP(int hp, int mp)
    {
        UpdateHP(-hp);
        UpdateMP(-mp);
        GameObject.FindGameObjectWithTag("UI_Canvas").GetComponent<BattleUIControl>().ShowRecoverTextUI(hp, mp, this.gameObject);
    }

    public int GetMoveDistanceInBattle()
    {
        //todo �ٶ�ת�����ƶ�����������ʽ����
        return this.speed;
    }

    public void OnSelectShentong(int index)
    {
        
        if (this.shentongInBattle[index].needMp <= this.mp)
        {
            this.selectedShentong = this.shentongInBattle[index];
            
            BattleController battleController = GameObject.FindGameObjectWithTag("Terrain").GetComponent<BattleController>();
            battleController.OnRoleSelectedShentong(this.selectedShentong);
        }
        else
        {
            //todo UI��ʾ��������
            //Debug.LogError("��������");
            BattleUIControl battleUIControl = GameObject.FindGameObjectWithTag("UI_Canvas").GetComponent<BattleUIControl>();
            battleUIControl.ShowTips("��������");
        }
    }

    public void DoCancelShentong()
    {
        this.selectedShentong = null;
    }

    public void DoCancelRoleItem()
    {
        this.selectRoleItem = null;
    }

    //�����Ƿ�����
    public bool DoAck(BaseRole enemy)
    {
        Debug.Log("DoAck");
        isDidUseShentongOrBagCurrentRound = true;
        int damage = CalculateDamage(enemy, this.selectedShentong);

        if (damage <= 0) damage = 1;

        GameObject uiParent = GameObject.FindGameObjectWithTag("UI_Canvas");
        uiParent.GetComponent<BattleUIControl>().ShowDamageTextUI(damage, enemy.gameObject);

        if (enemy.hp > damage)
        {
            //this.mp -= this.selectedShentong.needMp;
            enemy.UpdateHP(damage);
            UpdateMP(this.selectedShentong.needMp);
            return false;
        }
        else
        {
            GameObject.Destroy(enemy.gameObject);
            //die
            return true;
        }
    }

    public int CalculateDamage(BaseRole beAttackedEnemy, Shentong attackShentong)
    {
        if (this.mp < attackShentong.needMp) return 0;
        //todo ��ʽ����
        int damage = this.gongJiLi + attackShentong.damage - beAttackedEnemy.fangYuLi;
        return damage;
    }

    /// <summary>
    /// ���һ�����ɵĿ��ƶ���Χ����ʵ���Ǳ��غϵĿ��ƶ���Χ���������ɱ��غ�AI����֮ǰ���ɣ�
    /// </summary>
    public List<GameObject> lastAllCanMoveGrids = new List<GameObject>();

    /// <summary>
    /// ��ͨ�ƶ����㷨��װ���˷��׳���ʹ�������پ���
    /// </summary>
    /// <param name="mapGridItems"></param>
    /// <param name="zhangAiWuGridItems"></param>
    /// <returns></returns>
    public List<GameObject> GetAllCanMoveGrids(GameObject[,] mapGridItems, List<GameObject> allRole)
    {
        List<GameObject> zhangAiWuGridItems = new List<GameObject>();
        foreach (GameObject roleGO in allRole) //���н�ɫ�ڿ��ƶ���Χ�ڶ����ϰ���
        {
            if (roleGO == null || !roleGO.activeInHierarchy || !roleGO.activeSelf) continue;
            BaseRole role = roleGO.GetComponent<BaseRole>();
            zhangAiWuGridItems.Add(mapGridItems[role.battleOriginPosX, role.battleOriginPosZ]);
        }

        //List<GameObject> allCanMoveGrids = new List<GameObject>();
        lastAllCanMoveGrids.Clear();

        List<GameObject> newNeighbourGrids = new List<GameObject>();
        newNeighbourGrids.Add(mapGridItems[battleOriginPosX, battleOriginPosZ]);

        int[] counter = new int[1];
        counter[0] = 0;

        HandleCanMoveGrids(lastAllCanMoveGrids, zhangAiWuGridItems, newNeighbourGrids, counter, mapGridItems);

        return lastAllCanMoveGrids;
    }

    private void HandleCanMoveGrids(List<GameObject> allCanMoveGrids, 
        List<GameObject> zhangAiWuGridItems, 
        List<GameObject> newNeighbourGrids, 
        int[] counter, 
        GameObject[,] mapGridItems)
    {
        List<GameObject> _newNeighbourGrids = new List<GameObject>();
        foreach (GameObject originGrid in newNeighbourGrids)
        {
            string[] position = originGrid.name.Split(",");
            //��ȷд��Ӧ���Ǵ�ԭ����ɢ��ȥ���������ϰ�����Ч
            int originX = int.Parse(position[0]);
            int originZ = int.Parse(position[1]);

            //��Խ�硢���ظ������ϰ���
            GameObject neighbourGridItem;
            if (originX - 1 >= 0)
            {
                neighbourGridItem = mapGridItems[originX - 1, originZ];
                if (!allCanMoveGrids.Contains(neighbourGridItem) && !zhangAiWuGridItems.Contains(neighbourGridItem)) _newNeighbourGrids.Add(neighbourGridItem);
            }

            if (originX + 1 < mapGridItems.GetLength(0))
            {
                neighbourGridItem = mapGridItems[originX + 1, originZ];
                if (!allCanMoveGrids.Contains(neighbourGridItem) && !zhangAiWuGridItems.Contains(neighbourGridItem)) _newNeighbourGrids.Add(neighbourGridItem);
            }

            if (originZ - 1 >= 0)
            {
                neighbourGridItem = mapGridItems[originX, originZ - 1];
                if (!allCanMoveGrids.Contains(neighbourGridItem) && !zhangAiWuGridItems.Contains(neighbourGridItem)) _newNeighbourGrids.Add(neighbourGridItem);
            }

            if (originZ + 1 < mapGridItems.GetLength(1))
            {
                neighbourGridItem = mapGridItems[originX, originZ + 1];
                if (!allCanMoveGrids.Contains(neighbourGridItem) && !zhangAiWuGridItems.Contains(neighbourGridItem)) _newNeighbourGrids.Add(neighbourGridItem);
            }
        }
        allCanMoveGrids.AddRange(_newNeighbourGrids);
        counter[0] += 1;
        if (counter[0] >= this.speed)
        {
            return;
        }
        HandleCanMoveGrids(allCanMoveGrids, zhangAiWuGridItems, _newNeighbourGrids, counter, mapGridItems);
    }




    private ActionStrategy mActionStrategy;

    /// <summary>
    /// ����ս�����˵���Ϊ����
    /// </summary>
    public void SetActionStrategy(ActionStrategy actionStrategy)
    {
        this.mActionStrategy = actionStrategy;
    }

    public ActionStrategy GetActionStrategy()
    {
        return this.mActionStrategy;
    }

}

public enum RoleInBattleStatus
{
    Activing = 1,
    Waiting = 2
}

public enum TeamNum
{
    //Us = 1,
    //Enemy = 2
    TEAM_ONE = 1,
    TEAM_TWO = 2
}

public interface RoleHitAnimListener
{
    public void OnEndRoleHitAnim();
}

public abstract class ActionStrategy
{

    //protected GameObject moveTargetGridItem;
    //protected Shentong selectShentong;
    //protected GameObject attackMapGrid;

    public abstract void GenerateStrategy(GameObject activingRoleGO, List<GameObject> allRoleGO, GameObject[,] mapGridItems);

    public abstract GameObject GetMoveTargetGridItem();

    public abstract GameObject GetAttackMapGridItem();

    public abstract bool IsPass();

}
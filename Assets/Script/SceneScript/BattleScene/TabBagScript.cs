using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class TabBagScript : MonoBehaviour
{

    public GameObject gridItemUIPrefab;

    MyGridLayout mMyGridLayout;

    List<RoleItem> datas;

    private void Start()
    {
        
        MyDBManager.GetInstance().ConnDB();
        datas = MyDBManager.GetInstance().GetRoleItemInBag(1, false);

        TabBagAdapter tabBagAdapter = new TabBagAdapter(datas, this, this.gridItemUIPrefab);

        mMyGridLayout = new MyGridLayout(this.gameObject, tabBagAdapter);
    }

    private void Update()
    {
        mMyGridLayout.Update();
    }






    //============道具全部描述 UI
    public GameObject imageGO;
    public GameObject buttonGO;
    public GameObject nameGO;
    public GameObject countGO;
    public GameObject effectDescGO;
    public GameObject itemDescGO;

    public void ShowItemDesc(RoleItem roleItem)
    {
        if (imageGO == null || buttonGO == null || nameGO == null || countGO == null || effectDescGO == null || itemDescGO == null) return;
        if (roleItem != null)
        {
            imageGO.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/ItemImage/" + roleItem.imageName);
            Button useButton = buttonGO.GetComponent<Button>();
            buttonGO.SetActive(true);
            useButton.onClick.RemoveAllListeners();
            useButton.onClick.AddListener(() => { OnUseButtonClick(roleItem); });
            nameGO.GetComponent<Text>().text = roleItem.itemName;
            countGO.GetComponent<Text>().text = "数量： " + roleItem.itemCount;
            effectDescGO.GetComponent<Text>().text = "功效：" + (roleItem.recoverHp > 0 ? " 气血+" + roleItem.recoverHp : "") + (roleItem.recoverMp > 0 ? " 灵力+" + roleItem.recoverMp : "");
            itemDescGO.GetComponent<Text>().text = roleItem.itemDesc;
        }
        else
        {
            imageGO.GetComponent<Image>().sprite = null;
            buttonGO.SetActive(false);
            nameGO.GetComponent<Text>().text = "";
            countGO.GetComponent<Text>().text = "";
            effectDescGO.GetComponent<Text>().text = "";
            itemDescGO.GetComponent<Text>().text = "";
        }
    }

    void OnUseButtonClick(RoleItem roleItem)
    {
        MyDBManager.GetInstance().ConnDB();
        MyDBManager.GetInstance().DeleteItemInBag(roleItem.itemId, 1, roleItem.itemCount);
        if (roleItem.itemCount == 1)
        {
            this.datas.Remove(roleItem);
        }
        else
        {
            roleItem.itemCount--;
        }
        mMyGridLayout.NotifyDatasetChange();

        Debug.Log("OnUseButtonClick() roleItem name " + roleItem.itemName);

        if(roleItem.recoverHp > 0 || roleItem.recoverMp > 0)
        {
            //数据库更新人物hp mp
            RoleInfo roleInfo = MyDBManager.GetInstance().GetRoleInfo(1);
            roleInfo.currentHp += roleItem.recoverHp;
            roleInfo.currentMp += roleItem.recoverMp;
            if (roleInfo.currentHp > roleInfo.maxHp) roleInfo.currentHp = roleInfo.maxHp;
            if (roleInfo.currentMp > roleInfo.maxMp) roleInfo.currentMp = roleInfo.maxMp;
            MyDBManager.GetInstance().UpdateRoleInfo(1, roleInfo.currentHp, roleInfo.currentMp);

            //更新左上角hp条mp条
            PlayerSimpleInfoUIScript psi = GameObject.Find("PlayerSimpleInfo_UI_Canvas").GetComponent<PlayerSimpleInfoUIScript>();
            psi.UpdateHPAndMp();

            if (roleItem.recoverHp > 0 && roleItem.recoverMp > 0) UIUtil.ShowTipsUI("血气补充" + roleItem.recoverHp + " 灵力补充"+ roleItem.recoverMp);
            if (roleItem.recoverHp > 0 && roleItem.recoverMp == 0) UIUtil.ShowTipsUI("血气补充" + roleItem.recoverHp);
            if (roleItem.recoverHp == 0 && roleItem.recoverMp > 0) UIUtil.ShowTipsUI("灵力补充" + roleItem.recoverMp);

        }
        else
        {
            UIUtil.ShowTipsUI("没有任何效果");
        }

        //this.transform.parent.GetComponent<BagAllContainerScript>().DoCloseBagContainer();
        //GameObject.FindGameObjectWithTag("Terrain").GetComponent<BattleController>().OnRoleItemSelectToUse(roleItem);

    }










    //=================test

    int i = 0, j = 0;
    public void AddFirst()
    {
        i++;
        RoleItem a = new RoleItem();
        a.itemCount = 1;
        a.itemDesc = "desc";
        a.itemName = "F_" + i;
        a.recoverHp = 999;
        this.datas.Insert(0, a);
        mMyGridLayout.NotifyDatasetChange();
        Debug.Log("data size " + this.datas.Count);
    }

    public void AddLast()
    {
        j++;
        RoleItem a = new RoleItem();
        a.itemCount = 1;
        a.itemDesc = "desc";
        a.itemName = "L_" + j;
        a.recoverHp = 999;
        this.datas.Add(a);

        mMyGridLayout.NotifyDatasetChange();
        Debug.Log("data size " + this.datas.Count);
    }

    public void ReduceFirst()
    {
        this.datas.RemoveAt(0);
        mMyGridLayout.NotifyDatasetChange();
        Debug.Log("data size " + this.datas.Count);
    }

    public void ReduceLast()
    {
        this.datas.RemoveAt(this.datas.Count - 1);
        mMyGridLayout.NotifyDatasetChange();
        Debug.Log("data size " + this.datas.Count);
    }

    //============ test

}

public class TabBagAdapter : GridLayoutAdapter<RoleItem>
{

    WeakReference<TabBagScript> mWeakReference;
    GameObject gridItemUIPrefab;

    public TabBagAdapter(List<RoleItem> datas, TabBagScript tabBagScript, GameObject gridItemUIPrefab) : base(datas)
    {
        this.mWeakReference = new WeakReference<TabBagScript>(tabBagScript);
        this.gridItemUIPrefab = gridItemUIPrefab;
    }

    public override GameObject GetGridItemView(int index, Transform parent)
    {
        return GameObject.Instantiate(gridItemUIPrefab, parent);
    }

    public override void BindView(GameObject gridItemView, int index)
    {
        RoleItem roleItem = this.datas[index];
        gridItemView.GetComponentInChildren<Text>().text = roleItem.itemId + "_" + roleItem.itemName + "_" + roleItem.itemCount; //todo 测试用id
        gridItemView.GetComponentInChildren<Image>().sprite = Resources.Load<Sprite>("Images/ItemImage/" + roleItem.imageName);
    }

    public override bool IsNeedAutoSelectState()
    {
        return true;
    }

    public override void OnGridItemClick(GameObject gridItem, int index)
    {
        Debug.LogWarning("OnItemClick " + index);
    }

    public override void OnGridItemSelect(GameObject gridItem, int index)
    {
        Debug.LogWarning("OnSelectGridItem " + index);
        TabBagScript bbs;
        mWeakReference.TryGetTarget(out bbs);
        if (index >= 0)
        {
            if (bbs != null) bbs.ShowItemDesc(this.datas[index]);
        }
        else
        {
            if (bbs != null) bbs.ShowItemDesc(null);
        }
    }
}

using UnityEngine.EventSystems;
using UnityEngine;

public class BaseMono : MonoBehaviour
{

    //0��ʾ��ʾ��û��ʰȡ����1��ʾ�Ѿ�ʰȡ��
    //ȡ�����򣺳���path + gameObject.name
    [HideInInspector]
    public string uniquePrefenceKey;

    public GameObjectType gameObjectType;

    void Awake()
    {
        //uniquePrefenceKey = SceneUtility.GetScenePathByBuildIndex(this.gameObject.scene.buildIndex) + "_" + gameObject.name;
        uniquePrefenceKey = this.gameObject.scene.path + "_" + gameObject.name;
    }

    protected bool ShowOrHideGameObjByUniquePrefenceKey()
    {
        if (PlayerPrefs.GetInt(uniquePrefenceKey, 0) == 0)
        {
            this.gameObject.SetActive(true);
            return true;
        }
        else
        {
            this.gameObject.SetActive(false);
            return false;
        }
    }

    protected bool IsClickUpOnUI()
    {
        return Input.GetMouseButtonUp(0) && EventSystem.current.IsPointerOverGameObject();
    }

    protected bool IsPointerOnUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

}

public enum GameObjectType
{
    Role = 1,
    Building = 2,
    Other = 3
}

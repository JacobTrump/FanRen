using UnityEditor;
using UnityEngine;

public class MyMenu : MonoBehaviour
{
    [MenuItem("���˲˵�/��ʼ��Ϸ")]
    public static void CustomMenu()
    {
        EdtorUtil.StartScene("Assets/Scenes/FirstScene_GameDesc.unity");
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}

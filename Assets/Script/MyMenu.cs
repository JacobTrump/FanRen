using UnityEditor;
using UnityEngine;

public class MyMenu : MonoBehaviour
{
    [MenuItem("���˲˵�/��ʼ��Ϸ")]
    public static void CustomMenu()
    {
        EdtorUtil.StartScene("Assets/Scenes/FirstScene_GameDesc.unity");
    }

}

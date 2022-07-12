using UnityEngine;

public class SceneMusic : BaseMono
{

    public GameObject globalMusicPrefab;

    GameObject musicObject;

    // Start is called before the first frame update
    void Start()
    {
        musicObject = GameObject.FindGameObjectWithTag("GlobalUIMusic");
        if (musicObject == null)
        {
            Debug.Log("����BGM");
            musicObject = Instantiate(globalMusicPrefab);
        }
        else
        {
            string moName = musicObject.name.ToLower();
            moName = moName.Replace("(clone)", "");

            string prefabName = globalMusicPrefab.name.ToLower();

            if (!(moName + musicObject.tag).Equals(prefabName + globalMusicPrefab.tag))
            {
                Debug.Log("��Ҫ�л�BGM, last bgm : " + musicObject.name + ", now bgm : " + globalMusicPrefab.name);
                Destroy(musicObject);
                musicObject = Instantiate(globalMusicPrefab);
            }
            else
            {
                Debug.Log("����Ҫ�л�����BGM");
            }
        }
    }

    //�������ֶ��л�BGM
    public void ForceChangeBGM(string bgmPrefabPath)
    {
        if(musicObject != null)
        {
            Destroy(musicObject);
        }
        globalMusicPrefab = Resources.Load<GameObject>(bgmPrefabPath);
        musicObject = Instantiate(globalMusicPrefab);
    }

    // Update is called once per frame
    void Update()
    {

    }

}

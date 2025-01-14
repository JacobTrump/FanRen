using UnityEngine;

public class SceneMusic : BaseMono
{

    /// <summary>
    /// 本场景需要的bgm prefab
    /// </summary>
    public GameObject globalMusicPrefab;

    /// <summary>
    /// 目前已挂载的bgm
    /// </summary>
    GameObject musicObject;

    // Start is called before the first frame update
    void Start()
    {
        musicObject = GameObject.FindGameObjectWithTag("GlobalUIMusic");
        if (musicObject == null)
        {
            Debug.Log("生成BGM");
            musicObject = Instantiate(globalMusicPrefab);
        }
        else
        {
            string moName = musicObject.name.ToLower();
            moName = moName.Replace("(clone)", "");

            string prefabName = globalMusicPrefab.name.ToLower();

            if (!(moName + musicObject.tag).Equals(prefabName + globalMusicPrefab.tag))
            {
                Debug.Log("需要切换BGM, last bgm : " + musicObject.name + ", now bgm : " + globalMusicPrefab.name);
                Destroy(musicObject);
                musicObject = Instantiate(globalMusicPrefab);
            }
            else
            {
                Debug.Log("不需要切换场景BGM");
            }
        }
    }

    //场景中手动切换BGM
    public void ForceChangeBGM(string bgmPrefabPath)
    {
        if(musicObject != null)
        {
            Destroy(musicObject);
        }
        globalMusicPrefab = Resources.Load<GameObject>(bgmPrefabPath);
        musicObject = Instantiate(globalMusicPrefab);
    }

    public void StopBGM()
    {
        if(musicObject != null) Destroy(musicObject);
    }

    // Update is called once per frame
    void Update()
    {

    }

}

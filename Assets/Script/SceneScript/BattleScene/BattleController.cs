using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class BattleController : BaseMono
{
    private int width = 30;
    private int height = 30;

    public GameObject gridCubePrefab;
    private GameObject[,] grids;

    private Material gridMat;
    private Material ackGridMat;
    private Material ackGridMouseMoveMat;
    private Material roleCanMoveGridMat;
    //�ص�ɫackGridMat+ackGridMouseMoveMat
    private Material overlapColorMat;

    //private GameObject sliderAvatarPrefab;

    private List<GameObject> allRole;

    void Start()
    {
        Debug.Log("BattleController Start");
        //MyAudioManager.GetInstance().PlayBGM("BGM/BattleBGM01");
    }

    public void Init(List<GameObject> allRole)
    {
        this.allRole = allRole;
        Terrain terrain = GetComponent<Terrain>();
        width = (int)terrain.terrainData.bounds.size.x;
        height = (int)terrain.terrainData.bounds.size.z;
        grids = new GameObject[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                GameObject cube = Instantiate(gridCubePrefab);
                cube.transform.position = new Vector3(x + 0.5f, -0.4f, z + 0.5f);
                cube.name = x + "," + z;
                cube.SetActive(false);
                grids[x, z] = cube;
            }
        }

        gridMat = Resources.Load<Material>("Mat/GridMat");
        ackGridMat = Resources.Load<Material>("Mat/AckGridMat");
        roleCanMoveGridMat = Resources.Load<Material>("Mat/RoleCanMoveMat");
        ackGridMouseMoveMat = Resources.Load<Material>("Mat/AckPlaneMat");
        overlapColorMat = Resources.Load<Material>("Mat/OverlapColorMat");

    }

    public void OnChangeRoleAction(GameObject roleGO)
    {
        DoSelectRole(roleGO);
    }

    //����˴�����ť�ص�,�˷���ֻ�����ⲿ���ã��������ڲ�����
    public void OnClickPass()
    {
        //if (isPlayingAnim) 
        //{
        //    Debug.Log("�������ڲ��ţ������Դ���");
        //    return;
        //}
        //else
        //{
        //    Debug.Log("?????????");
        //}
            
        ResetMouseAckRange();

        BaseRole selectedRoleCS = activingRoleGO.GetComponent<BaseRole>();
        selectedRoleCS.roleInBattleStatus = RoleInBattleStatus.Waiting;
        selectedRoleCS.DoCancelShentong();
        
        if (selectedRoleCS.battleToPosX != selectedRoleCS.battleOriginPosX) selectedRoleCS.battleOriginPosX = selectedRoleCS.battleToPosX;
        if (selectedRoleCS.battleToPosZ != selectedRoleCS.battleOriginPosZ) selectedRoleCS.battleOriginPosZ = selectedRoleCS.battleToPosZ;

        //GameObject.FindGameObjectWithTag("UI_Canvas").GetComponent<BattleUIControl>().OnClickPassButton();
        activingRoleGO = null;

        for(int i=0; i<width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                this.grids[i, j].SetActive(false);
            }
        }
    }

    //����
    public void OnClickReset()
    {
        //if (isPlayingAnim) return;
        ResetMouseAckRange();
        BaseRole selectedRoleCS = activingRoleGO.GetComponent<BaseRole>();
        selectedRoleCS.DoCancelShentong();
        
        selectedRoleCS.battleToPosX = selectedRoleCS.battleOriginPosX;
        selectedRoleCS.battleToPosZ = selectedRoleCS.battleOriginPosZ;
        activingRoleGO.transform.position = new Vector3(selectedRoleCS.battleOriginPosX+0.5f, 0, selectedRoleCS.battleOriginPosZ+0.5f);

        ChangeGridOnClickRoleOrShentong();
    }

    

    GameObject activingRoleGO = null;

    // Update is called once per frame
    void Update()
    {
        if (base.IsClickUpOnUI()) return;
        OnMouseLeftClick();
        OnMouseMoveToCanAckGrid();
    }

    //===========> ����ͨ���ߴ�������Ҫ���³�ʼ��
    private GameObject lastMoveGameObject;//���ⷴ��ִ��flag
    private GameObject lastChangeColorGOForPoint;

    private List<GameObject> lastNeedChangeColorGameObjects = new List<GameObject>();
    private List<GameObject> needChangeColorGameObjects = new List<GameObject>();

    private List<GameObject> lastChangeColorGOsForPlane = new List<GameObject>();
    //<===========

    private void OnMouseMoveToCanAckGrid()
    {
        if (isPlayingAnim) return;
        if (activingRoleGO != null)
        {
            BaseRole roleCS = activingRoleGO.GetComponent<BaseRole>();
            if(roleCS.selectedShentong != null && roleCS.selectedShentong.effType == ShentongEffType.Gong_Ji)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitInfo;
                if (Physics.Raycast(ray, out hitInfo))
                {
                    GameObject clickGridGameObj = hitInfo.collider.gameObject;

                    //���ⷴ��ִ��
                    if (lastMoveGameObject != clickGridGameObj) 
                    {                        
                        //ֻ�ں�ɫɫ�����ƶ������Ч
                        if ("canAck".Equals(clickGridGameObj.tag))
                        {

                            if (roleCS.selectedShentong.rangeType == ShentongRangeType.Point)
                            {
                                if (lastChangeColorGOForPoint != null)
                                {
                                    lastChangeColorGOForPoint.tag = "canAck";
                                    lastChangeColorGOForPoint.GetComponent<Renderer>().material = ackGridMat;
                                }
                                clickGridGameObj.GetComponent<Renderer>().material = ackGridMouseMoveMat;
                                lastChangeColorGOForPoint = clickGridGameObj;
                            }
                            else if (roleCS.selectedShentong.rangeType == ShentongRangeType.Line)
                            {                                
                                if(!lastNeedChangeColorGameObjects.Contains(clickGridGameObj))
                                {

                                    //��ԭ
                                    if (lastNeedChangeColorGameObjects.Count > 0)
                                    {
                                        foreach(GameObject tmp in lastNeedChangeColorGameObjects)
                                        {
                                            tmp.tag = "canAck";
                                            tmp.GetComponent<Renderer>().material = ackGridMat;
                                        }
                                        lastNeedChangeColorGameObjects.Clear();
                                    }

                                    string[] pos = clickGridGameObj.name.Split(',');
                                    int x = int.Parse(pos[0]);
                                    int z = int.Parse(pos[1]);
                                    needChangeColorGameObjects.Clear();

                                    //����x�������
                                    //GameObject roleGrid = null;
                                    for (int i = 0; i < width; i++)
                                    {
                                        if (this.grids[x, i].tag.Equals("canAck"))
                                        {
                                            needChangeColorGameObjects.Add(this.grids[x, i]);
                                        }
                                        else
                                        {
                                            if(roleCS.battleToPosX == x && roleCS.battleToPosZ == i)
                                            {
                                                needChangeColorGameObjects.Add(this.grids[x, i]); //�����ɫ��վ��grid����Ϊ�˸������ѭ�������                                                
                                            } 
                                        }
                                    }
                                    
                                    if (needChangeColorGameObjects.Count == 1) //ֻ��1��˵������x�����
                                    {                                        
                                        needChangeColorGameObjects.Clear();                                        
                                        for (int i = 0; i < height; i++) //Ӧ�ñ���z�������
                                        {
                                            if (this.grids[i, z].tag.Equals("canAck"))
                                            {
                                                needChangeColorGameObjects.Add(this.grids[i, z]);
                                            }
                                            else
                                            {
                                                if (roleCS.battleToPosX == i && roleCS.battleToPosZ == z)
                                                {
                                                    needChangeColorGameObjects.Add(this.grids[i, z]); //�����ɫ��վ��grid����Ϊ�˸������ѭ�������                                                    
                                                }
                                            }
                                        }
                                    }                                    

                                    //��ȡ������ڵ�grid���ڼ��ϵ�λ�ã�Ȼ��˫��ѭ��
                                    int clickGOIndex = needChangeColorGameObjects.IndexOf(clickGridGameObj);
                                    for(int i=clickGOIndex; i< int.MaxValue; i++)
                                    {
                                        if (i >= needChangeColorGameObjects.Count || !needChangeColorGameObjects[i].tag.Equals("canAck"))//���������ɫ��վ��grid����ֹͣ
                                        {
                                            break;
                                        }
                                        needChangeColorGameObjects[i].GetComponent<Renderer>().material = ackGridMouseMoveMat;
                                        lastNeedChangeColorGameObjects.Add(needChangeColorGameObjects[i]);                                        
                                    }
                                    for (int i = clickGOIndex; i >= 0; i--)
                                    {
                                        if (!needChangeColorGameObjects[i].tag.Equals("canAck"))//���������ɫ��վ��grid����ֹͣ
                                        {
                                            break;
                                        }
                                        needChangeColorGameObjects[i].GetComponent<Renderer>().material = ackGridMouseMoveMat;
                                        lastNeedChangeColorGameObjects.Add(needChangeColorGameObjects[i]);
                                    }
                                    
                                }
                            }
                            else if (roleCS.selectedShentong.rangeType == ShentongRangeType.Plane)
                            {
                                if(lastChangeColorGOsForPlane.Count > 0)
                                {
                                    foreach (GameObject tmp in lastChangeColorGOsForPlane)
                                    {
                                        if (tmp.tag.Equals("Untagged"))
                                        {
                                            tmp.GetComponent<Renderer>().material = gridMat;
                                        }
                                        else if (tmp.tag.Equals("canMove"))
                                        {
                                            tmp.GetComponent<Renderer>().material = roleCanMoveGridMat;
                                        }
                                        else if (tmp.tag.Equals("canAck"))
                                        {
                                            tmp.GetComponent<Renderer>().material = ackGridMat;
                                        }
                                    }
                                    lastChangeColorGOsForPlane.Clear();
                                }                                

                                string[] pos = clickGridGameObj.name.Split(',');
                                int x = int.Parse(pos[0]);
                                int z = int.Parse(pos[1]);
                                int planeR = activingRoleGO.GetComponent<BaseRole>().selectedShentong.planeRadius;
                                //����Ҫѭ���ķ�Χ��С
                                int minX = x - planeR < 0 ? 0 : x - planeR;
                                int maxX = x + planeR >= width ? width : x + planeR;
                                int minZ = z - planeR < 0 ? 0 : z - planeR;
                                int maxZ = z + planeR >= height ? height : z + planeR;
                                for(int i = minX; i <= maxX; i++)
                                {
                                    for(int j = minZ; j <= maxZ; j++)
                                    {
                                        if ((Mathf.Abs(x-i) + Mathf.Abs(z-j)) <= planeR)
                                        {
                                            if(i>=0 && j>=0 && i<width && j < height)
                                            {
                                                if(this.grids[i, j].tag.Equals("canAck"))
                                                {
                                                    //�ص�ɫ                                                    
                                                    this.grids[i, j].GetComponent<Renderer>().material = overlapColorMat;
                                                }
                                                else
                                                {
                                                    this.grids[i, j].GetComponent<Renderer>().material = ackGridMouseMoveMat;
                                                }                                                
                                                lastChangeColorGOsForPlane.Add(this.grids[i, j]);
                                            }
                                        }
                                    }
                                }
                            }

                        }
                        else
                        {
                            if (roleCS.selectedShentong.rangeType == ShentongRangeType.Point && lastChangeColorGOForPoint != null)
                            {
                                Debug.Log("clear lastChangeColorGOForPoint");
                                lastChangeColorGOForPoint.GetComponent<Renderer>().material = ackGridMat;
                            }
                            else if (roleCS.selectedShentong.rangeType == ShentongRangeType.Line && lastNeedChangeColorGameObjects.Count > 0)
                            {

                                foreach (GameObject tmp in lastNeedChangeColorGameObjects)
                                {
                                    tmp.tag = "canAck";
                                    tmp.GetComponent<Renderer>().material = ackGridMat;
                                }
                                Debug.Log("clear lastNeedChangeColorGameObjects");
                                lastNeedChangeColorGameObjects.Clear();
                            }
                            else if (roleCS.selectedShentong.rangeType == ShentongRangeType.Plane && lastChangeColorGOsForPlane.Count > 0)
                            {
                                foreach (GameObject tmp in lastChangeColorGOsForPlane)
                                {
                                    if (tmp.tag.Equals("Untagged"))
                                    {
                                        tmp.GetComponent<Renderer>().material = gridMat;
                                    }
                                    else if (tmp.tag.Equals("canMove"))
                                    {
                                        tmp.GetComponent<Renderer>().material = roleCanMoveGridMat;
                                    }
                                    else if (tmp.tag.Equals("canAck"))
                                    {
                                        tmp.GetComponent<Renderer>().material = ackGridMat;
                                    }
                                }
                                Debug.Log("clear lastChangeColorGOsForPlane");
                                lastChangeColorGOsForPlane.Clear();
                            }
                        }

                        

                        lastMoveGameObject = clickGridGameObj;
                    }
                    
                        



                }
            }
        }
    }

    private bool HasRoleOnTheGrid(GameObject clickGrid)
    {
        //GameObject[] allRoles = GameObject.FindGameObjectWithTag("RootBattleInit").GetComponent<RootBattleInit>().roles;
        foreach (GameObject roleGO in this.allRole)
        {
            if (roleGO == null || !roleGO.activeInHierarchy || !roleGO.activeSelf) continue;
            BaseRole roleCS = roleGO.GetComponent<BaseRole>();
            string[] pos = clickGrid.name.Split(',');
            if (roleCS.battleOriginPosX == int.Parse(pos[0])
                && roleCS.battleOriginPosZ == int.Parse(pos[1]))
            {
                return true;
            }
        }
        return false;
    }

    private void OnMouseLeftClick()
    {
        if (isPlayingAnim) return;
        if (Input.GetMouseButtonUp(0))
        {
            //�������������������������
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo))
            {
                //�������ߣ�ֻ����scene��ͼ�в��ܿ���
                //Debug.DrawLine(ray.origin, hitInfo.point);
                GameObject clickGameObj = hitInfo.collider.gameObject;

                //Debug.Log("click object name is " + clickGameObj.tag);

                if (activingRoleGO == clickGameObj || clickGameObj.tag.Equals("Untagged") || clickGameObj.tag.Equals("Terrain"))
                {
                    return;
                }



                //����˿��ƶ��ĵذ�
                if (clickGameObj.tag.Equals("canMove") && activingRoleGO != null && activingRoleGO.GetComponent<BaseRole>().selectedShentong == null)
                {
                    //����˿��ƶ��ĵذ�
                    //selectedGO.transform.LookAt(clickGameObj.transform);
                    //this.transform.Translate(Vector3.forward * 10);

                    if (HasRoleOnTheGrid(clickGameObj))
                    {
                        return;
                    }

                    List<Vector3> path = new List<Vector3>();
                    path.Add(new Vector3(activingRoleGO.transform.position.x, activingRoleGO.transform.position.y + 5, activingRoleGO.transform.position.z));
                    path.Add(new Vector3(clickGameObj.transform.position.x, 3f, clickGameObj.transform.position.z));
                    path.Add(new Vector3(clickGameObj.transform.position.x, 0f, clickGameObj.transform.position.z));

                    activingRoleGO.transform.LookAt(path[path.Count - 1]);

                    Hashtable args = new Hashtable();
                    //lookahead
                    //args.Add("lookahead", 0.9f);
                    args.Add("path", path.ToArray());
                    args.Add("looptype", iTween.LoopType.none);
                    args.Add("time", 1.4f);
                    //args.Add();
                    //args.Add();
                    args.Add("oncomplete", "OnComplete");
                    args.Add("onCompleteTarget", this.gameObject);
                    //args.Add("speed", 7);
                    //args.Add("orienttopath", true);
                    //args.Add("position", );
                    isPlayingAnim = true;
                    Debug.Log("�ƶ�������ʼ");

                    activingRoleGO.GetComponent<Animator>().SetBool("isRun", true);
                    iTween.MoveTo(activingRoleGO, args);



                    string[] indexs = clickGameObj.name.Split(',');
                    BaseRole role = activingRoleGO.GetComponent<BaseRole>();
                    role.battleToPosX = int.Parse(indexs[0]);
                    role.battleToPosZ = int.Parse(indexs[1]);

                    
                }
                else if (clickGameObj.GetComponent<BaseMono>().gameObjectType == GameObjectType.Role) //������˽�ɫ����
                {
                    Debug.LogError("???????????????????������˽�ɫ����??????? ����log��Զ�������ӡ����ӡ�˱�Ȼ���߼����� " + clickGameObj.name);

                    //������������߼�
                    if(activingRoleGO != null && activingRoleGO.GetComponent<BaseRole>().roleInBattleStatus == RoleInBattleStatus.Activing) //ȫ��ͷ�����ƶ���ʱ�� activingRoleGO is null
                    {
                        return;
                    }
                    //������������߼�
                    DoSelectRole(clickGameObj);
                }
                else if (clickGameObj.tag.Equals("canAck") && activingRoleGO != null && activingRoleGO.GetComponent<BaseRole>().selectedShentong != null)
                {

                    Vector3 targetP = clickGameObj.transform.position;
                    targetP.y = activingRoleGO.transform.position.y;
                    activingRoleGO.transform.LookAt(targetP);
                    

                    //bool flag = true;
                    isPlayingAnim = true;
                    Debug.Log("��ʼ�������﹥����������ͨ����");
                    Shentong shentong = activingRoleGO.GetComponent<BaseRole>().selectedShentong;
                    if (shentong.rangeType == ShentongRangeType.Point)
                    {
                        //MyAudioManager.GetInstance().PlaySE(shentong.soundEffPath);

                        //GameObject shentongEffPrefab = Resources.Load<GameObject>(shentong.effPath);
                        //ParticleSystem particleSystem = shentongEffPrefab.GetComponent<ParticleSystem>();
                        //MainModule mainModule = particleSystem.main;
                        //mainModule.stopAction = ParticleSystemStopAction.Callback;

                        //GameObject stEffGO = Instantiate(shentongEffPrefab);
                        //stEffGO.transform.position = new Vector3(clickGameObj.transform.position.x, 1, clickGameObj.transform.position.z);

                        RoleHitAnimListener rhal = new MyPointRoleHitAnimListener(shentong, new Vector3(clickGameObj.transform.position.x, 1, clickGameObj.transform.position.z));
                        activingRoleGO.GetComponent<BaseRole>().StartRoleHitAnim(rhal);

                    }
                    else if (shentong.rangeType == ShentongRangeType.Line)
                    {

                        //MyAudioManager.GetInstance().PlaySE(shentong.soundEffPath);

                        //GameObject shentongEffPrefab = Resources.Load<GameObject>(shentong.effPath);
                        //ParticleSystem particleSystem = shentongEffPrefab.GetComponent<ParticleSystem>();
                        //MainModule mainModule = particleSystem.main;
                        //mainModule.stopAction = ParticleSystemStopAction.Destroy;

                        //foreach (GameObject tmp in lastNeedChangeColorGameObjects)
                        //{
                        //    GameObject stEffGO = Instantiate(shentongEffPrefab);
                        //    if (flag)
                        //    {
                        //        flag = false;
                        //        particleSystem = stEffGO.GetComponent<ParticleSystem>();
                        //        mainModule = particleSystem.main;
                        //        mainModule.stopAction = ParticleSystemStopAction.Callback;
                        //    }
                        //    stEffGO.transform.position = new Vector3(tmp.transform.position.x, 1, tmp.transform.position.z);
                        //}

                        RoleHitAnimListener rhal = new MyLineRoleHitAnimListener(shentong, lastNeedChangeColorGameObjects);
                        activingRoleGO.GetComponent<BaseRole>().StartRoleHitAnim(rhal);

                    }
                    else if (shentong.rangeType == ShentongRangeType.Plane)
                    {

                        //MyAudioManager.GetInstance().PlaySE(shentong.soundEffPath);

                        //GameObject shentongEffPrefab = Resources.Load<GameObject>(shentong.effPath);
                        //ParticleSystem particleSystem = shentongEffPrefab.GetComponent<ParticleSystem>();
                        //MainModule mainModule = particleSystem.main;
                        //mainModule.stopAction = ParticleSystemStopAction.Destroy;

                        //foreach (GameObject tmp in lastChangeColorGOsForPlane)
                        //{
                        //    GameObject stEffGO = Instantiate(shentongEffPrefab);
                        //    if (flag)
                        //    {
                        //        flag = false;
                        //        particleSystem = stEffGO.GetComponent<ParticleSystem>();
                        //        mainModule = particleSystem.main;
                        //        mainModule.stopAction = ParticleSystemStopAction.Callback;
                        //    }
                        //    stEffGO.transform.position = new Vector3(tmp.transform.position.x, 1, tmp.transform.position.z);
                        //}

                        RoleHitAnimListener rhal = new MyPlaneRoleHitAnimListener(shentong, lastChangeColorGOsForPlane);
                        activingRoleGO.GetComponent<BaseRole>().StartRoleHitAnim(rhal);

                    }
                    //enemyCount = HandleAfterAck();
                }

            }
        }
    }

    private class MyPointRoleHitAnimListener : RoleHitAnimListener
    {
        Shentong shentong;
        Vector3 position;
        public MyPointRoleHitAnimListener(Shentong shentong, Vector3 position)
        {
            this.shentong = shentong;
            this.position = position;
        }
        public void OnEndRoleHitAnim()
        {
            MyAudioManager.GetInstance().PlaySE(shentong.soundEffPath);

            GameObject shentongEffPrefab = Resources.Load<GameObject>(shentong.effPath);
            ParticleSystem particleSystem = shentongEffPrefab.GetComponent<ParticleSystem>();
            MainModule mainModule = particleSystem.main;
            mainModule.stopAction = ParticleSystemStopAction.Callback;

            GameObject stEffGO = Instantiate(shentongEffPrefab);
            stEffGO.transform.position = position;
        }
    }

    private class MyLineRoleHitAnimListener : RoleHitAnimListener
    {

        Shentong shentong;
        bool flag = true;
        List<GameObject> lastNeedChangeColorGameObjects;
        public MyLineRoleHitAnimListener(Shentong shentong, List<GameObject> lastNeedChangeColorGameObjects)
        {
            this.shentong = shentong;
            this.lastNeedChangeColorGameObjects = lastNeedChangeColorGameObjects;
        }
        public void OnEndRoleHitAnim()
        {
            MyAudioManager.GetInstance().PlaySE(shentong.soundEffPath);

            GameObject shentongEffPrefab = Resources.Load<GameObject>(shentong.effPath);
            ParticleSystem particleSystem = shentongEffPrefab.GetComponent<ParticleSystem>();
            MainModule mainModule = particleSystem.main;
            mainModule.stopAction = ParticleSystemStopAction.Destroy;

            foreach (GameObject tmp in lastNeedChangeColorGameObjects)
            {
                GameObject stEffGO = Instantiate(shentongEffPrefab);
                if (flag)
                {
                    flag = false;
                    particleSystem = stEffGO.GetComponent<ParticleSystem>();
                    mainModule = particleSystem.main;
                    mainModule.stopAction = ParticleSystemStopAction.Callback;
                }
                stEffGO.transform.position = new Vector3(tmp.transform.position.x, 1, tmp.transform.position.z);
            }
        }
    }

    private class MyPlaneRoleHitAnimListener : RoleHitAnimListener
    {
        Shentong shentong;
        bool flag = true;
        List<GameObject> lastChangeColorGOsForPlane;
        public MyPlaneRoleHitAnimListener(Shentong shentong, List<GameObject> lastChangeColorGOsForPlane)
        {
            this.shentong = shentong;
            this.lastChangeColorGOsForPlane = lastChangeColorGOsForPlane;
        }
        public void OnEndRoleHitAnim()
        {
            MyAudioManager.GetInstance().PlaySE(shentong.soundEffPath);

            GameObject shentongEffPrefab = Resources.Load<GameObject>(shentong.effPath);
            ParticleSystem particleSystem = shentongEffPrefab.GetComponent<ParticleSystem>();
            MainModule mainModule = particleSystem.main;
            mainModule.stopAction = ParticleSystemStopAction.Destroy;

            foreach (GameObject tmp in lastChangeColorGOsForPlane)
            {
                GameObject stEffGO = Instantiate(shentongEffPrefab);
                if (flag)
                {
                    flag = false;
                    particleSystem = stEffGO.GetComponent<ParticleSystem>();
                    mainModule = particleSystem.main;
                    mainModule.stopAction = ParticleSystemStopAction.Callback;
                }
                stEffGO.transform.position = new Vector3(tmp.transform.position.x, 1, tmp.transform.position.z);
            }
        }
    }

    //�����ƶ����������﹥������
    public bool isPlayingAnim = false;

    //��û���ĵ�������
    int enemyCount;

    //�ƶ���������
    private void OnComplete()
    {
        Debug.Log("�ƶ���������");
        isPlayingAnim = false;
        activingRoleGO.GetComponent<Animator>().SetBool("isRun", false);
    }

    public void OnShentongParticleSystemStopped()
    {
        enemyCount = HandleAfterAck();
        isPlayingAnim = false;
        if (enemyCount == 0)
        {
            //���������ˣ���ʾ��Ӧ����
            if (this.activingRoleGO.GetComponent<HanLi>() != null)
            {
                Debug.Log("�ҷ�ʤ������ʾ��Ӧ����");
                PlayerPrefs.SetInt(RootBattleInit.triggerToBattleGameObjUnionPreKey, 1); //�ر�ս��������
                GameObject.FindGameObjectWithTag("UI_Canvas").GetComponent<BattleUIControl>().OnBattleEnd(true);
            }
            else
            {
                Debug.Log("�ҷ�ʧ�ܣ���ʾ��Ӧ����");
                GameObject.FindGameObjectWithTag("UI_Canvas").GetComponent<BattleUIControl>().OnBattleEnd(false);
            }
        }
        else
        {
            //OnClickPass();
            GameObject.FindGameObjectWithTag("UI_Canvas").GetComponent<BattleUIControl>().OnClickPassButton();
        }
    }

    //����  ������ʣ�¶��ٵ���û����
    private int HandleAfterAck()
    {
        BaseRole activingRoleCS = activingRoleGO.GetComponent<BaseRole>();
        Shentong selectedShentong = activingRoleCS.selectedShentong;
        int enemyCount = 0;
        if (selectedShentong.rangeType == ShentongRangeType.Point)
        {
            string[] xz = lastChangeColorGOForPoint.name.Split(',');
            foreach (GameObject roleGO in this.allRole)
            {
                if (roleGO == null || !roleGO.activeInHierarchy || !roleGO.activeSelf) continue;
                BaseRole roleCS = roleGO.GetComponent<BaseRole>();
                if (roleCS.teamNum != activingRoleCS.teamNum) //����
                {
                    enemyCount++;
                    if (roleCS.battleOriginPosX == int.Parse(xz[0])
                    && roleCS.battleOriginPosZ == int.Parse(xz[1])) //�����grid������
                    {
                        if (activingRoleCS.DoAck(roleCS))
                        {
                            //this.allRole.Remove(roleGO);
                            enemyCount--;
                        }
                    }
                }
            }
        }
        else if (selectedShentong.rangeType == ShentongRangeType.Line)
        {
            Dictionary<string, GameObject> pos_gridGO = new Dictionary<string, GameObject>();
            foreach (GameObject ackRangeGrid in lastNeedChangeColorGameObjects)
            {
                pos_gridGO[ackRangeGrid.name] = ackRangeGrid;
            }
            GameObject valueOut = null;
            foreach (GameObject roleGO in this.allRole)
            {
                if (roleGO == null || !roleGO.activeInHierarchy || !roleGO.activeSelf) continue;
                BaseRole roleCS = roleGO.GetComponent<BaseRole>();
                if (roleCS.teamNum != activingRoleCS.teamNum) //����
                {
                    enemyCount++;
                    if (pos_gridGO.TryGetValue(roleCS.battleOriginPosX + "," + roleCS.battleOriginPosZ, out valueOut)) //�����grid������
                    {
                        if (activingRoleCS.DoAck(roleCS))
                        {
                            enemyCount--;
                        }
                    }
                }
            }
        }
        else if (selectedShentong.rangeType == ShentongRangeType.Plane)
        {
            Dictionary<string, GameObject> pos_gridGO = new Dictionary<string, GameObject>();
            foreach (GameObject ackRangeGrid in lastChangeColorGOsForPlane)
            {
                pos_gridGO[ackRangeGrid.name] = ackRangeGrid;
            }
            GameObject valueOut = null;
            foreach (GameObject roleGO in this.allRole)
            {
                if (roleGO == null || !roleGO.activeInHierarchy || !roleGO.activeSelf) continue;
                BaseRole roleCS = roleGO.GetComponent<BaseRole>();
                if (roleCS.teamNum != activingRoleCS.teamNum)
                {
                    enemyCount++;
                    if (pos_gridGO.TryGetValue(roleCS.battleOriginPosX + "," + roleCS.battleOriginPosZ, out valueOut)) //�����grid������
                    {
                        if (activingRoleCS.DoAck(roleCS))
                        {
                            enemyCount--;
                        }
                    }
                }
            }
        }
        return enemyCount;
    }

    private void DoSelectRole(GameObject activingGameObj)
    {
        this.activingRoleGO = activingGameObj;

        //Debug.Log("click object name is " + clickGameObj.name);

        BaseRole selectRoleCS = this.activingRoleGO.GetComponent<BaseRole>();
        selectRoleCS.roleInBattleStatus = RoleInBattleStatus.Activing;

        //GameObject.FindGameObjectWithTag("UI_Canvas").GetComponent<BattleUIControl>().OnRoleSelected(selectRoleCS);
        //passButton?.SetActive(true);
        //resetButton?.SetActive(true);

        ChangeGridOnClickRoleOrShentong();

        BattleCameraController rcc = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<BattleCameraController>();
        rcc.SetSelectedRole(activingRoleGO);

    }

    private void ResetMouseAckRange()
    {
        lastMoveGameObject = null;
        lastChangeColorGOForPoint = null;
        lastNeedChangeColorGameObjects.Clear();
        needChangeColorGameObjects.Clear();
        lastChangeColorGOsForPlane.Clear();
    }

    public void OnRoleSelectedShentong(Shentong shentong)
    {
        ResetMouseAckRange();
        ChangeGridOnClickRoleOrShentong();
    }

    //public Shentong selectedShentong;

    public void ChangeGridOnClickRoleOrShentong()
    {

        BaseRole selectedRoleCS = activingRoleGO.GetComponent<BaseRole>();
        if(selectedRoleCS == null)
        {
            Debug.LogError("ChangeGridOnClickRole() baseRole is null");
            return;
        }

        //string[] posIndex = selectedGO.name.Split(',');
        int clickRoleOriginX = selectedRoleCS.battleOriginPosX;
        int clickRoleOriginZ = selectedRoleCS.battleOriginPosZ;
        //Debug.Log("click object name is " + gameObj.name);

        //Renderer renderer = gameObj.GetComponent<Renderer>();
        //Material material = renderer.material;                

        Renderer renderer;
        GameObject gridGO;
        int disX;
        int disY;

        int disToX;
        int disToY;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {                

                gridGO = grids[x, z];
                gridGO.SetActive(true);
                renderer = gridGO.GetComponent<Renderer>();

                
                if(selectedRoleCS.selectedShentong != null 
                    && selectedRoleCS.selectedShentong.effType == ShentongEffType.Gong_Ji)
                {

                    disToX = Math.Abs(x - selectedRoleCS.battleToPosX);
                    disToY = Math.Abs(z - selectedRoleCS.battleToPosZ);

                    if (selectedRoleCS.selectedShentong.rangeType == ShentongRangeType.Line)
                    {
                        if (selectedRoleCS.battleToPosX == x || selectedRoleCS.battleToPosZ == z)
                        {
                            if((disToX + disToY) <= selectedRoleCS.selectedShentong.unitDistance && (disToX + disToY) != 0)
                            {
                                //���                           
                                gridGO.tag = "canAck";
                                if (renderer.material.color.r != ackGridMat.color.r)
                                {
                                    renderer.material = ackGridMat;
                                }
                                continue;
                            }                            
                        }
                    }
                    else if (selectedRoleCS.selectedShentong.rangeType == ShentongRangeType.Point || selectedRoleCS.selectedShentong.rangeType == ShentongRangeType.Plane)
                    {
                        
                        if ((disToX + disToY) <= selectedRoleCS.selectedShentong.unitDistance && (disToX + disToY) != 0)
                        {
                            //���                           
                            gridGO.tag = "canAck";
                            if (renderer.material.color.r != ackGridMat.color.r) 
                            {
                                renderer.material = ackGridMat;
                            }
                            continue;
                        }
                    }  
                    
                }


                disX = Math.Abs(x - clickRoleOriginX);
                disY = Math.Abs(z - clickRoleOriginZ);

                if ((disX + disY) < selectedRoleCS.GetMoveDistanceInBattle()) //404EFF��139,150,219,107,    A4D7A3��164,214,163,107 
                    {
                        //����
                        //Debug.Log("�����ƶ�: " + x + "," + z);
                        gridGO.tag = "canMove";
                        if (renderer.material.color.r != roleCanMoveGridMat.color.r) renderer.material = roleCanMoveGridMat;
                    }
                    else
                    {
                        //����
                        //grids[x, z].GetComponent<Renderer>().material = gridMat;
                        gridGO.tag = "Untagged";
                        if (renderer.material.color.r != gridMat.color.r) renderer.material = gridMat;
                    }
                
                
            }
        }
    }

    private void OnDestroy()
    {
        //MyAudioManager.GetInstance().StopBGM();
        Resources.UnloadUnusedAssets();
    }

}



using DG.Tweening;
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

    /// <summary>
    /// ����A*�㷨����
    /// </summary>
    private const bool testAstar = false;

    void Start()
    {
        Debug.Log("BattleController Start");
        //MyAudioManager.GetInstance().PlayBGM("BGM/BattleBGM01");
        TestAddObstacles();
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
                cube.transform.position = new Vector3(x + 0.5f, 0.002f, z + 0.5f);
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

    /// <summary>
    /// ����˴�����ť�ص�,�˷���ֻ�����ⲿ���ã��������ڲ�����
    /// </summary>
    /// /// <summary>
    /// ����˴�����ť�ص�,�˷���ֻ�����ⲿ���ã��������ڲ�����
    /// </summary>
    /// /// <summary>
    /// ����˴�����ť�ص�,�˷���ֻ�����ⲿ���ã��������ڲ�����
    /// </summary>
    /// /// <summary>
    /// ����˴�����ť�ص�,�˷���ֻ�����ⲿ���ã��������ڲ�����
    /// </summary>
    /// /// <summary>
    /// ����˴�����ť�ص�,�˷���ֻ�����ⲿ���ã��������ڲ�����
    /// </summary>
    /// /// <summary>
    /// ����˴�����ť�ص�,�˷���ֻ�����ⲿ���ã��������ڲ�����
    /// </summary>
    public void OnClickPassAllowInvokeOutsideOnly()
    {
        ResetMouseAckRange();

        BaseRole selectedRoleCS = activingRoleGO.GetComponent<BaseRole>();
        selectedRoleCS.roleInBattleStatus = RoleInBattleStatus.Waiting;
        selectedRoleCS.DoCancelShentong();
        
        if(selectedRoleCS.battleToPosX >= 0)
        {
            selectedRoleCS.battleOriginPosX = selectedRoleCS.battleToPosX;
        }
        selectedRoleCS.battleToPosX = -1;

        if (selectedRoleCS.battleToPosZ >= 0)
        {
            selectedRoleCS.battleOriginPosZ = selectedRoleCS.battleToPosZ;
        }
        selectedRoleCS.battleToPosZ = -1;

        activingRoleGO = null;

        for(int i=0; i<width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                this.grids[i, j].SetActive(false);
            }
        }

        TestDestory();
    }

    //����
    public void OnClickReset()
    {
        //if (isPlayingAnim) return;
        ResetMouseAckRange();
        BaseRole selectedRoleCS = activingRoleGO.GetComponent<BaseRole>();
        selectedRoleCS.DoCancelShentong();
        
        selectedRoleCS.battleToPosX = -1;
        selectedRoleCS.battleToPosZ = -1;
        activingRoleGO.transform.position = new Vector3(selectedRoleCS.battleOriginPosX+0.5f, 0, selectedRoleCS.battleOriginPosZ+0.5f);

        ChangeGridOnClickRoleOrShentong();
    }

    

    GameObject activingRoleGO = null;

    // Update is called once per frame
    void Update()
    {
        if(this.activingRoleGO != null && this.activingRoleGO.GetComponent<BaseRole>().teamNum == TeamNum.TEAM_ONE)
        {
            if (base.IsClickUpOnUI()) return;
            OnMouseLeftClick();
            OnMouseMoveToCanAckGrid(null);
        }
    }

    //===========> ����ͨ���ߴ�������Ҫ���³�ʼ��
    private GameObject lastMoveGameObject;//���ⷴ��ִ��flag
    private GameObject lastChangeColorGOForPoint;

    private List<GameObject> lastNeedChangeColorGameObjects = new List<GameObject>();
    private List<GameObject> needChangeColorGameObjects = new List<GameObject>();

    private List<GameObject> lastChangeColorGOsForPlane = new List<GameObject>();
    //<===========

    private void OnMouseMoveToCanAckGrid(GameObject moveToGridItemByAI)
    {
        if (isPlayingAnim) return;
        if (activingRoleGO == null) return;
        BaseRole roleCS = activingRoleGO.GetComponent<BaseRole>();
        if (roleCS.selectedShentong != null && roleCS.selectedShentong.effType == ShentongEffType.Gong_Ji)
        {

            GameObject clickGridGameObj;
            if (moveToGridItemByAI != null) //AI
            {
                clickGridGameObj = moveToGridItemByAI;
            }
            else //�˹�
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitInfo;
                if (!Physics.Raycast(ray, out hitInfo))
                {
                    //Debug.LogWarning("���߻�ȡԤ��GameObject�쳣");
                    return;
                }
                clickGridGameObj = hitInfo.collider.gameObject;
            }

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
                        if (!lastNeedChangeColorGameObjects.Contains(clickGridGameObj))
                        {

                            //��ԭ
                            if (lastNeedChangeColorGameObjects.Count > 0)
                            {
                                foreach (GameObject tmp in lastNeedChangeColorGameObjects)
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
                                    if (roleCS.BattleToPosXWillOriginPosXIfNone == x && roleCS.BattleToPosZWillOriginPosZIfNone == i)
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
                                        if (roleCS.BattleToPosXWillOriginPosXIfNone == i && roleCS.BattleToPosZWillOriginPosZIfNone == z)
                                        {
                                            needChangeColorGameObjects.Add(this.grids[i, z]); //�����ɫ��վ��grid����Ϊ�˸������ѭ�������                                                    
                                        }
                                    }
                                }
                            }

                            //��ȡ������ڵ�grid���ڼ��ϵ�λ�ã�Ȼ��˫��ѭ��
                            int clickGOIndex = needChangeColorGameObjects.IndexOf(clickGridGameObj);
                            for (int i = clickGOIndex; i < int.MaxValue; i++)
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
                        if (lastChangeColorGOsForPlane.Count > 0)
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
                        for (int i = minX; i <= maxX; i++)
                        {
                            for (int j = minZ; j <= maxZ; j++)
                            {
                                if ((Mathf.Abs(x - i) + Mathf.Abs(z - j)) <= planeR)
                                {
                                    if (i >= 0 && j >= 0 && i < width && j < height)
                                    {
                                        if (this.grids[i, j].tag.Equals("canAck"))
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
                    this.DoMove(clickGameObj);
                }
                else if (clickGameObj.tag.Equals("canAck") && activingRoleGO != null && activingRoleGO.GetComponent<BaseRole>().selectedShentong != null)
                {
                    this.DoAttack(clickGameObj);
                }

            }
        }
    }

    //�����ƶ����������﹥������
    public bool isPlayingAnim = false;

    //��û���ĵ�������
    int enemyCount;

    //��ͨ���������ص�
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
                PlayerPrefs.SetInt(RootBattleInit.triggerToBattleGameObjUnionPreKey, 1); //�����ر�ս��������
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

    public void OnChangeRoleCameraMove(CameraState cameraState, GameObject targetRole)
    {
        Debug.Log("OnChangeRoleCameraMove CameraState " + cameraState);
        if(cameraState == CameraState.Stopped && targetRole == this.activingRoleGO)
        {
            BaseRole selectRoleCS = this.activingRoleGO.GetComponent<BaseRole>();
            selectRoleCS.roleInBattleStatus = RoleInBattleStatus.Activing;

            ChangeGridOnClickRoleOrShentong();

            TestAStart(selectRoleCS);

            if (selectRoleCS.teamNum == TeamNum.TEAM_TWO && selectRoleCS.GetActionStrategy() != null) //�ֵ������ж�
            {
                selectRoleCS.GetActionStrategy().GenerateStrategy(this.activingRoleGO, this.allRole, this.grids);
                GameObject targetGridItem = selectRoleCS.GetActionStrategy().GetMoveTargetGridItem();
                if (targetGridItem == this.grids[selectRoleCS.battleOriginPosX, selectRoleCS.battleOriginPosZ]) //Ŀ�����ԭ��
                {
                    //ֱ�ӹ���
                    ActionAfterAIMove();
                }
                else
                {
                    DoMove(targetGridItem);
                }
            }

            //һ����������ȹ�������ɹ���Ŀ�꣬������������ڣ����ȹ������ǣ�
            //����ѡ���˺��������������̷�Χ�ڵ���ͨ
            //������Χ���л���ĨɱĿ�������£���������ȣ���ǰ���ǰ���£��ᾡ����ͬʱ�������Ŀ��
            //����û�����������Ȳ�������������Ҫ
        }
    }

    private void DoSelectRole(GameObject activingGameObj)
    {
        this.activingRoleGO = activingGameObj;

        BattleCameraController rcc = Camera.main.GetComponent<BattleCameraController>();
        rcc.SetSelectedRole(activingRoleGO);
    }

    //�ƶ���������
    private void OnComplete()
    {
        Debug.Log("�ƶ���������");
        isPlayingAnim = false;
        activingRoleGO.GetComponent<Animator>().SetBool("isRun", false);

        ActionAfterAIMove();
    }

    private void ActionAfterAIMove()
    {
        BaseRole activingRole = activingRoleGO.GetComponent<BaseRole>();
        if (activingRole.teamNum == TeamNum.TEAM_TWO && activingRole.GetActionStrategy() != null)
        {
            if (activingRole.GetActionStrategy().IsPassAfterMove())
            {
                GameObject.FindGameObjectWithTag("UI_Canvas").GetComponent<BattleUIControl>().OnClickPassButton();
            }
            else
            {
                activingRole.selectedShentong = activingRole.GetActionStrategy().GetSelectShentong();
                OnRoleSelectedShentong(activingRole.selectedShentong);
                OnMouseMoveToCanAckGrid(activingRole.GetActionStrategy().GetAttackMapGridItem());
                this.DoAttack(activingRole.GetActionStrategy().GetAttackMapGridItem());
            }
        }
    }

    private List<(int, int)> GetAllBattleObstacles()
    {
        List<(int, int)> battleObstacles = new List<(int, int)>();
        foreach (GameObject roleGO in this.allRole)
        {
            if (roleGO == this.activingRoleGO) continue;
            BaseRole role = roleGO.GetComponent<BaseRole>();
            battleObstacles.Add((role.battleOriginPosX, role.battleOriginPosZ));
        }
        return battleObstacles;
    }

    private void DoMove(GameObject clickGridItem)
    {
        if (HasRoleOnTheGrid(clickGridItem))
        {
            return;
        }

        BaseRole activingRole = this.activingRoleGO.GetComponent<BaseRole>();
        AStarPathUtil aStarPathUtil = new AStarPathUtil();
        string[] clickPosition = clickGridItem.name.Split(",");
        List<(int, int)> battleObstacles = GetAllBattleObstacles();
        aStarPathUtil.Reset(this.width, this.height, (activingRole.battleOriginPosX, activingRole.battleOriginPosZ), (int.Parse(clickPosition[0]), int.Parse(clickPosition[1])), battleObstacles);
        List<AStarPathUtil.Node> aStartPaths = aStarPathUtil.GetShortestPath(false);

        List<Vector3> path = new List<Vector3>();

        if (activingRole.battleToPosX == -1 && activingRole.battleToPosZ == -1) //˵���غ����״��ƶ�
        {
            Debug.Log("�غ����״��ƶ�");
            for (int i = 1; i < aStartPaths.Count; i++)
            {
                AStarPathUtil.Node node = aStartPaths[i];
                path.Add(new Vector3(node.x + 0.5f, this.activingRoleGO.transform.position.y, node.y + 0.5f));
            }
        }
        else
        {
            Debug.LogWarning("�غ��ڶ���ƶ�");
            int startX = activingRole.battleToPosX;
            int startZ = activingRole.battleToPosZ;
            Debug.LogWarning("�غ��ڶ���ƶ� battleOriginPosX " + activingRole.battleOriginPosX + ", battleOriginPosZ " + activingRole.battleOriginPosZ);
            Debug.LogWarning("�غ��ڶ���ƶ� battleToPosX " + startX + ", battleToPosZ " + startZ);
            battleObstacles = GetAllBattleObstacles();
            Debug.LogWarning("�غ��ڶ���ƶ� �ϰ������� ��" + battleObstacles.Count);
            aStarPathUtil.Reset(this.width, this.height, (startX, startZ), (int.Parse(clickPosition[0]), int.Parse(clickPosition[1])), battleObstacles);
            aStartPaths = aStarPathUtil.GetShortestPath(false);
            for (int i = 1; i < aStartPaths.Count; i++)
            {
                AStarPathUtil.Node node = aStartPaths[i];
                path.Add(new Vector3(node.x + 0.5f, this.activingRoleGO.transform.position.y, node.y + 0.5f));
            }

            //���Դ���
            //GameObject showPathBallPrefab = Resources.Load<GameObject>("Prefab/SphereShowPath");
            //foreach(GameObject oldGO in pathGO)
            //{
            //    Destroy(oldGO);
            //}
            //pathGO.Clear();
            //foreach (AStarPathUtil.Node n in aStartPaths)
            //{
            //    GameObject pgo = Instantiate(showPathBallPrefab);
            //    pgo.transform.position = new Vector3(n.x + 0.5f, 0.5f, n.y + 0.5f);
            //    pathGO.Add(pgo);
            //}
        }



        //Hashtable args = new Hashtable();
        //args.Add("path", path.ToArray());
        //args.Add("looptype", iTween.LoopType.none);
        //args.Add("speed", 5f);
        //args.Add("orienttopath", true);
        //args.Add("easeType", iTween.EaseType.spring);
        //args.Add("oncomplete", "OnComplete");
        //args.Add("oncompletetarget", this.gameObject);
        //isPlayingAnim = true;
        //Debug.Log("�ƶ�������ʼ");
        //iTween.MoveTo(activingRoleGO, args);

        activingRoleGO.GetComponent<Animator>().SetBool("isRun", true);

        Tween t = activingRoleGO.transform.DOPath(path.ToArray(), 0.1f * path.Count, PathType.Linear, PathMode.Full3D)
            .SetOptions(false)
            .SetLookAt(0.001f)
            .SetEase(Ease.Linear)
            .OnComplete(OnComplete);

        activingRole.battleToPosX = int.Parse(clickPosition[0]);
        activingRole.battleToPosZ = int.Parse(clickPosition[1]);
    }

    private void DoAttack(GameObject clickGameObj)
    {
        Vector3 targetP = clickGameObj.transform.position;
        targetP.y = activingRoleGO.transform.position.y;
        activingRoleGO.transform.LookAt(targetP);

        bool flag = true;
        isPlayingAnim = true;
        Debug.Log("��ʼ���β������﹥����������ͨ����+��ͨ��Ч"); //�������궯����ſ�ʼ������ͨ��������ͨ��Ч����������ſ�ʼ���㹥��(�������������Բ��Ŷ�����ʱ���첽����)
        Shentong shentong = activingRoleGO.GetComponent<BaseRole>().selectedShentong;

        //todo ��Ҫ�ع���ȥ��if else�ĳɲ��
        if (shentong.rangeType == ShentongRangeType.Point)
        {
            activingRoleGO.GetComponent<BaseRole>().StartRoleHitAnim(delegate () {
                MyAudioManager.GetInstance().PlaySE(shentong.soundEffPath);

                GameObject shentongEffPrefab = Resources.Load<GameObject>(shentong.effPath);
                ParticleSystem particleSystem = shentongEffPrefab.GetComponent<ParticleSystem>();
                MainModule mainModule = particleSystem.main;
                mainModule.stopAction = ParticleSystemStopAction.Callback;

                GameObject stEffGO = Instantiate(shentongEffPrefab);
                stEffGO.transform.position = new Vector3(clickGameObj.transform.position.x, 1, clickGameObj.transform.position.z);
            });
        }
        else if (shentong.rangeType == ShentongRangeType.Line)
        {
            activingRoleGO.GetComponent<BaseRole>().StartRoleHitAnim(delegate () {
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
            });
        }
        else if (shentong.rangeType == ShentongRangeType.Plane)
        {
            activingRoleGO.GetComponent<BaseRole>().StartRoleHitAnim(delegate () {
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
            });
        }
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

        Renderer renderer;
        GameObject gridGO;
        //int disX;
        //int disY;

        int disToX;
        int disToY;

        List<GameObject> allCanMoveGrids = GetAllCanMoveGrids(selectedRoleCS);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {                

                gridGO = grids[x, z];
                gridGO.SetActive(true);
                renderer = gridGO.GetComponent<Renderer>();
                
                if(selectedRoleCS.selectedShentong != null) //ѡ������ͨ
                {

                    if(selectedRoleCS.selectedShentong.effType == ShentongEffType.Gong_Ji) //������ ��ͨ
                    {
                        disToX = Math.Abs(x - selectedRoleCS.BattleToPosXWillOriginPosXIfNone);
                        disToY = Math.Abs(z - selectedRoleCS.BattleToPosZWillOriginPosZIfNone);

                        if (selectedRoleCS.selectedShentong.rangeType == ShentongRangeType.Line)
                        {
                            if (selectedRoleCS.BattleToPosXWillOriginPosXIfNone == x || selectedRoleCS.BattleToPosZWillOriginPosZIfNone == z)
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

                    }else if (selectedRoleCS.selectedShentong.effType == ShentongEffType.Fang_Yu) //��������ͨ
                    {
                        Debug.LogError("��������ͨ");
                    }else if (selectedRoleCS.selectedShentong.effType == ShentongEffType.Bian_Shen) //������ͨ
                    {
                        Debug.LogError("������ͨ");
                    }

                    
                }

                if (allCanMoveGrids.Contains(gridGO))
                {
                    gridGO.tag = "canMove";
                    if (renderer.material.color.r != roleCanMoveGridMat.color.r) renderer.material = roleCanMoveGridMat;
                    continue;
                }

                gridGO.tag = "Untagged";
                if (renderer.material.color.r != gridMat.color.r) renderer.material = gridMat;

                //disX = Math.Abs(x - clickRoleOriginX);
                //disY = Math.Abs(z - clickRoleOriginZ);

                //todo ����д���������ϰ��ģ���Ҫװ���˷��׳�ſ��ԣ��ȱ���ע��������
                //if ((disX + disY) <= selectedRoleCS.GetMoveDistanceInBattle()) //404EFF��139,150,219,107,    A4D7A3��164,214,163,107 
                //{
                //    //����
                //    gridGO.tag = "canMove";
                //    if (renderer.material.color.r != roleCanMoveGridMat.color.r) renderer.material = roleCanMoveGridMat;
                //}
                //else
                //{
                //    //����
                //    gridGO.tag = "Untagged";
                //    if (renderer.material.color.r != gridMat.color.r) renderer.material = gridMat;
                //}

            }
        } //end for()

        
        
    }

    /// <summary>
    /// ��ȡƽ�������ϰ�
    /// (ƽ�����߼����ƶ�·����Ҫ����ƽ���ϰ������װ���˷��׳�֮����Ժ����ϰ���·����Ӱ�죬����ֱ��)
    /// </summary>
    /// <param name="selectedRoleCS"></param>
    /// <returns></returns>
    private List<GameObject> GetAllCanMoveGrids(BaseRole selectedRoleCS)
    {
        List<GameObject> zhangAiWuGridItems = new List<GameObject>();
        foreach (GameObject roleGO in this.allRole) //���н�ɫ�����ϰ���
        {
            BaseRole role = roleGO.GetComponent<BaseRole>();
            zhangAiWuGridItems.Add(this.grids[role.battleOriginPosX, role.battleOriginPosZ]);
        }

        List<GameObject> allCanMoveGrids = new List<GameObject>();

        List<GameObject> newNeighbourGrids = new List<GameObject>();
        newNeighbourGrids.Add(this.grids[selectedRoleCS.battleOriginPosX, selectedRoleCS.battleOriginPosZ]);

        int[] counter = new int[1];
        counter[0] = 0;

        //��һ����ɢ��һ����Ҫ��ɢselectedRoleCS.speed��

        HandleCanMoveGrids(allCanMoveGrids, zhangAiWuGridItems, newNeighbourGrids, counter, selectedRoleCS.speed);

        return allCanMoveGrids;

        //foreach (GameObject canMoveGridItem in allCanMoveGrids)
        //{
        //    canMoveGridItem.tag = "canMove";
        //    Renderer renderer = canMoveGridItem.GetComponent<Renderer>();
        //    if (renderer.material.color.r != roleCanMoveGridMat.color.r) renderer.material = roleCanMoveGridMat;
        //}
    }

    private void HandleCanMoveGrids(List<GameObject> allCanMoveGrids, List<GameObject> zhangAiWuGridItems, List<GameObject> newNeighbourGrids, int[] counter, int maxLoopCount)
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
                neighbourGridItem = this.grids[originX - 1, originZ];
                if (!allCanMoveGrids.Contains(neighbourGridItem) && !zhangAiWuGridItems.Contains(neighbourGridItem)) _newNeighbourGrids.Add(neighbourGridItem);
            }

            if (originX + 1 < this.width)
            {
                neighbourGridItem = this.grids[originX + 1, originZ];
                if (!allCanMoveGrids.Contains(neighbourGridItem) && !zhangAiWuGridItems.Contains(neighbourGridItem)) _newNeighbourGrids.Add(neighbourGridItem);
            }

            if (originZ - 1 >= 0)
            {
                neighbourGridItem = this.grids[originX, originZ - 1];
                if (!allCanMoveGrids.Contains(neighbourGridItem) && !zhangAiWuGridItems.Contains(neighbourGridItem)) _newNeighbourGrids.Add(neighbourGridItem);
            }

            if (originZ + 1 < this.height)
            {
                neighbourGridItem = this.grids[originX, originZ + 1];
                if (!allCanMoveGrids.Contains(neighbourGridItem) && !zhangAiWuGridItems.Contains(neighbourGridItem)) _newNeighbourGrids.Add(neighbourGridItem);
            }
        }
        allCanMoveGrids.AddRange(_newNeighbourGrids);
        counter[0] += 1;
        if (counter[0] >= maxLoopCount)
        {
            return;
        }
        HandleCanMoveGrids(allCanMoveGrids, zhangAiWuGridItems, _newNeighbourGrids, counter, maxLoopCount);
    }

    private void OnDestroy()
    {
        //MyAudioManager.GetInstance().StopBGM();
        Resources.UnloadUnusedAssets();
    }

    //test A*
    List<GameObject> pathGO = new List<GameObject>();
    //test A*
    List<(int, int)> obstacles = new List<(int, int)>();

    //test A*
    private void TestAStart(BaseRole selectRoleCS)
    {
        if (!testAstar) return;
        //test A*
        if (selectRoleCS.teamNum == TeamNum.TEAM_ONE)
        {
            foreach (GameObject item in allRole)
            {
                AStarPathUtil aStarPathUtil = new AStarPathUtil();
                BaseRole br = item.GetComponent<BaseRole>();
                if (br.teamNum == TeamNum.TEAM_TWO)
                {
                    aStarPathUtil.Reset(this.width, this.height, (selectRoleCS.battleOriginPosX, selectRoleCS.battleOriginPosZ), (br.battleOriginPosX, br.battleOriginPosZ), obstacles);
                    List<AStarPathUtil.Node> path = aStarPathUtil.GetShortestPath(false);
                    GameObject showPathBallPrefab = Resources.Load<GameObject>("Prefab/SphereShowPath");
                    foreach (AStarPathUtil.Node n in path)
                    {
                        GameObject pgo = Instantiate(showPathBallPrefab);
                        pgo.transform.position = new Vector3(n.x + 0.5f, 0.5f, n.y + 0.5f);
                        pathGO.Add(pgo);
                    }
                }
            }
        }
    }

    //test A*
    private void TestAddObstacles()
    {
        if (!testAstar) return;
        GameObject showPathBallPrefab = Resources.Load<GameObject>("Prefab/SphereRed");
        GameObject pgo = Instantiate(showPathBallPrefab);
        pgo.transform.position = new Vector3(10f + 0.5f, 1.9f, 0f + 0.5f);
        obstacles.Add((10, 0));

        pgo = Instantiate(showPathBallPrefab);
        pgo.transform.position = new Vector3(0f + 0.5f, 1.9f, 10f + 0.5f);
        obstacles.Add((0, 10));

        pgo = Instantiate(showPathBallPrefab);
        pgo.transform.position = new Vector3(1f + 0.5f, 1.9f, 10f + 0.5f);
        obstacles.Add((1, 10));

        pgo = Instantiate(showPathBallPrefab);
        pgo.transform.position = new Vector3(2f + 0.5f, 1.9f, 10f + 0.5f);
        obstacles.Add((2, 10));

        pgo = Instantiate(showPathBallPrefab);
        pgo.transform.position = new Vector3(3f + 0.5f, 1.9f, 10f + 0.5f);
        obstacles.Add((3, 10));

        pgo = Instantiate(showPathBallPrefab);
        pgo.transform.position = new Vector3(4f + 0.5f, 1.9f, 10f + 0.5f);
        obstacles.Add((4, 10));

        pgo = Instantiate(showPathBallPrefab);
        pgo.transform.position = new Vector3(5f + 0.5f, 1.9f, 10f + 0.5f);
        obstacles.Add((5, 10));

        pgo = Instantiate(showPathBallPrefab);
        pgo.transform.position = new Vector3(6f + 0.5f, 1.9f, 10f + 0.5f);
        obstacles.Add((6, 10));

        pgo = Instantiate(showPathBallPrefab);
        pgo.transform.position = new Vector3(7f + 0.5f, 1.9f, 10f + 0.5f);
        obstacles.Add((7, 10));

        pgo = Instantiate(showPathBallPrefab);
        pgo.transform.position = new Vector3(8f + 0.5f, 1.9f, 10f + 0.5f);
        obstacles.Add((8, 10));
    }

    //test A*
    private void TestDestory()
    {
        if (!testAstar) return;
        foreach (GameObject go in pathGO)
        {
            Destroy(go);
        }
    }

}



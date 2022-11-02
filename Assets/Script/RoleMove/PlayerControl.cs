using UnityEngine;


public class PlayerControl : MonoBehaviour
{

    public const bool IS_DEBUG = true;

    public Camera playerCamera;
    private Animator animator;

    TalkButtonController talkButtonController = null;

    private CharacterController cc;

    Rigidbody mRigidbody;
    Vector3 moveDir;

    public float moveSpeed = 5f;

    private GameObject lastHitGameObject;
    private IColliderWithCC colliderWithCCScript;

    private TabUIRootScript mTabUIRootScript;

    //������״̬������Ϊ�˽�����ػ��߶���ʱ�������λ�ò��ܻ�ԭ�������տ�ʼ���أ����ﴴ����ʱ������position��cc��move�г�ͻ������������ǰ��1�벻������cc.move
    //����������ʵ����������������� �������Ӧ����unity�ڲ����߳����⵼�µġ�
    //todo ���ԼӸ���������������1��
    bool normalState = false;

    private void SetNormalState()
    {
        normalState = true;
    }

    private void Awake()
    {
        //debug
        //PlayerPrefs.DeleteAll();
        if (IS_DEBUG)
        {

            Debug.LogWarning("��ʼ����Ϸ���ݣ�������Ҫ��������");
            MyDBManager.GetInstance().ConnDB();
            RoleInfo roleInfo = MyDBManager.GetInstance().GetRoleInfo(1);
            if (roleInfo == null) {
                MyDBManager.GetInstance().InsertRoleInfo(1);
                MyDBManager.GetInstance().InsertCollectionNPC(1);
                MyDBManager.GetInstance().InsertCollectionPlace(1);
            }
            



            //Debug.LogWarning("Awake() debugģʽ�������Ĭ�Ϸ�1-33����Ʒ��������");
            //MyDBManager.GetInstance().ConnDB();
            //for (int i = 1; i <= 33; i++)
            //{
            //    MyDBManager.GetInstance().AddItemToBag(i, 2);
            //}
        }

        HandleSaveOrLoad();

    }

    private void OnApplicationQuit()
    {
        if (IS_DEBUG)
        {
            Debug.LogWarning("OnApplicationQuit() debugģʽ���˳���Ϸ�����������Ϸ��������");
            //PlayerPrefs�����ļ�������������ʰȡ��Ʒ������ս������ײ�塢������ ������Ϸ����������ֻ��ʾ1�ε�flag
            PlayerPrefs.DeleteAll();

            //һ����Ϸ�������ݶ������ݿ���
            MyDBManager.GetInstance().DeleteAllRWGameData();
        }
    }

    private bool needStopMove = false;
    public void StopMove(bool needStop)
    {
        this.needStopMove = needStop;
    }

    void Start()
    {

        animator = GetComponent<Animator>();

        talkButtonController = GameObject.FindGameObjectWithTag("DoTalkButton")?.GetComponent<TalkButtonController>();

        //mRigidbody = GetComponent<Rigidbody>();

        cc = GetComponent<CharacterController>();

        //cc.detectCollisions = false;

        if (playerCamera == null) playerCamera = Camera.main;

        mTabUIRootScript = GameObject.Find("TAB_UI_Canvas").GetComponent<TabUIRootScript>();

        Invoke("SetNormalState", 1);
    }

    private void HandleSaveOrLoad()
    {
        int lastSceneIndex = SaveUtil.GetLastSceneBuildIndex();
        if (lastSceneIndex >= 0 && lastSceneIndex == this.gameObject.scene.buildIndex) //�б����¼,�ұ��泡���͵�ǰһ����˵���Ƿ��ػ��߶���
        {
            Debug.Log("=================���ڷ���ǰ��ĳ��� ���� ������󱣴�ĳ���");
            Vector3 lastPositon = SaveUtil.GetLastPosition();
            if (lastPositon != Vector3.zero)
            {
                Debug.Log("���� role hanli positon " + lastPositon);
                //this.transform.position = lastPositon;
                this.transform.position = lastPositon;
            }
            else
            {
                Debug.LogError("���ݴ��� position is 0");
            }
        }
        else
        {
            //ÿ�δ�������(�����³���)������߽���ս��ǰ����
            Debug.Log("=================���ڱ����ɫ����");
            SaveUtil.SaveGameObjLastState(this.gameObject);
        }
    }

    private void DoTurn(Vector3 dir)
    {
        this.moveDir = dir;
        this.moveDir.y = 0f;
        transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, this.moveDir, 0.7f, 0f));
    }

    private void DoMove()
    {
        if (!cc.isGrounded)
        {
            this.moveDir += new Vector3(0f, -1f, 0f);
        }
        if (this.moveDir.x != 0f || this.moveDir.z != 0f || this.moveDir.y == -1f)
        {
            if (normalState)
            {
                CollisionFlags cf = cc.Move(this.moveDir / 8f);
                if (colliderWithCCScript != null && ((MonoBehaviour)colliderWithCCScript).gameObject.activeInHierarchy && cf == CollisionFlags.None && (this.moveDir.x != 0f || this.moveDir.y != 0f))
                {
                    colliderWithCCScript.OnPlayerCollisionExit(this.gameObject);
                    lastHitGameObject = null;
                    colliderWithCCScript = null;
                }
            }
        }
    }

    float horizontal;
    float vertical;

    void Update()
    {
        if(talkButtonController != null && talkButtonController.IsTalkUIShowing())
        {
            return;
        }
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
    }

    private void FixedUpdate()
    {
        if (mTabUIRootScript.IsTabUIShowing()) return;

        if (this.needStopMove) {
            animator.SetBool("isRun", false);
            return;
        }

        if (horizontal != 0 && vertical != 0) //б
        {
            Vector3 camForword = playerCamera.transform.forward;
            Vector3 dir = (horizontal > 0 ? playerCamera.transform.right : -playerCamera.transform.right) + (vertical > 0 ? camForword : -camForword);
            DoTurn(dir.normalized);
        }
        else if (horizontal != 0 && vertical == 0) //����
        {
            DoTurn(horizontal > 0 ? playerCamera.transform.right : -playerCamera.transform.right);
        }
        else if (horizontal == 0 && vertical != 0) //ǰ��
        {
            Vector3 camForword = playerCamera.transform.forward;
            DoTurn(vertical > 0 ? camForword : -camForword);

        }
        if (horizontal != 0f || vertical != 0f)
        {
            animator.SetBool("isRun", true);
        }
        else
        {
            animator.SetBool("isRun", false);
            this.moveDir = Vector3.zero;
        }
        DoMove();
    }


    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("PlayerControl OnCollisionEnter");
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        IColliderWithCC tmp = hit.collider.gameObject.GetComponent<IColliderWithCC>();
        if(tmp != null) //��һ����Ҫ��CC��ײ������
        {
            if (lastHitGameObject == hit.collider.gameObject) return; //�ظ���ײһ���������
            colliderWithCCScript = tmp;
            colliderWithCCScript.OnPlayerCollisionEnter(this.gameObject);
            lastHitGameObject = hit.collider.gameObject;
        }
    }

}

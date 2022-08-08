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

    private void Awake()
    {
        //debug
        //PlayerPrefs.DeleteAll();

        int lastSceneIndex = SaveUtil.GetLastSceneBuildIndex();
        if (lastSceneIndex >= 0 && lastSceneIndex == this.gameObject.scene.buildIndex) //�б����¼,�ұ��泡���͵�ǰһ����˵���Ƿ��ػ��߶���
        {
            Debug.LogWarning("=================���ڷ���ǰ��ĳ��� ���� ������󱣴�ĳ���");
            Vector3 position = SaveUtil.GetLastPosition();
            if (position != Vector3.zero)
            {
                this.transform.position = position;
            }
            else
            {
                Debug.LogError("���ݴ��� position is 0");
            }
        }
        else
        {
            //ÿ�δ�������(�����³���)������߽���ս��ǰ����
            Debug.LogWarning("=================���ڱ����ɫ����");
            SaveUtil.SaveGameObjLastState(this.gameObject);
        }

    }

    private void OnApplicationQuit()
    {
        Debug.LogWarning("OnApplicationQuit()");
        PlayerPrefs.DeleteAll();
    }

    void Start()
    {
        animator = GetComponent<Animator>();

        talkButtonController = GameObject.FindGameObjectWithTag("DoTalkButton")?.GetComponent<TalkButtonController>();

        //mRigidbody = GetComponent<Rigidbody>();

        cc = GetComponent<CharacterController>();

        //cc.detectCollisions = false;

        if (playerCamera == null) playerCamera = Camera.main;
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
        CollisionFlags cf = cc.Move(this.moveDir / 8);
        //Debug.Log(this.moveDir);
        if (colliderWithCCScript != null && ((MonoBehaviour)colliderWithCCScript).gameObject.activeInHierarchy && cf == CollisionFlags.None && (this.moveDir.x != 0f || this.moveDir.y != 0f))
        {
            colliderWithCCScript.OnPlayerCollisionExit(this.gameObject);
            lastHitGameObject = null;
            colliderWithCCScript = null;
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
            animator?.SetBool("isRun", true);
        }
        else
        {
            animator?.SetBool("isRun", false);
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

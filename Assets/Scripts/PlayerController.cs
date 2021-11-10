using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using static Define;
using UnityEngine.UI;

public class PlayerController : MonoBehaviourPunCallbacks,IPunObservable
{
    int _mask = (1 << (int)Define.Layer.Player) | (1 << (int)Define.Layer.Object);
    public GameObject Bullet;

    public SpriteRenderer _sprite;
    public Animator _anim;
    public PhotonView PV;
    public Transform tr;
    public Text _nickName;
    public Image _imageHP;

    Rigidbody2D _rigid;
    Coroutine _coSkill;

    [SerializeField]
    GameObject _scanObject;

    // Photon 연동 관련 변수들
    bool isSkill = false;
    bool isStop = false;
    bool isMove = true;

    bool currSkill = true;
    bool currStop = true;

   // float currHp = 1.0f;
    Vector3 currPos;

    // Define 관련
    CreatureState _state = CreatureState.Idle;
    public MoveDir _dir = MoveDir.None;
    public MoveDir _lastDir = MoveDir.Down;

    float Speed = 10f;
    public Vector3 dirVec;

    public MoveDir Dir
    {
        get { return _dir; }
        set
        {
            if (_dir == value)
                return;
            _dir = value;

            if (value != MoveDir.None)
                _lastDir = value;

            UpdateMoving();
        }
    }
    public CreatureState State
    {
        get { return _state; }
        set
        {
            if (_state == value)
                return;

            _state = value;

            UpdateMoving();
        }
    }

    void Awake()
    {
        Init();
    }
    protected virtual void Init()
    {
        _sprite = GetComponent<SpriteRenderer>();
        _rigid = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
    }
    void Start()
    {
        _nickName.text = PV.IsMine ? PhotonNetwork.NickName : PV.Owner.NickName;
        _nickName.color = PV.IsMine ? Color.green : Color.red;
    }
    private void FixedUpdate()
    {
        // Ray
        Debug.DrawRay(_rigid.position + new Vector2(0, 0.5f), dirVec * 1f, new Color(1, 0, 0));
        RaycastHit2D rayHit = Physics2D.Raycast(_rigid.position + new Vector2(0, 0.5f), dirVec, 1f, _mask);

        // Target Lock On
        if (rayHit == true)
            _scanObject = rayHit.collider.gameObject;
        else
            _scanObject = null;
    }
    private void Update()
    {
        UpdateController();
        if (PV.IsMine)
            Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
    }

    void GetDirInput() 
    {
        if (PV.IsMine)
        {
            PV.RPC("FlipX", RpcTarget.AllBuffered, _lastDir);
            PV.RPC("MoveAnim", RpcTarget.AllBuffered, _lastDir, isMove);

            if (Input.GetKey(KeyCode.W))
            {
                Dir = MoveDir.Up;
                isMove = true;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                Dir = MoveDir.Down;
                isMove = true;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                Dir = MoveDir.Left;
                isMove = true;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                Dir = MoveDir.Right;
                isMove = true;
            }
            else
            {
                Dir = MoveDir.None;
                isMove = false;
                _anim.SetTrigger("isMoveStop");
            }
        }
        else
        {
            tr.position = Vector3.Lerp(tr.position, currPos, Time.deltaTime * 10.0f);

            isSkill = false;
            if (currSkill)
            {
                _anim.SetTrigger("isChangeSkill");
                _anim.SetBool("_isPunch", true);
                _coSkill = StartCoroutine("CoStartPunch");
                currSkill = false; 
            }

            isStop = false;
            if (currStop) { _anim.SetTrigger("isMoveStop"); currStop = false; }
        }
    }
    void UpdateController()
    {
        switch (State)
        {
            case CreatureState.Idle:
                GetDirInput();
                UpdateIdle();
                break;
            case CreatureState.Moving:
                GetDirInput();
                UpdateMoving();
                break;
            case CreatureState.Skill:
                //UpdateSkill();
                break;
            case CreatureState.Dead:
                //UpdateDead();
                break;
        }
    }
    void UpdateIdle()
    {
        // Photon
        isStop = true;

        // isIdle ?
        if (Dir != MoveDir.None) { State = CreatureState.Moving; return; }

        // Attack Button
        if (Input.GetKey(KeyCode.E) && PV.IsMine)
        {
            State = CreatureState.Skill;
            _coSkill = StartCoroutine("CoStartPunch");
            currSkill = true;

            if (_scanObject == null)
                return;
        }

        if (Input.GetKeyDown(KeyCode.R) && PV.IsMine)
        {
            PhotonNetwork.Instantiate(Bullet.name, transform.position + dirVec, Quaternion.identity);
        }
    }

    void UpdateMoving()
    {
        if (_state == CreatureState.Moving)
        {
            switch (_dir)
            {
                case MoveDir.Up:
                    _rigid.velocity = new Vector2(0, 1) * Speed;
                    dirVec = Vector3.up;
                    _sprite.flipX = false;
                  
                    _anim.Play("PlayerWalk_Back");
                    _anim.SetInteger("vAxisRaw", (int)dirVec.y);
                    _anim.SetInteger("hAxisRaw", 0);
                    break;

                case MoveDir.Down:
                    _rigid.velocity = new Vector2(0, -1) * Speed;
                    dirVec = Vector3.down;
                    _sprite.flipX = false;

                    _anim.Play("PlayerWalk_Front");
                    _anim.SetInteger("vAxisRaw", (int)dirVec.y);
                    _anim.SetInteger("hAxisRaw", 0);
                    break;

                case MoveDir.Left:
                    _rigid.velocity = new Vector2(-1, 0) * Speed;
                    dirVec = Vector3.left;
                    _sprite.flipX = false;

                    _anim.Play("PlayerWalk_Side");
                    _anim.SetInteger("hAxisRaw", (int)dirVec.x);
                    _anim.SetInteger("vAxisRaw", 0);
                    break;

                case MoveDir.Right:
                    _rigid.velocity = new Vector2(1, 0) * Speed;
                    dirVec = Vector3.right;
                    _sprite.flipX = true;

                    _anim.Play("PlayerWalk_Side");
                    _anim.SetInteger("hAxisRaw", (int)dirVec.x);
                    _anim.SetInteger("vAxisRaw", 0);
                    break;

                case MoveDir.None:
                    _rigid.velocity = Vector2.zero;

                    _anim.SetInteger("hAxisRaw", 0);
                    _anim.SetInteger("vAxisRaw", 0);
                    State = CreatureState.Idle;
                    break;
            }
        }
        else if (_state == CreatureState.Skill)
        {
            switch (_lastDir)
            {
                case MoveDir.Up:
                    _anim.SetBool("_isPunch", true);
                    _anim.SetTrigger("isChangeSkill");
                    _sprite.flipX = false;
                    isSkill = true;
                    break;

                case MoveDir.Down:
                    _anim.SetBool("_isPunch", true);                    
                    _anim.SetTrigger("isChangeSkill");
                    _sprite.flipX = false;
                    isSkill = true;
                    break;

                case MoveDir.Left:
                    _anim.SetBool("_isPunch", true);
                    _anim.SetTrigger("isChangeSkill");
                    _sprite.flipX = false;
                    isSkill = true;
                    break;

                case MoveDir.Right:
                    _anim.SetBool("_isPunch", true);
                    _anim.SetTrigger("isChangeSkill");
                    _sprite.flipX = true;                    
                    isSkill = true;
                    break;
            }
        }
    }
  
    [PunRPC]
    void FlipX(MoveDir dir)
    {
        if (dir == MoveDir.Right)
            _sprite.flipX = true;
        else
            _sprite.flipX = false;
    }

    [PunRPC]
    void MoveAnim(MoveDir dir, bool isMove)
    {
        if (isMove == true)
        {
            isStop = false;

            if (dir == MoveDir.Up)
                _anim.Play("PlayerWalk_Back");
            else if (dir == MoveDir.Down)
                _anim.Play("PlayerWalk_Front");
            else if (dir == MoveDir.Left)
                _anim.Play("PlayerWalk_Side");
            else if (dir == MoveDir.Right)
                _anim.Play("PlayerWalk_Side");
        }
    }

    IEnumerator CoStartPunch()
    {        
        if (PV.IsMine)
        {
            Debug.Log("CoStartPunch !");

            // 대기 시간
            yield return new WaitForSeconds(0.5f);
            State = CreatureState.Idle;
            _coSkill = null;
            _anim.SetBool("_isPunch", false);
        }
        else if (!PV.IsMine)
        {
            yield return new WaitForSeconds(0.5f);
            State = CreatureState.Idle;
            _coSkill = null;
            _anim.SetBool("_isPunch", false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bullet"))
        {
            Debug.Log("Player Damaged !");

            _imageHP.fillAmount -= 0.1f;

            if(PV.IsMine && _imageHP.fillAmount <= 0.0f)
            {
                _imageHP.fillAmount = 1.0f;
            }
        }
    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(tr.position);
            stream.SendNext(isSkill);
            stream.SendNext(isStop);
            stream.SendNext(_imageHP.fillAmount);

            isSkill = false;
            isStop = false;
        }
        else
        {
            currPos = (Vector3)stream.ReceiveNext();
            currSkill = (bool)stream.ReceiveNext();
            currStop = (bool)stream.ReceiveNext();
            _imageHP.fillAmount = (float)stream.ReceiveNext();
        }
    }
}

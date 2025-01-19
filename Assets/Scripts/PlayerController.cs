using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using static Define;
using TMPro;

/// <summary>
/// 플레이어 조작 스크립트
/// </summary>
/// 
public class PlayerController : MonoBehaviourPunCallbacks,IPunObservable
{
    public TextMeshProUGUI _playerName;
    public SpriteRenderer _sprite;
    public Animator _anim;
    Rigidbody2D _rigid;
    GameObject _scanObject;
    Coroutine _coSkill;
    float Speed = 10f;

    // Photon 관련 변수들
    bool isSkill = false;
    bool isStop = false;
    bool isMove = true;

    bool currSkill = true;
    bool currStop = true;

    Vector3 currPos;

    public PhotonView PV;
    public Transform tr;

    // Define 관련
    CreatureState _state = CreatureState.Idle;
    public MoveDir _dir = MoveDir.None;
    public MoveDir _lastDir = MoveDir.Down;
    Vector3 dirVec;    

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

    void Start()
    {
        Init();
    }
    void Init()
    {
        _sprite = GetComponent<SpriteRenderer>();
        _rigid = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();

        if (!PV.IsMine)
        {
            _playerName.color = Color.red;
        }
    }

    private void FixedUpdate()
    {
        // Ray
        Debug.DrawRay(_rigid.position + new Vector2(0, 0.5f), dirVec * 1f, new Color(1, 0, 0));
        RaycastHit2D rayHit = Physics2D.Raycast(_rigid.position + new Vector2(0, 0.5f), dirVec, 1f, LayerMask.GetMask("Object"));

        if (rayHit.collider != null)
        {
            _scanObject = rayHit.collider.gameObject;
        }
        else
            _scanObject = null;
    }
    private void Update()
    {
        UpdateController();

        // Scan
        if (Input.GetButtonDown("Jump") && _scanObject != null)
            Debug.Log("this name is : " + _scanObject.name);

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
        isStop = true;

        if (Dir != MoveDir.None)
        {
            State = CreatureState.Moving;
            return;
        }
        if (Input.GetKey(KeyCode.E) && PV.IsMine)
        {
            State = CreatureState.Skill;
            _coSkill = StartCoroutine("CoStartPunch");
            currSkill = true;
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
        //GameObject go = _scanObject;

        //if(go != null)
        //{
        //    PlayerController pc = go.GetComponent<PlayerController>();
        //    if (pc != null)
        //        pc.OnDamaged();
        //}
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
            _anim.SetBool("_isPunch", false);
            _coSkill = null;
        }

    }
    public void OnDamaged()
    {
        Debug.Log("Player Hit !");
    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(tr.position);
            stream.SendNext(isSkill);
            stream.SendNext(isStop);

            isSkill = false;
            isStop = false;

        }
        else
        {
            currPos = (Vector3)stream.ReceiveNext();
            currSkill = (bool)stream.ReceiveNext();
            currStop = (bool)stream.ReceiveNext();
        }
    }
}

using Photon.Pun;
using Settings;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class CannoneerCannon : MonoBehaviourPun
{
    [Header("Cannon Parts")]
    [SerializeField] private Transform CanBase;
    [SerializeField] private Transform Barrel;
    [SerializeField] private Transform BarrelEnd;
    [SerializeField] private Transform HumanMount;

    private PhotonView PV;

    private GameObject Hero;

    protected GeneralInputSettings _input;
    protected HumanInputSettings _humanInput;
    protected InteractionInputSettings _interactionInput;

    private float currentRot = 0f;
    private float RotateSpeed = 30f;
    private float SmoothingDelay = 5f;
    private Quaternion correctBarrelRot = Quaternion.identity;
    public LineRenderer myCannonLine;

    private void Awake()
    {
        PV = gameObject.GetComponent<PhotonView>();
        Hero = PhotonExtensions.GetPlayerFromID(PV.Owner.ActorNumber);
        this.correctBarrelRot = this.Barrel.rotation;
        this.myCannonLine = this.BarrelEnd.GetComponent<LineRenderer>();
    }

    void Start()
    {
        Hero.transform.position = HumanMount.transform.position;
        Hero.transform.SetParent(HumanMount.transform);

        if (PV.IsMine)
        {
            _input = SettingsManager.InputSettings.General;
            _humanInput = SettingsManager.InputSettings.Human;
            _interactionInput = SettingsManager.InputSettings.Interaction;
        }
    }

    void Shoot()
    {

    }

    public void UnMount() //Gotta Send RPC Here
    {
        Hero.transform.parent = null; //RPC
        PhotonNetwork.Destroy(gameObject);
    }

    void FixedUpdate()
    {
        if (!PV.IsMine)
        {
            this.Barrel.rotation = Quaternion.Lerp(this.Barrel.rotation, this.correctBarrelRot, Time.deltaTime * this.SmoothingDelay);
            return;
        }

        DrawLine();
        Controls();
    }

    private void Controls()
    {
        if (_interactionInput.Interact.GetKeyDown()) { UnMount(); }
        if (_humanInput.AttackDefault.GetKeyDown()) { Shoot(); }

        if (_input.Forward.GetKey())
        {
            if (this.currentRot <= 32f)
            {
                this.currentRot += Time.deltaTime * RotateSpeed;
                this.Barrel.Rotate(new Vector3(Time.deltaTime * RotateSpeed, 0f, 0f));
            }
        }
        else if (_input.Back.GetKey() && (this.currentRot >= -18f))
        {
            this.currentRot += Time.deltaTime * -RotateSpeed;
            this.Barrel.Rotate(new Vector3(Time.deltaTime * -RotateSpeed, 0f, 0f));
        }
        if (_input.Left.GetKey())
        {
            base.transform.Rotate(new Vector3(0f, Time.deltaTime * -RotateSpeed, 0f));
        }
        else if (_input.Right.GetKey())
        {
            base.transform.Rotate(new Vector3(0f, Time.deltaTime * RotateSpeed, 0f));
        }
    }

    private void DrawLine()
    {
        Vector3 vector = new Vector3(0f, -30f, 0f);
        Vector3 position = this.BarrelEnd.position;
        Vector3 vector3 = (Vector3)(this.BarrelEnd.forward * 300f);
        float num = 40f / vector3.magnitude;
        this.myCannonLine.SetWidth(0.5f, 40f);
        this.myCannonLine.SetVertexCount(100);
        for (int i = 0; i < 100; i++)
        {
            this.myCannonLine.SetPosition(i, position);
            position += (Vector3)((vector3 * num) + (((0.5f * vector) * num) * num));
            vector3 += (Vector3)(vector * num);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(base.transform.position);
            stream.SendNext(base.transform.rotation);
            stream.SendNext(this.Barrel.rotation);
        }
        else
        {
            //this.correctPlayerPos = (Vector3)stream.ReceiveNext();
            //this.correctPlayerRot = (Quaternion)stream.ReceiveNext();
            this.correctBarrelRot = (Quaternion)stream.ReceiveNext();


            //float lag = Mathf.Abs((float)(PhotonNetwork.time - info.timestamp));
            //rigidbody.position += rigidbody.velocity * lag;
        }
    }
}

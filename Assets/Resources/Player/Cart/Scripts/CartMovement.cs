using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;
using DG.Tweening;
using Unity.Netcode;
using Unity.Netcode.Components;

public class CartMovement : NetworkBehaviour
{
    #region Values
    [ReadOnly] public NetworkVariable<float> SpeedInput = new (0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner), 
    SteeringInput = new (0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [ReadOnly] public float CurrentSpeed { get => walkSpeed.Value * speedFactor.Value; }
    [ReadOnly] public float CurrentSteerSpeed;

    [ReadOnly] private NetworkVariable<float> speedFactor = new (1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner), 
    steerFactor = new (1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [SerializeField] NetworkVariable<float> walkSpeed = new (0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner), 
    walkSteerSpeed = new (0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public float chargeSpeedFactor, chargeSteerFactor;
    public float chargeTime, chargeCooldownTime;
    [ReadOnly] public bool charge = false,
    chargeCooldown = false;

    private Tween speedTween, steerTween;
    private Coroutine _charge;
    #endregion

    #region References
    [HideInInspector] public Cart Cart;
    #endregion

    // public override void OnReanticipate(double ab)
    // {
    //     //Debug.Log(ab);
    //     var ant = transform.parent.GetComponent<AnticipatedNetworkTransform>();
    //     var prev = ant.PreviousAnticipatedState;

    //     if (ant.IsHost)
    //         ant.Smooth(ant.AnticipatedState, ant.AuthoritativeState, 0.05f);
    //     else 
    //     {
    //         // var sqDist = Vector3.SqrMagnitude(prev.Position - ant.AnticipatedState.Position);
    //         // var sqRot = Vector3.SqrMagnitude(prev.Rotation.eulerAngles - ant.AnticipatedState.Rotation.eulerAngles);
    //         // if (sqDist <= 0.02f || sqRot <= 0.02f)
    //         // {
    //         //     // This prevents small amounts of wobble from slight differences.
    //         //     ant.AnticipateState(prev);
    //         // }
    //         // else 
    //             ant.Smooth(prev, ant.AnticipatedState, 0.05f);
    //     }
    // }

    void FixedUpdate()
    {
        ApplyDirectionForce();
        ApplySteering();
    }

    void ApplyDirectionForce() =>
        Cart.Rigidbody.AddForce(transform.up * walkSpeed.Value * speedFactor.Value * (charge ? 1 : SpeedInput.Value), ForceMode2D.Force);

    void ApplySteering() =>
        Cart.Rigidbody.AddTorque(SteeringInput.Value * (-walkSteerSpeed.Value) * steerFactor.Value, ForceMode2D.Force);

    public void Charge()
    {
        if (charge || chargeCooldown) return;
        _charge = StartCoroutine(ChargeRoutine());
    }

    private IEnumerator ChargeRoutine()
    {
        charge = true;
        float halfChargeTime = chargeTime / 2f;

        speedTween = DOTween.To(() => speedFactor.Value, x => speedFactor.Value = x, chargeSpeedFactor, halfChargeTime);
        steerTween = DOTween.To(() => steerFactor.Value, x => steerFactor.Value = x, chargeSteerFactor, halfChargeTime);
        
        yield return new WaitForSeconds(halfChargeTime);
        
        charge = false;

        speedTween = DOTween.To(() => speedFactor.Value, x => speedFactor.Value = x, 1f, halfChargeTime);
        steerTween = DOTween.To(() => steerFactor.Value, x => steerFactor.Value = x, 1f, halfChargeTime);
        
        yield return new WaitForSeconds(halfChargeTime);
        
        chargeCooldown = true;
        yield return new WaitForSeconds(chargeCooldownTime);
        chargeCooldown = false;
    }

    public void StopCharge()
    {
        if (_charge != null)
        {
            StopCoroutine(_charge);
            _charge = null;
        }
        
        speedTween.Kill();
        steerTween.Kill();
        
        speedFactor.Value = 1f;
        steerFactor.Value = 1f;
        
        charge = false;
        chargeCooldown = false;
    }

    // TextureImporterShape tisha ganin lena i love her she is DeliveryNotificationOptions smart beautiful girl.....
    // ModelImporterGenerateAnimation game game gaming dota 2 game i like arkana krip4ik )) kek ha ha lena TOP TOP TOP !!! private void OnAnimatorIK(int layerIndex)
    // LOVE LENA!! MySearchField name is daniil oykin///. i love my cats - lena and tisha. they are so cute and mrrr mrrr memememe haha) i love cat tisha and lena))) DANYA OYKIN 
}

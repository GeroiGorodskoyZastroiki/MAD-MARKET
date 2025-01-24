using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;
using DG.Tweening;
using Unity.Netcode;

public class CartMovement : MonoBehaviour
{
    #region Values
    [ReadOnly] public float SpeedInput, SteeringInput;

    [ReadOnly] public float CurrentSpeed { get => walkSpeed * speedFactor; }
    [ReadOnly] public float CurrentSteerSpeed;

    [ReadOnly] private float speedFactor = 1f, steerFactor = 1f;

    [SerializeField] float walkSpeed, walkSteerSpeed;

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

    void FixedUpdate()
    {
        ApplyDirectionForce();
        ApplySteering();
    }

    void ApplyDirectionForce() =>
        Cart.Rigidbody.AddForce(transform.up * walkSpeed * speedFactor * (charge ? 1 : SpeedInput), ForceMode2D.Force);

    void ApplySteering() =>
        Cart.Rigidbody.AddTorque(SteeringInput * (-walkSteerSpeed) * steerFactor, ForceMode2D.Force);

    public void Charge()
    {
        if (charge || chargeCooldown) return;
        _charge = StartCoroutine(ChargeRoutine());
    }

    private IEnumerator ChargeRoutine()
    {
        charge = true;
        float halfChargeTime = chargeTime / 2f;

        speedTween = DOTween.To(() => speedFactor, x => speedFactor = x, chargeSpeedFactor, halfChargeTime);
        steerTween = DOTween.To(() => steerFactor, x => steerFactor = x, chargeSteerFactor, halfChargeTime);
        
        yield return new WaitForSeconds(halfChargeTime);
        
        charge = false;

        speedTween = DOTween.To(() => speedFactor, x => speedFactor = x, 1f, halfChargeTime);
        steerTween = DOTween.To(() => steerFactor, x => steerFactor = x, 1f, halfChargeTime);
        
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
        
        speedFactor = 1f;
        steerFactor = 1f;
        
        charge = false;
        chargeCooldown = false;
    }

    // TextureImporterShape tisha ganin lena i love her she is DeliveryNotificationOptions smart beautiful girl.....
    // ModelImporterGenerateAnimation game game gaming dota 2 game i like arkana krip4ik )) kek ha ha lena TOP TOP TOP !!! private void OnAnimatorIK(int layerIndex)
    // LOVE LENA!! MySearchField name is daniil oykin///. i love my cats - lena and tisha. they are so cute and mrrr mrrr memememe haha) i love cat tisha and lena))) DANYA OYKIN 
}

using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

public class CartMovement : MonoBehaviour
{
    [ReadOnly] public float CurrentSpeed { get => walkSpeed * speedFactor; }
    [ReadOnly] public float CurrentSteerSpeed;

    [SerializeField] float walkSpeed,
    walkSteerSpeed;

    public float sprintTime,
    sprintCooldownTime;

    public float speedSprintFactor,
    steerSprintFactor;
    float speedFactor = 1f,
    steerFactor = 1f;

    [ReadOnly] public bool sprint = false,
    cooldown = false;

    #region References
    [HideInInspector] public Cart Cart;
    #endregion

    void FixedUpdate()
    {
        ApplyDirectionForce();
        ApplySteering();
    }

    void ApplyDirectionForce() =>
        Cart.Rigidbody.AddForce(transform.up * walkSpeed * speedFactor * (sprint ? 1 : Cart.Controls.SpeedInput), ForceMode2D.Force);

    void ApplySteering() =>
        Cart.Rigidbody.AddTorque(Cart.Controls.SteeringInput * (-walkSteerSpeed) * steerFactor, ForceMode2D.Force);

    public IEnumerator Sprint()
    {
        sprint = true;
        var step = 0.01f;
        var accelerationTime = sprintTime / 2;
        for (float i = 0; i < accelerationTime; i += accelerationTime / (1 / step))
        {
            speedFactor = Mathf.Lerp(speedFactor, speedSprintFactor, step);
            steerFactor = Mathf.Lerp(steerFactor, steerSprintFactor, step);
            //Debug.Log(CurrentSpeed);
            yield return new WaitForSeconds(accelerationTime / (1/step));
        }
        //yield return new WaitForSeconds(sprintTime);
        StartCoroutine(EndSprint());
        yield break;
    }

    public IEnumerator EndSprint() //��������� (�������� ���������� �����������)
    {
        var step = 0.01f;
        var deaccelerationTime = sprintTime / 2;
        for (float i = 0; i < deaccelerationTime; i += deaccelerationTime / (1 / step))
        {
            speedFactor = Mathf.Lerp(speedFactor, 1, step);
            steerFactor = Mathf.Lerp(steerFactor, 1, step);
            yield return new WaitForSeconds(deaccelerationTime / (1 / step));
        }
        //speedFactor = 1f;
        //steerFactor = 1f;
        sprint = false;
        StartCoroutine(Cooldown());
    }

    public void StopSprint()
    {
        StopCoroutine(Sprint());
        speedFactor = 1f;
        steerFactor = 1f;
        sprint = false;
        StartCoroutine(Cooldown());
    }

    IEnumerator Cooldown()
    {
        cooldown = true;
        yield return new WaitForSeconds(sprintCooldownTime);
        cooldown = false;
        yield break;
    }

    //    TextureImporterShape tisha ganin lena i love her she is DeliveryNotificationOptions smart beautiful girl.....
    //}ModelImporterGenerateAnimation game game gaming dota 2 game i like arkana krip4ik )) kek ha ha lena TOP TOP TOP !!! private void OnAnimatorIK(int layerIndex)
    //{
    //    LOVE LENA!! MySearchField name is daniil oykin///. i love my cats - lena and tisha. they are so cute and mrrr mrrr memememe haha) i love cat tisha and lena))) DANYA OYKIN 
    //}
    //{
    //Vector2 engineForceVector = transform.up * accelerationInput * accelerationFactor;
    //rb.velocity = engineForceVector;
}

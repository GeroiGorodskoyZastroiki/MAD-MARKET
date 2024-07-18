using UnityEngine;

public class CartCamera : MonoBehaviour
{
    #region References
    [HideInInspector] public Cart Cart;
    #endregion

    void Update()
    {
        transform.localRotation = Quaternion.Inverse(transform.parent.rotation);
        //Camera.main.transform.position = gameObject.transform.position + new Vector3(0, 0, -10);
    }
}

using System.Collections.Generic;
using UnityEngine;

public class WheelContactScript : MonoBehaviour
{
    #region Variables
    public bool test = false;

    [SerializeField, Tooltip("Le layer du sol")]
    private LayerMask groundLayer;

    private float wheelSize;
    #endregion

    #region Awake
    private void Awake()
    {
        wheelSize = transform.localScale.x * 0.55f;
    }
    #endregion

    #region Public_Methods
    //public bool isContacting() => groundContacts.Count > 0;
    public bool isContacting()
    {
        if (test) Debug.Log(gameObject.name + " " + false);
        else Debug.Log(gameObject.name + " " + Physics.Raycast(transform.position, Vector3.down, wheelSize, groundLayer));

        Debug.DrawLine(transform.position, transform.position + Vector3.down * wheelSize, Color.red);
        if (test) return false;
        return Physics.Raycast(transform.position, Vector3.down, wheelSize, groundLayer);
    }
    #endregion
}

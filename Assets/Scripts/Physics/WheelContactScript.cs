using System.Collections.Generic;
using UnityEngine;

public class WheelContactScript : MonoBehaviour
{
    #region Variables
    public bool test;

    [SerializeField, Tooltip("Le layer du sol")]
    private LayerMask groundLayer;

    private float wheelSize; //La taille de notre raycast pour check le sol, legerement plus gros que la vraie roue
    #endregion

    #region Awake
    private void Awake()
    {
        //On calcule notre variable
        wheelSize = transform.localScale.x * 0.53f;
    }
    #endregion

    #region Public_Methods
    public bool isContacting() => Physics.Raycast(transform.position, Vector3.down, wheelSize, groundLayer);
    #endregion
}

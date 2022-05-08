using System;
using UnityEngine;

public class WheelsContactEffectScript : MonoBehaviour
{
    #region Variables
    [SerializeField, Tooltip("Les roues motrices du vehicule")]
    private Collider[] drivingWheels; //Pour rappel, les roues sont dans l'ordre gauche, droite, gauche, droite...
    [SerializeField, Tooltip("Les roues directrices du vehicule")]
    private Collider[] steeringWheels; //Pour rappel, les roues sont dans l'ordre gauche, droite, gauche, droite...
    [SerializeReference, Tooltip("Les autres roues du vehicule, si necessaire")]
    private Collider[] remainingWheels; //Pour rappel, les roues sont dans l'ordre gauche, droite, gauche, droite...

    private WheelContactScript[] wheelContacts;
    private int index;

    private bool onTheGround = false;
    private float torqueMultiplier, forwardScalar, turningScalar;
    #endregion

    #region Start
    private void Start()
    {
        wheelContacts = new WheelContactScript[drivingWheels.Length + steeringWheels.Length + remainingWheels.Length];
        index = 0;
        ExtractList(drivingWheels);
        ExtractList(steeringWheels);
        ExtractList(remainingWheels);
    }
    #endregion

    #region Public_Methods
    public void ContactEffects()
    {
        onTheGround = true;

        torqueMultiplier = 0;
        forwardScalar = 0;
        index = 0;
        while(index < drivingWheels.Length)
        {
            if (wheelContacts[index].isContacting())
            {
                torqueMultiplier++;
                if (index % 2 == 0) forwardScalar--;
                else forwardScalar++;
            }
            else onTheGround = false;
            index++;
        }
        torqueMultiplier /= drivingWheels.Length;
        forwardScalar /= drivingWheels.Length;

        turningScalar = 0;
        while(index < drivingWheels.Length + steeringWheels.Length)
        {
            if (wheelContacts[index].isContacting())
            {
                if (index % 2 == 0) turningScalar--;
                else turningScalar++;
            }
            else onTheGround = false;
            index++;
        }
        turningScalar /= steeringWheels.Length;

        while (index < drivingWheels.Length + steeringWheels.Length + remainingWheels.Length)
        {
            if (!wheelContacts[index].isContacting()) onTheGround = false;
            index++;
        }
    }

    public bool IsOnTheGround() => onTheGround;
    public float GetTorqueMultiplier() => torqueMultiplier;
    public float GetForwardScalar() => forwardScalar;
    public float GetTurningScalar() => turningScalar;
    #endregion

    #region Private_Methods
    private void ExtractList(Collider[] wheels)
    {
        foreach (Collider wheel in wheels)
        {
            wheelContacts[index] = wheel.GetComponent<WheelContactScript>();
            index++;
        }
    }
    #endregion
}

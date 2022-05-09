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

    private WheelContactScript[] wheelContacts; //La liste des scipts de contact presents sur le vehicule
    private int index; //Un index pour parcourir la liste du dessus

    private bool onTheGround = false; //Est-ce qu'on est au sol ?
    private int drivingOnTheGround = 0, steeringOnTheGround = 0; //Combien de roues de chaque type sont au sol
    private float torqueMultiplier, forwardScalar, turningScalar; //Des scalaires qu'on calcule a partir de notre situation actuelle
    #endregion

    #region Start
    private void Start()
    {
        //On instancie nos variables
        wheelContacts = new WheelContactScript[drivingWheels.Length + steeringWheels.Length + remainingWheels.Length];
        index = 0;

        //On utilise les listes specifiees par l'utilisateur pour remplir la liste du dessus
        ExtractList(drivingWheels);
        ExtractList(steeringWheels);
        ExtractList(remainingWheels);
    }
    #endregion

    #region Public_Methods
    /// <summary>
    /// Permet de calculer les effets qui s'appliquent au comportement du vehicule en fonction des roues qui touchent le sol (ou non)
    /// </summary>
    public void ContactEffects()
    {
        //On passera a false des qu'une roue est en l'air
        onTheGround = true;

        //On commence par verifier l'etat des roues motrices
        drivingOnTheGround = drivingWheels.Length;
        torqueMultiplier = 0;
        forwardScalar = 0;
        index = 0;
        //On fait le tour des roues motrices
        while(index < drivingWheels.Length)
        {
            //Si elle est au sol...
            if (wheelContacts[index].isContacting())
            {
                //... sa puissance peut s'appliquer au vehicule
                torqueMultiplier++;
                if (index % 2 == 0) forwardScalar++; //Pour rappel, les roues paires sont a gauche...
                else forwardScalar--;//... les roues impaires sont a droite
            }
            //Si elle est pas au sol, on en tient compte dans nos variables de contact
            else
            {
                onTheGround = false;
                drivingOnTheGround--;
            }
            index++;
        }
        //On finit de calculer les variables de comportement pour notre vehicule
        torqueMultiplier /= drivingWheels.Length;
        forwardScalar /= drivingWheels.Length;

        //On travaille sur l'effet des roues directrices
        steeringOnTheGround = steeringWheels.Length;
        turningScalar = 0;
        //On fait le tour des roues directrices
        while(index < drivingWheels.Length + steeringWheels.Length)
        {
            //Si elle est au sol...
            if (wheelContacts[index].isContacting())
            {
                //...sa direction peut s'appliquer au vehicule
                if (index % 2 == 0) turningScalar++;//Pour rappel, les roues paires sont a gauche...
                else turningScalar--;//... les roues impaires sont a droites
            }
            //Si elle est pas au sol, on en tient compte dans nos variables de contact
            else
            {
                onTheGround = false;
                steeringOnTheGround--;
            }
            index++;
        }
        //On finit de calculer les variables de comportement pour notre vehicule
        turningScalar /= steeringWheels.Length;

        //On finit par verifier les roues inutiles, des fois qu'une d'entre elles ne soit pas au sol
        while (index < drivingWheels.Length + steeringWheels.Length + remainingWheels.Length)
        {
            if (!wheelContacts[index].isContacting()) onTheGround = false;
            index++;
        }
    }

    public bool IsOnTheGround() => onTheGround;
    public bool IsDrivingOnTheGround() => drivingOnTheGround > 0;
    public bool IsSteeringOnTheGround() => steeringOnTheGround > 0;
    public float GetTorqueMultiplier() => torqueMultiplier;
    public float GetForwardScalar() => forwardScalar;
    public float GetTurningScalar() => turningScalar;
    #endregion

    #region Private_Methods
    /// <summary>
    /// Permet d'extraire des components d'une roue pour les placer dans la liste appropriee
    /// </summary>
    /// <param name="wheels">Les roues sur lesquelles on veut faire l'extraction</param>
    private void ExtractList(Collider[] wheels)
    {
        //Pour chaque roue, on extrait le bon script pour le placer dans la liste centrale
        foreach (Collider wheel in wheels)
        {
            wheelContacts[index] = wheel.GetComponent<WheelContactScript>();
            index++;
        }
    }
    #endregion
}

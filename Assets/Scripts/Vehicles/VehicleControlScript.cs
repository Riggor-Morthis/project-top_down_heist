using UnityEngine;
using System.Collections;
using System;

public class VehicleControlScript : MonoBehaviour
{
    #region Variables
    private Rigidbody playerRigidbody; //Le rigidbody de notre vehicule
    private WheelsContactEffectScript wheelsContact; //Les roues qui sont au sol et leurs effets

    private float pedalInput = 0f, steeringWheelInput = 0f; //Les inputs qui sont passes a notre joueur
    private bool handbrakeInput = false; //Input du joueur

    private float statPower = 69, realPower; //La puissance annoncee au joueur, et la puissance utilisee par ce script
    private float statTorque = 108, realTorque; //La puissance annoncee au joueur, et la puissance utilisee par ce script
    private float statDisplacement = 1397, realDisplacement; //La cylindree au joueur, et la cylindree utilisee par ce script
    private float statBrake = 66, realBrake; //Les freins annoncee au joueur, et les freins utilises par ce script
    private float statHandling = 115, realHandling; //Le maniement annonce au joueur, et le maniement utilise par ce script
    private float statWeight = 962, realWeight; //La seule stat du joueur qui ne demande pas d'etre modifiee

    private const float unitySpeedScalar = 13.879442142f; //Pour que la puissance correspondate vraiment a la vitesse maximale
    private const float maxRpm = 6000; //Les tours maximum pour le vilebrequin

    private bool isClutchEngaged = true; //Est-ce qu'on est embraye ou pas ?
    private bool isStartingReverse = false, isReversing = false; //Est-ce qu'on est en train de voir si on recule ou pas ? Est-ce qu'on est en train de reculer ?
    private float[] gearBox; //Les differents ratio dans la boite de vitesse
    private float[] gearThresholds; //La vitesse de chaque ratio a 5000rpm
    private int currentGear = 0, maxGear; //Le ratio actuellement selectionne, et le nombre de ratio dans la boite
    private float gearSwitchTime = 0.4f; //Le temps qu'il faut pour passer une vitesse
    private float reverseTimer; //Combien de temps s'est ecoule depuis qu'on a demande a passer la marche arriere ?

    private float currentSpeed = 0, lostSpeed; //La vitesse (axe Z) actuelle du vehicule, et la vitesse qu'on perd lorsqu'on accelere pas
    private float currentRpm = 0, gainedRpm = 0; //Les tours actuels du vilebrequin, et les tours qu'on va gagner durant l'update actuelle

    private float steeringAngle = 0, wheelsAngle = 0; //L'angle du volant, et l'angle resultat pour les roues du vehicule
    private float steeringTurnSpeed = 1, steeringComeBackSpeed; //La vitesse a laquelle le volant atteint son maximum, et la vitesse a laquelle le volant retourne a son zero

    private bool onTheGround, drivingOnTheGround, steeringOnTheGround; //Est-ce qu'on est au sol, est-ce que les roues motrices sont au sol, est-ce que les roues directrices sont au sol ?
    private float torqueScalar, forwardScalar, steeringScalar; //Les modifications du comportement en fonction des roues qui sont au sol (ou non)
    private Vector3 forwardScalarVector = new Vector3(1f, 0f, 0f); //Le vecteur "tout droit" en fonction des roues motrices au sol
    private float downwardMovement, downwardForce; //La force de la gravite, et notre mouvement vers le bas actuel
    #endregion

    #region Awake
    private void Awake()
    {
        //On recupere les components
        GatherComponents();

        //On initialise/calcule les variables
        CalculateVariables();
    }
    #endregion

    #region Update
    private void FixedUpdate()
    {
        //On gere la gravite du vehicule
        GravityHandler();
        //On gere les pedales
        PedalsHandler();
        //On gere le volant
        SteeringWheelHandler();
    }
    #endregion

    #region Public_Methods
    /// <summary>
    /// Permet de recevoir les inputs fournis par le script qui les gere
    /// </summary>
    /// <param name="pI">input pour les pedales</param>
    /// <param name="sWI">input pour le volant</param>
    public void ReceiveInputs(float pI, float sWI, bool hI)
    {
        pedalInput = pI;
        steeringWheelInput = sWI;
        handbrakeInput = hI;
    }

    public int GetCurrentSpeedInt() => (int)currentSpeed;
    public int GetCurrentRpm() => (int)currentRpm;
    public int GetCurrentGear() => currentGear;
    public float GetRealPower() => (int)realPower;
    public float GetCurrentSpeedFloat() => currentSpeed;
    public bool GetIsClutchEngaged() => isClutchEngaged;
    public bool GetIsReversing() => isReversing;
    #endregion

    #region Private_Methods
    /// <summary>
    /// Permet de recuperer les differents components essentiels
    /// </summary>
    private void GatherComponents()
    {
        //On recupere le rigidbody
        playerRigidbody = GetComponent<Rigidbody>();
        //On recupere le script pour les contact
        wheelsContact = GetComponent<WheelsContactEffectScript>();
    }

    /// <summary>
    /// Permet de caculer les variables utiles pour le script a partir des valeurs annoncees au joueur
    /// </summary>
    private void CalculateVariables()
    {
        //"Calcul" du poids
        realWeight = statWeight;
        playerRigidbody.mass = realWeight;

        //Initialisation de la boite de vitesse
        gearBox = new float[] { 2.5f, 1.6f, 1.25f, 1f };
        maxGear = gearBox.Length;

        //Calcul de la puissance reelle
        if (statPower / gearBox[maxGear - 1] >= 50) realPower = -64.1985918658f + 49.999493352f * Mathf.Log(statPower / gearBox[maxGear - 1]);
        else realPower = 2.628011527739513f * (statPower / gearBox[maxGear - 1]);

        //On utilise les deux etapes precedents pour calculer les seuils de la boite de vitesse
        gearThresholds = new float[maxGear];
        for (int i = 0; i < maxGear; i++) gearThresholds[i] = (5000f / maxRpm) * (realPower / gearBox[i]);

        //Calcul du couple reel
        if (statTorque >= 50) realTorque = 0.007673411f * (statTorque * statTorque * statTorque) - 7.938807578f * (statTorque * statTorque) + 3271.529874f * statTorque - 1282.489182f;
        else realTorque = 2868.12323896f * statTorque;

        //Calcul du frein moteur
        realDisplacement = 2.19661982f * statDisplacement;

        //Calcul du maniement
        if (statHandling >= 50) realHandling = -201.0846980636f + 55.6008637377f * Mathf.Log(statHandling);
        else realHandling = 0.32854319999915954f * statHandling;

        //Calcul des freins
        if (statBrake >= 50) realBrake = -8963.1752480886f + 5091.5895510969f * Mathf.Log(statBrake);
        else realBrake = 219.10480420000079f * statBrake;

        //Calcul de la vitesse de retour du volant
        if (statHandling >= 50) steeringComeBackSpeed = 1 - 2.0484550067f + 0.6514417229f * Mathf.Log(statHandling);
        else steeringComeBackSpeed = 1.3f;

        //Calcul de la force de la gravite sur le vehicule
        downwardForce = 0.981f * realWeight;
    }

    /// <summary>
    /// Permet de gerer les effets de la gravite sur notre vehicule si on est pas au sol
    /// </summary>
    private void GravityHandler()
    {
        GatherWheelContacts();
        CalculateGravity();
    }

    /// <summary>
    /// On recupere les informations sur les roues qui sont au sol, et leur type
    /// </summary>
    private void GatherWheelContacts()
    {
        //On ordonne le calcul
        wheelsContact.ContactEffects();

        //Si on a pas toutes les roues motrices au sol, on ira pas "tout droit"
        if (forwardScalar != wheelsContact.GetForwardScalar())
        {
            forwardScalar = wheelsContact.GetForwardScalar();
            forwardScalarVector = new Vector3(0.7f + 0.3f * (1 - Mathf.Abs(forwardScalar)), 0f, 0.3f * Mathf.Abs(forwardScalar) * Mathf.Sign(forwardScalar));
            forwardScalarVector.Normalize();
        }

        //On recupere les autres scalaires pour appliquer la puissance et le volant
        torqueScalar = wheelsContact.GetTorqueMultiplier();
        steeringScalar = wheelsContact.GetTurningScalar();

        //On apprend quelle roue sont au sol, ou non
        onTheGround = wheelsContact.IsOnTheGround();
        drivingOnTheGround = wheelsContact.IsDrivingOnTheGround();
        steeringOnTheGround = wheelsContact.IsSteeringOnTheGround();
    }

    /// <summary>
    /// Envoie le vehicule au sol si on est en l'air
    /// </summary>
    private void CalculateGravity()
    {
        if (onTheGround) downwardMovement = 0f;
        else downwardMovement -= downwardForce * Time.fixedDeltaTime;
    }

    /// <summary>
    /// Permet de gerer les pedales du vehicule a partir des inputs du joueur
    /// </summary>
    private void PedalsHandler()
    {
        //Si les roues motriches sont pas au sol, forcement on ralentit sans pouvoir utiliser les pedales
        if (!drivingOnTheGround)
        {
            Decelerate(false);
        }
        //Sinon, le comportement est normal
        else
        {
            //Si on est pas en train de reculer, on a le fonctionnement normal
            if (!isReversing)
            {
                //Est-ce qu'il est temps de changer de vitesse ?
                if ((currentGear < maxGear - 1 && currentRpm + gainedRpm >= maxRpm) || (currentGear != 0 && currentSpeed < gearThresholds[currentGear - 1])) StartCoroutine(GearShift());

                //On commence par choisir la bonne fonction pour gerer notre vitesse
                if (pedalInput > 0 && !handbrakeInput)
                {
                    if (isClutchEngaged) Accelerate();
                    else Decelerate(false);
                }
                else if (currentSpeed > 0)
                {
                    if (pedalInput < 0) Decelerate(true);
                    else Decelerate(false);
                }
                //Si on a pas d'input pour acceler et qu'on est a l'arret, peut etre que le joueur veut reculer ?
                else if (currentSpeed == 0 && pedalInput < 0 && !handbrakeInput && !isStartingReverse) StartCoroutine(ReverseShift());
            }
            //Sinon, on a un fonctionnement inverse etrange
            else
            {
                //Est-ce qu'on a remplit les conditions pour repasser en marche avant ?
                if (currentSpeed == 0 && pedalInput >= 0 && !handbrakeInput)
                {
                    isReversing = false;
                    StartCoroutine(GearShift());
                }

                //On oublie que ca fonctionne en inverse par ici
                if (pedalInput < 0 && !handbrakeInput) Accelerate();
                else if (currentSpeed > 0)
                {
                    if (pedalInput > 0) Decelerate(true);
                    else Decelerate(false);
                }
            }
        }

        //On applique les changements de vitesse
        ApplySpeed();
    }

    /// <summary>
    /// Permet de gerer l'acceleration de notre vehicule
    /// </summary>
    private void Accelerate()
    {
        //On commence par augmenter la vitesse moteur
        if(!isReversing) gainedRpm = Time.fixedDeltaTime * ((realTorque * gearBox[currentGear]) / (realWeight * 1 * 1));
        else gainedRpm = Time.fixedDeltaTime * ((realTorque * (gearBox[0] + 1)) / (realWeight * 1 * 1));
        gainedRpm *= torqueScalar;
        //On fait un check pour s'assurer qu'on respecte la realite
        if (currentRpm + gainedRpm > maxRpm) currentRpm = maxRpm;
        else currentRpm += gainedRpm;

        //On peut maintenant deduire la vitesse du vehicule
        CalculateSpeedFromRpm();
    }

    /// <summary>
    /// Permet de gerer la deceleration de notre vehicule
    /// </summary>
    /// <param name="braking">Est-ce qu'on utilise notre frein (true) ou juste notre frein moteur (false)</param>
    private void Decelerate(bool braking)
    {
        //On diminue la vitesse du vehicule
        if (!isReversing) lostSpeed = Time.fixedDeltaTime * (((realDisplacement * gearBox[currentGear] + realBrake * Convert.ToSingle(braking)) * 1) / (realWeight * 1));
        else lostSpeed = Time.fixedDeltaTime * (((realDisplacement * (gearBox[0] + 1) + realBrake * Convert.ToSingle(braking)) * 1) / (realWeight * 1));
        //On fait un check pour s'assurer qu'on respecte la realite
        if (currentSpeed - lostSpeed < 0) currentSpeed = 0;
        else currentSpeed -= lostSpeed;

        //On peut maintenant deduire la vitesse du moteur
        CalculateRpmFromSpeed();
    }

    /// <summary>
    /// Permet de calculer la vitesse dans l'air a partir de la vitesse du vehicule
    /// </summary>
    private void CalculateSpeedFromRpm()
    {
        if (!isReversing) currentSpeed = (currentRpm / maxRpm) * (realPower / gearBox[currentGear]);
        else currentSpeed = (currentRpm / maxRpm) * (realPower / (gearBox[0] + 1));
    }

    /// <summary>
    /// Permet de calculer la vitesse moteur a partir de la vitesse dans l'air
    /// </summary>
    private void CalculateRpmFromSpeed()
    {
        if (!isReversing) currentRpm = (currentSpeed * maxRpm * gearBox[currentGear]) / realPower;
        else currentRpm = (currentSpeed * maxRpm * (gearBox[0] + 1)) / realPower;
    }

    /// <summary>
    /// Applique notre vitesse actuelle au vehicule
    /// </summary>
    private void ApplySpeed()
    {
        if (!isReversing) playerRigidbody.velocity = transform.forward * currentSpeed * unitySpeedScalar * Time.fixedDeltaTime * forwardScalarVector.x
                + Vector3.up * downwardMovement * Time.fixedDeltaTime
                + transform.right * currentSpeed * unitySpeedScalar * Time.fixedDeltaTime * forwardScalarVector.z;

        else playerRigidbody.velocity = -transform.forward * currentSpeed * unitySpeedScalar * Time.fixedDeltaTime * forwardScalarVector.x
                + Vector3.up * downwardMovement * Time.fixedDeltaTime
                + transform.right * currentSpeed * unitySpeedScalar * Time.fixedDeltaTime * forwardScalarVector.z;
    }

    /// <summary>
    /// Permet de transformer les inputs volant de l'utilisateur en rotation pour le vehicule
    /// </summary>
    private void SteeringWheelHandler()
    {
        CalculateSteeringAngle();
        CalculateWheelsAngle();
        //On ne peut appliquer les changements du volant que si les roues directrices sont au sol
        if(steeringOnTheGround) ApplySteering();
    }

    /// <summary>
    /// Permet de calculer l'angle du volant a partir de l'input utilisateur
    /// </summary>
    private void CalculateSteeringAngle()
    {
        //On commence par aider le volant a retourner vers sa position neutre (si necessaire)
        if ((steeringWheelInput == 0 && steeringAngle != 0) || (steeringWheelInput != 0 && Mathf.Sign(steeringWheelInput) != Mathf.Sign(steeringAngle)))
        {
            //Si on passe de l'autre cote de zero, on se force a zero
            if (Mathf.Sign(steeringAngle) != Mathf.Sign(steeringAngle + Time.fixedDeltaTime * steeringComeBackSpeed * -Mathf.Sign(steeringAngle))) steeringAngle = 0;
            else steeringAngle += Time.fixedDeltaTime * steeringComeBackSpeed * -Mathf.Sign(steeringAngle);
        }

        //Ensuite, on rajoute l'ordre de l'utilisateur
        if (steeringWheelInput != 0 && Mathf.Abs(steeringAngle) != 1)
        {
            steeringAngle += Time.fixedDeltaTime * steeringTurnSpeed * steeringWheelInput;
            //On s'assure qu'on respecte les bornes du volant
            if (Mathf.Abs(steeringAngle) > 1)
            {
                if (steeringAngle > 1) steeringAngle = 1;
                else if (steeringAngle < -1) steeringAngle = -1;
            }
        }
    }

    /// <summary>
    /// Permet de calculer l'angle des roues a partir de l'angle du volant
    /// </summary>
    private void CalculateWheelsAngle()
    {
        if (currentSpeed < 10) wheelsAngle = (steeringAngle * realHandling + (steeringScalar / 3.6f) * realHandling) * (currentSpeed / 10f);
        else wheelsAngle = steeringAngle * realHandling + (steeringScalar / 3.6f) * realHandling;
    }

    /// <summary>
    /// Applique les instructions du volant sur le vehicule
    /// </summary>
    private void ApplySteering()
    {
        if (!isReversing) transform.Rotate(Vector3.up, wheelsAngle * Time.fixedDeltaTime);
        else transform.Rotate(Vector3.up, -wheelsAngle * Time.fixedDeltaTime);
    }
    #endregion

    #region Coroutines
    /// <summary>
    /// Debraye, puis trouve la meilleur vitesse pour notre vehicule
    /// </summary>
    IEnumerator GearShift()
    {
        //On commence par debrayer
        isClutchEngaged = false;

        //On change de ratio
        yield return new WaitForSeconds(gearSwitchTime);

        //On determine quel ratio on vient de passer
        currentGear = 0;
        while (gearThresholds[currentGear] < currentSpeed) currentGear++;

        //On peut enfin embrayer et appliquer les bons rpm en fonction de notre vitesse
        CalculateRpmFromSpeed();
        isClutchEngaged = true;
    }

    /// <summary>
    /// Verifie que le joueur est en train d'essayer de passer une marche arriere
    /// </summary>
    IEnumerator ReverseShift()
    {
        //On commence le check et on debraye
        isStartingReverse = true;
        isClutchEngaged = false;

        //On s'assure qu'on maintient les inputs assez longtemps
        reverseTimer = 0;
        do
        {
            yield return null;
            reverseTimer += Time.deltaTime;
        } while (reverseTimer < gearSwitchTime && pedalInput < 0 && !handbrakeInput);
        if (reverseTimer >= gearSwitchTime) isReversing = true;

        //Une fois qu'on a fini, on arrete le check et embraye
        isClutchEngaged = true;
        isStartingReverse = false;
    }
    #endregion
}

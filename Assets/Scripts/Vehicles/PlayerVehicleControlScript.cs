using UnityEngine;
using System.Collections;
using System;

public class PlayerVehicleControlScript : MonoBehaviour
{
    #region Variables
    private Rigidbody playerRigidbody; //Le rigidbody de notre vehicule

    private float pedalInput = 0f, steeringWheelInput = 0f; //Les inputs qui sont passes a notre joueur

    private float statPower = 69, realPower; //La puissance annoncee au joueur, et la puissance utilisee par ce script
    private float statTorque = 108, realTorque; //La puissance annoncee au joueur, et la puissance utilisee par ce script
    private float statDisplacement = 1397, realDisplacement; //La cylindree au joueur, et la cylindree utilisee par ce script
    private float statBrake = 66, realBrake; //Les freins annoncee au joueur, et les freins utilises par ce script
    private float statHandling = 115, realHandling; //Le maniement annonce au joueur, et le maniement utilise par ce script
    private float realWeight = 962; //La seule stat du joueur qui ne demande pas d'etre modifiee

    private const float unitySpeedScalar = 13.879442142f; //Pour que la puissance correspondate vraiment a la vitesse maximale
    private const float maxRpm = 6000; //Les tours maximum pour le vilebrequin

    private bool isClutchEngaged = true; //Est-ce qu'on est embraye ou pas ?
    private float[] gearBox; //Les differents ratio dans la boite de vitesse
    private float[] gearThresholds; //La vitesse de chaque ratio a 5000rpm
    private int currentGear = 0, maxGear; //Le ratio actuellement selectionne, et le nombre de ratio dans la boite

    private float currentSpeed = 0, lostSpeed; //La vitesse (axe Z) actuelle du vehicule, et la vitesse qu'on perd lorsqu'on accelere pas
    private float currentRpm = 0, gainedRpm = 0; //Les tours actuels du vilebrequin, et les tours qu'on va gagner durant l'update actuelle

    private float steeringAngle = 0, wheelsAngle = 0; //L'angle du volant, et l'angle resultat pour les roues du vehicule
    private float steeringTurnSpeed = 2, steeringComeBackSpeed = 4; //La vitesse a laquelle le volant atteint son maximum, et la vitesse a laquelle le volant retourne a son zero
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

    #region FixedUpdate
    private void FixedUpdate()
    {
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
    public void ReceiveInputs(float pI, float sWI)
    {
        pedalInput = pI;
        steeringWheelInput = sWI;
    }

    public int GetCurrentSpeedInt() => (int)currentSpeed;
    public int GetCurrentRpm() => (int)currentRpm;
    public int GetCurrentGear() => currentGear;
    public float GetRealPower() => (int)realPower;
    public float GetCurrentSpeedFloat() => currentSpeed;
    #endregion

    #region Private_Methods
    /// <summary>
    /// Permet de recuperer les differents components essentiels
    /// </summary>
    private void GatherComponents()
    {
        //On recupere le rigidbody
        playerRigidbody = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Permet de caculer les variables utiles pour le script a partir des valeurs annoncees au joueur
    /// </summary>
    private void CalculateVariables()
    {
        //Initialisation de la boite de vitesse
        gearBox = new float[] { 2.5f, 1.6f, 1.25f, 1f };
        maxGear = gearBox.Length;

        //Calcul de la puissance reelle
        if (statPower / gearBox[maxGear - 1] >= 50) realPower = -64.1985918658f + 49.999493352f * Mathf.Log(statPower / gearBox[maxGear - 1]);
        else realPower = 2.628011527739513f * (statPower / gearBox[maxGear - 1]);

        //On utilise les deux etapes precedents pour calculer les seuils de la boite de vitesse
        gearThresholds = new float[maxGear];
        for (int i = 0; i < maxGear; i++) gearThresholds[i] = (5500f / maxRpm) * (realPower / gearBox[i]);

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
    }

    /// <summary>
    /// Permet de gerer les pedales du vehicule a partir des inputs du joueur
    /// </summary>
    private void PedalsHandler()
    {
        //Est-ce qu'il est temps de changer de vitesse ?
        if ((currentGear < maxGear - 1 && currentRpm + gainedRpm >= maxRpm) || (currentGear != 0 && currentSpeed < gearThresholds[currentGear - 1])) StartCoroutine(GearShift());

        //On commence par choisir la bonne fonction pour gerer notre vitesse
        if (pedalInput > 0)
        {
            if (isClutchEngaged) Accelerate();
            else Decelerate(false);
        }
        else if (currentSpeed > 0)
        {
            if (pedalInput < 0) Decelerate(true);
            else Decelerate(false);
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
        gainedRpm = Time.fixedDeltaTime * ((realTorque * gearBox[currentGear]) / (realWeight * 1 * 1));
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
        lostSpeed = Time.fixedDeltaTime * (((realDisplacement * gearBox[currentGear] + realBrake * Convert.ToSingle(braking)) * 1) / (realWeight * 1));
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
        currentSpeed = (currentRpm / maxRpm) * (realPower / gearBox[currentGear]);
    }

    /// <summary>
    /// Permet de calculer la vitesse moteur a partir de la vitesse dans l'air
    /// </summary>
    private void CalculateRpmFromSpeed()
    {
        currentRpm = (currentSpeed * maxRpm * gearBox[currentGear]) / realPower;
    }

    /// <summary>
    /// Applique notre vitesse actuelle au vehicule
    /// </summary>
    private void ApplySpeed()
    {
        playerRigidbody.velocity = transform.forward * currentSpeed * unitySpeedScalar * Time.fixedDeltaTime;
    }

    /// <summary>
    /// Permet de transformer les inputs volant de l'utilisateur en rotation pour le vehicule
    /// </summary>
    private void SteeringWheelHandler()
    {
        CalculateSteeringAngle();
        CalculateWheelsAngle();
        ApplySteering();
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
        if (Mathf.Abs(steeringAngle) < 1)
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
        if (currentSpeed < 10) wheelsAngle = steeringAngle * realHandling * (currentSpeed / 10);
        else wheelsAngle = steeringAngle * realHandling;
    }

    /// <summary>
    /// Applique les instructions du volant sur le vehicule
    /// </summary>
    private void ApplySteering()
    {
        transform.Rotate(Vector3.up, wheelsAngle * Time.fixedDeltaTime);
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
        yield return new WaitForSeconds(0.25f);

        //On determine quel ratio on vient de passer
        currentGear = 0;
        while (gearThresholds[currentGear] < currentSpeed) currentGear++;

        //On peut enfin embrayer et appliquer les bons rpm en fonction de notre vitesse
        CalculateRpmFromSpeed();
        isClutchEngaged = true;
    }
    #endregion
}

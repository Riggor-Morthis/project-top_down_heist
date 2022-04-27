using UnityEngine;
using TMPro;
using System.Collections;

public class VehicleUIScript : MonoBehaviour
{
    #region Variables
    [SerializeField, Tooltip("Le champ pour afficher nos centaines actuelles")]
    private TextMeshProUGUI uiSpeedHundreds;
    [SerializeField, Tooltip("Le champ pour afficher nos dizaines actuelles")]
    private TextMeshProUGUI uiSpeedTens;
    [SerializeField, Tooltip("Le champ pour afficher nos unites actuelles")]
    private TextMeshProUGUI uiSpeedOnes;
    [SerializeField, Tooltip("Le champ pour afficher notre ratio actuel")]
    private TextMeshProUGUI uiGear;
    [SerializeField, Tooltip("La couleur utilisee par l'ui pour indiquer une vitesse embrayee")]
    private Color clutchedGear;
    [SerializeField, Tooltip("La couleur utilisee par l'ui pour indiquer une vitesse debrayee")]
    private Color declutchedGear;
    [SerializeField, Tooltip("L'aiguille pour le carburant")]
    private GameObject arrowFuelGauge;
    [SerializeField, Tooltip("L'aiguille pour la vitesse dans l'air")]
    private GameObject arrowSpeedGauge;
    [SerializeField, Tooltip("L'aiguille pour la vitesse moteur")]
    private GameObject arrowRpmGauge;

    private VehicleControlScript playerVehicle; //Le vehicule du joueur, dont on affiche les informations

    private int playerSpeed, playerMaxSpeed; //La vitesse dans l'air du joueur, et la vitesse maximale dans l'air du joueur
    private int playerSpeedHundreds, playerSpeedTens, playerSpeedOnes; //La decomposition des puissances de 10 de la vitesse dans l'air
    private int playerRpm; //La vitesse moteur du joueur
    private int playerGear; //La vitesse de la boite du joueur
    private bool isPlayerClutched; //Est-ce que le joueur est embraye ?
    private Vector3 positionStart; //La position locale de depart pour le tableau de bord
    private int positionOffsetX = 0, positionOffsetY = 0; //De combien on decale ce tableau de bord a chaque fois
    private float positionDeltaX = 0, positionDeltaY = 0; //Pour s'assurer que les calculs restent vrais pour toutes les tailles d'ecrans
    private bool isSwayStarted = false; //Est-ce qu'on a commence le mouvement du tableau de bord ?
    private float currentAngle = 0, targetAngle; //Les angles actuel et vise par l'aiguile de carburant
    private float arrowAngleSpeed = 2f; //La vitesse de rotation de l'aiguille de carburant
    #endregion

    #region Awake
    private void Awake()
    {
        //On recupere le component dont on a besoin
        playerVehicle = FindObjectOfType<VehicleControlScript>();

        //On demarre la pompe a carburant
        StartCoroutine(FuelGauge());
    }
    #endregion

    #region Update
    private void LateUpdate()
    {
        //On recupere les infos du joueur
        GatherInformations();
        //On affiche les infos du joueur
        UpdateUI();

        //On demarre le mouvement si on n'est pas a l'arret et si il est pas encore demarre
        if (!isSwayStarted && playerSpeed > 0) StartTheSway();
    }
    #endregion

    #region Private_Methods
    /// <summary>
    /// Utilisee pour recuperer les infos du joueur grace a ses getters
    /// </summary>
    private void GatherInformations()
    {
        playerSpeed = playerVehicle.GetCurrentSpeedInt();
        playerMaxSpeed = Mathf.RoundToInt(playerVehicle.GetRealPower());
        playerRpm = playerVehicle.GetCurrentRpm();
        playerGear = playerVehicle.GetCurrentGear();
        isPlayerClutched = playerVehicle.GetIsClutchEngaged();
    }

    /// <summary>
    /// Changes les valeurs des champs de l'UI en fonction des valeurs du joueur
    /// </summary>
    private void UpdateUI()
    {
        //On met a jour la vitesse dans l'air affichee
        UpdateSpeed();
        //On met a jour le ratio
        UpdateGears();
        //On met a jour la vitesse moteur affichee
        UpdateRPM();
    }

    private void UpdateSpeed()
    {
        //On calcule chacun des champs separement
        playerSpeedHundreds = (int)(playerSpeed / 100f);
        playerSpeedTens = (int)(playerSpeed / 10f - playerSpeedHundreds * 10);
        playerSpeedOnes = (int)(playerSpeed - playerSpeedTens * 10f - playerSpeedHundreds * 100f);

        //Il est temps de mettre l'UI a jour, a commencer par les nombres
        uiSpeedHundreds.text = playerSpeedHundreds.ToString();
        uiSpeedTens.text = playerSpeedTens.ToString();
        uiSpeedOnes.text = playerSpeedOnes.ToString();
        //Et maintenant l'aiguille
        arrowSpeedGauge.transform.rotation = Quaternion.Euler(0f, 0f, ((float)playerSpeed / (float)playerMaxSpeed) * -180);
    }

    /// <summary>
    /// Permet d'afficher le bon ration dans la bonne couleur
    /// </summary>
    private void UpdateGears()
    {
        //On affiche la bonne chose a l'ecran
        uiGear.text = (playerGear + 1).ToString();

        //On change la couleur si necessaire
        if (isPlayerClutched && uiGear.color != clutchedGear) uiGear.color = clutchedGear;
        else if (!isPlayerClutched && uiGear.color != declutchedGear) uiGear.color = declutchedGear;
    }

    /// <summary>
    /// Permet de mettre a jour le cadran de la vitesse moteur
    /// </summary>
    private void UpdateRPM()
    {
        //On considere qu'on a un ralenti de 800 tr/min
        if (playerRpm < 800) playerRpm = 800;
        //On met a jour l'aiguille
        arrowRpmGauge.transform.rotation = Quaternion.Euler(0f, 0f, (playerRpm / 6000f) * -180);
    }

    /// <summary>
    /// Permet de commencer le mouvement du tableau de bord
    /// </summary>
    private void StartTheSway()
    {
        //On pourra pas (re)commener le sway
        isSwayStarted = true;

        //On commence par initialiser les valeurs de depart si necessaire
        if(positionDeltaX == 0 && positionDeltaY == 0)
        {
            positionStart = transform.position;
            positionDeltaX = Screen.width / 864f;
            positionDeltaY = Screen.height / 1008f;
        }
        
        //Puis on peut lancer la coroutine recursive qui gere le mouvement du tableau de bord
        StartCoroutine(Sway(.1f));
    }
    #endregion

    #region Coroutines
    /// <summary>
    /// Permet de faire bouger le tableau de bord aleatoirement
    /// </summary>
    /// <param name="time">Combien de temps avant le prochain mouvement</param>
    IEnumerator Sway(float time)
    {
        //On commence par attendre une duree determinee
        yield return new WaitForSeconds(time);

        //On calcule de combien on veut decaler le tableau de bord (on a des securites en place)
        positionOffsetX += Mathf.RoundToInt(Random.Range(-1f, 1f));
        positionOffsetY += Mathf.RoundToInt(Random.Range(-1f, 1f));
        if (positionOffsetX > 2) positionOffsetX = 1;
        else if (positionOffsetX < -2) positionOffsetX = -1;
        if (positionOffsetY > 2) positionOffsetY = 1;
        else if (positionOffsetY < -2) positionOffsetY = -1;

        //On bouge le tableau de bord et on lance le mouvement suivant si on de la vitesse
        transform.position = new Vector3(positionStart.x + (4 * positionOffsetX) * positionDeltaX, positionStart.y + (4 * positionOffsetY) * positionDeltaY, positionStart.z);
        if (playerSpeed > 0) StartCoroutine(Sway(Random.Range(1f - (float)playerSpeed / (float)playerMaxSpeed, 2.2f - ((float)playerSpeed / (float)playerMaxSpeed) * 2)));
        else isSwayStarted = false;
    }

    /// <summary>
    /// Permet de faire bouger l'aiguille de carburant aleatoirement
    /// </summary>
    IEnumerator FuelGauge()
    {
        //On choisit ou on va
        targetAngle = Random.Range(-5f, -175f);

        //Tant qu'on y est pas, on continue d'avancer
        while(Mathf.Abs(targetAngle - currentAngle) > arrowAngleSpeed * Time.deltaTime)
        {
            currentAngle += Mathf.Sign(targetAngle - currentAngle) * arrowAngleSpeed * Time.deltaTime;
            arrowFuelGauge.transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);
            yield return null;
        }

        //Une fois qu'on y est, on recommence
        StartCoroutine(FuelGauge());
    }
    #endregion
}

using UnityEngine;
using TMPro;

public class VehicleUIScript : MonoBehaviour
{
    #region Variables
    [SerializeField, Tooltip("Le champ pour afficher notre vitesse actuelle")]
    private TextMeshProUGUI uiSpeed;
    [SerializeField, Tooltip("Le champ pour afficher notre ratio actuel")]
    private TextMeshProUGUI uiRpm;
    [SerializeField, Tooltip("Le champ pour afficher notre ratio actuel")]
    private TextMeshProUGUI uiGear;

    private VehicleControlScript playerVehicle; //Le vehicule du joueur, dont on affiche les informations

    private int playerSpeed; //La vitesse dans l'air du joueur
    private int playerRpm; //La vitesse moteur du joueur
    private int playerGear; //La vitesse de la boite du joueur
    #endregion

    #region Awake
    private void Awake()
    {
        //On recupere le component dont on a besoin
        playerVehicle = FindObjectOfType<VehicleControlScript>();
    }
    #endregion

    #region Update
    private void LateUpdate()
    {
        //On recupere les infos du joueur
        GatherInformations();
        //On affiche les infos du joueur
        UpdateUI();
    }
    #endregion

    #region Private_Methods
    /// <summary>
    /// Utilisee pour recuperer les infos du joueur grace a ses getters
    /// </summary>
    private void GatherInformations()
    {
        playerSpeed = playerVehicle.GetCurrentSpeedInt();
        playerRpm = playerVehicle.GetCurrentRpm();
        playerGear = playerVehicle.GetCurrentGear();
    }

    /// <summary>
    /// Changes les valeurs des champs de l'UI en fonction des valeurs du joueur
    /// </summary>
    private void UpdateUI()
    {
        uiSpeed.text = playerSpeed.ToString() + "km/h";
        uiRpm.text = playerRpm.ToString();
        uiGear.text = (playerGear + 1).ToString();
    }
    #endregion
}

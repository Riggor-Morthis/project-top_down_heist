using UnityEngine;
using TMPro;

public class SpeedGaugeUIScript : MonoBehaviour
{
    #region Variables
    [SerializeField, Tooltip("Le champ pour afficher nos centaines actuelles")]
    private TextMeshProUGUI uiSpeedHundreds;
    [SerializeField, Tooltip("Le champ pour afficher nos dizaines actuelles")]
    private TextMeshProUGUI uiSpeedTens;
    [SerializeField, Tooltip("Le champ pour afficher nos unites actuelles")]
    private TextMeshProUGUI uiSpeedOnes;

    private VehicleControlScript playerVehicle; //Le vehicule du joueur, dont on affiche les informations

    private int playerSpeed, playerMaxSpeed; //La vitesse dans l'air du joueur, et la vitesse maximale dans l'air du joueur
    private int playerSpeedHundreds, playerSpeedTens, playerSpeedOnes; //La decomposition des puissances de 10 de la vitesse dans l'air
    #endregion

    #region Start
    private void Start()
    {
        //On recupere le component et les variables dont on a besoin
        playerVehicle = FindObjectOfType<VehicleControlScript>();
        playerMaxSpeed = Mathf.RoundToInt(playerVehicle.GetRealPower());
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
    }

    /// <summary>
    /// Permet d'afficher la vitesse dans l'air du vehicule
    /// </summary>
    private void UpdateUI()
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
        transform.rotation = Quaternion.Euler(0f, 0f, ((float)playerSpeed / (float)playerMaxSpeed) * -180f);
    }
    #endregion
}

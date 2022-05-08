using UnityEngine;
using TMPro;

public class RpmGaugeScript : MonoBehaviour
{
    #region Variables
    [SerializeField, Tooltip("Le champ pour afficher notre ratio actuel")]
    private TextMeshProUGUI uiGear;
    [SerializeField, Tooltip("La couleur utilisee par l'ui pour indiquer une vitesse embrayee")]
    private Color clutchedGear;
    [SerializeField, Tooltip("La couleur utilisee par l'ui pour indiquer une vitesse debrayee")]
    private Color declutchedGear;

    private VehicleControlScript playerVehicle; //Le vehicule du joueur, dont on affiche les informations

    private int playerRpm, falsifiedRpm, addedRpm; //La vitesse moteur du joueur, la vitesse qu'on affiche (potentiellement falsifiee), et enfin les modifications en terme de Rpm a cette frame
    private bool deSynchro = false; //Est-ce que les rpm affiches ne correspondent plus aux rpm reels ?
    private int arrowSpeed = 6000; //La vitesse a laquelle l'aiguille se deplace lorsqu'on est en desynchro
    private int playerGear; //La vitesse de la boite du joueur
    private bool isPlayerClutched; //Est-ce que le joueur est embraye ?
    private bool isPlayerReversing; //Est-ce que le joueur est en marche arriere ?
    #endregion

    #region Start
    private void Start()
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
        UpdateGear();
        UpdateRpm();
    }
    #endregion

    #region Private_Methods
    /// <summary>
    /// Utilisee pour recuperer les infos du joueur grace a ses getters
    /// </summary>
    private void GatherInformations()
    {
        playerRpm = playerVehicle.GetCurrentRpm();
        playerGear = playerVehicle.GetCurrentGear();
        isPlayerClutched = playerVehicle.GetIsClutchEngaged();
        isPlayerReversing = playerVehicle.GetIsReversing();
    }

    /// <summary>
    /// Permet d'afficher le bon ration dans la bonne couleur
    /// </summary>
    private void UpdateGear()
    {
        //On affiche la bonne chose a l'ecran
        if (!isPlayerReversing) uiGear.text = (playerGear + 1).ToString();
        else uiGear.text = "R";

        //On change la couleur si necessaire
        if (isPlayerClutched && uiGear.color != clutchedGear) uiGear.color = clutchedGear;
        else if (!isPlayerClutched && uiGear.color != declutchedGear) uiGear.color = declutchedGear;
    }

    /// <summary>
    /// Permet d'afficher la bonne vitesse moteur, corrigee si necessaire
    /// </summary>
    private void UpdateRpm()
    {
        if (deSynchro)
        {
            if(playerRpm < 800 && falsifiedRpm < 800)
            {
                deSynchro = false;
                falsifiedRpm = 800;
            }
            else
            {
                addedRpm = Mathf.RoundToInt(Mathf.Sign(playerRpm - falsifiedRpm) * arrowSpeed * Time.deltaTime);
                if (Mathf.Abs(playerRpm - falsifiedRpm) < Mathf.Abs(addedRpm))
                {
                    deSynchro = false;
                    falsifiedRpm = playerRpm;
                }
                else falsifiedRpm += addedRpm;
            }
        }
        else
        {
            //On considere qu'on a un ralenti de 800 tr/min
            if (playerRpm < 800) falsifiedRpm = 800;
            //Si on a plus de 5 degres de difference entre les "nouveaux" rpm et les "anciens rpm", on declence une desynchro
            else if (Mathf.Abs(((falsifiedRpm / 6000f) * -180) - ((playerRpm / 6000f) * -180)) > 5f) deSynchro = true;
            else falsifiedRpm = playerRpm;
        }

        //On met a jour l'aiguille
        transform.rotation = Quaternion.Euler(0f, 0f, (falsifiedRpm / 6000f) * -180);
    }
    #endregion
}

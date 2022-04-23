using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InVehicleCameraScript : MonoBehaviour
{
    #region Variables
    private PlayerVehicleControlScript playerVehicle; //Le vehicule actuel du joueur

    private float playerMaxSpeed, playerCurrentSpeed; //La vitesse maximale du vehicule, la vitesse actuelle du joueur
    private float playerSpeedFraction; //Le pourcentage (entre 0 et 1) entre vitesse max et vitesse actuelle
    private float yMinimum = 10, yOffset = 20, zMinimum = 0, zOffset = 11; //La position de depart en y et z, et le maximum atteignable via offset
    #endregion

    #region Start
    private void Start()
    {
        GatherComponent();
        GatherMaxSpeed();
    }
    #endregion

    #region Update
    private void LateUpdate()
    {
        //On recupere des infos a jour sur le joueur
        GatherCurrentSpeed();
        //On met a jour la position de la camera
        UpdateCam();
    }
    #endregion

    #region Private_Methods
    /// <summary>
    /// Permet de recuperer le script de mouvement du vehicule joueur
    /// </summary>
    private void GatherComponent()
    {
        playerVehicle = gameObject.GetComponentInParent<PlayerVehicleControlScript>();
    }

    /// <summary>
    /// Permet de recuperer la vitesse maximale du joueur
    /// </summary>
    private void GatherMaxSpeed()
    {
        playerMaxSpeed = playerVehicle.GetRealPower();
    }

    /// <summary>
    /// Permet de recuperer la vitesse actuelle du joueur
    /// </summary>
    private void GatherCurrentSpeed()
    {
        playerCurrentSpeed = playerVehicle.GetCurrentSpeedFloat();
    }

    /// <summary>
    /// Permet de mettre a jour la position de la camera
    /// </summary>
    private void UpdateCam()
    {
        //On calcule un pourcentage pour l'offset, puis on l'applique
        playerSpeedFraction = Mathf.Log(1 + (playerCurrentSpeed / playerMaxSpeed), 2);
        transform.localPosition = new Vector3(0f, yMinimum + playerSpeedFraction * yOffset, zMinimum + playerSpeedFraction * zOffset);
    }
    #endregion
}

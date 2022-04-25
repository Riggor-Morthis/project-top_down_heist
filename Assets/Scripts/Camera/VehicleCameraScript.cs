using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleCameraScript : MonoBehaviour
{
    #region Variables
    [SerializeField, Tooltip("Le vehicule actuel du joueur")]
    private VehicleControlScript playerVehicle;

    private float playerMaxSpeed, playerCurrentSpeed; //La vitesse maximale du vehicule, la vitesse actuelle du joueur
    private float playerSpeedFraction; //Le pourcentage (entre 0 et 1) entre vitesse max et vitesse actuelle
    private const float yMinimum = 10, yOffset = 10, zMinimum = 1.5f, zOffset = 5.5f; //La position de depart en y et z, et le maximum atteignable via offset
    private float yCurrentOffset, zCurrentOffset; //L'offset qui doit etre actuellement applique a notre camera, en y et z respectivement
    private Vector3 playerPosition; //La position actuelle du joueur, et son angle actuel
    private float playerYAngle;// L'angle actuel du joueur selon l'axe Y (le seul angle qui nous interesse)
    private Vector3 cameraForward; // La direction "Avant" actuelle de notre camera
    #endregion

    #region Start
    private void Start()
    {
        //On recupere des infos a jour sur le joueur
        GatherMaxSpeed();

        //On met a jour la position de la camera
        UpdateCam();
    }
    #endregion

    #region Update
    private void LateUpdate()
    {
        //On met a jour la position de la camera
        UpdateCam();
    }
    #endregion

    #region Private_Methods
    /// <summary>
    /// Permet de recuperer la vitesse maximale du joueur
    /// </summary>
    private void GatherMaxSpeed()
    {
        playerMaxSpeed = playerVehicle.GetRealPower();
    }

    private void UpdateCam()
    {
        GatherCurrentSpeed();
        GatherCurrentPlayerCoordinates();
        CalculateCurrentOffset();
        UpdateCameraPosition();
    }

    /// <summary>
    /// Permet de recuperer la vitesse actuelle du joueur
    /// </summary>
    private void GatherCurrentSpeed()
    {
        playerCurrentSpeed = playerVehicle.GetCurrentSpeedFloat();
    }

    private void GatherCurrentPlayerCoordinates()
    {
        //On recupere simplement les coordonnees du joueur
        playerPosition = playerVehicle.transform.position;

        //On recupere maintenant son angle, et on calcule notre offset au passage
        playerYAngle = playerVehicle.transform.rotation.eulerAngles.y;
    }

    private void CalculateCurrentOffset()
    {
        playerSpeedFraction = Mathf.Log(0.5f + 1.5f * (playerCurrentSpeed / playerMaxSpeed), 2);
        yCurrentOffset = (1f + playerSpeedFraction) * (yOffset / 2f);
        zCurrentOffset = (1f + playerSpeedFraction) * (zOffset / 2f);
    }

    /// <summary>
    /// Permet de mettre a jour la position de la camera
    /// </summary>
    private void UpdateCameraPosition()
    {
        //On applique la rotation en premier, vu qu'on va vouloir recuperer le .forward pour notre position
        transform.rotation = Quaternion.Euler(0, playerYAngle, 0);

        cameraForward = transform.forward;
        transform.position = playerPosition + new Vector3(cameraForward.x * (zMinimum + zCurrentOffset), yMinimum + yCurrentOffset, cameraForward.z * (zMinimum + zCurrentOffset));
    }
    #endregion
}

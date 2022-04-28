using UnityEngine;
using System.Collections;

public class DashboardUIScript : MonoBehaviour
{
    #region Variables
    private VehicleControlScript playerVehicle; //Le vehicule du joueur, dont on affiche les informations

    private float playerSpeed, playerMaxSpeed; //La vitesse dans l'air du joueur, et la vitesse maximale dans l'air du joueur
    private float playerSpeedRatio; //Le ratio vitesse actuelle par vitesse maximale
    private Vector3 positionStart; //La position locale de depart pour le tableau de bord
    private int positionOffsetX = 0, positionOffsetY = 0; //De combien on decale ce tableau de bord a chaque fois
    private float positionDeltaX = 0, positionDeltaY = 0; //Pour s'assurer que les calculs restent vrais pour toutes les tailles d'ecrans
    private bool isSwayStarted = false; //Est-ce qu'on a commence le mouvement du tableau de bord ?
    #endregion

    #region Start
    private void Start()
    {
        //On recupere le component et les variables dont on a besoin
        playerVehicle = FindObjectOfType<VehicleControlScript>();
        playerMaxSpeed = playerVehicle.GetRealPower();
    }
    #endregion

    #region Update
    private void LateUpdate()
    {
        //On recupere les infos du joueur
        GatherInformations();
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
        playerSpeed = playerVehicle.GetCurrentSpeedFloat();
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

        //On bouge le tableau de bord et on lance le mouvement suivant si on de la vitesse
        if (playerSpeed > 0)
        {
            //On calcule de combien on veut decaler le tableau de bord (on a des securites en place)
            positionOffsetX += Mathf.RoundToInt(Random.Range(-1f, 1f));
            positionOffsetY += Mathf.RoundToInt(Random.Range(-1f, 1f));
            if (positionOffsetX > 2) positionOffsetX = 1;
            else if (positionOffsetX < -2) positionOffsetX = -1;
            if (positionOffsetY > 2) positionOffsetY = 1;
            else if (positionOffsetY < -2) positionOffsetY = -1;

            //On bouge le tableau de bord en consequence
            transform.position = new Vector3(positionStart.x + (4 * positionOffsetX) * positionDeltaX, positionStart.y + (4 * positionOffsetY) * positionDeltaY, positionStart.z);

            //On lance la nouvelle coroutine avec un delai proportionnel a la vitesse
            playerSpeedRatio = playerSpeed / playerMaxSpeed;
            StartCoroutine(Sway(Random.Range(1f - playerSpeedRatio * 0.9f, 2f - playerSpeedRatio * 1.8f)));
        }
        //Sinon on s'arrete
        else isSwayStarted = false;
    }
    #endregion
}

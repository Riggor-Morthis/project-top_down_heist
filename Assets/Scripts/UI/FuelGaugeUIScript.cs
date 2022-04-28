using System.Collections;
using UnityEngine;

public class FuelGaugeUIScript : MonoBehaviour
{
    #region Variables
    private float currentAngle = 0, targetAngle; //Les angles actuel et vise par l'aiguile de carburant
    private float arrowAngleSpeed = 6f; //La vitesse de rotation de l'aiguille de carburant
    #endregion

    #region Start
    private void Start()
    {
        //On demarre la pompe a carburant
        StartCoroutine(FuelGauge());
    }
    #endregion

    #region Coroutines
    /// <summary>
    /// Permet de faire bouger l'aiguille de carburant aleatoirement
    /// </summary>
    IEnumerator FuelGauge()
    {
        //On choisit ou on va
        targetAngle = Random.Range(0f, -180f);

        //Tant qu'on y est pas, on continue d'avancer
        while (Mathf.Abs(targetAngle - currentAngle) > arrowAngleSpeed * Time.deltaTime)
        {
            currentAngle += Mathf.Sign(targetAngle - currentAngle) * arrowAngleSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);
            yield return null;
        }

        //Une fois qu'on y est, on recommence
        StartCoroutine(FuelGauge());
    }
    #endregion
}

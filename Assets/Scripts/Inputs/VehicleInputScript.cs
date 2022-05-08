using UnityEngine;
using UnityEngine.InputSystem;

public class VehicleInputScript : MonoBehaviour
{
    #region Variables
    private float pedalInput = 0f, steeringWheelInput = 0f; //Permet de stocker quelle pedale est appuyee, et dans quel sens on tourne le volant
    private bool handbrakeInput = false; //Est-ce qu'on utlise le frein a main ?

    private VehicleControlScript playerVehicleControl; //Le script pour controler le vehicule du joueur
    #endregion

    #region Awake
    private void Awake()
    {
        playerVehicleControl = GetComponent<VehicleControlScript>();
    }
    #endregion

    #region Update
    private void Update()
    {
        playerVehicleControl.ReceiveInputs(pedalInput, steeringWheelInput, handbrakeInput);
    }
    #endregion

    #region Public_Methods
    /// <summary>
    /// Permet de savoir quelle pedale est appuyee (-1 = frein, 0 = rien, +1 = accelerateur)
    /// </summary>
    public void OnPedals(InputValue iv)
    {
        pedalInput = iv.Get<float>();
    }

    /// <summary>
    /// Permet de savoir vers ou pointe le volant (-1 = gauche, 0 = centre, +1 = droite)
    /// </summary>
    public void OnSteeringWheel(InputValue iv)
    {
        steeringWheelInput = iv.Get<float>();
    }

    /// <summary>
    /// Permet de savoir si on appuie sur le frein a main ou pas
    /// </summary>
    public void OnHandbrake()
    {
        handbrakeInput = !handbrakeInput;
    }
    #endregion
}

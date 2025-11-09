using UnityEngine;
using TMPro;

public class AsignadorDeInputs : MonoBehaviour
{
    [Header("Inputs de la Escena Actual")]
    public TMP_InputField inputNombre;
    public TMP_InputField inputApellidos;
    public TMP_InputField inputTelefono;
    public TMP_InputField inputCorreo; 
    public TMP_InputField inputContrasena;

    void Start()
    {
        // Esto se ejecutará inmediatamente después de que se cargue la escena.
        if (ConectorBD.Instance != null)
        {
            ConectorBD.Instance.SetRegistrationInputReferences(inputNombre,
                                                               inputApellidos,
                                                               inputTelefono,
                                                               inputCorreo,
                                                               inputContrasena);
        }
        else
        {
            Debug.LogError("❌ ConectorBD.Instance es null. Asegúrate de que el Singleton se cargue primero.");
        }
    }
}
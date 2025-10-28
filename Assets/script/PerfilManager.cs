using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class PerfilManager : MonoBehaviour
{
    [Header("Referencias de Inputs de Perfil")]
    public TMP_InputField inputNombre;
    public TMP_InputField inputApellidos;
    public TMP_InputField inputTelefono;
    public TMP_InputField inputCorreo;
    public TMP_InputField inputContrasena;
    
    [Header("Referencias de UI")]
    public Button btnGuardarCambios; 
    public Button btnVolver; 
    public TextMeshProUGUI textoMensaje; 
    
    [Header("Referencias de BD")]
    public ConectorBD dbManager; 
    
    private string correoOriginal; // Guarda el correo original para el UPDATE

    void Start()
    {
        if (dbManager == null)
        {
            dbManager = FindObjectOfType<ConectorBD>();
        }

        if (dbManager == null || string.IsNullOrEmpty(ConectorBD.UsuarioLogueadoCorreo))
        {
            Debug.LogError("No hay usuario logueado. Redirigiendo al Login.");
            SceneManager.LoadScene("LoginScene"); // **IMPORTANTE: Cambia "LoginScene" si tu escena se llama diferente**
            return;
        }

        CargarDatosUsuario(ConectorBD.UsuarioLogueadoCorreo);

        btnGuardarCambios.onClick.AddListener(GuardarCambios); 
        btnVolver.onClick.AddListener(Volver); 
        
        textoMensaje.text = "";
    }

    // --- Carga los datos de la BD y los muestra en los Inputs ---
    void CargarDatosUsuario(string correo)
    {
        Dictionary<string, string> userData = dbManager.GetUserData(correo);
        
        if (userData.Count > 0)
        {
            inputNombre.text = userData["nombre"];
            inputApellidos.text = userData["apellidos"];
            inputTelefono.text = userData["telefono"];
            inputCorreo.text = userData["correo"];
            inputContrasena.text = userData["contrasena"];
            
            correoOriginal = correo; 
        }
        else
        {
            MostrarMensaje("🔴 Error al cargar el perfil.", Color.red);
        }
    }

    // --- Se ejecuta al presionar el botón "Guardar Cambios" (Lógica de UPDATE) ---
    public void GuardarCambios()
    {
        // 1. Obtener los nuevos valores
        string nuevoNombre = inputNombre.text.Trim();
        string nuevoApellido = inputApellidos.text.Trim();
        string nuevoTelefono = inputTelefono.text.Trim();
        string nuevoCorreo = inputCorreo.text.Trim();
        string nuevaContrasena = inputContrasena.text.Trim();
        
        string mensajeBD;

        // 2. Llamar a la función de actualización de la BD
        if (dbManager.UpdateUserData(
            correoOriginal, 
            nuevoNombre, 
            nuevoApellido, 
            nuevoTelefono, 
            nuevoCorreo, 
            nuevaContrasena,
            out mensajeBD) // El mensaje de error/advertencia viene de la BD
        )
        {
            MostrarMensaje("✅ Datos actualizados con éxito.", Color.green);
            
            // Si el correo cambió en la BD, se actualiza el identificador de la sesión actual
            correoOriginal = ConectorBD.UsuarioLogueadoCorreo; 
        }
        else
        {
            // Muestra el error generado por el ConectorBD (incluyendo validación de vacío/formato)
            MostrarMensaje(mensajeBD, Color.red);
        }
    }
    
    public void Volver()
    {
        SceneManager.LoadScene("LoginScene"); // **IMPORTANTE: Cambia por el nombre de tu escena de menú o login**
    }
    
    // --- Utilidad para mostrar mensajes con color ---
    void MostrarMensaje(string mensaje, Color color)
    {
        if (textoMensaje != null)
        {
            textoMensaje.text = mensaje;
            textoMensaje.color = color;
        }
    }
}
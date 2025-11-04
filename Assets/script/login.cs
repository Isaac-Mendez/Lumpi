using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 
// ⚠️ Se eliminan las referencias a MySQL (MySql.Data.MySqlClient)

public class Login : MonoBehaviour
{
    // Las referencias deben ser públicas para enlazarse en el Inspector
    [Header("Referencias UI")]
    // Estos Input Fields capturan los datos de la escena 'entrar'
    public TMP_InputField usuario; // Campo de Correo
    public TMP_InputField contrasena;
    public Button Entrar;
    
    // Campo de mensaje de error específico para esta escena
    public TextMeshProUGUI textoMensajeError; 
    
    void Start()
    {
        // Enlaza el botón para llamar a la función de intento de login
        if (Entrar != null)
        {
            Entrar.onClick.AddListener(AttemptLogin);
        }
        
        // Ocultar el mensaje de error al iniciar la escena
        if (textoMensajeError != null)
        {
            textoMensajeError.gameObject.SetActive(false);
        }
    }

    public void AttemptLogin()
    {
        // 🛑 Paso de verificación crucial para la sesión
        if (ConectorBD.Instance == null)
        {
            Debug.LogError("🔴 Error FATAL: ConectorBD (Singleton) no está inicializado. No se puede hacer login.");
            MostrarError("Error crítico. Reinicie la aplicación.");
            return;
        }

        // 1. Asignar las referencias de esta escena al Singleton.
        // Esto permite que el ConectorBD pueda leer los valores de los Inputs de esta escena.
        ConectorBD.Instance.inputCorreo = usuario;
        ConectorBD.Instance.inputContrasena = contrasena;
        // Asignar el campo de mensaje de error para que el Singleton pueda escribir el error aquí
        ConectorBD.Instance.textoMensajeError = textoMensajeError; 
        
        // 2. Ejecutar la lógica de Login centralizada en el Singleton.
        // El destino después del éxito es la escena que mencionaste: "crearhabitos".
        ConectorBD.Instance.VerificarLoginDesdeFormulario("crearhabitos");
    }
    
    // Función local para mostrar mensajes de error
    void MostrarError(string mensaje)
    {
        if (textoMensajeError != null)
        {
            textoMensajeError.text = mensaje;
            textoMensajeError.gameObject.SetActive(true);
        }
    }

    // Mantener esta función si la necesitas para redireccionar a otras escenas (ej. registro)
    public void CambiarEscena(string nombre)
    {
        SceneManager.LoadScene(nombre);
    }
}
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class PerfilManager : MonoBehaviour
{
    [Header("Referencias de Textos de Perfil")]
    // 🛑 MODIFICACIÓN: Cambiado de TMP_InputField a TextMeshProUGUI
    public TextMeshProUGUI textoNombre;
    public TextMeshProUGUI textoApellidos;
    public TextMeshProUGUI textoTelefono;
    public TextMeshProUGUI textoCorreo;
    public TextMeshProUGUI textoContrasena;
    
    [Header("Referencias de UI")]
    // Solo se mantiene la referencia para el botón "Volver"
    public Button btnVolver; 
    
    private string correoOriginal; // Se mantiene solo para la carga de datos

    void Start()
    {
        // 🛑 VERIFICACIÓN DIRECTA DEL SINGLETON:
        if (ConectorBD.Instance == null || string.IsNullOrEmpty(ConectorBD.UsuarioLogueadoCorreo))
        {
            Debug.LogError("🔴 No hay usuario logueado. Redirigiendo a Entrar.");
            SceneManager.LoadScene("entrar"); 
            return;
        }
        
        correoOriginal = ConectorBD.UsuarioLogueadoCorreo;
        
        // 🔑 Carga los datos usando el correo guardado en la sesión
        CargarDatosUsuario(correoOriginal);

        // Enlazar solo el botón "Volver"
        if (btnVolver != null) btnVolver.onClick.AddListener(Volver); 
        
        // ❌ ELIMINADA: La función SetInputsReadOnly() ya no es necesaria
    }

    // --- Carga los datos de la BD y los muestra en los Textos ---
    void CargarDatosUsuario(string correo)
    {
        // ✅ USO DIRECTO DEL SINGLETON:
        if (ConectorBD.Instance == null) return; 
        
        Dictionary<string, string> userData = ConectorBD.Instance.GetUserData(correo);
        
        if (userData.Count > 0)
        {
            // ✅ MODIFICACIÓN: Asignamos el valor a la propiedad .text de los TextMeshProUGUI
            if (textoNombre != null) textoNombre.text = userData["nombre"];
            if (textoApellidos != null) textoApellidos.text = userData["apellidos"];
            if (textoTelefono != null) textoTelefono.text = userData["telefono"];
            if (textoCorreo != null) textoCorreo.text = userData["correo"];
            if (textoContrasena != null) textoContrasena.text = userData["contrasena"];
        }
        else
        {
            Debug.LogError("🔴 Error al cargar el perfil. Usuario no encontrado o error de BD.");
        }
    }
    
    public void Volver()
    {
        // Redirige al menú principal (escena de hábitos)
        SceneManager.LoadScene("crearhabitos"); 
    }
    
    // ❌ ELIMINADA: La función SetInputsReadOnly()
}
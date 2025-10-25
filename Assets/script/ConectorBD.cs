using UnityEngine;
using MySql.Data.MySqlClient; 
using TMPro;
using System.Data; 
using UnityEngine.SceneManagement; 

public class ConectorBD : MonoBehaviour
{
    // *** CONFIGURACIÓN DE LA BASE DE DATOS ***
    private string server = "localhost"; 
    private string database = "Lumpi_0.1"; 
    private string uid = "root";         
    private string password = "";        
    
    private MySqlConnection dbconnection; 

    // *** VARIABLES PÚBLICAS PARA EL FORMULARIO DE REGISTRO ***
    [Header("Formulario de Registro (Inputs)")]
    public TMP_InputField inputNombre;
    public TMP_InputField inputApellidos;
    public TMP_InputField inputTelefono;
    public TMP_InputField inputCorreo;
    public TMP_InputField inputContrasena;

    // *** REFERENCIAS PARA LOS MENSAJES EN PANTALLA ***
    [Header("Mensajes UI")]
    public TextMeshProUGUI textoMensajeError; 
    public TextMeshProUGUI textoMensajeExito; 


    void Start()
    {
        // Ocultar mensajes de error y éxito al inicio
        if (textoMensajeError != null) textoMensajeError.gameObject.SetActive(false);
        if (textoMensajeExito != null) textoMensajeExito.gameObject.SetActive(false);
    }
    
    // --- FUNCIÓN DE CONEXIÓN BÁSICA ---
    private bool OpenConnection()
    {
        string connectionString = $"Server={server};Database={database};Uid={uid};Pwd={password};SslMode=None;AllowUserVariables=True;";
        dbconnection = new MySqlConnection(connectionString);

        try
        {
            dbconnection.Open();
            return true;
        }
        catch (MySqlException ex)
        {
            MostrarMensaje("🔴 Error de Conexión: Verifica que XAMPP esté activo.", true);
            Debug.LogError($"Error de Conexión ({ex.Number}): {ex.Message}");
            return false;
        }
    }

    private void CloseConnection()
    {
        if (dbconnection != null && dbconnection.State == ConnectionState.Open)
        {
            dbconnection.Close();
        }
    }


    // =========================================================================
    //                            FUNCIONES DE REGISTRO
    // =========================================================================

    // --- 1. FUNCIÓN QUE SE LLAMA AL PRESIONAR EL BOTÓN "REGISTRARSE" ---
    public void RegistrarUsuarioDesdeFormulario()
    {
        // Limpiar mensajes
        if (textoMensajeError != null) textoMensajeError.gameObject.SetActive(false);
        if (textoMensajeExito != null) textoMensajeExito.gameObject.SetActive(false);

        // 1. Obtener los valores de los campos de texto
        string nombre = inputNombre.text.Trim();
        string apellidos = inputApellidos.text.Trim();
        string telefono = inputTelefono.text.Trim(); 
        string correo = inputCorreo.text.Trim();
        string contrasena = inputContrasena.text.Trim();

        // 2. Validación: Verificar datos antes de tocar la BD 
        if (!ValidarDatosDeRegistro(nombre, apellidos, telefono, correo, contrasena))
        {
            return; 
        }

        // Si la validación pasa, llama a la función de inserción
        InsertarNuevoUsuario(nombre, apellidos, telefono, correo, contrasena);
    }

    // --- FUNCIÓN PARA LA VALIDACIÓN DE CAMPOS VACÍOS EN C# ---
    private bool ValidarDatosDeRegistro(string nombre, string apellidos, string telefono, string correo, string contrasena)
    {
        // VALIDACIÓN 1: Campo vacío (Mensaje genérico)
        if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(apellidos) || string.IsNullOrEmpty(telefono) || string.IsNullOrEmpty(correo) || string.IsNullOrEmpty(contrasena))
        {
            // Muestra el mensaje genérico cuando falta un dato, tal como solicitaste.
            MostrarMensaje("🔴 Error: Por favor, rellena todos los datos del formulario.", true);
            return false; 
        }

        // VALIDACIÓN 2: Campo Teléfono solo números (Previene el error 'Incorrect Integer Value' para Teléfono)
        // Solo verificamos si no está vacío (ya lo hicimos arriba) y si no es un número.
        if (!int.TryParse(telefono, out int result))
        {
            // Si el teléfono tiene texto, interceptamos el error aquí, antes de la BD.
            MostrarMensaje("🔴 Error: El formulario tiene datos incorrectos. Por favor, verifíquelo.", true);
            return false;
        }
        
        return true;
    }


    // --- 3. FUNCIÓN PARA INSERTAR DATOS EN MySQL ---
    public void InsertarNuevoUsuario(string nombre, string apellidos, string telefono, string correo, string contrasena)
    {
        if (!OpenConnection()) return; // Falla si no conecta

        try
        {
            string sql = "INSERT INTO registro_del_login (Nombre, Apellidos, Telefono, Correo, Contraseña) VALUES (@nombre, @apellidos, @telefono, @correo, @contrasena)";

            MySqlCommand command = new MySqlCommand(sql, dbconnection);

            // Asignar los valores del formulario a los parámetros SQL
            command.Parameters.AddWithValue("@nombre", nombre);
            command.Parameters.AddWithValue("@apellidos", apellidos);
            command.Parameters.AddWithValue("@telefono", telefono);
            command.Parameters.AddWithValue("@correo", correo);
            command.Parameters.AddWithValue("@contrasena", contrasena);

            int rowsAffected = command.ExecuteNonQuery();
            
            // Mostrar mensaje de éxito en la UI
            MostrarMensaje($"✅ Usuario {nombre} registrado con éxito.", false);

            // Limpiar los campos de entrada después de un registro exitoso
            inputNombre.text = "";
            inputApellidos.text = "";
            inputTelefono.text = "";
            inputCorreo.text = "";
            inputContrasena.text = "";
        }
        catch (MySqlException ex)
        {
            string mensajeError;
            
            // CÓDIGO CLAVE: Intercepta errores de tipo de dato (el "garabato") y de claves duplicadas
            if (ex.Number == 1062) // Código de error: Duplicado (si el Correo es UNIQUE)
            {
                mensajeError = "🔴 Error: Este correo electrónico ya está registrado.";
            }
            // Error 1366: Código genérico para "Incorrect String Value" o "Incorrect Integer Value" (el garabato)
            else if (ex.Number == 1366) 
            {
                // Reemplaza el mensaje técnico con el mensaje simple que solicitaste.
                mensajeError = "🔴 Error: El formulario tiene datos incorrectos. Por favor, verifíquelo.";
            }
            else
            {
                // Para cualquier otro error inesperado de la BD
                mensajeError = $"🔴 Error de registro inesperado (Código {ex.Number}).";
            }
            
            Debug.LogError($"❌ Error al insertar usuario (Código {ex.Number}): {ex.Message}");
            MostrarMensaje(mensajeError, true);
        }
        finally
        {
            CloseConnection();
        }
    }
    
    // --- FUNCIÓN CENTRALIZADA PARA MOSTRAR MENSAJES EN PANTALLA ---
    // isError: true para error (rojo), false para éxito (verde/azul)
    void MostrarMensaje(string mensaje, bool isError)
    {
        if (isError && textoMensajeError != null)
        {
            textoMensajeError.text = mensaje;
            textoMensajeError.gameObject.SetActive(true);
            
            // Ocultar éxito si está activo
            if (textoMensajeExito != null) textoMensajeExito.gameObject.SetActive(false);
        }
        else if (!isError && textoMensajeExito != null)
        {
            textoMensajeExito.text = mensaje;
            textoMensajeExito.gameObject.SetActive(true);
            
            // Ocultar error si está activo
            if (textoMensajeError != null) textoMensajeError.gameObject.SetActive(false);
        }
        else
        {
            // Si no hay objeto de UI enlazado, al menos lo logueamos
            Debug.Log($"Mensaje UI ({ (isError ? "Error" : "Éxito") }): {mensaje}");
        }
    }
    
    // Asegura que la conexión se cierre si la aplicación se cierra
    private void OnApplicationQuit()
    {
        CloseConnection();
    }
    
    // NOTA: Si utilizas las funciones de Login, deberás copiar y pegar la lógica de Login 
    // y la referencia a los InputFields de Login, como te mostré en el código anterior.
}
using UnityEngine;
using MySql.Data.MySqlClient; 
using TMPro;

public class ConectorBD : MonoBehaviour
{
    // *** CONFIGURACIÓN DE LA BASE DE DATOS ***
    private string server = "localhost"; 
    private string database = "Lumpi_0.1"; // Asegúrate de que sea EXACTAMENTE el nombre de tu BD
    private string uid = "root";         
    private string password = "";        
    
    private MySqlConnection dbconnection; 

    // *** VARIABLES PÚBLICAS PARA EL FORMULARIO ***
    public TMP_InputField inputNombre;
    public TMP_InputField inputApellidos;
    public TMP_InputField inputTelefono;
    public TMP_InputField inputCorreo;
    public TMP_InputField inputContrasena;

    // *** NUEVA REFERENCIA PARA EL MENSAJE DE ERROR EN PANTALLA ***
    public TextMeshProUGUI textoMensajeError; 
    
    // *** REFERENCIA PARA EL MENSAJE DE ÉXITO EN PANTALLA (Opcional) ***
    public TextMeshProUGUI textoMensajeExito; 


    void Start()
    {
        // Ocultar mensajes de error y éxito al inicio
        if (textoMensajeError != null) textoMensajeError.gameObject.SetActive(false);
        if (textoMensajeExito != null) textoMensajeExito.gameObject.SetActive(false);
        
        // El trabajo de conexión se realiza en la función RegistrarUsuarioDesdeFormulario
    }

    // --- 1. FUNCIÓN QUE SE LLAMA AL PRESIONAR EL BOTÓN "REGISTRARSE" ---
    public void RegistrarUsuarioDesdeFormulario()
    {
        // Ocultar mensajes anteriores
        if (textoMensajeError != null) textoMensajeError.gameObject.SetActive(false);
        if (textoMensajeExito != null) textoMensajeExito.gameObject.SetActive(false);

        // 1. Obtener los valores de los campos de texto
        string nombre = inputNombre.text.Trim();
        string apellidos = inputApellidos.text.Trim();
        string telefono = inputTelefono.text.Trim(); 
        string correo = inputCorreo.text.Trim();
        string contrasena = inputContrasena.text.Trim();

        // 2. Validación: Verificar que los campos obligatorios NO estén vacíos
        if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(correo) || string.IsNullOrEmpty(contrasena))
        {
            // Mostrar error de validación en la UI
            MostrarMensaje("🔴 Error: Nombre, Correo y Contraseña son obligatorios.", true);
            return; 
        }

        // Si la validación pasa, llama a la función de inserción
        InsertarNuevoUsuario(nombre, apellidos, telefono, correo, contrasena);
    }


    // --- 2. FUNCIÓN PARA INSERTAR DATOS EN MySQL ---
    public void InsertarNuevoUsuario(string nombre, string apellidos, string telefono, string correo, string contrasena)
    {
        string connectionString = $"Server={server};Database={database};Uid={uid};Pwd={password};SslMode=None;AllowUserVariables=True;";
        dbconnection = new MySqlConnection(connectionString);

        try
        {
            dbconnection.Open();
            Debug.Log("Conexión a MySQL abierta para registro.");
            
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
            // Manejar errores comunes de BD y mostrarlos en la UI
            string mensajeError;
            if (ex.Number == 1062) // Código de error: Duplicado (ej. si el correo es único y se repite)
            {
                mensajeError = "🔴 Error: Este correo electrónico ya está registrado.";
            }
            else if (ex.Number == 1045) // Error de autenticación/conexión (aunque ya lo corregimos, mejor prevenir)
            {
                mensajeError = "🔴 Error de conexión a la base de datos. Verifica tu servidor.";
            }
            else
            {
                mensajeError = $"🔴 Error de registro: {ex.Message}";
            }
            
            Debug.LogError($"❌ Error al insertar usuario (Código {ex.Number}): {ex.Message}");
            MostrarMensaje(mensajeError, true);
        }
        finally
        {
            if (dbconnection != null && dbconnection.State == System.Data.ConnectionState.Open)
            {
                dbconnection.Close();
            }
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
}
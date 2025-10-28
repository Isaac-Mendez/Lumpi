using UnityEngine;
using MySql.Data.MySqlClient; 
using TMPro;
using System.Data; 
using UnityEngine.SceneManagement; 
using System.Collections.Generic; // REQUERIDO: Añadido para usar Dictionary en GetUserData

public class ConectorBD : MonoBehaviour
{
    // *** CONFIGURACIÓN DE LA BASE DE DATOS ***
    private string server = "localhost"; 
    private string database = "Lumpi_0.1"; 
    private string uid = "root";         
    private string password = "";        
    
    private MySqlConnection dbconnection; 

    // *** VARIABLE ESTÁTICA PARA MANTENER LA SESIÓN ***
    public static string UsuarioLogueadoCorreo { get; private set; } 

    // *** VARIABLES PÚBLICAS PARA EL FORMULARIO DE REGISTRO/LOGIN ***
    [Header("Formulario de Registro/Login (Inputs)")]
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
        
        // OPCIONAL: Esto asegura que el gestor de BD persista entre escenas
        // DontDestroyOnLoad(gameObject); 
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
        if (textoMensajeError != null) textoMensajeError.gameObject.SetActive(false);
        if (textoMensajeExito != null) textoMensajeExito.gameObject.SetActive(false);

        string nombre = inputNombre.text.Trim();
        string apellidos = inputApellidos.text.Trim();
        string telefono = inputTelefono.text.Trim(); 
        string correo = inputCorreo.text.Trim();
        string contrasena = inputContrasena.text.Trim();

        if (!ValidarDatosDeRegistro(nombre, apellidos, telefono, correo, contrasena))
        {
            return; 
        }

        InsertarNuevoUsuario(nombre, apellidos, telefono, correo, contrasena);
    }

    // --- FUNCIÓN PARA LA VALIDACIÓN DE CAMPOS VACÍOS EN C# ---
    private bool ValidarDatosDeRegistro(string nombre, string apellidos, string telefono, string correo, string contrasena)
    {
        // VALIDACIÓN 1: Campo vacío (Mensaje genérico)
        if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(apellidos) || string.IsNullOrEmpty(telefono) || string.IsNullOrEmpty(correo) || string.IsNullOrEmpty(contrasena))
        {
            MostrarMensaje("🔴 Error: Por favor, rellena todos los datos del formulario.", true);
            return false; 
        }

        // VALIDACIÓN 2: Campo Teléfono solo números 
        if (!long.TryParse(telefono, out long result)) 
        {
            MostrarMensaje("🔴 Error: El formulario tiene datos incorrectos. Por favor, verifíquelo.", true);
            return false;
        }
        
        return true;
    }


    // --- 3. FUNCIÓN PARA INSERTAR DATOS EN MySQL ---
    public void InsertarNuevoUsuario(string nombre, string apellidos, string telefono, string correo, string contrasena)
    {
        if (!OpenConnection()) return; 

        try
        {
            string sql = "INSERT INTO registro_del_login (Nombre, Apellidos, Telefono, Correo, Contraseña) VALUES (@nombre, @apellidos, @telefono, @correo, @contrasena)";

            MySqlCommand command = new MySqlCommand(sql, dbconnection);

            command.Parameters.AddWithValue("@nombre", nombre);
            command.Parameters.AddWithValue("@apellidos", apellidos);
            command.Parameters.AddWithValue("@telefono", telefono);
            command.Parameters.AddWithValue("@correo", correo);
            command.Parameters.AddWithValue("@contrasena", contrasena);

            int rowsAffected = command.ExecuteNonQuery();
            
            MostrarMensaje($"✅ Usuario {nombre} registrado con éxito.", false);

            inputNombre.text = "";
            inputApellidos.text = "";
            inputTelefono.text = "";
            inputCorreo.text = "";
            inputContrasena.text = "";
        }
        catch (MySqlException ex)
        {
            string mensajeError;
            
            if (ex.Number == 1062) 
            {
                mensajeError = "🔴 Error: Este correo electrónico ya está registrado.";
            }
            else if (ex.Number == 1366) 
            {
                mensajeError = "🔴 Error: El formulario tiene datos incorrectos. Por favor, verifíquelo.";
            }
            else
            {
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
    
    // =========================================================================
    //                            FUNCIONES DE LOGIN Y SESIÓN
    // =========================================================================

    public void VerificarLoginDesdeFormulario()
    {
        string correoIngresado = inputCorreo.text.Trim();
        string passwordIngresado = inputContrasena.text.Trim();
        
        if (CheckLogin(correoIngresado, passwordIngresado))
        {
            MostrarMensaje("✅ ¡Login correcto! Accediendo al perfil...", false);
            
            // *** INICIO DE SESIÓN ***
            UsuarioLogueadoCorreo = correoIngresado; 
            
            SceneManager.LoadScene("PerfilUsuario"); // Carga la escena de perfil
        }
        else
        {
            MostrarMensaje("🔴 Error: Correo o contraseña incorrectos.", true);
        }
    }
    
    private bool CheckLogin(string correo, string contrasena)
    {
        if (!OpenConnection()) return false;
        
        string query = "SELECT Correo FROM registro_del_login WHERE Correo = @correo AND Contraseña = @contrasena";
        MySqlCommand cmd = new MySqlCommand(query, dbconnection);
        cmd.Parameters.AddWithValue("@correo", correo);
        cmd.Parameters.AddWithValue("@contrasena", contrasena);
        
        bool loginExitoso = false;
        
        try
        {
            object result = cmd.ExecuteScalar();
            if (result != null)
            {
                loginExitoso = true;
            }
        }
        catch (MySqlException ex)
        {
            Debug.LogError("Error al verificar login: " + ex.Message);
        }
        finally
        {
            CloseConnection();
        }
        
        return loginExitoso;
    }
    
    // --- 7. FUNCIÓN PARA CARGAR TODOS LOS DATOS DEL USUARIO (SELECT) ---
    public Dictionary<string, string> GetUserData(string correo)
    {
        Dictionary<string, string> userData = new Dictionary<string, string>();
        
        string query = "SELECT Nombre, Apellidos, Telefono, Correo, Contraseña FROM registro_del_login WHERE Correo = @correo";

        if (!OpenConnection()) return userData;

        try
        {
            MySqlCommand cmd = new MySqlCommand(query, dbconnection);
            cmd.Parameters.AddWithValue("@correo", correo);

            MySqlDataReader reader = cmd.ExecuteReader();
            
            if (reader.Read())
            {
                userData["nombre"] = reader["Nombre"].ToString();
                userData["apellidos"] = reader["Apellidos"].ToString();
                userData["telefono"] = reader["Telefono"].ToString();
                userData["correo"] = reader["Correo"].ToString();
                userData["contrasena"] = reader["Contraseña"].ToString();
            }
            
            reader.Close();
        }
        catch (MySqlException ex)
        {
            Debug.LogError("Error al cargar datos del usuario: " + ex.Message);
        }
        finally
        {
            CloseConnection();
        }
        
        return userData;
    }

    // --- 8. FUNCIÓN PARA ACTUALIZAR DATOS EN LA BASE DE DATOS (UPDATE) (NUEVO) ---
    public bool UpdateUserData(string correoAntiguo, string nuevoNombre, string nuevoApellido, string nuevoTelefono, string nuevoCorreo, string nuevaContrasena, out string mensajeError)
    {
        mensajeError = "";
        
        // 1. Revalidación de datos sensibles antes de enviar a la BD
        if (string.IsNullOrEmpty(nuevoNombre) || string.IsNullOrEmpty(nuevoCorreo) || string.IsNullOrEmpty(nuevaContrasena) || !long.TryParse(nuevoTelefono, out long result))
        {
            mensajeError = "🔴 Error: Por favor, rellena todos los campos correctamente.";
            return false;
        }

        string query = "UPDATE registro_del_login SET Nombre = @nombre, Apellidos = @apellido, Telefono = @telefono, Correo = @nuevoCorreo, Contraseña = @contrasena WHERE Correo = @correoAntiguo";

        if (!OpenConnection())
        {
            mensajeError = "🔴 Error de conexión a la base de datos.";
            return false;
        }

        try
        {
            MySqlCommand cmd = new MySqlCommand(query, dbconnection);
            
            cmd.Parameters.AddWithValue("@nombre", nuevoNombre);
            cmd.Parameters.AddWithValue("@apellido", nuevoApellido);
            cmd.Parameters.AddWithValue("@telefono", nuevoTelefono);
            cmd.Parameters.AddWithValue("@nuevoCorreo", nuevoCorreo);
            cmd.Parameters.AddWithValue("@contrasena", nuevaContrasena);
            cmd.Parameters.AddWithValue("@correoAntiguo", correoAntiguo);

            int rowsAffected = cmd.ExecuteNonQuery();
            
            if (rowsAffected > 0)
            {
                // Si el correo ha cambiado, actualizamos la variable estática de la sesión
                if (correoAntiguo != nuevoCorreo)
                {
                    ConectorBD.UsuarioLogueadoCorreo = nuevoCorreo;
                }
                return true;
            }
            // Si rowsAffected es 0, no hubo cambios o no se encontró el usuario.
            mensajeError = "⚠️ No se realizaron cambios o el usuario no fue encontrado.";
            return false;

        }
        catch (MySqlException ex)
        {
            if (ex.Number == 1062) // Error: Duplicado (si el nuevo correo ya existe)
            {
                mensajeError = "🔴 Error: El nuevo correo electrónico ya está registrado por otro usuario.";
            }
            else if (ex.Number == 1366) // Error: Valor incorrecto (ej. texto en campo INT)
            {
                mensajeError = "🔴 Error: Formato de datos incorrecto. Verifica el campo Teléfono.";
            }
            else
            {
                mensajeError = $"🔴 Error al guardar: {ex.Message}";
            }
            
            Debug.LogError($"❌ Error de UPDATE (Código {ex.Number}): {ex.Message}");
            return false;
        }
        finally
        {
            CloseConnection();
        }
    }


    // =========================================================================
    //                            FUNCIONES UTILITARIAS
    // =========================================================================

    // --- FUNCIÓN CENTRALIZADA PARA MOSTRAR MENSAJES EN PANTALLA ---
    void MostrarMensaje(string mensaje, bool isError)
    {
        if (isError && textoMensajeError != null)
        {
            textoMensajeError.text = mensaje;
            textoMensajeError.gameObject.SetActive(true);
            
            if (textoMensajeExito != null) textoMensajeExito.gameObject.SetActive(false);
        }
        else if (!isError && textoMensajeExito != null)
        {
            textoMensajeExito.text = mensaje;
            textoMensajeExito.gameObject.SetActive(true);
            
            if (textoMensajeError != null) textoMensajeError.gameObject.SetActive(false);
        }
        else
        {
            Debug.Log($"Mensaje UI ({ (isError ? "Error" : "Éxito") }): {mensaje}");
        }
    }
    
    private void OnApplicationQuit()
    {
        CloseConnection();
    }
}
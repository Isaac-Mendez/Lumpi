using UnityEngine;
using MySql.Data.MySqlClient; 
using TMPro;
using System.Data; 
using UnityEngine.SceneManagement; 
using System.Collections;
using System.Collections.Generic;

public class ConectorBD : MonoBehaviour
{
    // *** Singleton: Instancia estática para acceso global ***
    public static ConectorBD Instance { get; private set; } 

    // *** CONFIGURACIÓN DE LA BASE DE DATOS ***
    private string server = "localhost"; 
    private string database = "Lumpi_0.1";
    private string uid = "root";         
    private string password = "";        
    
    private MySqlConnection dbconnection; 

    // *** VARIABLE ESTÁTICA PARA MANTENER LA SESIÓN ***
    public static string UsuarioLogueadoCorreo { get; private set; } 

    // *** VARIABLES PÚBLICAS (Inputs) ***
    [Header("Formulario de Registro/Login (Inputs)")]
    // Estos inputs deben ser ASIGNADOS en la escena de registro/login.
    public TMP_InputField inputNombre;
    public TMP_InputField inputApellidos;
    public TMP_InputField inputTelefono;
    public TMP_InputField inputCorreo; 
    public TMP_InputField inputContrasena; 

    // *** REFERENCIAS PARA LOS MENSAJES EN PANTALLA ***
    [Header("Mensajes UI")]
    public TextMeshProUGUI textoMensajeError; 
    public TextMeshProUGUI textoMensajeExito; 

    void Awake()
    {
        // 🔑 IMPLEMENTACIÓN DEL SINGLETON:
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
            Debug.Log("✅ ConectorBD inicializado correctamente y marcado como DontDestroyOnLoad.");

            string currentScene = SceneManager.GetActiveScene().name;
            
            if (currentScene == "login") 
            {
                Debug.Log($"➡️ Iniciando con escena '{currentScene}'. Cargando: entrar.");
                SceneManager.LoadScene("entrar"); 
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        if (textoMensajeError != null) textoMensajeError.gameObject.SetActive(false);
        if (textoMensajeExito != null) textoMensajeExito.gameObject.SetActive(false);
        
        // El script AsignadorDeInputs se encargará de asignar las referencias de Input en las escenas de formulario.
    }

    // ============================= GESTIÓN DE REFERENCIAS (FUNCIÓN FALTANTE) =============================

    /// <summary>
    /// Permite a cualquier escena con el formulario reasignar los InputFields
    /// al Singleton de ConectorBD. DEBE ser llamado en el Start() de la escena de registro/login por el AsignadorDeInputs.
    /// </summary>
    public void SetRegistrationInputReferences(TMP_InputField nombre, TMP_InputField apellidos, TMP_InputField telefono, TMP_InputField correo, TMP_InputField contrasena)
    {
        inputNombre = nombre;
        inputApellidos = apellidos;
        inputTelefono = telefono;
        inputCorreo = correo; 
        inputContrasena = contrasena; 
        Debug.Log("✅ Referencias de InputFields (Registro/Login) asignadas desde la escena actual.");
    }
    
    // ---------------------------------------------------------------------------------------------

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

    // =============================== REGISTRO ===============================
    public void RegistrarUsuarioDesdeFormulario()
    {
        if (textoMensajeError != null) textoMensajeError.gameObject.SetActive(false);
        if (textoMensajeExito != null) textoMensajeExito.gameObject.SetActive(false);

        if (inputCorreo == null || inputContrasena == null)
        {
             Debug.LogError("🔴 Error: Los campos de Correo o Contraseña no están asignados. Llama a SetRegistrationInputReferences.");
             MostrarMensaje("🔴 Error de configuración: Los inputs no están conectados. Revisa el AsignadorDeInputs.", true);
             return;
        }
        
        string nombre = inputNombre.text.Trim();
        string apellidos = inputApellidos.text.Trim();
        string telefono = inputTelefono.text.Trim(); 
        string correo = inputCorreo.text.Trim();
        string contrasena = inputContrasena.text.Trim();

        if (!ValidarDatosDeRegistro(nombre, apellidos, telefono, correo, contrasena)) return; 

        InsertarNuevoUsuario(nombre, apellidos, telefono, correo, contrasena);
    }

    private bool ValidarDatosDeRegistro(string nombre, string apellidos, string telefono, string correo, string contrasena)
    {
        if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(correo) || string.IsNullOrEmpty(contrasena))
        {
            MostrarMensaje("🔴 Error: Nombre, Correo y Contraseña son obligatorios.", true);
            return false; 
        }

        if (string.IsNullOrEmpty(telefono)) 
        {
            MostrarMensaje("🔴 Error: Por favor, rellena el campo Teléfono.", true);
            return false;
        }
        
        return true;
    }

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

            command.ExecuteNonQuery();
            
            MostrarMensaje($"✅ Usuario {nombre} registrado con éxito. ¡Ya puedes iniciar sesión!", false);

            inputNombre.text = "";
            inputApellidos.text = "";
            inputTelefono.text = "";
            inputCorreo.text = "";
            inputContrasena.text = "";
        }
        catch (MySqlException ex)
        {
            string mensajeError;
            
            if (ex.Number == 1062) mensajeError = "🔴 Error: Este correo electrónico ya está registrado.";
            else if (ex.Number == 1366) mensajeError = "🔴 Error en el formato de datos (ej. Teléfono no es número). Verifica phpMyAdmin."; 
            else mensajeError = $"🔴 Error de registro inesperado. Intente de nuevo. (Código: {ex.Number})";
            
            Debug.LogError($"❌ Error al insertar usuario (Código {ex.Number}): {ex.Message}");
            MostrarMensaje(mensajeError, true);
        }
        finally
        {
            CloseConnection();
        }
    }

    // ================================ LOGIN ================================
    
    public void VerificarLoginDesdeFormulario(string escenaDestino) 
    {
        if (textoMensajeError != null) textoMensajeError.gameObject.SetActive(false);
        if (textoMensajeExito != null) textoMensajeExito.gameObject.SetActive(false);

        if (inputCorreo == null || inputContrasena == null)
        {
             Debug.LogError("🔴 Error: Los campos de Correo o Contraseña no están asignados por el script de la escena.");
             MostrarMensaje("🔴 Error: Configuración de la escena 'entrar' incorrecta. Reasigna los inputs.", true);
             return;
        }
        
        string correoIngresado = inputCorreo.text.Trim();
        string passwordIngresado = inputContrasena.text.Trim();
        
        if (CheckLogin(correoIngresado, passwordIngresado))
        {
            UsuarioLogueadoCorreo = correoIngresado; 
            Debug.Log("📨 Usuario guardado en sesión: " + UsuarioLogueadoCorreo);

            MostrarMensaje("✅ ¡Login correcto! Accediendo...", false);

            StartCoroutine(CargarEscenaConDelay(escenaDestino, 0.5f));
        }
        else
        {
            MostrarMensaje("🔴 Error: Correo o contraseña incorrectos.", true);
        }
    }
    
    private IEnumerator CargarEscenaConDelay(string escenaDestino, float delay)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log("➡️ Cargando escena: " + escenaDestino);
        SceneManager.LoadScene(escenaDestino);
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
            if (result != null) loginExitoso = true;
        }
        catch (MySqlException ex)
        {
            Debug.LogError("Error al verificar login: " + ex.Message);
            MostrarMensaje("🔴 Error de conexión al verificar el usuario.", true);
        }
        finally
        {
            CloseConnection();
        }
        
        return loginExitoso;
    }

    // ============================= CARGAR DATOS =============================
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
            MostrarMensaje("🔴 Error al cargar datos del perfil.", true);
        }
        finally
        {
            CloseConnection();
        }
        
        return userData;
    }

    // ============================= ACTUALIZAR DATOS =============================
    public bool UpdateUserData(string correoAntiguo, string nuevoNombre, string nuevoApellido, string nuevoTelefono, string nuevoCorreo, string nuevaContrasena, out string mensajeError)
    {
        mensajeError = "";
        
        if (string.IsNullOrEmpty(nuevoNombre) || string.IsNullOrEmpty(nuevoCorreo) || string.IsNullOrEmpty(nuevaContrasena) || string.IsNullOrEmpty(nuevoTelefono))
        {
            mensajeError = "🔴 Error: Por favor, rellena todos los campos de perfil correctamente.";
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
                if (correoAntiguo != nuevoCorreo) ConectorBD.UsuarioLogueadoCorreo = nuevoCorreo;
                return true;
            }
            mensajeError = "⚠️ No se realizaron cambios o el usuario no fue encontrado.";
            return false;

        }
        catch (MySqlException ex)
        {
            if (ex.Number == 1062) mensajeError = "🔴 Error: El nuevo correo electrónico ya está registrado por otro usuario.";
            else mensajeError = $"🔴 Error al guardar: {ex.Message}";
            
            Debug.LogError($"❌ Error de UPDATE (Código {ex.Number}): {ex.Message}");
            return false;
        }
        finally
        {
            CloseConnection();
        }
    }

    // ============================= GESTIÓN DE HÁBITOS =============================

    public void RegistrarHabito(string nombreHabito, int duracionMinutos, bool finalizado)
    {
        if (string.IsNullOrEmpty(ConectorBD.UsuarioLogueadoCorreo))
        {
            Debug.LogError("❌ No hay usuario logueado. No se puede registrar el hábito.");
            return;
        }

        if (!OpenConnection()) return; 

        try
        {
            string sql = "INSERT INTO tabla_habitos (CorreoUsuario, Nombre, DuracionMinutos, Finalizado, FechaRegistro) VALUES (@correo, @nombre, @duracion, @finalizado, NOW())";

            MySqlCommand command = new MySqlCommand(sql, dbconnection);

            command.Parameters.AddWithValue("@correo", ConectorBD.UsuarioLogueadoCorreo);
            command.Parameters.AddWithValue("@nombre", nombreHabito);
            command.Parameters.AddWithValue("@duracion", duracionMinutos);
            command.Parameters.AddWithValue("@finalizado", finalizado ? 1 : 0); 
            
            command.ExecuteNonQuery();
            
            Debug.Log($"✅ Hábito registrado: '{nombreHabito}'. Duración: {duracionMinutos} min. Finalizado: {finalizado}.");
        }
        catch (MySqlException ex)
        {
            Debug.LogError($"❌ Error al registrar el hábito (Código {ex.Number}): {ex.Message}");
        }
        finally
        {
            CloseConnection();
        }
    }

    // ============================= UTILIDADES =============================
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
using UnityEngine;
using MySql.Data.MySqlClient; 
using TMPro; // Necesario para enlazar con los Input Fields de TextMeshPro

public class ConectorBD : MonoBehaviour
{
    // *** CONFIGURACIÓN DE LA BASE DE DATOS ***
    private string server = "localhost"; 
    private string database = "Lumpi_0.1"; // Nombre de tu BD
    private string uid = "root";         // Usuario de MySQL
    private string password = "";        // Contraseña de MySQL (deja "" si no tienes)
    
    private MySqlConnection dbconnection; 

    // *** VARIABLES PÚBLICAS PARA EL FORMULARIO (Arrastrar desde el Inspector) ***
    // Estas variables recibirán los datos de la interfaz
    public TMP_InputField inputNombre;
    public TMP_InputField inputApellidos;
    public TMP_InputField inputTelefono;
    public TMP_InputField inputCorreo;
    public TMP_InputField inputContrasena;

    // La función Start() se vacía porque la conexión solo se inicia al presionar el botón
    void Start()
    {
        // El trabajo de conexión se realiza en la función RegistrarUsuarioDesdeFormulario
    }

    // --- 1. FUNCIÓN QUE SE LLAMA AL PRESIONAR EL BOTÓN "REGISTRARSE" ---
    public void RegistrarUsuarioDesdeFormulario()
    {
        // 1. Obtener los valores de los campos de texto
        string nombre = inputNombre.text;
        string apellidos = inputApellidos.text;
        string telefono = inputTelefono.text; 
        string correo = inputCorreo.text;
        string contrasena = inputContrasena.text;

        // 2. Validación simple (Evitar inserciones vacías)
        if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(correo) || string.IsNullOrEmpty(contrasena))
        {
            Debug.LogError("🔴 Error de Registro: Por favor, completa Nombre, Correo y Contraseña.");
            return; 
        }

        // 3. Llamar a la función de inserción con los datos del formulario
        InsertarNuevoUsuario(nombre, apellidos, telefono, correo, contrasena);
    }


    // --- 2. FUNCIÓN PARA INSERTAR DATOS EN MySQL ---
    public void InsertarNuevoUsuario(string nombre, string apellidos, string telefono, string correo, string contrasena)
    {
        // Creamos la cadena de conexión
        string connectionString = $"Server={server};Database={database};Uid={uid};Pwd={password};";
        dbconnection = new MySqlConnection(connectionString);

        try
        {
            // Abrir la conexión
            dbconnection.Open();
            
            // Comando SQL. Los @campos evitan inyección SQL.
            string sql = "INSERT INTO registro_del_login (Nombre, Apellidos, Telefono, Correo, Contraseña) VALUES (@nombre, @apellidos, @telefono, @correo, @contrasena)";

            MySqlCommand command = new MySqlCommand(sql, dbconnection);

            // Asignar los valores del formulario a los parámetros SQL
            command.Parameters.AddWithValue("@nombre", nombre);
            command.Parameters.AddWithValue("@apellidos", apellidos);
            command.Parameters.AddWithValue("@telefono", telefono);
            command.Parameters.AddWithValue("@correo", correo);
            command.Parameters.AddWithValue("@contrasena", contrasena);

            // Ejecutar la inserción
            int rowsAffected = command.ExecuteNonQuery();
            Debug.Log($"✅ Registro exitoso para: {nombre}. Filas afectadas: {rowsAffected}");
            
            // Limpiar los campos de entrada después de un registro exitoso
            inputNombre.text = "";
            inputApellidos.text = "";
            inputTelefono.text = "";
            inputCorreo.text = "";
            inputContrasena.text = "";
        }
        catch (MySqlException ex)
        {
            // Manejo de errores de MySQL (ej. si el correo ya existe, si el campo es muy largo, etc.)
            Debug.LogError($"❌ Error al insertar usuario (Código {ex.Number}): {ex.Message}");
        }
        finally
        {
            // Aseguramos que la conexión se cierre
            if (dbconnection != null && dbconnection.State == System.Data.ConnectionState.Open)
            {
                dbconnection.Close();
            }
        }
    }
}
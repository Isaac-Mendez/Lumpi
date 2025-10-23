using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using MySql.Data.MySqlClient; 

public class login : MonoBehaviour
{
    // *** CONFIGURACIÓN DE LA BASE DE DATOS ***
    private string server = "localhost"; 
    private string database = "Lumpi_0.1"; // Tu base de datos
    private string uid = "root";         
    private string password = "";        // Vacío, como funcionó en el registro//
    
    // El objeto de conexión se declara a nivel de clase para que sea accesible
    private MySqlConnection dbconnection; 

    [Header("Referencias UI")]
    public TMP_InputField usuario; // Campo de Correo
    public TMP_InputField contrasena;
    public Button Entrar;
    
    // *** NUEVA REFERENCIA PARA MOSTRAR MENSAJES EN PANTALLA ***
    // (Asegúrate de que este campo esté enlazado al 'MensajeErrorLogin' en el Inspector)
    public TextMeshProUGUI textoMensajeError; 
    

    void Start()
    {
        // Conectamos el botón al método VerificarLogin()
        Entrar.onClick.AddListener(VerificarLogin);
        
        // Ocultar el mensaje de error al iniciar la escena
        if (textoMensajeError != null)
        {
            textoMensajeError.gameObject.SetActive(false);
        }
    }

    void VerificarLogin()
    {
        // Ocultar cualquier error anterior antes de intentar un nuevo login
        if (textoMensajeError != null)
        {
            textoMensajeError.gameObject.SetActive(false);
        }
        
        string emailIngresado = usuario.text.Trim();
        string passwordIngresada = contrasena.text.Trim();
        
        string connectionString = $"Server={server};Database={database};Uid={uid};Pwd={password};SslMode=None;AllowUserVariables=True;";
        dbconnection = new MySqlConnection(connectionString);

        try
        {
            dbconnection.Open();
            Debug.Log("Conexión a MySQL abierta para login.");

            string sql = "SELECT COUNT(*) FROM registro_del_login WHERE Correo = @email AND Contraseña = @password";
            
            MySqlCommand command = new MySqlCommand(sql, dbconnection);

            command.Parameters.AddWithValue("@email", emailIngresado);
            command.Parameters.AddWithValue("@password", passwordIngresada);

            int userCount = System.Convert.ToInt32(command.ExecuteScalar());

            if (userCount == 1)
            {
                Debug.Log("✅ Login correcto. Usuario encontrado.");
                CambiarEscena("crearhabitos");
            }
            else
            {
                Debug.Log("❌ Credenciales incorrectas o usuario no encontrado.");
                // ** Lógica para mostrar el error en pantalla **
                MostrarError("Usuario o Contraseña incorrectos.");
            }
        }
        catch (MySqlException ex)
        {
            Debug.LogError($"❌ Error de conexión o consulta de Login (Código {ex.Number}): {ex.Message}");
            // ** Lógica para mostrar el error de conexión en pantalla **
            MostrarError("Error de conexión al servidor. Intente de nuevo.");
        }
        finally
        {
            if (dbconnection != null && dbconnection.State == System.Data.ConnectionState.Open)
            {
                dbconnection.Close();
            }
        }
    }
    
    // --- NUEVA FUNCIÓN PARA MOSTRAR MENSAJE EN PANTALLA ---
    void MostrarError(string mensaje)
    {
        if (textoMensajeError != null)
        {
            textoMensajeError.text = mensaje;
            textoMensajeError.gameObject.SetActive(true);
        }
    }

    public void CambiarEscena(string nombre)
    {
        SceneManager.LoadScene(nombre);
    }
}
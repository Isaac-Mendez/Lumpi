using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using MySql.Data.MySqlClient; // ¡Importante para MySQL!

public class login : MonoBehaviour
{
    // *** CONFIGURACIÓN DE LA BASE DE DATOS ***
    private string server = "localhost"; 
    private string database = "Lumpi_0.1"; // Tu base de datos
    private string uid = "root";         
    private string password = "";        // Vacío, como funcionó en el registro
    
    [Header("Referencias UI")]
    public TMP_InputField usuario; // Campo de Correo
    public TMP_InputField contrasena;
    public Button Entrar;

    // Ya no necesitamos variables de credenciales correctas estáticas
    // [Header("Credenciales Correctas")]
    // public string usuarioCorrecto = "admin";
    // public string passwordCorrecto = "1234";

    void Start()
    {
        // Conectamos el botón al método VerificarLogin()
        Entrar.onClick.AddListener(VerificarLogin);
    }

    void VerificarLogin()
    {
        string emailIngresado = usuario.text.Trim();
        string passwordIngresada = contrasena.text.Trim();
        
        // Cadena de conexión con los parámetros de compatibilidad que funcionaron
        string connectionString = $"Server={server};Database={database};Uid={uid};Pwd={password};SslMode=None;AllowUserVariables=True;";
        MySqlConnection dbconnection = new MySqlConnection(connectionString);

        try
        {
            dbconnection.Open();
            Debug.Log("Conexión a MySQL abierta para login.");

            // Consulta SQL: Contar cuántos usuarios tienen este Correo Y esta Contraseña
            string sql = "SELECT COUNT(*) FROM registro_del_login WHERE Correo = @email AND Contraseña = @password";
            
            MySqlCommand command = new MySqlCommand(sql, dbconnection);

            // Usamos parámetros para prevenir inyección SQL
            command.Parameters.AddWithValue("@email", emailIngresado);
            command.Parameters.AddWithValue("@password", passwordIngresada);

            // ExecuteScalar devuelve el primer valor de la primera fila (en este caso, COUNT(*))
            int userCount = System.Convert.ToInt32(command.ExecuteScalar());

            if (userCount == 1)
            {
                Debug.Log("✅ Login correcto. Usuario encontrado.");
                CambiarEscena("crearhabitos");
            }
            else
            {
                Debug.Log("❌ Credenciales incorrectas o usuario no encontrado.");
                // Opcional: Mostrar un mensaje de error en la UI aquí
            }
        }
        catch (MySqlException ex)
        {
            Debug.LogError($"❌ Error de conexión o consulta de Login (Código {ex.Number}): {ex.Message}");
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

    public void CambiarEscena(string nombre)
    {
        SceneManager.LoadScene(nombre);
    }
}


using UnityEngine;
using MySql.Data.MySqlClient; 
using TMPro;
using System.Data; 
using UnityEngine.SceneManagement; 
using System.Collections;
using System.Collections.Generic;
using System;

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

    // ============================= LISTA DE HÁBITOS EN MEMORIA =============================
    public static List<(string id, string nombre, int duracion, bool finalizado)> HabitosGuardados
        = new List<(string id, string nombre, int duracion, bool finalizado)>();

    // ============================= VARIABLES FROM UI =============================
    [Header("Formulario de Registro/Login (Inputs)")]
    public TMP_InputField inputNombre;
    public TMP_InputField inputApellidos;
    public TMP_InputField inputTelefono;
    public TMP_InputField inputCorreo; 
    public TMP_InputField inputContrasena; 

    [Header("Mensajes UI")]
    public TextMeshProUGUI textoMensajeError; 
    public TextMeshProUGUI textoMensajeExito; 

    // =====================================================================
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
            Debug.Log("✅ ConectorBD inicializado correctamente.");
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
    }

    // =====================================================================
    public void SetRegistrationInputReferences(TMP_InputField nombre, TMP_InputField apellidos,
                                               TMP_InputField telefono, TMP_InputField correo,
                                               TMP_InputField contrasena)
    {
        inputNombre = nombre;
        inputApellidos = apellidos;
        inputTelefono = telefono;
        inputCorreo = correo; 
        inputContrasena = contrasena; 
        Debug.Log("✅ Inputs ligados al Singleton.");
    }

    // =====================================================================
    private bool OpenConnection()
    {
        string connectionString =
            $"Server={server};Database={database};Uid={uid};Pwd={password};SslMode=None;AllowUserVariables=True;";

        dbconnection = new MySqlConnection(connectionString);

        try { dbconnection.Open(); return true; }
        catch (MySqlException ex)
        {
            MostrarMensaje("🔴 Error de conexión. Activa XAMPP.", true);
            Debug.LogError($"Error DB: {ex.Message}");
            return false;
        }
    }

    private void CloseConnection()
    {
        if (dbconnection != null && dbconnection.State == ConnectionState.Open)
            dbconnection.Close();
    }

    // =====================================================================
    // ============================= REGISTRO ==============================
    public void RegistrarUsuarioDesdeFormulario()
    {
        if (textoMensajeError != null) textoMensajeError.gameObject.SetActive(false);
        if (textoMensajeExito != null) textoMensajeExito.gameObject.SetActive(false);

        if (inputCorreo == null || inputContrasena == null)
        {
            MostrarMensaje("Error: Inputs no asignados.", true);
            return;
        }
        
        string nombre = inputNombre.text.Trim();
        string apellidos = inputApellidos.text.Trim();
        string telefono = inputTelefono.text.Trim(); 
        string correo = inputCorreo.text.Trim();
        string contrasena = inputContrasena.text.Trim();

        if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(correo) ||
            string.IsNullOrEmpty(contrasena) || string.IsNullOrEmpty(telefono))
        {
            MostrarMensaje("🔴 Debes rellenar todos los campos.", true);
            return;
        }

        InsertarNuevoUsuario(nombre, apellidos, telefono, correo, contrasena);
    }

    public void InsertarNuevoUsuario(string nombre, string apellidos,
                                     string telefono, string correo, string contrasena)
    {
        if (!OpenConnection()) return;

        try
        {
            string sql =
                "INSERT INTO registro_del_login (Nombre, Apellidos, Telefono, Correo, Contraseña) " +
                "VALUES (@nombre, @apellidos, @telefono, @correo, @contrasena)";

            MySqlCommand cmd = new MySqlCommand(sql, dbconnection);
            cmd.Parameters.AddWithValue("@nombre", nombre);
            cmd.Parameters.AddWithValue("@apellidos", apellidos);
            cmd.Parameters.AddWithValue("@telefono", telefono);
            cmd.Parameters.AddWithValue("@correo", correo);
            cmd.Parameters.AddWithValue("@contrasena", contrasena);

            cmd.ExecuteNonQuery();

            MostrarMensaje($"Usuario {nombre} registrado correctamente.", false);
        }
        catch (MySqlException ex)
        {
            if (ex.Number == 1062)
                MostrarMensaje("🔴 El correo ya existe.", true);
            else
                MostrarMensaje("🔴 Error inesperado al registrar.", true);

            Debug.LogError("DB Error: " + ex.Message);
        }
        finally { CloseConnection(); }
    }

    // =====================================================================
    // ============================= LOGIN ================================
    public void VerificarLoginDesdeFormulario(string escenaDestino)
    {
        if (textoMensajeError != null) textoMensajeError.gameObject.SetActive(false);
        if (textoMensajeExito != null) textoMensajeExito.gameObject.SetActive(false);

        if (inputCorreo == null || inputContrasena == null)
        {
            MostrarMensaje("🔴 Error: Inputs no asignados en la escena.", true);
            return;
        }

        string correo = inputCorreo.text.Trim();
        string pass = inputContrasena.text.Trim();

        if (CheckLogin(correo, pass))
        {
            UsuarioLogueadoCorreo = correo;
            CargarHabitosDesdeBD();
            MostrarMensaje($"Bienvenido {correo}", false);
            StartCoroutine(CargarEscenaConDelay(escenaDestino, 0.3f));
        }
        else
        {
            MostrarMensaje("🔴 Credenciales incorrectas.", true);
        }
    }

    private IEnumerator CargarEscenaConDelay(string escena, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(escena);
    }

    private bool CheckLogin(string correo, string contrasena)
    {
        if (!OpenConnection()) return false;

        string sql = "SELECT Correo FROM registro_del_login WHERE Correo=@correo AND Contraseña=@contrasena";
        MySqlCommand cmd = new MySqlCommand(sql, dbconnection);
        cmd.Parameters.AddWithValue("@correo", correo);
        cmd.Parameters.AddWithValue("@contrasena", contrasena);

        object result = null;
        try
        {
            result = cmd.ExecuteScalar();
        }
        catch (MySqlException ex)
        {
            Debug.LogError("Error CheckLogin: " + ex.Message);
        }
        finally
        {
            CloseConnection();
        }

        return result != null;
    }

    // =====================================================================
    // ====================== GET USER DATA ===============================
    public Dictionary<string, string> GetUserData(string correo)
    {
        Dictionary<string, string> data = new Dictionary<string, string>();

        if (!OpenConnection()) return data;

        string sql =
            "SELECT Nombre, Apellidos, Telefono, Correo, Contraseña " +
            "FROM registro_del_login WHERE Correo=@correo";

        try
        {
            MySqlCommand cmd = new MySqlCommand(sql, dbconnection);
            cmd.Parameters.AddWithValue("@correo", correo);

            MySqlDataReader r = cmd.ExecuteReader();

            if (r.Read())
            {
                data["nombre"] = r["Nombre"].ToString();
                data["apellidos"] = r["Apellidos"].ToString();
                data["telefono"] = r["Telefono"].ToString();
                data["correo"] = r["Correo"].ToString();
                data["contrasena"] = r["Contraseña"].ToString();
            }

            r.Close();
        }
        catch (MySqlException ex)
        {
            Debug.LogError("Error GetUserData: " + ex.Message);
        }
        finally { CloseConnection(); }

        return data;
    }

    public string GetNombreUsuario()
    {
        if (string.IsNullOrEmpty(UsuarioLogueadoCorreo))
            return null;

        var d = GetUserData(UsuarioLogueadoCorreo);
        return d.ContainsKey("nombre") ? d["nombre"] : null;
    }

    // =====================================================================
    // ====================== CONTADOR DE HÁBITOS ==========================
    public int ObtenerContadorHabitos()
    {
        if (string.IsNullOrEmpty(UsuarioLogueadoCorreo))
        {
            Debug.LogWarning("Usuario no logueado al contar hábitos.");
            return 0;
        }

        if (!OpenConnection()) return 0;

        int total = 0;

        try
        {
            string sql = @"
                SELECT 
                    (SELECT COUNT(*) FROM tabla_habitos WHERE CorreoUsuario=@correo) +
                    (SELECT COUNT(*) FROM tabla_habitos_finalizados WHERE CorreoUsuario=@correo)
            AS Total;";

            MySqlCommand cmd = new MySqlCommand(sql, dbconnection);
            cmd.Parameters.AddWithValue("@correo", UsuarioLogueadoCorreo);

            object res = cmd.ExecuteScalar();
            if (res != null) total = int.Parse(res.ToString());
        }
        catch (MySqlException ex)
        {
            Debug.LogError("Error ObtenerContadorHabitos: " + ex.Message);
        }
        finally { CloseConnection(); }

        return total;
    }

    // =====================================================================
    // ======================= APODO DEL HÁBITO ============================
    public string GenerarApodoHabito()
    {
        string nombre = GetNombreUsuario();
        if (nombre == null) return null;

        int num = ObtenerContadorHabitos() + 1;
        string codigo = num.ToString("D3");

        return $"{nombre}-{codigo}";
    }

    // =====================================================================
    // ======================== REGISTRAR HÁBITO ===========================
    public void RegistrarHabito(string nombreHabito, int duracion, bool finalizado, string idHabito = null)
    {
        if (string.IsNullOrEmpty(UsuarioLogueadoCorreo))
        {
            Debug.LogError("❌ No hay usuario logueado. No se puede registrar el hábito.");
            return;
        }

        if (string.IsNullOrEmpty(idHabito))
            idHabito = GenerarApodoHabito();

        // asegurar no duplicados
        HabitosGuardados.RemoveAll(h => h.id == idHabito);
        HabitosGuardados.Add((idHabito, nombreHabito, duracion, finalizado));

        if (!OpenConnection()) return;

        try
        {
            string checkSql =
                "SELECT COUNT(*) FROM tabla_habitos WHERE CorreoUsuario=@correo AND ID_Habito=@id";

            MySqlCommand c = new MySqlCommand(checkSql, dbconnection);
            c.Parameters.AddWithValue("@correo", UsuarioLogueadoCorreo);
            c.Parameters.AddWithValue("@id", idHabito);

            long existe = (long)c.ExecuteScalar();

            if (existe == 0)
            {
                // insertar
                string insert =
                    "INSERT INTO tabla_habitos (CorreoUsuario,ID_Habito,Nombre,DuracionMinutos,Finalizado,FechaRegistro) " +
                    "VALUES(@correo,@id,@nombre,@duracion,@fin,NOW())";

                MySqlCommand cmd = new MySqlCommand(insert, dbconnection);
                cmd.Parameters.AddWithValue("@correo", UsuarioLogueadoCorreo);
                cmd.Parameters.AddWithValue("@id", idHabito);
                cmd.Parameters.AddWithValue("@nombre", nombreHabito);
                cmd.Parameters.AddWithValue("@duracion", duracion);
                cmd.Parameters.AddWithValue("@fin", finalizado ? 1 : 0);

                cmd.ExecuteNonQuery();
            }
            else
            {
                // si existe, actualizamos nombre/duración/finalizado según necesites
                string update = "UPDATE tabla_habitos SET Nombre=@nombre, DuracionMinutos=@duracion, Finalizado=@fin WHERE CorreoUsuario=@correo AND ID_Habito=@id";
                MySqlCommand u = new MySqlCommand(update, dbconnection);
                u.Parameters.AddWithValue("@nombre", nombreHabito);
                u.Parameters.AddWithValue("@duracion", duracion);
                u.Parameters.AddWithValue("@fin", finalizado ? 1 : 0);
                u.Parameters.AddWithValue("@correo", UsuarioLogueadoCorreo);
                u.Parameters.AddWithValue("@id", idHabito);
                u.ExecuteNonQuery();
            }
        }
        catch (MySqlException ex)
        {
            Debug.LogError("Error RegistrarHabito: " + ex.Message);
        }
        finally { CloseConnection(); }
    }


public void GuardarFechaInicioHabito(string idHabito)
{
    if (string.IsNullOrEmpty(UsuarioLogueadoCorreo))
    {
        Debug.LogError("❌ No hay usuario logueado.");
        return;
    }

    if (!OpenConnection()) return;

    try
    {
        string sql = @"
            UPDATE tabla_habitos 
            SET FechaInicio = NOW()
            WHERE CorreoUsuario = @correo AND ID_Habito = @id;
        ";

        MySqlCommand cmd = new MySqlCommand(sql, dbconnection);
        cmd.Parameters.AddWithValue("@correo", UsuarioLogueadoCorreo);
        cmd.Parameters.AddWithValue("@id", idHabito);
        cmd.ExecuteNonQuery();

        Debug.Log($"⏱ FechaInicio guardada para hábito (ID: {idHabito})");
    }
    catch (MySqlException ex)
    {
        Debug.LogError("❌ Error al guardar FechaInicio: " + ex.Message);
    }
    finally
    {
        CloseConnection();
    }
}

   // ======================= CARGAR HÁBITOS DESDE BD =======================
public void CargarHabitosDesdeBD()
{
    if (string.IsNullOrEmpty(UsuarioLogueadoCorreo))
    {
        Debug.LogWarning("⚠️ No hay usuario logueado, no se pueden cargar hábitos.");
        return;
    }

    if (!OpenConnection())
    {
        Debug.LogError("❌ No se pudo conectar a la base de datos para cargar hábitos.");
        return;
    }

    try
    {
        string sql = @"
            SELECT ID_Habito, Nombre, DuracionMinutos, Finalizado, FechaInicio
            FROM tabla_habitos
            WHERE CorreoUsuario = @correo
        ";

        MySqlCommand cmd = new MySqlCommand(sql, dbconnection);
        cmd.Parameters.AddWithValue("@correo", UsuarioLogueadoCorreo);

        MySqlDataReader reader = cmd.ExecuteReader();

        HabitosGuardados.Clear();

        while (reader.Read())
        {
            // ===================== ID =====================
            string id = reader["ID_Habito"]?.ToString();

            // ===================== Nombre =====================
            string nombre = reader["Nombre"]?.ToString() ?? "";

            // ===================== Duración =====================
            int duracion = 0;
            int.TryParse(reader["DuracionMinutos"]?.ToString(), out duracion);

            // ===================== Finalizado =====================
            bool finalizado = false;
            string finStr = reader["Finalizado"]?.ToString()?.ToLower();

            if (finStr == "1" || finStr == "true")
                finalizado = true;

            // ===================== Fecha Inicio =====================
            DateTime? fechaInicio = null;
            string fechaStr = reader["FechaInicio"]?.ToString();

            if (!string.IsNullOrEmpty(fechaStr))
            {
                DateTime tmp;
                if (DateTime.TryParse(fechaStr, out tmp))
                    fechaInicio = tmp;
            }

            // ===================== Calcular minutos restantes =====================
            int minutosRestantes = duracion;

            if (fechaInicio.HasValue && !finalizado)
            {
                TimeSpan dif = DateTime.Now - fechaInicio.Value;
                int minutosPasados = Mathf.Max(0, (int)dif.TotalMinutes);
                minutosRestantes = Mathf.Max(0, duracion - minutosPasados);
            }

            if (!string.IsNullOrEmpty(id))
                PlayerPrefs.SetInt(id + "_restante", minutosRestantes);

            // ===================== Guardar en memoria =====================
            HabitosGuardados.Add((id, nombre, duracion, finalizado));
        }

        reader.Close();
        Debug.Log($"📦 Hábitos cargados desde BD: {HabitosGuardados.Count}");
    }
    catch (MySqlException ex)
    {
        Debug.LogError("❌ Error al cargar hábitos: " + ex.Message);
    }
    finally
    {
        CloseConnection();
    }
}


    // =====================================================================
    // ====================== MARCAR Y ELIMINAR HABITOS =====================
    public void MarcarHabitoComoCompletadoPorID(string idHabito)
    {
        if (string.IsNullOrEmpty(UsuarioLogueadoCorreo))
        {
            Debug.LogError("❌ No hay usuario logueado.");
            return;
        }

        if (!OpenConnection()) return;

        try
        {
            string sql = "UPDATE tabla_habitos SET Finalizado = 1 WHERE CorreoUsuario=@correo AND ID_Habito=@id";
            MySqlCommand cmd = new MySqlCommand(sql, dbconnection);
            cmd.Parameters.AddWithValue("@correo", UsuarioLogueadoCorreo);
            cmd.Parameters.AddWithValue("@id", idHabito);
            cmd.ExecuteNonQuery();

            // actualizar memoria
            for (int i = 0; i < HabitosGuardados.Count; i++)
            {
                if (HabitosGuardados[i].id == idHabito)
                    HabitosGuardados[i] = (HabitosGuardados[i].id, HabitosGuardados[i].nombre, HabitosGuardados[i].duracion, true);
            }

            Debug.Log($"✔ Hábito marcado como completado (ID: {idHabito})");
        }
        catch (MySqlException ex)
        {
            Debug.LogError("❌ Error al marcar completado por ID: " + ex.Message);
        }
        finally
        {
            CloseConnection();
        }
    }

    public void EliminarHabito(string nombreHabito)
    {
        if (string.IsNullOrEmpty(UsuarioLogueadoCorreo))
        {
            Debug.LogError("❌ No hay usuario logueado para eliminar hábitos.");
            return;
        }

        if (!OpenConnection()) return;

        try
        {
            string sql = "DELETE FROM tabla_habitos WHERE CorreoUsuario=@correo AND Nombre=@nombre";
            MySqlCommand cmd = new MySqlCommand(sql, dbconnection);
            cmd.Parameters.AddWithValue("@correo", UsuarioLogueadoCorreo);
            cmd.Parameters.AddWithValue("@nombre", nombreHabito);
            int rows = cmd.ExecuteNonQuery();

            if (rows > 0)
            {
                HabitosGuardados.RemoveAll(h => h.nombre == nombreHabito);
                Debug.Log($"🗑️ Hábito eliminado: {nombreHabito}");
            }
        }
        catch (MySqlException ex)
        {
            Debug.LogError("Error al eliminar hábito: " + ex.Message);
        }
        finally { CloseConnection(); }
    }

    public void EliminarHabitoPorID(string idHabito)
    {
        if (string.IsNullOrEmpty(UsuarioLogueadoCorreo))
        {
            Debug.LogError("❌ No hay usuario logueado.");
            return;
        }

        if (!OpenConnection()) return;

        try
        {
            string sql = "DELETE FROM tabla_habitos WHERE CorreoUsuario=@correo AND ID_Habito=@id";
            MySqlCommand cmd = new MySqlCommand(sql, dbconnection);
            cmd.Parameters.AddWithValue("@correo", UsuarioLogueadoCorreo);
            cmd.Parameters.AddWithValue("@id", idHabito);

            int rows = cmd.ExecuteNonQuery();

            if (rows > 0)
            {
                HabitosGuardados.RemoveAll(h => h.id == idHabito);
                Debug.Log($"🗑️ Hábito eliminado por ID: {idHabito}");
            }
        }
        catch (MySqlException ex)
        {
            Debug.LogError("❌ Error al eliminar hábito por ID: " + ex.Message);
        }
        finally
        {
            CloseConnection();
        }
    }


public void EliminaHabitoPorID(string idHabito)
{
    if (string.IsNullOrEmpty(UsuarioLogueadoCorreo))
    {
        Debug.LogError("❌ No hay usuario logueado. No se puede eliminar hábito.");
        return;
    }

    if (!OpenConnection())
    {
        Debug.LogError("❌ No se pudo conectar para eliminar hábito por ID.");
        return;
    }

    try
    {
        string sql = "DELETE FROM tabla_habitos WHERE CorreoUsuario=@correo AND ID_Habito=@id";

        MySqlCommand cmd = new MySqlCommand(sql, dbconnection);
        cmd.Parameters.AddWithValue("@correo", UsuarioLogueadoCorreo);
        cmd.Parameters.AddWithValue("@id", idHabito);

        int rows = cmd.ExecuteNonQuery();

        if (rows > 0)
        {
            HabitosGuardados.RemoveAll(h => h.id == idHabito);
            Debug.Log($"🗑️ Hábito eliminado correctamente (ID: {idHabito})");
        }
        else
        {
            Debug.LogWarning($"⚠️ No se encontró hábito con ID: {idHabito}");
        }
    }
    catch (MySqlException ex)
    {
        Debug.LogError("❌ Error eliminando hábito por ID: " + ex.Message);
    }
    finally
    {
        CloseConnection();
    }
}


    // =====================================================================
    // ======================= UTILIDADES ================================
    void MostrarMensaje(string m, bool err)
    {
        if (err)
        {
            if (textoMensajeError != null) { textoMensajeError.text = m; textoMensajeError.gameObject.SetActive(true); }
            if (textoMensajeExito != null) textoMensajeExito.gameObject.SetActive(false);
        }
        else
        {
            if (textoMensajeExito != null) { textoMensajeExito.text = m; textoMensajeExito.gameObject.SetActive(true); }
            if (textoMensajeError != null) textoMensajeError.gameObject.SetActive(false);
        }
    }

    public void EjecutarMoverHabitosFinalizados()
    {
        if (!OpenConnection()) return;

        try
        {
            MySqlCommand cmd = new MySqlCommand("CALL MoverHabitosFinalizados()", dbconnection);
            cmd.ExecuteNonQuery();
        }
        catch (MySqlException ex)
        {
            Debug.LogError("Error procedimiento: " + ex.Message);
        }
        finally { CloseConnection(); }
    }

    private void OnApplicationQuit()
    {
        CloseConnection();
    }

        // ================================================================
    // ========== OBTENER LISTA DE HÁBITOS FINALIZADOS =================
    // ================================================================
    public List<string> ObtenerHabitosFinalizados()
    {
        List<string> lista = new List<string>();

        if (string.IsNullOrEmpty(UsuarioLogueadoCorreo))
        {
            Debug.LogWarning("⚠️ No hay usuario logueado.");
            return lista;
        }

        if (!OpenConnection())
        {
            Debug.LogError("❌ No se pudo conectar para cargar hábitos finalizados.");
            return lista;
        }

        try
        {
            string sql = @"
                SELECT Nombre 
                FROM tabla_habitos_finalizados
                WHERE CorreoUsuario = @correo
                ORDER BY FechaFinalizacion DESC;
            ";

            MySqlCommand cmd = new MySqlCommand(sql, dbconnection);
            cmd.Parameters.AddWithValue("@correo", UsuarioLogueadoCorreo);

            MySqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                string nombre = reader["Nombre"]?.ToString();
                if (!string.IsNullOrEmpty(nombre))
                    lista.Add(nombre);
            }

            reader.Close();
        }
        catch (MySqlException ex)
        {
            Debug.LogError("❌ Error al cargar hábitos finalizados: " + ex.Message);
        }
        finally
        {
            CloseConnection();
        }

        return lista;
    }

}



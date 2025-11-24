using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HabitoItem : MonoBehaviour
{
    public TMP_Text nombreTexto;
    public Button botonCompletar;
    public Button botonEliminar;

    private string idHabito;    // AHORA usa ID, no nombre
    private string nombreHabito;

    /// <summary>
    /// Configura la UI del ítem del hábito.
    /// </summary>
    public void Configurar(string id, string nombre, int duracion, bool finalizado)
    {
        idHabito = id;
        nombreHabito = nombre;

        if (nombreTexto != null)
        {
            nombreTexto.text = $"{nombre} ({duracion} min)";
            nombreTexto.color = finalizado ? Color.green : Color.white;
        }

        if (botonCompletar != null)
            botonCompletar.onClick.AddListener(Completar);

        if (botonEliminar != null)
            botonEliminar.onClick.AddListener(Eliminar);
    }

    // ========================= COMPLETAR =========================
    private void Completar()
{
    if (ConectorBD.Instance == null)
    {
        Debug.LogError("❌ ConectorBD no está disponible.");
        return;
    }

    // Marca finalizado por ID
    ConectorBD.Instance.MarcarHabitoComoCompletadoPorID(idHabito);

    // Opcional: cambiar color visual
    if (nombreTexto != null)
        nombreTexto.color = Color.green;

    // Mover hábito a tabla_habitos_finalizados
    ConectorBD.Instance.EjecutarMoverHabitosFinalizados();

    // ================================================================
    // ========== GUARDAR ÚLTIMO HÁBITO FINALIZADO =====================
    // ================================================================
    PlayerPrefs.SetString("UltimoHabitoFinalizado", nombreHabito);

    // Quitar del scroll
    Destroy(gameObject);

    Debug.Log($"✔ Hábito finalizado y registrado para celebración: {nombreHabito}");
}


    // ========================= ELIMINAR =========================
    private void Eliminar()
    {
        if (ConectorBD.Instance == null)
        {
            Debug.LogError("❌ ConectorBD no disponible.");
            return;
        }

        ConectorBD.Instance.EliminarHabitoPorID(idHabito);
        Destroy(gameObject);

        Debug.Log($"🗑️ Hábito eliminado correctamente: {idHabito}");
    }
}

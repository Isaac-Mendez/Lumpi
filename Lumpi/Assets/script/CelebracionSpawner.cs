using UnityEngine;

public class CelebracionSpawner : MonoBehaviour
{
    [Header("Referencias")]
    public Transform content;              // Content del ScrollView
    public GameObject panelPrefab;         // Prefab: PanelCelebracion

    void Start()
    {
        // ================================================================
        // ========== MOSTRAR ÚLTIMO HÁBITO FINALIZADO ====================
        // ================================================================
        string ultimo = PlayerPrefs.GetString("UltimoHabitoFinalizado", "");

        if (!string.IsNullOrEmpty(ultimo))
        {
            CrearPanelCelebracion(ultimo);
            PlayerPrefs.DeleteKey("UltimoHabitoFinalizado");
        }

        // ================================================================
        // ========== CARGAR HÁBITOS FINALIZADOS DESDE LA BD ===============
        // ================================================================
        if (ConectorBD.Instance == null)
        {
            Debug.LogError("❌ ConectorBD no disponible.");
            return;
        }

        var lista = ConectorBD.Instance.ObtenerHabitosFinalizados();

        foreach (string nombre in lista)
        {
            CrearPanelCelebracion(nombre);
        }
    }

    // ================================================================
    // ========== MÉTODO PARA CREAR PANEL DE CELEBRACIÓN ===============
    // ================================================================
    public void CrearPanelCelebracion(string nombreHabito)
    {
        GameObject panel = Instantiate(panelPrefab, content);

        PanelCelebracionUI ui = panel.GetComponent<PanelCelebracionUI>();

        if (ui != null)
        {
            ui.Configurar(nombreHabito);
        }
        else
        {
            Debug.LogError("❌ El prefab no tiene PanelCelebracionUI.");
        }
    }
}



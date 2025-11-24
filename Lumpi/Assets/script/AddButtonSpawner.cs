using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class AddButtonSpawner : MonoBehaviour
{
    [Header("Referencias")]
    public Button addButton;          // Botón "Añadir"
    public Transform content;         // Content del ScrollView
    public GameObject buttonPrefab;   // Prefab del ItemButton

    void Start()
    {
        addButton.onClick.RemoveAllListeners();
        addButton.onClick.AddListener(AddNewButton);

        // Repintar hábitos cargados desde BD
        if (ConectorBD.HabitosGuardados.Count > 0)
        {
            Debug.Log($"🔄 Repintando {ConectorBD.HabitosGuardados.Count} hábitos...");

            foreach (Transform child in content)
                Destroy(child.gameObject);

            foreach (var h in ConectorBD.HabitosGuardados)
                CrearBotonHabito(h.id, h.nombre, h.duracion, h.finalizado);
        }
    }

    // ================================================================
    // ====================   CREACIÓN NUEVA   ========================
    // ================================================================
    void AddNewButton()
    {
        string nombreHabito = "Hábito " + (content.childCount + 1);
        int duracion = 60; // 60 minutos por defecto
        bool finalizado = false;

        // Generar ID nuevo
        string nuevoID = ConectorBD.Instance.GenerarApodoHabito();

        // Crear UI
        GameObject go = Instantiate(buttonPrefab, content);

        // Configurar comportamiento
        ItemButton item = go.GetComponent<ItemButton>();
        if (item != null)
        {
            item.SetupFromData(nuevoID, nombreHabito, duracion, finalizado);
        }
        else
        {
            Debug.LogError("❌ El prefab no contiene ItemButton.");
        }

        // Registrar en BD
        ConectorBD.Instance.RegistrarHabito(nombreHabito, duracion, finalizado, nuevoID);

        Debug.Log($"🟦 Hábito creado con ID: {nuevoID}");
    }

    // ================================================================
    // ==============    AL CARGAR DESDE LA BASE DE DATOS    =========
    // ================================================================
    private void CrearBotonHabito(string idHabito, string nombre, int duracionOriginal, bool finalizado)
{
    GameObject newButton = Instantiate(buttonPrefab, content);

    ItemButton item = newButton.GetComponent<ItemButton>();
    if (item == null)
    {
        Debug.LogError("❌ El prefab no contiene ItemButton.");
        return;
    }
    
    item.DesactivarStart();

    // ⚡ Cargar datos base del hábito
    item.SetupFromData(idHabito, nombre, duracionOriginal, finalizado);

    // ⚡ Intentar cargar tiempo restante real desde PlayerPrefs
    int minutosRestantes = PlayerPrefs.GetInt(idHabito + "_restante", -1);

    if (minutosRestantes <= 0)
    {
        // ⭕ Si NO hay tiempo guardado → usar duración editada
        item.ForzarTiempoRestante(duracionOriginal * 60);
        Debug.Log($"⏳ Sin PlayerPrefs → usando duración editada: {duracionOriginal} minutos");
    }
    else
    {
        // ⭕ Si hay tiempo guardado → usar tiempo restante real
        item.ForzarTiempoRestante(minutosRestantes * 60);
        Debug.Log($"⏳ PlayerPrefs encontrado → usando {minutosRestantes} minutos restantes");
    }
}

}


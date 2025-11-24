using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;

public class ItemButton : MonoBehaviour
{
    [Header("Referencias UI")]
    public Toggle toggleDone;
    public TMP_InputField labelInput;      // Nombre editable
    public TMP_InputField timerInput;      // Tiempo HH:MM:SS
    public Button startButton;
    public Button editButton;
    public Button deleteButton;

    [Header("Identificador del hábito")]
    public string idHabitoOriginal;

    [Header("Configuración")]
    private int durationInSeconds = 3600;
    private int currentTime;
    private bool isRunning = false;
    private Coroutine timerCoroutine;
    private bool isEditing = false;

    private int totalDurationMinutes = 60;

    private bool evitarStart = false;



    // ================================================================
    // =========================   START   =============================
    // ================================================================
    void Start()
    {
        if (evitarStart) return;
        AutoAssignIfMissing();

        if (labelInput == null || timerInput == null)
        {
            Debug.LogError("❌ Faltan InputFields en el prefab.");
            return;
        }

        if (toggleDone != null)
        {
            toggleDone.isOn = false;
            toggleDone.interactable = false;
        }

        labelInput.interactable = false;
        timerInput.interactable = false;

        if (string.IsNullOrEmpty(timerInput.text))
            timerInput.text = "01:00:00";

        // Asignación segura de listeners
        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(StartTimer);
        }

        if (editButton != null)
        {
            editButton.onClick.RemoveAllListeners();
            editButton.onClick.AddListener(ToggleEditMode);
        }

        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(DeleteHabit);
        }

        timerInput.onEndEdit.RemoveAllListeners();
        timerInput.onEndEdit.AddListener(OnTimerEndEdit);

        ParseTimerFromInput();
        UpdateTimerDisplay();
    }


    // ================================================================
    // ===============   AUTO-ASIGNACIÓN SEGURA   =====================
    // ================================================================
    private void AutoAssignIfMissing()
    {
        TMP_InputField[] inputs = GetComponentsInChildren<TMP_InputField>();
        Button[] buttons = GetComponentsInChildren<Button>();

        if (labelInput == null && inputs.Length > 0) labelInput = inputs[0];
        if (timerInput == null && inputs.Length > 1) timerInput = inputs[1];

        foreach (var b in buttons)
        {
            string n = b.gameObject.name.ToLower();
            if (startButton == null && n.Contains("start")) startButton = b;
            if (editButton == null && n.Contains("edit")) editButton = b;
            if (deleteButton == null && (n.Contains("del") || n.Contains("delete"))) deleteButton = b;
        }
    }


    // ================================================================
    // ===============   SETUP DESDE BD O CREACIÓN   ==================
    // ================================================================
public void SetupFromData(string id, string nombre, int duracionMinutos, bool finalizado)
{
    Debug.Log("SETUP llamado para: " + id);

    idHabitoOriginal = id;

    if (labelInput != null)
        labelInput.text = nombre;

    totalDurationMinutes = duracionMinutos;
    durationInSeconds = duracionMinutos * 60;

    // ❌ ANTES: currentTime = minutosRestantes * 60;
    // ✔ AHORA: solo cargamos la duración base (AddButtonSpawner aplica el tiempo real)
    currentTime = durationInSeconds;

    if (finalizado && toggleDone != null)
        toggleDone.isOn = true;

    UpdateTimerDisplay();

    isRunning = false;
    if (startButton != null) startButton.interactable = true;
    if (editButton != null) editButton.interactable = true;
}


public void ForzarTiempoRestante(int segundos)
{
    Debug.Log("FORZAR llamado: " + segundos + " segundos");

    currentTime = Mathf.Max(0, segundos);
    UpdateTimerDisplay();
}

public void DesactivarStart()
{
    evitarStart = true;
}


    // ================================================================
    // =========================   TIMER   =============================
    // ================================================================
public void StartTimer()
{
    if (isRunning) return;

    // 🔥 Recuperar tiempo real desde PlayerPrefs SI es válido
    if (!string.IsNullOrEmpty(idHabitoOriginal))
    {
        int mins = PlayerPrefs.GetInt(idHabitoOriginal + "_restante", -1);

        if (mins <= 0)
        {
            // ⭕ SIN PlayerPrefs válido → usar duración editada
            currentTime = durationInSeconds;
            Debug.Log("⏳ StartTimer usando duración editada: " + durationInSeconds);
        }
        else
        {
            // ⭕ Con PlayerPrefs → usar tiempo restante real
            currentTime = mins * 60;
            Debug.Log("⏳ StartTimer usando PlayerPrefs (mins): " + mins);
        }
    }
    else
    {
        ParseTimerFromInput();
        currentTime = durationInSeconds;
    }

    isRunning = true;

    // Guardar fecha de inicio solo la primera vez
    if (PlayerPrefs.GetInt(idHabitoOriginal + "_run", 0) != 1)
        ConectorBD.Instance.GuardarFechaInicioHabito(idHabitoOriginal);

    startButton.interactable = false;
    editButton.interactable = false;

    labelInput.interactable = false;
    timerInput.interactable = false;
    if (toggleDone != null) toggleDone.interactable = false;

    timerCoroutine = StartCoroutine(TimerCountdown());

}

    private Coroutine StartCoroutine(Func<IEnumerator> timerCountdown)
    {
        throw new NotImplementedException();
    }


    //////////////////////////////////////////Aqui
    /// 
    private IEnumerator TimerCountdown()
{
    // Marcar como corriendo
    PlayerPrefs.SetInt(idHabitoOriginal + "_run", 1);

    if (currentTime <= 0)
        currentTime = durationInSeconds;

    while (currentTime > 0)
    {
        UpdateTimerDisplay();
        yield return new WaitForSeconds(1);
        currentTime--;

        int mins = Mathf.CeilToInt(currentTime / 60f);
        PlayerPrefs.SetInt(idHabitoOriginal + "_restante", mins);
    }

    // Finalizar
    timerInput.text = "00:00:00";
    if (toggleDone != null) toggleDone.isOn = true;

    isRunning = false;

    PlayerPrefs.SetInt(idHabitoOriginal + "_run", 0);

    startButton.interactable = true;
    editButton.interactable = true;

    RegistrarHabitoEnBD(labelInput.text, totalDurationMinutes, true);

    ConectorBD.Instance.EjecutarMoverHabitosFinalizados();
    ConectorBD.Instance.EliminarHabitoPorID(idHabitoOriginal);

    Destroy(gameObject);
}


    // ================================================================
    // =======================   EDICIÓN   =============================
    // ================================================================
  private void ToggleEditMode()
{
    if (isRunning) return;

    isEditing = !isEditing;

    labelInput.interactable = isEditing;
    timerInput.interactable = isEditing;

    if (!isEditing)
    {
        // Procesar nueva duración desde el input
        OnTimerEndEdit(timerInput.text);
        ParseTimerFromInput();

        // ⚡️ ACTUALIZAR TIEMPO REAL DEL TEMPORIZADOR
        currentTime = durationInSeconds;

        // ⚡️ GUARDAR NUEVO TIEMPO RESTANTE
        if (!string.IsNullOrEmpty(idHabitoOriginal))
            PlayerPrefs.SetInt(idHabitoOriginal + "_restante", totalDurationMinutes);

        UpdateTimerDisplay();

        // Guardar en la BD
        ConectorBD.Instance.RegistrarHabito(labelInput.text, totalDurationMinutes, false, idHabitoOriginal);
    }

    var tmp = editButton.GetComponentInChildren<TMP_Text>();
    if (tmp != null) tmp.text = isEditing ? "Guardar" : "Editar";
}



    // ================================================================
    // =======================   FORMATO TIEMPO   ======================
    // ================================================================
    private void OnTimerEndEdit(string input)
    {
        string digits = "";

        foreach (char c in input)
        {
            if (char.IsDigit(c)) digits += c;
            if (digits.Length >= 2) break;
        }

        if (digits.Length == 0) digits = "01";
        if (digits.Length == 1) digits = "0" + digits;

        timerInput.text = $"{digits}:00:00";
    }

    private void ParseTimerFromInput()
    {
        string txt = timerInput.text.Trim();

        if (txt.Length < 2)
        {
            durationInSeconds = 3600;
            totalDurationMinutes = 60;
            return;
        }

        int hours;
        if (!int.TryParse(txt.Substring(0, 2), out hours))
            hours = 1;

        durationInSeconds = Mathf.Max(60, hours * 3600);
        totalDurationMinutes = Mathf.Max(1, hours * 60);
    }

    private void UpdateTimerDisplay()
    {
        int hours = currentTime / 3600;
        int minutes = (currentTime % 3600) / 60;
        int seconds = currentTime % 60;

        timerInput.text = $"{hours:00}:{minutes:00}:{seconds:00}";
    }


    // ================================================================
    // =======================   BASE DE DATOS   =======================
    // ================================================================
    private void RegistrarHabitoEnBD(string nombre, int duracion, bool finalizado)
    {
        if (string.IsNullOrEmpty(idHabitoOriginal))
            idHabitoOriginal = ConectorBD.Instance.GenerarApodoHabito();

        ConectorBD.Instance.RegistrarHabito(nombre, duracion, finalizado, idHabitoOriginal);
    }


    // ================================================================
    // ===========================   ELIMINAR   ========================
    // ================================================================
    public void DeleteHabit()
    {
        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);

        if (!string.IsNullOrEmpty(idHabitoOriginal))
            ConectorBD.Instance.EliminarHabitoPorID(idHabitoOriginal);

        Destroy(gameObject);
    }

    void OnApplicationPause(bool paused)
{
    if (paused && !string.IsNullOrEmpty(idHabitoOriginal))
    {
        int mins = Mathf.CeilToInt(currentTime / 60f);
        PlayerPrefs.SetInt(idHabitoOriginal + "_restante", mins);
        PlayerPrefs.Save();   // 🔥 NECESARIO EN ANDROID
    }
}

void OnApplicationQuit()
{
    if (!string.IsNullOrEmpty(idHabitoOriginal))
    {
        int mins = Mathf.CeilToInt(currentTime / 60f);
        PlayerPrefs.SetInt(idHabitoOriginal + "_restante", mins);
        PlayerPrefs.Save();
    }
}

   
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ItemButton : MonoBehaviour
{
    [Header("Referencias UI")]
    public Toggle toggleDone;
    public TMP_InputField labelInput;  // Nombre editable
    public TMP_InputField timerInput;  // Solo editable las horas (HH)
    public Button startButton;
    public Button editButton;
    public Button deleteButton;

    [Header("Configuración")]
    private int durationInSeconds = 3600; // 1 hora por defecto (3600s)
    private int currentTime;
    private bool isRunning = false;
    private Coroutine timerCoroutine;
    private bool isEditing = false;
    
    // Almacena la duración total en minutos para el registro en BD
    private int totalDurationMinutes = 60; 

    void Start()
    {
        AutoAssignIfMissing();

        if (labelInput == null || timerInput == null)
        {
            Debug.LogError($"❌ Faltan referencias de InputFields en '{gameObject.name}'. Asigna LabelInput y TimerInput en el Inspector.");
            return;
        }

        // Estado inicial del toggle
        if (toggleDone != null)
        {
            toggleDone.isOn = false;
            toggleDone.interactable = false;
        }

        // por defecto no editable
        labelInput.interactable = false;
        timerInput.interactable = false;

        if (string.IsNullOrEmpty(timerInput.text))
            timerInput.text = "01:00:00";

        if (startButton != null) startButton.onClick.AddListener(StartTimer);
        if (editButton != null) editButton.onClick.AddListener(ToggleEditMode);
        if (deleteButton != null) deleteButton.onClick.AddListener(DeleteHabit);

        // usar onEndEdit para validar/formatear cuando el usuario termine
        timerInput.onEndEdit.AddListener(OnTimerEndEdit);

        ParseTimerFromInput();
        UpdateTimerDisplay();
        
        // 🛑 REGISTRO DEL HÁBITO CREADO (Estado inicial: NO FINALIZADO)
        // Se asume que este Start() se ejecuta al crear el ítem.
        RegistrarHabitoEnBD(labelInput.text, totalDurationMinutes, false);
    }
    
    private void AutoAssignIfMissing()
    {
        if (labelInput == null)
        {
            TMP_InputField[] inputs = GetComponentsInChildren<TMP_InputField>();
            foreach (var input in inputs)
            {
                string n = input.gameObject.name.ToLower();
                if (n.Contains("label") || n.Contains("name") || n.Contains("label_tmp"))
                    labelInput = input;
                else if (n.Contains("timer") || n.Contains("time") || n.Contains("timer_tmp"))
                    timerInput = input;
            }
            if (labelInput == null && inputs.Length >= 1) labelInput = inputs[0];
            if (timerInput == null && inputs.Length >= 2) timerInput = inputs[1];
        }

        Button[] buttons = GetComponentsInChildren<Button>();
        foreach (var b in buttons)
        {
            string n = b.gameObject.name.ToLower();
            if (startButton == null && (n.Contains("start") || n.Contains("iniciar"))) startButton = b;
            if (editButton == null && (n.Contains("edit") || n.Contains("editar"))) editButton = b;
            if (deleteButton == null && (n.Contains("del") || n.Contains("delete") || n.Contains("borrar"))) deleteButton = b;
        }
    }


    public void StartTimer()
    {
        if (isRunning) return;

        ParseTimerFromInput();

        isRunning = true;
        if (startButton != null) startButton.interactable = false;
        if (editButton != null) editButton.interactable = false;
        labelInput.interactable = false;
        timerInput.interactable = false;
        if (toggleDone != null) toggleDone.interactable = false;

        timerCoroutine = StartCoroutine(TimerCountdown());
    }

    private IEnumerator TimerCountdown()
    {
        currentTime = durationInSeconds;

        while (currentTime > 0)
        {
            UpdateTimerDisplay();
            yield return new WaitForSeconds(1f);
            currentTime--;
        }

        if (timerInput != null) timerInput.text = "00:00:00";
        if (toggleDone != null) toggleDone.isOn = true;

        isRunning = false;
        if (startButton != null) startButton.interactable = true;
        if (editButton != null) editButton.interactable = true;
        
        // 🛑 REGISTRO DE HÁBITO FINALIZADO
        // Se registra el hábito con el estado FINALIZADO (true)
        RegistrarHabitoEnBD(labelInput.text, totalDurationMinutes, true);
    }

    private void ToggleEditMode()
    {
        if (isRunning) return;

        isEditing = !isEditing;
        labelInput.interactable = isEditing;
        timerInput.interactable = isEditing;

        if (isEditing)
        {
            // prepara el input para editar solo las horas:
            // deja el texto en formato HH:00:00 y coloca el caret al principio
            if (string.IsNullOrEmpty(timerInput.text)) timerInput.text = "01:00:00";
            // activar input field para que el usuario pueda escribir
            timerInput.ActivateInputField();
            // poner caret al inicio (la mayoría de versiones de TMP respetan caretPosition)
            timerInput.caretPosition = 0;
        }
        else
        {
            // usuario terminó edición -> validar y formatear
            OnTimerEndEdit(timerInput.text);
            ParseTimerFromInput();
            UpdateTimerDisplay();
        }

        // Cambiar texto del botón Edit
        if (editButton != null)
        {
            var tmp = editButton.GetComponentInChildren<TMP_Text>();
            if (tmp != null) tmp.text = isEditing ? "Guardar" : "Editar";
        }
    }

    // Formatea y valida cuando el usuario termina de editar (Enter o pierde foco)
    private void OnTimerEndEdit(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            timerInput.text = "01:00:00";
            return;
        }

        // Extraer solo dígitos del input (en orden) hasta 2
        string digits = "";
        foreach (char c in input)
        {
            if (char.IsDigit(c)) digits += c;
            if (digits.Length >= 2) break;
        }

        if (digits.Length == 0) digits = "01";
        else if (digits.Length == 1) digits = "0" + digits;

        // Formato fijo HH:00:00
        string formatted = $"{digits}:00:00";

        // Asignar el resultado (sin provocar reentrada infinita)
        timerInput.text = formatted;
    }

    private void ParseTimerFromInput()
    {
        if (timerInput == null)
        {
            durationInSeconds = 3600;
            totalDurationMinutes = 60;
            return;
        }

        string txt = timerInput.text.Trim();
        int hours = 0;

        if (txt.Length >= 2 && int.TryParse(txt.Substring(0, 2), out hours))
        {
            // La duración en segundos se actualiza
            durationInSeconds = Mathf.Max(60, hours * 3600); // Mínimo 1 minuto (60s)
            // La duración total en minutos para la BD se actualiza
            totalDurationMinutes = hours * 60; 
        }
        else
        {
            durationInSeconds = 3600;
            totalDurationMinutes = 60;
        }
    }

    private void UpdateTimerDisplay()
    {
        int hours = currentTime / 3600;
        int minutes = (currentTime % 3600) / 60;
        int seconds = currentTime % 60;
        if (timerInput != null)
            timerInput.text = $"{hours:00}:{minutes:00}:{seconds:00}";
    }

    // -------------------------------------------------------------
    // FUNCIÓN DE LLAMADA A LA BASE DE DATOS
    // -------------------------------------------------------------
    private void RegistrarHabitoEnBD(string nombre, int duracion, bool finalizado)
    {
        if (ConectorBD.Instance == null)
        {
            Debug.LogError("❌ ConectorBD no está disponible. No se pudo registrar el hábito.");
            return;
        }

        ConectorBD.Instance.RegistrarHabito(nombre, duracion, finalizado);
    }
    
    private void RestrictTimerInput(string value)
    {
        // ya no se usa; mantenido por compatibilidad (no ligado)
    }

    public void DeleteHabit()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }

        RectTransform parentRect = transform.parent as RectTransform;
        Destroy(gameObject);

        if (parentRect != null) StartCoroutine(DelayedRebuild(parentRect));
    }

    private IEnumerator DelayedRebuild(RectTransform parentRect)
    {
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
    }
}
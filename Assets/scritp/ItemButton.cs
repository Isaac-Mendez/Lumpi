
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
    private int durationInSeconds = 3600; // 1 hora por defecto
    private int currentTime;
    private bool isRunning = false;
    private Coroutine timerCoroutine;
    private bool isEditing = false;

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

        labelInput.interactable = false;
        timerInput.interactable = false;

        if (string.IsNullOrEmpty(timerInput.text))
            timerInput.text = "01:00:00";

        if (startButton != null) startButton.onClick.AddListener(StartTimer);
        if (editButton != null) editButton.onClick.AddListener(ToggleEditMode);
        if (deleteButton != null) deleteButton.onClick.AddListener(DeleteHabit);

        // Evento para limitar la edición solo a las horas
        timerInput.onValueChanged.AddListener(RestrictTimerInput);

        ParseTimerFromInput();
        UpdateTimerDisplay();
    }

    private void AutoAssignIfMissing()
    {
        if (labelInput == null)
            labelInput = GetComponentInChildren<TMP_InputField>();

        if (labelInput != null && timerInput == null)
        {
            TMP_InputField[] inputs = GetComponentsInChildren<TMP_InputField>();
            if (inputs.Length >= 2)
            {
                foreach (var input in inputs)
                {
                    string n = input.gameObject.name.ToLower();
                    if (n.Contains("label") || n.Contains("name") || n.Contains("label_tmp"))
                        labelInput = input;
                    else if (n.Contains("timer") || n.Contains("time") || n.Contains("timer_tmp"))
                        timerInput = input;
                }
            }
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
        startButton.interactable = false;
        editButton.interactable = false;
        labelInput.interactable = false;
        timerInput.interactable = false;
        toggleDone.interactable = false;

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

        timerInput.text = "00:00:00";
        if (toggleDone != null) toggleDone.isOn = true;

        isRunning = false;
        startButton.interactable = true;
        editButton.interactable = true;
    }

    private void ToggleEditMode()
    {
        if (isRunning) return;

        isEditing = !isEditing;
        labelInput.interactable = isEditing;
        timerInput.interactable = isEditing;

        if (!isEditing)
        {
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

    private void ParseTimerFromInput()
    {
        if (timerInput == null)
        {
            durationInSeconds = 3600;
            return;
        }

        string txt = timerInput.text.Trim();

        // solo tomamos las horas (primeros 2 dígitos)
        if (txt.Length >= 2 && int.TryParse(txt.Substring(0, 2), out int hours))
            durationInSeconds = Mathf.Max(3600, hours * 3600); // al menos 1 hora
        else
            durationInSeconds = 3600;
    }

    private void UpdateTimerDisplay()
    {
        int hours = currentTime / 3600;
        int minutes = (currentTime % 3600) / 60;
        int seconds = currentTime % 60;
        if (timerInput != null)
            timerInput.text = $"{hours:00}:{minutes:00}:{seconds:00}";
    }

    private void RestrictTimerInput(string value)
    {
        // siempre fuerza el formato HH:00:00 y limita la entrada a 2 dígitos
        string digits = "";
        foreach (char c in value)
        {
            if (char.IsDigit(c)) digits += c;
            if (digits.Length >= 2) break;
        }

        if (digits.Length == 0) digits = "00";
        else if (digits.Length == 1) digits = "0" + digits;

        timerInput.text = $"{digits}:00:00";
        timerInput.caretPosition = 2; // mueve el cursor después de las horas
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

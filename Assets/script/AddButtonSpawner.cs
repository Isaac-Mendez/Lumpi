using UnityEngine;

using UnityEngine.UI;

using TMPro;   // 👈 Importante para TMP_Text



public class AddButtonSpawner : MonoBehaviour

{

    public Button addButton;         // Botón "Añadir"

    public Transform content;        // Content del ScrollView

    public GameObject buttonPrefab;  // Prefab del botón a instanciar



    void Start()

    {

        addButton.onClick.AddListener(AddNewButton);

    }



    void AddNewButton()

    {

        GameObject newButton = Instantiate(buttonPrefab, content);



        // Buscar el texto TMP dentro del botón

        TMP_Text tmpText = newButton.GetComponentInChildren<TMP_Text>();

        if (tmpText != null)

        {

            tmpText.text = "Ediatr" + content.childCount;

        }

        else

        {

            Debug.LogWarning("⚠️ El prefab no tiene un TMP_Text asignado.");

        }

    }

}

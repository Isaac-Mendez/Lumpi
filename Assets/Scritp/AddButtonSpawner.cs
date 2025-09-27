using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AddButtonSpawner : MonoBehaviour
{
    public Button addButton;      // El botón "Añadir"
    public Transform content;     // El Content del ScrollView
    public GameObject buttonPrefab; // Prefab del botón a instanciar

    void Start()
    {
        addButton.onClick.AddListener(AddNewButton);
    }

    void AddNewButton()
    {
        GameObject newButton = Instantiate(buttonPrefab, content);
        newButton.GetComponentInChildren<Text>().text = "Item " + content.childCount;
    }
}


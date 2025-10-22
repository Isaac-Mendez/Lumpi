using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class cambio : MonoBehaviour
{

    public void CambiarEscena(string nombre)
    {
        SceneManager.LoadScene(nombre);
    }
}
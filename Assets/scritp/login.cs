using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class login : MonoBehaviour
{
    [Header("Referencias UI")]
    public TMP_InputField usuario;
    public TMP_InputField contrasena;
    public Button Entrar;

    [Header("Credenciales Correctas")]
    public string usuarioCorrecto = "admin";
    public string passwordCorrecto = "1234";

    void Start()
    {
        // Aquí conectamos el botón al método VerificarLogin()
        Entrar.onClick.AddListener(VerificarLogin);
    }

    void VerificarLogin()
    {
        string usuarioIngresado = usuario.text.Trim();
        string passwordIngresado = contrasena.text.Trim();

        Debug.Log("Intentando iniciar sesión con usuario: " + usuarioIngresado);

        if (usuarioIngresado == usuarioCorrecto && passwordIngresado == passwordCorrecto)
        {
            Debug.Log("Login correcto. Cambiando de escena...");
            CambiarEscena("crearhabitos");
        }
        else
        {
            Debug.Log("Credenciales incorrectas.");
        }
    }

    public void CambiarEscena(string nombre)
    {
        SceneManager.LoadScene(nombre);
    }
}



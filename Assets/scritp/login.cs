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

   

    void VerificarLogin()
    {
        string usuarioIngresado = usuario.text.Trim();
        string passwordIngresado = contrasena.text.Trim();

        if (usuarioIngresado == usuarioCorrecto && passwordIngresado == passwordCorrecto)
        {
            // Si es correcto, cambiamos de escena
            CambiarEscena("crearhabitos");
        }
        
    }

    public void CambiarEscena(string nombre)
    {
        SceneManager.LoadScene(nombre);
    }
}



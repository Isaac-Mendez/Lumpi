using UnityEngine;
using TMPro;

public class PanelCelebracionUI : MonoBehaviour
{
    public TextMeshProUGUI titulo;
    public TextMeshProUGUI nombreHabito;

    public void Configurar(string nombre)
    {
        if (titulo != null)
            titulo.text = "¡Felicitaciones, has terminado:";

        if (nombreHabito != null)
            nombreHabito.text = nombre;
    }
}

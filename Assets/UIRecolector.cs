using UnityEngine;
using TMPro;

public class UIRecolector : MonoBehaviour
{
    public TextMeshProUGUI textoRecolectando;

    void Update()
    {
        var peones = GameObject.FindObjectsOfType<Peon>();
        int total = 0;
        int activos = 0;

        foreach (var peon in peones)
        {
            if (peon.esJugador)
            {
                total++;
                if (peon.EstaRecolectando()) activos++;
            }
        }

        textoRecolectando.text = $"Recolectando: {activos} / {total}";
    }
}

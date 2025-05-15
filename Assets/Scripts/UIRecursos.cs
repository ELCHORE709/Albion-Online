using UnityEngine;
using TMPro;

public class UIRecursos : MonoBehaviour
{
    public TextMeshProUGUI textoCristales;

    void Update()
    {
        if (GameManager.instancia != null)
            textoCristales.text = $"{GameManager.instancia.Recursos}";
    }
}

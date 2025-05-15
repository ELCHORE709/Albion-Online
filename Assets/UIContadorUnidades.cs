using UnityEngine;
using TMPro;

public class UIContadorUnidades : MonoBehaviour
{
    public TextMeshProUGUI textoUnidades;

    void Update()
    {
        int actual = GameManager.instancia.GetCantidadUnidadesJugador();
        int maximo = GameManager.instancia.limiteUnidadesJugador;
        textoUnidades.text = $"Unidades: {actual} / {maximo}";
    }
}

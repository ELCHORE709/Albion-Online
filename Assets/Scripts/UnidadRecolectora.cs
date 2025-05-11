using System.Collections;
using UnityEngine;

public class Peon : MonoBehaviour
{
    public float vida = 100f;
    public float velocidad = 3f;
    public bool esJugador = true;

    private NodoRecurso nodoActual;
    private Coroutine recoleccion;

    public void IniciarRecoleccion(NodoRecurso nodo)
    {
        nodoActual = nodo;
        recoleccion = StartCoroutine(Recolectar());
    }

    public void DetenerRecoleccion()
    {
        if (recoleccion != null) StopCoroutine(recoleccion);
        nodoActual = null;
    }

    private IEnumerator Recolectar()
    {
        while (nodoActual != null)
        {
            yield return new WaitForSeconds(1f);
            if (nodoActual.Extraer(2))
            {
                GameManager.instancia.AgregarRecursos(2);
            }
        }
    }
}

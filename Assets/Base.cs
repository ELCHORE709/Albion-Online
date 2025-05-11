using UnityEngine;

public class Base : MonoBehaviour
{
    public float vida = 1000f;
    public bool esJugador = true;
    public GameObject[] prefabsUnidades;
    public Transform puntoSpawn;
    public float tiempoEntreUnidades = 5f;
    public int limiteUnidades = 30;

    private float tiempoUltimaCreacion = -999f;

    void Update()
    {
        if (!esJugador)
        {
            if (Time.time - tiempoUltimaCreacion >= tiempoEntreUnidades)
            {
                CrearUnidadAleatoria();
                tiempoUltimaCreacion = Time.time;
            }
        }
    }

    public void CrearUnidadJugador(int tipo)
    {
        if (!esJugador || Time.time - tiempoUltimaCreacion < tiempoEntreUnidades) return;
        if (!GameManager.instancia.PuedeCrearUnidad()) return;

        int costo = GameManager.instancia.GetCostoUnidad(tipo);
        if (GameManager.instancia.Recursos < costo) return;

        GameObject prefab = prefabsUnidades[tipo];
        GameObject unidad = Instantiate(prefab, puntoSpawn.position, Quaternion.identity);
        GameManager.instancia.RegistrarUnidad(unidad);
        GameManager.instancia.RestarRecursos(costo);

        tiempoUltimaCreacion = Time.time;
    }

    void CrearUnidadAleatoria()
    {
        if (!GameManager.instancia.PuedeCrearUnidad()) return;

        int tipo = Random.Range(0, prefabsUnidades.Length);
        GameObject prefab = prefabsUnidades[tipo];
        GameObject unidad = Instantiate(prefab, puntoSpawn.position, Quaternion.identity);
        if (unidad.TryGetComponent(out Rey r)) r.esJugador = false;
        if (unidad.TryGetComponent(out Alfil a)) a.esJugador = false;
        if (unidad.TryGetComponent(out Reina q)) q.esJugador = false;

        GameManager.instancia.RegistrarUnidad(unidad);
    }

    public void RecibirDaño(float cantidad)
    {
        vida -= cantidad;
        if (vida <= 0)
        {
            GameManager.instancia.BaseDestruida(esJugador);
            Destroy(gameObject);
        }
    }
}

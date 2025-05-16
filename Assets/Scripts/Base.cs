using UnityEngine;
using UnityEngine.UI;

public class Base : MonoBehaviour
{
    public float vida = 1000f;
    public bool esJugador = true;
    public Transform puntoSpawn;
    public float tiempoEntreUnidades = 5f;

    [Header("Prefabs del Jugador")]
    public GameObject prefabRey;
    public GameObject prefabAlfil;
    public GameObject prefabReina;

    [Header("Prefabs del Enemigo")]
    public GameObject[] prefabsEnemigo;

    [Header("UI")]
    public GameObject panelBotones;
    public Button botonCrearRey;
    public Button botonCrearAlfil;
    public Button botonCrearReina;

    [Header("Visual de Selección")]
    public GameObject seleccionVisual;

    [Header("Barra visual de vida")]
    public Transform barraVisual; 
    public float anchoOriginal = 1f; 

    private float tiempoUltimaCreacion = -999f;

    void Start()
    {
        if (esJugador && panelBotones != null)
        {
            panelBotones.SetActive(false);

            botonCrearRey.onClick.AddListener(() => CrearUnidadJugador(prefabRey, GameManager.instancia.costoRey));
            botonCrearAlfil.onClick.AddListener(() => CrearUnidadJugador(prefabAlfil, GameManager.instancia.costoAlfil));
            botonCrearReina.onClick.AddListener(() => CrearUnidadJugador(prefabReina, GameManager.instancia.costoReina));
        }

        if (seleccionVisual != null)
            seleccionVisual.SetActive(false);

        if (barraVisual != null)
            anchoOriginal = barraVisual.localScale.x;
    }

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

    public void CrearUnidadJugador(GameObject prefab, int costo)
    {
        if (!esJugador || Time.time - tiempoUltimaCreacion < tiempoEntreUnidades) return;
        if (!GameManager.instancia.PuedeCrearUnidad(true) || GameManager.instancia.Recursos < costo) return;

        GameObject unidad = Instantiate(prefab, puntoSpawn.position, Quaternion.identity);
        GameManager.instancia.RegistrarUnidad(unidad, true);
        GameManager.instancia.RestarRecursos(costo);

        tiempoUltimaCreacion = Time.time;
    }

    void CrearUnidadAleatoria()
    {
        if (!GameManager.instancia.PuedeCrearUnidad(false)) return;

        int tipo = Random.Range(0, prefabsEnemigo.Length);
        GameObject prefab = prefabsEnemigo[tipo];
        GameObject unidad = Instantiate(prefab, puntoSpawn.position, Quaternion.identity);

        GameManager.instancia.RegistrarUnidad(unidad, false);
    }

    public void RecibirDaño(float cantidad)
    {
        vida -= cantidad;

        if (barraVisual != null)
        {
            float factor = Mathf.Clamp01(vida / 10000f);
            Vector3 escala = barraVisual.localScale;
            escala.x = anchoOriginal * factor;
            barraVisual.localScale = escala;
        }

        if (vida <= 0)
        {
            GameManager.instancia.BaseDestruida(esJugador);
            Destroy(gameObject);
        }
    }

    public void MostrarUI(bool mostrar)
    {
        if (esJugador && panelBotones != null)
            panelBotones.SetActive(mostrar);

        if (seleccionVisual != null)
            seleccionVisual.SetActive(mostrar);
    }
}

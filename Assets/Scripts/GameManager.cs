using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager instancia;

    [Header("Recursos")]
    public int recursosJugador = 0;
    public int Recursos => recursosJugador;

    [Header("Unidades y límites")]
    public int limiteUnidadesJugador = 30;
    public int limiteUnidadesEnemigo = 30;

    private List<GameObject> unidadesJugador = new();
    private List<GameObject> unidadesEnemigo = new();

    [Header("Costos por tipo de unidad")]
    public int costoRey = 50;
    public int costoAlfil = 30;
    public int costoReina = 70;

    [Header("Pantallas de resultado (opcional)")]
    public GameObject pantallaVictoria;
    public GameObject pantallaDerrota;

    void Awake()
    {
        if (instancia == null) instancia = this;
        else Destroy(gameObject);
    }

    public void AgregarRecursos(int cantidad)
    {
        recursosJugador += cantidad;
        Debug.Log("Recursos: " + recursosJugador);
    }

    public void RestarRecursos(int cantidad)
    {
        recursosJugador -= cantidad;
        if (recursosJugador < 0) recursosJugador = 0;
    }

    public bool PuedeCrearUnidad(bool esJugador)
    {
        if (esJugador)
        {
            unidadesJugador.RemoveAll(obj => obj == null);
            return unidadesJugador.Count < limiteUnidadesJugador;
        }
        else
        {
            unidadesEnemigo.RemoveAll(obj => obj == null);
            return unidadesEnemigo.Count < limiteUnidadesEnemigo;
        }
    }

    public void RegistrarUnidad(GameObject unidad, bool esJugador)
    {
        if (esJugador)
            unidadesJugador.Add(unidad);
        else
            unidadesEnemigo.Add(unidad);
    }

    // Versión anterior por compatibilidad si ya usas esta firma en otras partes
    public void RegistrarUnidad(GameObject unidad)
    {
        bool esJugador = true;

        if (unidad.TryGetComponent(out Rey r)) esJugador = r.esJugador;
        else if (unidad.TryGetComponent(out Alfil a)) esJugador = a.esJugador;
        else if (unidad.TryGetComponent(out Reina q)) esJugador = q.esJugador;

        RegistrarUnidad(unidad, esJugador);
    }

    public int GetCostoUnidad(int tipo)
    {
        return tipo switch
        {
            0 => costoRey,
            1 => costoAlfil,
            2 => costoReina,
            _ => 0
        };
    }

    public void BaseDestruida(bool baseEraDelJugador)
    {
        if (baseEraDelJugador)
        {
            Debug.Log("❌ Has perdido");
            if (pantallaDerrota != null) pantallaDerrota.SetActive(true);
        }
        else
        {
            Debug.Log("🏆 Has ganado");
            if (pantallaVictoria != null) pantallaVictoria.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    public int GetCantidadUnidadesJugador()
    {
        unidadesJugador.RemoveAll(obj => obj == null);
        return unidadesJugador.Count;
    }

}

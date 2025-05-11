using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager instancia;

    [Header("Recursos")]
    public int recursosJugador = 0;
    public int Recursos => recursosJugador; // ✅ Propiedad para acceso externo

    [Header("Unidades")]
    public int limiteUnidades = 30;
    private List<GameObject> unidadesEnJuego = new();

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

    public bool PuedeCrearUnidad()
    {
        unidadesEnJuego.RemoveAll(obj => obj == null);
        return unidadesEnJuego.Count < limiteUnidades;
    }

    public void RegistrarUnidad(GameObject unidad)
    {
        unidadesEnJuego.Add(unidad);
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
}

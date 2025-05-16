using System.Collections.Generic;
using UnityEngine;

public class CambioDeEstadoInteligente : MonoBehaviour
{
    public float intervaloEvaluacion = 30f;
    public float ventajaAtaque = 1.2f; // 20% más fuerte
    public float distanciaAlertaBase = 15f;
    public int cantidadCambioEstado = 3;

    private float tiempoUltimaEvaluacion;

    void Update()
    {
        if (Time.time - tiempoUltimaEvaluacion >= intervaloEvaluacion)
        {
            EvaluarSituacion();
            tiempoUltimaEvaluacion = Time.time;
        }
    }

    void EvaluarSituacion()
    {
        List<GameObject> enemigos = new();
        List<GameObject> jugadores = new();
        List<GameObject> patrullando = new();
        List<GameObject> atacando = new();

        float fuerzaEnemigo = 0, fuerzaJugador = 0;

        foreach (var unidad in FindObjectsOfType<MonoBehaviour>())
        {
            GameObject go = unidad.gameObject;

            if (unidad is Rey r)
            {
                if (r.esJugador) { jugadores.Add(go); fuerzaJugador += r.vida; }
                else { enemigos.Add(go); fuerzaEnemigo += r.vida; if (r.estadoActual == EstadoUnidad.Patrulla) patrullando.Add(go); else if (r.estadoActual == EstadoUnidad.Ataque) atacando.Add(go); }
            }
            else if (unidad is Reina q)
            {
                if (q.esJugador) { jugadores.Add(go); fuerzaJugador += q.vida; }
                else { enemigos.Add(go); fuerzaEnemigo += q.vida; if (q.estadoActual == EstadoUnidad.Patrulla) patrullando.Add(go); else if (q.estadoActual == EstadoUnidad.Ataque) atacando.Add(go); }
            }
            else if (unidad is Alfil a)
            {
                if (a.esJugador) { jugadores.Add(go); fuerzaJugador += a.vida; }
                else { enemigos.Add(go); fuerzaEnemigo += a.vida; if (a.estadoActual == EstadoUnidad.Patrulla) patrullando.Add(go); else if (a.estadoActual == EstadoUnidad.Ataque) atacando.Add(go); }
            }
        }


        if (fuerzaEnemigo > fuerzaJugador * ventajaAtaque)
        {
            CambiarEstadoDeGrupo(patrullando, EstadoUnidad.Ataque, cantidadCambioEstado);
        }
        else if (fuerzaJugador > fuerzaEnemigo * 1.2f)
        {
            CambiarEstadoDeGrupo(atacando, EstadoUnidad.Patrulla, cantidadCambioEstado);
        }

        Transform baseEnemiga = EncontrarBase(false);
        if (baseEnemiga != null)
        {
            foreach (var unidad in patrullando)
            {
                foreach (var jugador in jugadores)
                {
                    if (Vector3.Distance(baseEnemiga.position, jugador.transform.position) < distanciaAlertaBase)
                    {
                        CambiarEstado(unidad, EstadoUnidad.Patrulla);
                        break;
                    }
                }
            }
        }
    }

    void CambiarEstadoDeGrupo(List<GameObject> lista, EstadoUnidad nuevoEstado, int cantidad)
    {
        for (int i = 0; i < cantidad && lista.Count > 0; i++)
        {
            int index = Random.Range(0, lista.Count);
            GameObject unidad = lista[index];
            lista.RemoveAt(index);

            CambiarEstado(unidad, nuevoEstado);
        }
    }

    void CambiarEstado(GameObject unidad, EstadoUnidad estado)
    {
        if (unidad.TryGetComponent(out Rey r) && !r.esJugador)
        {
            r.CambiarEstado(estado);
        }
        else if (unidad.TryGetComponent(out Reina q) && !q.esJugador)
        {
            q.CambiarEstado(estado);
        }
        else if (unidad.TryGetComponent(out Alfil a) && !a.esJugador)
        {
            a.CambiarEstado(estado);
        }
    }

    Transform EncontrarBase(bool delJugador)
    {
        foreach (var b in FindObjectsOfType<Base>())
        {
            if (b.esJugador == delJugador)
                return b.transform;
        }
        return null;
    }
}

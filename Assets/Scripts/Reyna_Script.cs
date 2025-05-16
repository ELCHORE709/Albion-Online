using UnityEngine;
using UnityEngine.AI;

public class Reina : MonoBehaviour
{
    public float vida = 80f;
    public float daño = 20f;
    public float velocidad = 3f;
    public float rangoDisparo = 8f;
    public float rangoAlerta = 10f;
    public float rangoPersecucionMaxima = 8f;
    public float distanciaMinimaSegura = 5f;
    public GameObject proyectilPrefab;
    public GameObject basePropia;
    public GameObject baseEnemiga;
    public Transform puntoDisparo;
    public bool esJugador = false;
    public EstadoUnidad estadoActual = EstadoUnidad.Defensa;

    private NavMeshAgent agent;
    private GameObject objetivo;
    private float tiempoProximaPatrulla;
    private float tiempoEspera = 2f;
    private float rangoPatrulla = 10f;
    private float tiempoUltimoDisparo = -999f;
    public float intervaloEntreDisparos = 1f;

    private float tiempoProximaDefensa;
    private float intervaloDefensa = 2f;
    private float rangoDefensa = 6f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = velocidad;
        InvokeRepeating(nameof(DetectarObjetivo), 0f, 1f);

        foreach (var b in GameObject.FindGameObjectsWithTag("Base"))
        {
            var baseScript = b.GetComponent<Base>();
            if (baseScript == null) continue;

            if (baseScript.esJugador == esJugador)
                basePropia = b;
            else
                baseEnemiga = b;
        }
    }

    void Update()
    {
        if (estadoActual == EstadoUnidad.Defensa)
        {
            if (objetivo == null)
            {
                DetectarObjetivo();

                if (Time.time > tiempoProximaDefensa && basePropia != null)
                {
                    Vector3 punto = basePropia.transform.position + Random.insideUnitSphere * rangoDefensa;
                    punto.y = transform.position.y;

                    if (NavMesh.SamplePosition(punto, out NavMeshHit hit, rangoDefensa, NavMesh.AllAreas))
                    {
                        agent.SetDestination(hit.position);
                    }

                    tiempoProximaDefensa = Time.time + intervaloDefensa;
                }
            }
            else
            {
                float dist = Vector3.Distance(transform.position, objetivo.transform.position);

                if (dist > rangoDisparo)
                {
                    Vector3 destino = objetivo.transform.position;
                    destino.y = transform.position.y;
                    agent.SetDestination(destino);
                }
                else if (dist < distanciaMinimaSegura)
                {
                    Vector3 alejamiento = (transform.position - objetivo.transform.position).normalized;
                    Vector3 nuevoDestino = transform.position + alejamiento * 3f;
                    nuevoDestino.y = transform.position.y;
                    agent.SetDestination(nuevoDestino);
                }
                else
                {
                    agent.ResetPath();
                }

                Disparar();
            }

            return;
        }

        if ((estadoActual == EstadoUnidad.Ataque || estadoActual == EstadoUnidad.Patrulla) && objetivo != null)
        {
            float dist = Vector3.Distance(transform.position, objetivo.transform.position);

            if (estadoActual == EstadoUnidad.Patrulla && dist > rangoPersecucionMaxima)
            {
                objetivo = null;
                agent.SetDestination(ObtenerPuntoCercaDeBase(rangoPatrulla));
                return;
            }

            if (dist > rangoDisparo)
            {
                Vector3 destino = objetivo.transform.position;
                destino.y = transform.position.y;
                agent.SetDestination(destino);
            }
            else if (dist < distanciaMinimaSegura)
            {
                Vector3 alejamiento = (transform.position - objetivo.transform.position).normalized;
                Vector3 nuevoDestino = transform.position + alejamiento * 3f;
                nuevoDestino.y = transform.position.y;
                agent.SetDestination(nuevoDestino);
            }
            else
            {
                agent.ResetPath();
            }

            Disparar();
        }
        else if (estadoActual == EstadoUnidad.Patrulla && !esJugador && objetivo == null && Time.time > tiempoProximaPatrulla && agent.remainingDistance < 1f)
        {
            Vector3 destino = ObtenerPuntoCercaDeBase(rangoPatrulla);
            agent.SetDestination(destino);
            tiempoProximaPatrulla = Time.time + tiempoEspera;
        }

        if (estadoActual == EstadoUnidad.Ataque && objetivo == null && baseEnemiga != null)
        {
            Vector3 destino = baseEnemiga.transform.position;
            destino.y = transform.position.y;
            agent.SetDestination(destino);
        }
    }

    void DetectarObjetivo()
    {
        if (objetivo != null || estadoActual == EstadoUnidad.Defensa) return;

        GameObject masCercano = null;
        float minDist = Mathf.Infinity;

        foreach (var obj in GameObject.FindGameObjectsWithTag("Unidad"))
        {
            if (obj == gameObject) continue;

            bool enemigo =
                (obj.TryGetComponent<Rey>(out var r) && r.esJugador != esJugador) ||
                (obj.TryGetComponent<Alfil>(out var a) && a.esJugador != esJugador) ||
                (obj.TryGetComponent<Reina>(out var q) && q.esJugador != esJugador);

            if (!enemigo) continue;

            float dist = Vector3.Distance(transform.position, obj.transform.position);
            if (dist <= rangoAlerta && dist < minDist)
            {
                masCercano = obj;
                minDist = dist;
            }
        }

        if (masCercano != null)
        {
            objetivo = masCercano;
        }
        else if (baseEnemiga != null)
        {
            float distBase = Vector3.Distance(transform.position, baseEnemiga.transform.position);
            if (distBase <= rangoAlerta)
            {
                objetivo = baseEnemiga;
            }
        }
    }

    void Disparar()
    {
        if (objetivo == null) return;

        if (objetivo.TryGetComponent(out Reina q) && q.esJugador == esJugador) return;
        if (objetivo.TryGetComponent(out Rey r) && r.esJugador == esJugador) return;
        if (objetivo.TryGetComponent(out Alfil a) && a.esJugador == esJugador) return;
        if (objetivo.TryGetComponent(out Base b) && b.esJugador == esJugador) return;

        float dist = Vector3.Distance(transform.position, objetivo.transform.position);

        if (dist <= rangoDisparo && Time.time - tiempoUltimoDisparo >= intervaloEntreDisparos)
        {
            Vector3 objetivoOffset = objetivo.transform.position + Vector3.down * 0.5f;
            Vector3 dir = (objetivoOffset - transform.position).normalized;
            Vector3 spawnPos = puntoDisparo != null ? puntoDisparo.position : transform.position + dir;
            GameObject bala = Instantiate(proyectilPrefab, spawnPos, Quaternion.identity);
            bala.GetComponent<Proyectil>().Inicializar(dir, daño, objetivo, gameObject);

            tiempoUltimoDisparo = Time.time;
        }
    }

    Vector3 ObtenerPuntoCercaDeBase(float rango)
    {
        if (basePropia == null) return transform.position;
        Vector3 centro = basePropia.transform.position;
        Vector3 aleatorio = centro + Random.insideUnitSphere * rango;
        NavMeshHit hit;
        return NavMesh.SamplePosition(aleatorio, out hit, rango, NavMesh.AllAreas) ? hit.position : transform.position;
    }

    public void CambiarEstado(EstadoUnidad nuevoEstado)
    {
        estadoActual = nuevoEstado;

        if (estadoActual == EstadoUnidad.Ataque)
        {
            objetivo = null;
            DetectarObjetivo();
            if (objetivo != null)
            {
                Vector3 destino = objetivo.transform.position;
                destino.y = transform.position.y;
                agent.SetDestination(destino);
            }
        }
        else if (estadoActual == EstadoUnidad.Patrulla)
        {
            objetivo = null;
            Vector3 destino = ObtenerPuntoCercaDeBase(rangoPatrulla);
            agent.SetDestination(destino);
            tiempoProximaPatrulla = Time.time + tiempoEspera;
        }
        else if (estadoActual == EstadoUnidad.Defensa)
        {
            objetivo = null;
            tiempoProximaDefensa = Time.time + intervaloDefensa;
        }
    }

    public void RecibirDaño(float cantidad)
    {
        vida -= cantidad;
        if (vida <= 0)
        {
            var controlador = FindObjectOfType<Controlador_Interaccion>();
            if (controlador != null)
            {
                controlador.unidadesSeleccionadas.Remove(gameObject);
            }

            Destroy(gameObject);
        }
    }
}

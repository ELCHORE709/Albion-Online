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
    public GameObject proyectilPrefab;
    public GameObject basePropia;
    public bool esJugador = false;
    public EstadoUnidad estadoActual = EstadoUnidad.Defensa;

    private NavMeshAgent agent;
    private GameObject objetivo;
    private float tiempoProximaPatrulla;
    private float tiempoEspera = 2f;
    private float rangoPatrulla = 10f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = velocidad;
        InvokeRepeating(nameof(DetectarObjetivo), 0f, 1f);
        InvokeRepeating(nameof(Disparar), 0.5f, 1f);
    }

    void Update()
    {
        if (estadoActual == EstadoUnidad.Defensa) return;

        if (estadoActual == EstadoUnidad.Patrulla && !esJugador)
        {
            if (objetivo == null && Time.time > tiempoProximaPatrulla && agent.remainingDistance < 1f)
            {
                Vector3 destino = ObtenerPuntoCercaDeBase(rangoPatrulla);
                agent.SetDestination(destino);
                tiempoProximaPatrulla = Time.time + tiempoEspera;
            }

            if (objetivo != null)
            {
                float dist = Vector3.Distance(transform.position, objetivo.transform.position);
                if (dist <= rangoDisparo)
                    agent.SetDestination(objetivo.transform.position);
                else if (dist > rangoPersecucionMaxima)
                {
                    objetivo = null;
                    agent.SetDestination(ObtenerPuntoCercaDeBase(rangoPatrulla));
                }
            }
        }

        if (estadoActual == EstadoUnidad.Ataque && objetivo != null)
        {
            agent.SetDestination(objetivo.transform.position);
        }
    }

    void DetectarObjetivo()
    {
        if (estadoActual == EstadoUnidad.Defensa || objetivo != null) return;

        foreach (var obj in GameObject.FindGameObjectsWithTag("Unidad"))
        {
            if (obj == gameObject) continue;

            bool enemigo =
                (obj.TryGetComponent<Rey>(out var r) && r.esJugador != esJugador) ||
                (obj.TryGetComponent<Alfil>(out var a) && a.esJugador != esJugador) ||
                (obj.TryGetComponent<Reina>(out var q) && q.esJugador != esJugador);

            if (!enemigo) continue;

            float dist = Vector3.Distance(transform.position, obj.transform.position);
            if (dist <= rangoAlerta)
            {
                objetivo = obj;
                break;
            }
        }
    }

    void Disparar()
    {
        if (objetivo == null) return;

        float dist = Vector3.Distance(transform.position, objetivo.transform.position);
        if (dist <= rangoDisparo)
        {
            Vector3 dir = (objetivo.transform.position - transform.position).normalized;
            GameObject bala = Instantiate(proyectilPrefab, transform.position + dir, Quaternion.identity);
            bala.GetComponent<Proyectil>().Inicializar(dir, daño, objetivo);
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
        if (!esJugador) return;
        estadoActual = nuevoEstado;
        Debug.Log($"{gameObject.name} cambió a estado: {estadoActual}");
    }

    public void RecibirDaño(float cantidad)
    {
        vida -= cantidad;
        if (vida <= 0) Destroy(gameObject);
    }
}

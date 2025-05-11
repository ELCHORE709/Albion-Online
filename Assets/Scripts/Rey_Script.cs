using UnityEngine;
using UnityEngine.AI;

public class Rey : MonoBehaviour
{
    public float vida = 250f;
    public float daño = 8f;
    public float velocidad = 2f;
    public float rangoAtaque = 1.5f;
    public float rangoAlerta = 10f;
    public float rangoPersecucionMaxima = 8f;
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

                if (dist <= rangoAtaque)
                    objetivo.SendMessage("RecibirDaño", daño * Time.deltaTime, SendMessageOptions.DontRequireReceiver);
                else if (dist <= rangoPersecucionMaxima)
                    agent.SetDestination(objetivo.transform.position);
                else
                {
                    objetivo = null;
                    agent.SetDestination(ObtenerPuntoCercaDeBase(rangoPatrulla));
                }
            }
        }

        if (estadoActual == EstadoUnidad.Ataque && objetivo != null)
        {
            float dist = Vector3.Distance(transform.position, objetivo.transform.position);
            agent.SetDestination(objetivo.transform.position);

            if (dist <= rangoAtaque)
                objetivo.SendMessage("RecibirDaño", daño * Time.deltaTime, SendMessageOptions.DontRequireReceiver);
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

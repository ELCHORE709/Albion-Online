using UnityEngine;
using UnityEngine.AI;

public class Alfil : MonoBehaviour
{
    public float vida = 100f;
    public float daño = 10f;
    public float velocidad = 5f;
    public float rangoAtaque = 1.5f;
    public float rangoAlerta = 10f;
    public float rangoPersecucionMaxima = 8f;
    public GameObject basePropia;
    public GameObject baseEnemiga;
    public bool esJugador = false;
    public EstadoUnidad estadoActual = EstadoUnidad.Defensa;

    private NavMeshAgent agent;
    private GameObject objetivo;
    private float tiempoProximaPatrulla;
    private float tiempoEspera = 2f;
    private float rangoPatrulla = 10f;

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

                if (dist > rangoAtaque)
                {
                    Vector3 destino = objetivo.transform.position;
                    destino.y = transform.position.y;
                    agent.SetDestination(destino);
                }
                else if (!objetivo.TryGetComponent<Base>(out _))
                {
                    agent.ResetPath();
                    objetivo.SendMessage("RecibirDaño", daño * Time.deltaTime, SendMessageOptions.DontRequireReceiver);
                }
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

            if (dist > rangoAtaque)
            {
                Vector3 destino = objetivo.transform.position;
                destino.y = transform.position.y;
                agent.SetDestination(destino);
            }
            else
            {
                if (!objetivo.TryGetComponent<Base>(out _))
                {
                    agent.ResetPath();
                    objetivo.SendMessage("RecibirDaño", daño * Time.deltaTime, SendMessageOptions.DontRequireReceiver);
                }
            }
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

    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent<Base>(out var baseObjetivo))
        {
            if (baseObjetivo.esJugador != esJugador)
            {
                baseObjetivo.RecibirDaño(daño * Time.deltaTime);
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

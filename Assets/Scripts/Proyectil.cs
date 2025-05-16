using UnityEngine;

public class Proyectil : MonoBehaviour
{
    private float daño;
    private GameObject objetivo;
    private GameObject lanzador;

    public void Inicializar(Vector3 direccion, float daño, GameObject objetivo, GameObject lanzador)
    {
        this.daño = daño;
        this.objetivo = objetivo;
        this.lanzador = lanzador;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = direccion.normalized * 10f;
        }

        Collider miCollider = GetComponent<Collider>();
        Collider lanzadorCollider = lanzador.GetComponent<Collider>();
        if (miCollider != null && lanzadorCollider != null)
        {
            Physics.IgnoreCollision(miCollider, lanzadorCollider);
        }

        Destroy(gameObject, 5f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (lanzador == null) return;

        bool lanzadorEsJugador = ObtenerEsJugador(lanzador);

        if (other.TryGetComponent<Base>(out var baseScript))
        {
            if (baseScript.esJugador != lanzadorEsJugador)
            {
                baseScript.RecibirDaño(daño);
                Destroy(gameObject);
            }
            return;
        }

        if (other.TryGetComponent<Rey>(out var r))
        {
            if (r.esJugador != lanzadorEsJugador)
            {
                r.RecibirDaño(daño);
                Destroy(gameObject);
            }
            return;
        }

        if (other.TryGetComponent<Alfil>(out var a))
        {
            if (a.esJugador != lanzadorEsJugador)
            {
                a.RecibirDaño(daño);
                Destroy(gameObject);
            }
            return;
        }

        if (other.TryGetComponent<Reina>(out var q))
        {
            if (q.esJugador != lanzadorEsJugador)
            {
                q.RecibirDaño(daño);
                Destroy(gameObject);
            }
        }
    }

    private bool ObtenerEsJugador(GameObject go)
    {
        if (go.TryGetComponent(out Rey r)) return r.esJugador;
        if (go.TryGetComponent(out Reina q)) return q.esJugador;
        if (go.TryGetComponent(out Alfil a)) return a.esJugador;
        return false;
    }
}

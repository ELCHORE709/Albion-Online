using UnityEngine;

public class Proyectil : MonoBehaviour
{
    private Vector3 direccion;
    private float daño;
    private GameObject objetivo;

    public void Inicializar(Vector3 dir, float daño, GameObject objetivo)
    {
        this.direccion = dir;
        this.daño = daño;
        this.objetivo = objetivo;
        Destroy(gameObject, 5f);
    }

    void Update()
    {
        transform.position += direccion * 10f * Time.deltaTime;

        if (objetivo != null && Vector3.Distance(transform.position, objetivo.transform.position) < 0.5f)
        {
            objetivo.SendMessage("RecibirDaño", daño, SendMessageOptions.DontRequireReceiver);
            Destroy(gameObject);
        }
    }
}

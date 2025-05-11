using UnityEngine;

public class Proyectil : MonoBehaviour
{
    private Vector3 direccion;
    private float da�o;
    private GameObject objetivo;

    public void Inicializar(Vector3 dir, float da�o, GameObject objetivo)
    {
        this.direccion = dir;
        this.da�o = da�o;
        this.objetivo = objetivo;
        Destroy(gameObject, 5f);
    }

    void Update()
    {
        transform.position += direccion * 10f * Time.deltaTime;

        if (objetivo != null && Vector3.Distance(transform.position, objetivo.transform.position) < 0.5f)
        {
            objetivo.SendMessage("RecibirDa�o", da�o, SendMessageOptions.DontRequireReceiver);
            Destroy(gameObject);
        }
    }
}

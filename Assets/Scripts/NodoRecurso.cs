using System.Collections.Generic;
using UnityEngine;

public class NodoRecurso : MonoBehaviour
{
    public int cantidad = 1000;
    public int recursoPorSegundo = 2;

    private List<Peon> recolectores = new();

    private void OnTriggerEnter(Collider other)
    {
        var unidad = other.GetComponent<Peon>();
        if (unidad != null && !recolectores.Contains(unidad))
        {
            recolectores.Add(unidad);
            unidad.IniciarRecoleccion(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var unidad = other.GetComponent<Peon>();
        if (unidad != null && recolectores.Contains(unidad))
        {
            recolectores.Remove(unidad);
            unidad.DetenerRecoleccion();
        }
    }

    public bool Extraer(int cantidad)
    {
        if (this.cantidad < cantidad) return false;

        this.cantidad -= cantidad;

        if (this.cantidad <= 0)
        {
            foreach (var peon in recolectores)
            {
                if (peon != null)
                    peon.DetenerRecoleccion();
            }

            Destroy(gameObject);
        }

        return true;
    }
}

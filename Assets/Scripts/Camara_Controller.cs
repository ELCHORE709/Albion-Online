using UnityEngine;
using UnityEngine.InputSystem;

public class CamaraController : MonoBehaviour
{
    private InputAction movimiento;
    private InputAction rotacion;
    private InputAction zoom;

    private Transform yaw;
    private Transform pitch;
    private Transform camaraTransform;

    [Header("Configuración de movimiento")]
    public float velocidadMovimiento = 5f;
    public float velocidadRotacion = 100f;

    [Header("Configuración de zoom")]
    public float velocidadZoom = 10f;
    public float minZoom = 5f;
    public float maxZoom = 50f;

    [Header("Límites del mapa")]
    public Vector2 limitesX = new Vector2(91.49598f, 901.3017f);
    public Vector2 limitesZ = new Vector2(238.9575f, 780.7451f);

    [Header("Posición inicial")]
    public Vector3 posicionInicial = new Vector3(744.7f, 2.9f, 396.2f);

    void Start()
    {
        transform.position = posicionInicial;

        movimiento = InputSystem.actions?.FindAction("Movimiento");
        rotacion = InputSystem.actions?.FindAction("Rotacion");
        zoom = InputSystem.actions?.FindAction("Zoom");

        if (movimiento == null || rotacion == null || zoom == null)
        {
        }

        yaw = transform.Find("Yaw");
        pitch = yaw?.Find("Pitch");
        camaraTransform = pitch?.Find("Camera");

        if (!yaw || !pitch || !camaraTransform)
        {
        }
    }

    void Update()
    {
        if (movimiento == null || rotacion == null || zoom == null || yaw == null || pitch == null || camaraTransform == null)
            return;

        Vector2 vectorMovimiento = movimiento.ReadValue<Vector2>();
        float cambioRotacion = rotacion.ReadValue<float>();
        float cambioZoom = zoom.ReadValue<float>();

        Vector3 movimientoRotado = yaw.rotation * new Vector3(vectorMovimiento.x, 0, vectorMovimiento.y);
        Vector3 nuevaPosicion = transform.position + movimientoRotado * velocidadMovimiento * Time.deltaTime;

        nuevaPosicion.x = Mathf.Clamp(nuevaPosicion.x, limitesX.x, limitesX.y);
        nuevaPosicion.z = Mathf.Clamp(nuevaPosicion.z, limitesZ.x, limitesZ.y);

        transform.position = nuevaPosicion;

        yaw.Rotate(0, cambioRotacion * velocidadRotacion * Time.deltaTime, 0);

        Vector3 zoomDirection = camaraTransform.localPosition.normalized;
        float newZoom = camaraTransform.localPosition.z + cambioZoom * velocidadZoom * Time.deltaTime;
        newZoom = Mathf.Clamp(newZoom, -maxZoom, -minZoom);
        camaraTransform.localPosition = new Vector3(0, 0, newZoom);
    }
}

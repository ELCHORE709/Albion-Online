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

    void Start()
    {
        movimiento = InputSystem.actions?.FindAction("Movimiento");
        rotacion = InputSystem.actions?.FindAction("Rotacion");
        zoom = InputSystem.actions?.FindAction("Zoom");

        if (movimiento == null || rotacion == null || zoom == null)
        {
            Debug.LogError("No se encontraron las acciones de entrada. Verifica el Input Action Asset.");
        }

        yaw = transform.Find("Yaw");
        if (yaw == null)
        {
            Debug.LogError("No se encontró el objeto 'Yaw'. Asegúrate de que existe como hijo del GameObject.");
        }

        pitch = yaw.Find("Pitch");
        if (pitch == null)
        {
            Debug.LogError("No se encontró el objeto 'Pitch'. Asegúrate de que existe como hijo de 'Yaw'.");
        }

        camaraTransform = pitch.Find("Camera");
        if (camaraTransform == null)
        {
            Debug.LogError("No se encontró la cámara como hijo de 'Pitch'. Asegúrate de que existe y se llama 'Camera'.");
        }
    }

    void Update()
    {
        if (movimiento == null || rotacion == null || zoom == null || yaw == null || pitch == null || camaraTransform == null)
        {
            return;
        }

        Vector2 vectorMovimiento = movimiento.ReadValue<Vector2>();
        float cambioRotacion = rotacion.ReadValue<float>();
        float cambioZoom = zoom.ReadValue<float>();

        Vector3 movimientoRotado = yaw.rotation * new Vector3(vectorMovimiento.x, 0, vectorMovimiento.y);
        transform.Translate(movimientoRotado * velocidadMovimiento * Time.deltaTime, Space.World);

        yaw.Rotate(0, cambioRotacion * velocidadRotacion * Time.deltaTime, 0);

        Vector3 zoomDirection = camaraTransform.localPosition.normalized;
        float newZoom = camaraTransform.localPosition.z + cambioZoom * velocidadZoom * Time.deltaTime;
        newZoom = Mathf.Clamp(newZoom, -maxZoom, -minZoom);
        camaraTransform.localPosition = new Vector3(0, 0, newZoom);
    }
}

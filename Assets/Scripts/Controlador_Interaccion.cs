using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.EventSystems; // ✅ Para detectar clics sobre UI

public class Controlador_Interaccion : MonoBehaviour
{
    [Header("Configuración de Selección")]
    public LayerMask capaSeleccionable;
    public string nombreMaterialOutliner = "Outliner_Mat";
    public float escalaSeleccionada = 1.05f;
    public float escalaNormal = 1.0f;

    [Header("Configuración del Click")]
    [Tooltip("Delay mínimo entre clics para evitar doble ejecución")]
    public float delayClick = 0.1f;

    [Header("Selección por Caja")]
    public RectTransform cajaSeleccionUI;
    private Vector2 inicioCaja;
    private Vector2 finCaja;
    private bool seleccionando = false;

    [Header("Debug")]
    [SerializeField] public List<GameObject> unidadesSeleccionadas = new List<GameObject>();

    private InputAction clickAction;
    private InputAction moverAction;
    private bool estaSuscrito = false;
    private float tiempoUltimoClick = 0f;

    void Start()
    {
        clickAction = InputSystem.actions?.FindAction("Click");
        moverAction = InputSystem.actions?.FindAction("Mover");

        if (clickAction != null && !estaSuscrito)
        {
            clickAction.performed += OnClick;
            estaSuscrito = true;
        }

        if (moverAction != null)
        {
            moverAction.performed += OnMoverClick;
        }

        if (cajaSeleccionUI != null)
        {
            cajaSeleccionUI.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            CambiarEstadoSeleccionados(EstadoUnidad.Ataque);
        if (Keyboard.current.digit2Key.wasPressedThisFrame)
            CambiarEstadoSeleccionados(EstadoUnidad.Patrulla);
        if (Keyboard.current.digit3Key.wasPressedThisFrame)
            CambiarEstadoSeleccionados(EstadoUnidad.Defensa);

        if (Mouse.current.leftButton.wasPressedThisFrame && !Keyboard.current.shiftKey.isPressed && !EventSystem.current.IsPointerOverGameObject())
        {
            seleccionando = true;
            inicioCaja = Mouse.current.position.ReadValue();
            if (cajaSeleccionUI != null) cajaSeleccionUI.gameObject.SetActive(true);
        }

        if (seleccionando)
        {
            finCaja = Mouse.current.position.ReadValue();
            ActualizarCajaUI();
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame && seleccionando)
        {
            seleccionando = false;
            if (cajaSeleccionUI != null) cajaSeleccionUI.gameObject.SetActive(false);

            float distancia = Vector2.Distance(inicioCaja, finCaja);
            if (distancia > 10f)
            {
                SeleccionarUnidadesEnCaja();
            }
        }
    }

    private void OnClick(InputAction.CallbackContext context)
    {
        if (EventSystem.current.IsPointerOverGameObject()) return; // ✅ Bloquear selección si se hizo clic en UI

        if (Time.time - tiempoUltimoClick < delayClick)
        {
            Debug.Log("⛔ Ignorando clic duplicado (demasiado rápido)");
            return;
        }

        tiempoUltimoClick = Time.time;

        bool shiftPresionado = Keyboard.current.shiftKey.isPressed;

        if (shiftPresionado)
        {
            SeleccionarUnidadesVisibles();
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, capaSeleccionable))
        {
            GameObject unidad = hit.collider.gameObject;

            if (unidadesSeleccionadas.Contains(unidad))
            {
                DeseleccionarUnidad(unidad);
            }
            else
            {
                SeleccionarUnidad(unidad);
            }
        }
        else
        {
            DeseleccionarTodasLasUnidades();
        }
    }

    private void OnMoverClick(InputAction.CallbackContext context)
    {
        if (EventSystem.current.IsPointerOverGameObject()) return; // ✅ Bloquear movimiento si se hizo clic en UI

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        {
            Vector3 destino = hit.point;

            unidadesSeleccionadas.RemoveAll(obj => obj == null);

            foreach (GameObject unidad in unidadesSeleccionadas)
            {
                NavMeshAgent agente = unidad.GetComponent<NavMeshAgent>();
                if (agente != null)
                {
                    agente.SetDestination(destino);
                }
            }
        }
    }

    private void SeleccionarUnidad(GameObject unidad)
    {
        unidadesSeleccionadas.Add(unidad);
        ModificarEscalaMaterial(unidad, escalaSeleccionada);

        if (unidad.TryGetComponent<Base>(out var baseScript))
        {
            baseScript.MostrarUI(true);
            Transform circulo = unidad.transform.Find("SeleccionVisual");
            if (circulo != null) circulo.gameObject.SetActive(true);
        }
    }

    private void DeseleccionarUnidad(GameObject unidad)
    {
        unidadesSeleccionadas.Remove(unidad);
        ModificarEscalaMaterial(unidad, escalaNormal);

        if (unidad.TryGetComponent<Base>(out var baseScript))
        {
            baseScript.MostrarUI(false);
            Transform circulo = unidad.transform.Find("SeleccionVisual");
            if (circulo != null) circulo.gameObject.SetActive(false);
        }
    }

    private void DeseleccionarTodasLasUnidades()
    {
        foreach (GameObject unidad in unidadesSeleccionadas)
        {
            ModificarEscalaMaterial(unidad, escalaNormal);

            if (unidad.TryGetComponent<Base>(out var baseScript))
            {
                baseScript.MostrarUI(false);
                Transform circulo = unidad.transform.Find("SeleccionVisual");
                if (circulo != null) circulo.gameObject.SetActive(false);
            }
        }
        unidadesSeleccionadas.Clear();
    }

    private void ModificarEscalaMaterial(GameObject unidad, float nuevaEscala)
    {
        Renderer renderer = unidad.GetComponent<Renderer>();
        if (renderer != null)
        {
            foreach (Material mat in renderer.materials)
            {
                if (mat.name.Contains(nombreMaterialOutliner))
                {
                    mat.SetFloat("_Scale", nuevaEscala);
                    break;
                }
            }
        }
    }

    private void SeleccionarUnidadesVisibles()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        GameObject[] todos = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject obj in todos)
        {
            if (((1 << obj.layer) & capaSeleccionable) == 0) continue;

            if (obj.TryGetComponent(out Renderer rend))
            {
                Vector3 viewportPos = cam.WorldToViewportPoint(obj.transform.position);
                if (viewportPos.z > 0 && viewportPos.x >= 0 && viewportPos.x <= 1 && viewportPos.y >= 0 && viewportPos.y <= 1)
                {
                    if (!unidadesSeleccionadas.Contains(obj))
                    {
                        SeleccionarUnidad(obj);
                    }
                }
            }
        }
    }

    private void ActualizarCajaUI()
    {
        if (cajaSeleccionUI == null) return;

        Vector2 size = new Vector2(Mathf.Abs(finCaja.x - inicioCaja.x), Mathf.Abs(finCaja.y - inicioCaja.y));
        Vector2 center = (inicioCaja + finCaja) / 2f;

        cajaSeleccionUI.position = center;
        cajaSeleccionUI.sizeDelta = size;
    }

    private void SeleccionarUnidadesEnCaja()
    {
        DeseleccionarTodasLasUnidades();

        Camera cam = Camera.main;
        GameObject[] todos = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        Rect caja = new Rect(
            Mathf.Min(inicioCaja.x, finCaja.x),
            Mathf.Min(inicioCaja.y, finCaja.y),
            Mathf.Abs(inicioCaja.x - finCaja.x),
            Mathf.Abs(inicioCaja.y - finCaja.y)
        );

        foreach (GameObject obj in todos)
        {
            if (((1 << obj.layer) & capaSeleccionable) == 0) continue;
            if (!obj.TryGetComponent(out Renderer rend)) continue;

            Vector3 pantalla = cam.WorldToScreenPoint(obj.transform.position);
            if (pantalla.z > 0 && caja.Contains(pantalla))
            {
                SeleccionarUnidad(obj);
            }
        }
    }

    private void CambiarEstadoSeleccionados(EstadoUnidad nuevoEstado)
    {
        foreach (GameObject unidad in unidadesSeleccionadas)
        {
            if (unidad.TryGetComponent<Rey>(out var rey) && rey.esJugador) rey.CambiarEstado(nuevoEstado);
            if (unidad.TryGetComponent<Alfil>(out var alfil) && alfil.esJugador) alfil.CambiarEstado(nuevoEstado);
            if (unidad.TryGetComponent<Reina>(out var reina) && reina.esJugador) reina.CambiarEstado(nuevoEstado);
        }
    }

    private void OnDestroy()
    {
        if (clickAction != null && estaSuscrito)
        {
            clickAction.performed -= OnClick;
        }

        if (moverAction != null)
        {
            moverAction.performed -= OnMoverClick;
        }
    }
}

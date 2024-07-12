using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;


public class Enemigo : MonoBehaviour
{
    public int saludInicial = 100; // Salud inicial del enemigo
    private int saludActual; // Salud actual del enemigo
    public float speed = 1f; // Velocidad de movimiento hacia el jugador
    private Transform player; // Transform del jugador
    private bool estaMuerto = false; // Bandera para verificar si el enemigo est� muerto
    private Animator animator; // Referencia al componente Animator para las animaciones
    public Text vidaEnemigoText;
    private AudioSource audioSource; // Referencia al componente AudioSource
    public AudioClip sonidoDa�o; // Clip de sonido que se reproducir� cuando el enemigo reciba da�o
    public float rangoDeDeteccion = 10f; // Rango en el que el enemigo puede detectar al jugador
    public float rangoAtaqueCuerpoACuerpo = 2f; // Rango para ataque cuerpo a cuerpo
    public float rangoAtaquePistola = 10f; // Rango para disparar con la pistola
    public int danioCuchillo = 10; // Da�o del cuchillo
    public int danioPistola = 20; // Da�o de la pistola
    public AudioClip sonidoCuchillo; // Sonido para el ataque con cuchillo
    public AudioClip sonidoPistola; // Sonido para el disparo de la pistola
    public Transform puntoDisparo; // Punto desde donde se dispara la pistola
    public GameObject balaPrefab; // Prefab de la bala

    private float tiempoEntreAtaques = 1f; // Tiempo entre ataques
    private float temporizadorAtaque; // Temporizador para controlar los ataques
    public GameObject impactEffect;
    void Start()
    {
        saludActual = saludInicial;
        ActualizarTextoVida(); // Actualizar el texto al iniciar

        player = GameObject.FindGameObjectWithTag("Player").transform; // Encontrar el jugador por etiqueta
        animator = GetComponent<Animator>(); // Obtener el componente Animator del enemigo si existe
        audioSource = GetComponent<AudioSource>(); // Obtener el componente AudioSource del enemigo
    }

    void Update()
    {
        if (!estaMuerto)
        {
            MoveTowardsPlayer();
            AttackPlayer();
        }
    }

    void MoveTowardsPlayer()
    {
        if (player == null)
        {
            Debug.LogWarning("No se encontr� al jugador.");
            return;
        }

        float distanciaAlJugador = Vector3.Distance(transform.position, player.position);
        if (distanciaAlJugador <= rangoDeDeteccion)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;

            // Rotar hacia la direcci�n del jugador
            transform.LookAt(player);
        }
    }

    void AttackPlayer()
    {
        float distanciaAlJugador = Vector3.Distance(transform.position, player.position);
        temporizadorAtaque += Time.deltaTime;

        if (temporizadorAtaque >= tiempoEntreAtaques)
        {
            if (distanciaAlJugador <= rangoAtaqueCuerpoACuerpo)
            {
                AtaqueCuchillo();
            }
            else if (distanciaAlJugador <= rangoAtaquePistola)
            {
                AtaquePistola();
            }
        }
    }

    void AtaqueCuchillo()
    {
        // Ataque con cuchillo
        animator.SetBool("AtaqueCuchillo", true);
        audioSource.PlayOneShot(sonidoCuchillo);
        player.GetComponent<Jugador>().TakeDamage(danioCuchillo);
        temporizadorAtaque = 0f;
    }

    void AtaquePistola()
    {
        // Ataque con pistola
        animator.SetBool("AtaquePistola", true  );
        audioSource.PlayOneShot(sonidoPistola);
        Shoot();
        temporizadorAtaque = 0f;
    }

    void Shoot()
    {
        if (balaPrefab != null && puntoDisparo != null)
        {
            // Calcular direcci�n hacia el jugador
            Vector3 direccion = (player.position - puntoDisparo.position).normalized;

            // Calcular distancia m�xima del disparo
            float distanciaMaxima = Vector3.Distance(puntoDisparo.position, player.position);

            // Comprobar si el jugador ha sido impactado
            RaycastHit hit;
            if (Physics.Raycast(puntoDisparo.position, direccion, out hit, distanciaMaxima))
            {
                Jugador jugador = hit.transform.GetComponent<Jugador>();
                if (jugador != null)
                {
                    // Restar 10 de vida al jugador por cada impacto de bala
                    jugador.TakeDamage(10);
                }

                // Mostrar efecto de impacto si est� configurado
                if (impactEffect != null)
                {
                    GameObject impacto = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(impacto, 2f);
                }
            }

            // Animar la bala hacia la posici�n final (ajusta la velocidad seg�n necesites)
            StartCoroutine(MoverBala(balaPrefab.transform, puntoDisparo.position + direccion * distanciaMaxima, 10f)); // 10f es la velocidad de la bala

            // Disparar sonido de pistola
            if (audioSource != null && sonidoPistola != null)
            {
                audioSource.PlayOneShot(sonidoPistola);
            }
        }
        else
        {
            Debug.LogError("BalaPrefab o puntoDisparo no asignados en el enemigo.");
        }
    }

    IEnumerator MoverBala(Transform objeto, Vector3 posicionFinal, float velocidad)
    {
        float distancia = Vector3.Distance(objeto.position, posicionFinal);
        float duracion = distancia / velocidad;
        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < duracion)
        {
            tiempoTranscurrido += Time.deltaTime;
            objeto.position = Vector3.Lerp(objeto.position, posicionFinal, tiempoTranscurrido / duracion);
            yield return null;
        }

        // Asegurarse de que la bala llegue exactamente a la posici�n final
        objeto.position = posicionFinal;
    }


    public void TakeDamage(int cantidadDanio)
    {
        int saludAnterior = saludActual; // Guardar la salud anterior

        saludActual -= cantidadDanio;

        // Mostrar la salud anterior y la nueva despu�s de recibir da�o
        Debug.Log("Vida anterior del enemigo: " + saludAnterior);
        Debug.Log("Vida actual del enemigo despu�s de recibir da�o: " + saludActual);

        if (audioSource != null && sonidoDa�o != null)
        {
            audioSource.PlayOneShot(sonidoDa�o);
        }

        if (saludActual <= 0)
        {
            Morir();
        }

        ActualizarTextoVida(); // Actualizar el texto despu�s de recibir da�o
    }

    void Morir()
    {
        // Marcar como muerto para evitar acciones adicionales si ya est� muerto
        if (estaMuerto)
            return;

        estaMuerto = true;

        // Incrementar el contador de enemigos muertos del jugador
        if (player != null)
        {
            Jugador jugador = player.GetComponent<Jugador>();
            if (jugador != null)
            {
                jugador.IncrementarEnemigosMuertos();
            }
        }

        // Desactivar el movimiento y cualquier otro comportamiento activo (como atacar)
        speed = 0f;

        // Activar la animaci�n de muerte si hay Animator
        if (animator != null)
        {
            animator.SetBool("Muerte", true); // Activar el trigger "Muerte" en el Animator
        }

        // Desactivar el collider del enemigo para evitar colisiones
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        // Ejemplo: Destruir el GameObject despu�s de 3 segundos
        Destroy(gameObject, 3f);
    }

    void OnDestroy()
    {
        // Aqu� podr�as agregar cualquier l�gica adicional que se ejecute al destruir el objeto del enemigo
        Debug.Log("El enemigo ha sido destruido.");
    }

    void ActualizarTextoVida()
    {
        // Actualizar el texto en pantalla con la vida actual del enemigo
        if (vidaEnemigoText != null)
        {
            vidaEnemigoText.text = "Vida Enemigo: " + saludActual.ToString();
        }
    }
}

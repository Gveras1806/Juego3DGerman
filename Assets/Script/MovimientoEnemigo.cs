using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovimientoEnemigo : MonoBehaviour
{

    public int saludInicial = 100; // Salud inicial del enemigo
    private int saludActual; // Salud actual del enemigo
    public float speed = 5f; // Velocidad de movimiento hacia el jugador
    private Transform player; // Transform del jugador
    private bool estaMuerto = false; // Bandera para verificar si el enemigo está muerto
    private Animator animator; // Referencia al componente Animator para las animaciones

    void Start()
    {
        saludActual = saludInicial;
        player = GameObject.FindGameObjectWithTag("Player").transform; // Encontrar el jugador por etiqueta
        animator = GetComponent<Animator>(); // Obtener el componente Animator del enemigo si existe
    }

    void Update()
    {
        if (!estaMuerto)
        {
            MoveTowardsPlayer();
        }
    }

    void MoveTowardsPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        // Rotar hacia la dirección del jugador (opcional)
        transform.LookAt(player);
    }

    public void TakeDamage(int cantidadDanio)
    {
        if (estaMuerto)
            return;

        saludActual -= cantidadDanio;

        // Mostrar la salud actual después de recibir daño
        Debug.Log("Vida actual del enemigo después de recibir daño: " + saludActual);

        // Si la salud llega a cero, llamar a la función de morir
        if (saludActual <= 0)
        {
            Morir();
        }
    }

    void Morir()
    {
        // Marcar como muerto para evitar acciones adicionales si ya está muerto
        if (estaMuerto)
            return;

        estaMuerto = true;

        // Desactivar el movimiento y cualquier otro comportamiento activo (como atacar)
        speed = 0f;

        // Activar la animación de muerte si hay Animator
        if (animator != null)
        {
            animator.SetTrigger("Muerte"); // Activar el trigger "Muerte" en el Animator
        }

        // Ejemplo: Destruir el GameObject después de 3 segundos
        Destroy(gameObject, 3f);
    }

    void OnDestroy()
    {
        // Aquí podrías agregar cualquier lógica adicional que se ejecute al destruir el objeto del enemigo
        Debug.Log("El enemigo ha sido destruido.");
    }


}

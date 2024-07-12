using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Jugador : MonoBehaviour
{
    public Text vidaJugador;
    public Text municionTexto;
    public Text enemigosMuertosTexto; // Texto para mostrar la cantidad de enemigos muertos
    public CharacterController controller;
    public float speed = 12f;
    public float gravity = -9.81f;
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    public Animator animator;
    public Camera playerCamera;
    public float aimFOV = 40f;
    public float normalFOV = 60f;
    public float aimSpeed = 10f;
    public Transform gun;
    public float gunRange = 100f;
    public ParticleSystem muzzleFlash;
    public GameObject impactEffect;
    public float fireRate = 10f;
    public int maxAmmo = 30;
    public int MaxEnemigo;
    private int currentAmmo;
    public float reloadTime = 1f;
    private bool isReloading = false;
    private Vector3 velocity;
    private bool isGrounded;
    private float nextTimeToFire = 0f;
    private int saludActual;
    public int saludInicial = 100;
    public AudioSource audioSource;
    public AudioClip outOfAmmoClip;
    public AudioClip reloadSoundClip;
    public AudioClip shootSoundClip;
    public AudioClip missSoundClip;

    private int enemigosMuertos = 0;
    private int cantidadMuertes = 0; // Contador de muertes del jugador

    void Start()
    {
        MaxEnemigo = CalcularMaxEnemigos();
        Debug.Log("Cantidad máxima de enemigos: " + MaxEnemigo);

        saludActual = saludInicial;
        ActualizarTextoVida();
        currentAmmo = maxAmmo;
        ActualizarTextoMunicion();
        ActualizarTextoEnemigosMuertos();
    }

    void Update()
    {
        if (isReloading)
            return;

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

        animator.SetFloat("speed", move.magnitude, 0.1f, Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (Input.GetMouseButton(1))
        {
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, aimFOV, aimSpeed * Time.deltaTime);
            animator.SetFloat("isAiming", 2);
        }
        else
        {
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, normalFOV, aimSpeed * Time.deltaTime);
            animator.SetFloat("isAiming", -1);
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (currentAmmo > 0)
            {
                Shoot();
            }
            else
            {
                PlayOutOfAmmoSound();
            }
        }

        if (Input.GetMouseButton(0) && Time.time >= nextTimeToFire)
        {
            if (currentAmmo > 0)
            {
                nextTimeToFire = Time.time + 1f / fireRate;
                Shoot();
            }
            else
            {
                PlayOutOfAmmoSound();
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(Reload());
            return;
        }
    }
    int CalcularMaxEnemigos()
    {
        GameObject[] enemigos = GameObject.FindGameObjectsWithTag("Enemigo");
        return enemigos.Length;
    }
    public void IncrementarEnemigosMuertos()
    {
        enemigosMuertos++;
        ActualizarTextoEnemigosMuertos();

        if (enemigosMuertos >= MaxEnemigo)
        {
            GanarJuego();
        }
    }
    void GanarJuego()
    {
        // Aquí puedes mostrar un mensaje de "Ganaste el juego"
        Debug.Log("¡Ganaste el juego!");

        
        Time.timeScale = 0f;

        MostrarMensajeGanador(); // Función para mostrar el mensaje en pantalla
    }

    void MostrarMensajeGanador()
    {
        // Mostrar un mensaje en la consola
        Debug.Log("¡Ganaste el juego!");

        // Detener el tiempo para pausar el juego
        Time.timeScale = 0f;

        CargarSiguienteEscena();
    }
    void CargarSiguienteEscena()
    {
        Time.timeScale = 1f;
        // Obtener el índice de la siguiente escena (ajusta esto según tus necesidades)
        int siguienteEscenaIndex = SceneManager.GetActiveScene().buildIndex + 1;

        // Verificar si la siguiente escena existe en la configuración de compilación
        if (siguienteEscenaIndex < SceneManager.sceneCountInBuildSettings)
        {
            // Cargar la siguiente escena por su índice
            SceneManager.LoadScene(siguienteEscenaIndex);
        }
        else
        {
            SceneManager.LoadScene(0);
        }
    }
    void Shoot()
    {
        currentAmmo--;
        ActualizarTextoMunicion();

        if (muzzleFlash != null && !muzzleFlash.isPlaying)
        {
            muzzleFlash.Play();
        }

        if (audioSource != null && shootSoundClip != null)
        {
            audioSource.PlayOneShot(shootSoundClip);
        }
        else
        {
            Debug.LogWarning("AudioSource o shootSoundClip no asignados.");
        }

        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, gunRange))
        {
            if (impactEffect != null)
            {
                GameObject impactGO = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impactGO, 2f);
            }

            Enemigo enemy = hit.transform.GetComponent<Enemigo>();
            if (enemy != null)
            {
                enemy.TakeDamage(100);
            }
            else
            {
                PlayMissSound();
            }
        }
        else
        {
            PlayMissSound();
        }
    }

    void PlayMissSound()
    {
        if (audioSource != null && missSoundClip != null)
        {
            audioSource.PlayOneShot(missSoundClip);
        }
        else
        {
            Debug.LogWarning("AudioSource o missSoundClip no asignados.");
        }
    }

    void PlayOutOfAmmoSound()
    {
        if (audioSource != null && outOfAmmoClip != null)
        {
            audioSource.PlayOneShot(outOfAmmoClip);
        }
        else
        {
            Debug.LogWarning("AudioSource o outOfAmmoClip no asignados.");
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        if (audioSource != null && reloadSoundClip != null)
        {
            audioSource.PlayOneShot(reloadSoundClip);
        }
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = maxAmmo;
        isReloading = false;
        ActualizarTextoMunicion();
    }

    void ActualizarTextoVida()
    {
        if (vidaJugador != null)
        {
            vidaJugador.text = "Vida Jugador: " + saludActual.ToString();
        }
    }

    void ActualizarTextoMunicion()
    {
        if (municionTexto != null)
        {
            municionTexto.text = currentAmmo.ToString() + "/" + maxAmmo.ToString();
        }
    }

    void ActualizarTextoEnemigosMuertos()
    {
        if (enemigosMuertosTexto != null)
        {
            enemigosMuertosTexto.text = "Enemigos Muertos: " + enemigosMuertos.ToString() + "/" + MaxEnemigo.ToString(); ;
        }
    }

    public void TakeDamage(int cantidadDanio)
    {
        int saludAnterior = saludActual;
        saludActual -= cantidadDanio;

        Debug.Log("Vida anterior del Jugador: " + saludAnterior);
        Debug.Log("Vida actual del jugador después de recibir daño: " + saludActual);

        // Verifica si la salud del jugador es igual o menor que cero para morir
        if (saludActual <= 0)
        {
            Morir();
        }
        else
        {
            ActualizarTextoVida();
        }
    }

    void Morir()
    {
        // Puedes implementar la lógica específica para la muerte del jugador aquí
        Debug.Log("El jugador ha muerto.");

        // Por ejemplo, podrías reiniciar la posición del jugador a un punto de respawn
        // transform.position = respawnPosition;

        // Detener el tiempo para pausar el juego
        Time.timeScale = 0f;

        // Mostrar un mensaje de game over en pantalla o ejecutar otras acciones
        MostrarGameOver();
    }

    void MostrarGameOver()
    {
        SceneManager.LoadScene(0);
        Time.timeScale = 0f;
    }
}
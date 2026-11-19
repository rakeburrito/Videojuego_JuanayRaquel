
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerPlatformerController : MonoBehaviour
{
    [Header("Movimiento y Físicas")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.2f;

    [Header("Reaparición")]
    public Transform respawnPoint;
    public float respawnDelay = 1f;

    [Header("Puntuación y Coleccionables")]
    public int totalScore = 0;

    public int coinsCollected = 0;
    public int totalCoins = 15;

    public int diamondsCollected = 0;
    public int totalDiamonds = 15;

    public int keysCollected = 0;
    public int totalKeys = 1;

    [Header("Interfaz (UI)")]
    public TextMeshProUGUI textScore;
    public TextMeshProUGUI textCoins;
    public TextMeshProUGUI textDiamonds;
    public TextMeshProUGUI textKeys;
    public GameObject instructionsPanel;
    public GameObject pausePanel;
    public GameObject settingsPanelInGame;
    public GameObject gameOverPanel;
    public GameObject winPanel;

    [Header("Efectos de Sonido (SFX)")]
    public AudioSource audioSource;
    public AudioClip jumpSFX;
    public AudioClip collectSFX;
    public AudioClip keySFX;
    public AudioClip chestSFX;
    public AudioClip deathSFX;
    public AudioClip winSFX;

    // Estado Interno
    private Rigidbody2D rb;
    private Animator animator;
    private float horizontalInput;
    private bool isGrounded;
    private bool isFacingRight = true;
    private bool isDead = false;
    private bool isRespawning = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Crear punto de aparición automático
        if (respawnPoint == null)
        {
            GameObject autoSpawn = new GameObject("AutoRespawnPoint");
            autoSpawn.transform.position = transform.position;
            respawnPoint = autoSpawn.transform;
        }

        // Pausa inicial para el panel de instrucciones
        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(true);
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }

        UpdateHUD();
    }

    void Update()
    {
        if (isDead || isRespawning) return;

        // Si el juego está pausado, no leemos controles
        if (Mathf.Approximately(Time.timeScale, 0f)) return;

        horizontalInput = Input.GetAxis("Horizontal");

        if (animator != null)
        {
            animator.SetBool("IsRunning", horizontalInput != 0);
        }

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            PlaySFX(jumpSFX);
        }

        FlipSprite();
    }

    void FixedUpdate()
    {
        if (isDead || isRespawning ||
            Mathf.Approximately(Time.timeScale, 0f)) return;

        // Comprobación de suelo mediante overlap
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        if (animator != null)
        {
            animator.SetBool("IsJumping", !isGrounded);
        }

        // Aplicar velocidad al Rigidbody2D
        rb.linearVelocity = new Vector2(
            horizontalInput * moveSpeed,
            rb.linearVelocity.y
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead || isRespawning) return;

        // Caída al vacío
        if (other.CompareTag("Boundary"))
        {
            StartCoroutine(RespawnAfterFall());
            return;
        }

        // Coleccionable: Coin
        if (other.CompareTag("Coin"))
        {
            coinsCollected++;
            totalScore += 10;
            PlaySFX(collectSFX);
            Destroy(other.gameObject);
            UpdateHUD();
        }

        // Coleccionable: Diamond
        else if (other.CompareTag("Diamond"))
        {
            diamondsCollected++;
            totalScore += 25;
            PlaySFX(collectSFX);
            Destroy(other.gameObject);
            UpdateHUD();
        }

        // Coleccionable: Key
        else if (other.CompareTag("Key"))
        {
            keysCollected++;
            totalScore += 50;
            PlaySFX(keySFX != null ? keySFX : collectSFX);
            Destroy(other.gameObject);
            UpdateHUD();
        }

        // Cofre interactuable
        // Cofre interactuable
        else if (other.CompareTag("Chest"))
        {
            Chest chest = other.GetComponent<Chest>();

            if (chest != null && !chest.isOpen)
             {
                int bonus = chest.OpenChest(
                    coinsCollected,
                    diamondsCollected,
                    keysCollected
                );

                if (bonus > 0)
                {
                    totalScore += bonus;
                    PlaySFX(chestSFX);
                    UpdateHUD();
                }
            }
        }

        // Condición de Victoria
        else if (other.CompareTag("WinCondition"))
        {
            CompleteLevel();
        }

        // Enemigos o trampas con Collider2D Trigger
        else if (other.CompareTag("Enemy") ||
                 other.CompareTag("Hazard"))
        {
            StartCoroutine(RespawnAfterFall());
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead || isRespawning) return;

        // Enemigos o trampas con Collider2D normal
        if (collision.gameObject.CompareTag("Enemy") ||
            collision.gameObject.CompareTag("Hazard"))
        {
            StartCoroutine(RespawnAfterFall());
        }
    }

    // Sistema de reaparición
    private IEnumerator RespawnAfterFall()
    {
        isRespawning = true;

        horizontalInput = 0f;
        rb.linearVelocity = Vector2.zero;

        if (animator != null)
        {
            animator.SetBool("IsRunning", false);
            animator.SetBool("IsJumping", false);
        }

        PlaySFX(deathSFX);

        yield return new WaitForSecondsRealtime(respawnDelay);

        transform.position = respawnPoint.position;
        rb.linearVelocity = Vector2.zero;

        isRespawning = false;
    }

    public void CloseInstructions()
    {
        if (instructionsPanel != null)
            instructionsPanel.SetActive(false);

        Time.timeScale = 1f;
    }

    public void TogglePause()
    {
        if (pausePanel != null)
        {
            bool isPaused = pausePanel.activeSelf;
            pausePanel.SetActive(!isPaused);
            Time.timeScale = isPaused ? 1f : 0f;
        }
    }

    public void OpenSettingsInGame(bool open)
    {
        if (settingsPanelInGame != null)
        {
            settingsPanelInGame.SetActive(open);
        }
    }

    public void Die()
    {
        isDead = true;
        PlaySFX(deathSFX);
        rb.linearVelocity = Vector2.zero;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    private void CompleteLevel()
    {
        PlaySFX(winSFX);

        if (winPanel != null)
            winPanel.SetActive(true);

        isDead = true;
        Time.timeScale = 0f;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    private void UpdateHUD()
    {
        if (textScore != null)
            textScore.text = "Puntaje: " + totalScore;

        if (textCoins != null)
            textCoins.text = $"Monedas: {coinsCollected}/{totalCoins}";

        if (textDiamonds != null)
            textDiamonds.text = $"Diamantes: {diamondsCollected}/{totalDiamonds}";

        if (textKeys != null)
            textKeys.text = $"Llaves: {keysCollected}/{totalKeys}";
    }

    private void FlipSprite()
    {
        if (horizontalInput < 0 && isFacingRight)
        {
            transform.localScale = new Vector3(
                -Mathf.Abs(transform.localScale.x),
                transform.localScale.y,
                transform.localScale.z
            );

            isFacingRight = false;
        }
        else if (horizontalInput > 0 && !isFacingRight)
        {
            transform.localScale = new Vector3(
                Mathf.Abs(transform.localScale.x),
                transform.localScale.y,
                transform.localScale.z
            );

            isFacingRight = true;
        }
    }

    private void PlaySFX(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundCheckRadius
        );
    }
}

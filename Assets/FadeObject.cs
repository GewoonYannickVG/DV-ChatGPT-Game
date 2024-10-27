using System.Collections;
using UnityEngine;

public class FadeObject2D : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 1f; // Duration to fade in/out
    [SerializeField] private float waitDuration = 0.5f; // Duration to wait before fading back in

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private Color originalColor;
    private Coroutine fadeCoroutine;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        originalColor = spriteRenderer.color;

        if (spriteRenderer == null || boxCollider == null)
        {
            Debug.LogError("SpriteRenderer or BoxCollider2D component is missing.");
            return;
        }

        fadeCoroutine = StartCoroutine(FadeLoop());
    }

    private IEnumerator FadeLoop()
    {
        while (true)
        {
            // Fade out
            yield return Fade(0f);

            // Disable collider
            boxCollider.enabled = false;

            // Wait
            yield return new WaitForSeconds(waitDuration);

            // Fade in
            yield return Fade(1f);

            // Enable collider at 0.5 opacity
            boxCollider.enabled = true;

            // Continue fading in to full opacity
            yield return Fade(1f, 0.5f);
        }
    }

    private IEnumerator Fade(float targetOpacity, float startOpacity = 1f)
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startOpacity, targetOpacity, elapsedTime / fadeDuration);
            SetOpacity(alpha);
            yield return null;
        }

        SetOpacity(targetOpacity);
    }

    private void SetOpacity(float alpha)
    {
        Color color = originalColor;
        color.a = alpha;
        spriteRenderer.color = color;
    }
}
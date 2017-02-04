using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]

public class ScreenFader : MonoBehaviour
{

    private float m_alphaValue = 1;
    private float m_actualValue;
    private Image m_image;

    void Start()
    {
        m_image = GetComponent<Image>();
    }

    void Update()
    {
        if (m_alphaValue < m_actualValue)
        {
            m_alphaValue += 1 * Time.deltaTime;
        }
        else if (m_alphaValue > m_actualValue)
        {
            m_alphaValue -= 1 * Time.deltaTime;
        }

        m_image.color = new Color(m_image.color.r, m_image.color.g, m_image.color.b, m_alphaValue);
    }

    public void FadeOut()
    {
        m_alphaValue = 0;
        m_actualValue = 1;
    }

    public void FadeIn()
    {
        m_alphaValue = 1;
        m_actualValue = 0;
    }
}

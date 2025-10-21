using UnityEngine;
using TMPro;
public class FrameRate : MonoBehaviour
{
    [SerializeField]TextMeshProUGUI m_TextMeshPro;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_TextMeshPro=gameObject.GetComponent<TextMeshProUGUI>();
        Application.targetFrameRate = 60;

    }

    // Update is called once per frame
    void Update()
    {
        
        float fps = 1.0f / Time.deltaTime;

       
        m_TextMeshPro.text = fps.ToString();
    }
}

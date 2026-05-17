using UnityEngine;

public class RedireccionWeb : MonoBehaviour
{
    [SerializeField] private string urlPagina = "https://2r13b9r4-3000.uks1.devtunnels.ms/";

    public void AbrirEnlaceWeb()
    {
        if (!string.IsNullOrEmpty(urlPagina))
        {
            Application.OpenURL(urlPagina);
            Debug.Log("Abriendo la web: " + urlPagina);
        }
        else
        {
            Debug.LogWarning("La URL está vacía. Añade un enlace en el Inspector.");
        }
    }
}
using UnityEngine;

namespace Hai.Project12.InteractionElements.Runtime
{
    public class P12Action : MonoBehaviour
    {
        public GameObject visuals;
        public AudioSource audioSource;

        private void OnTriggerEnter(Collider other)
        {
            if (other.name.Contains("[Key]"))
            {
                // other.gameObject.SetActive(false);
                audioSource.Play();
                visuals.SetActive(false);
            }
        }
    }
}

using UnityEngine;

namespace Hai.Project12.RigidbodyAdditions
{
    public class P12CollisionSystem : MonoBehaviour
    {
        [SerializeField] private AudioClip[] concrete;
        [SerializeField] private AudioSource audioSource;
        private float _lastTime;
        private int _collisionBudgetThisFrame;

        private const float DefaultVolume = 1f;

        public AudioClip TempSfx()
        {
            return concrete[Random.Range(0, concrete.Length)];
        }

        public void PlayCollision(AudioClip collisionSfx, Vector3 position, float collisionForce01)
        {
            if (Time.time != _lastTime)
            {
                _lastTime = Time.time;
                if (_collisionBudgetThisFrame < 6)
                {
                    _collisionBudgetThisFrame += 1;
                }
            }

            if (_collisionBudgetThisFrame > 0)
            {
                audioSource.transform.position = position;
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(collisionSfx, DefaultVolume * collisionForce01);
                _collisionBudgetThisFrame--;
            }
        }
    }
}

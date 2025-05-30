using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Hai.Project12.UserInterfaceElements
{
    public class P12AnimButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
    {
        private const float DurationSeconds = 0.25f;
        private const float Distance = 1.5f;

        private Vector3 _originalLocalPos;
        private float _timeout;

        public void OnPointerEnter(PointerEventData eventData)
        {
            PlayAnimation();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            PlayAnimation();
        }

        private void PlayAnimation()
        {
            if (_originalLocalPos == Vector3.zero) _originalLocalPos = transform.localPosition;

            var alreadyRunning = Time.time < _timeout;
            _timeout = Time.time + DurationSeconds;

            if (!alreadyRunning)
            {
                StartCoroutine(nameof(Animate));
            }
        }

        public IEnumerator Animate()
        {
            while (Time.time <= _timeout)
            {
                yield return new WaitForSeconds(0f);
                var timeLerp = ((_timeout - Time.time) / DurationSeconds);
                transform.localPosition = _originalLocalPos + Vector3.right * Mathf.Sin(timeLerp * 180 * Mathf.Deg2Rad) * Distance;
            }
            transform.localPosition = _originalLocalPos;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
#if !HVR_IS_BASIS
using UdonSharp;
using VRC.SDKBase;
#endif

namespace Hai.Project12.DataViz.Runtime
{
    public class H12LingeringText
    {
        public P12DebugInfoDisplay display;
        public float expirationTimeSeconds;
    }

#if !HVR_IS_BASIS || HVR_BASIS_USES_SHIMS
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class QuickDataViz : UdonSharpBehaviour
#else
    public class P12QuickDataViz : MonoBehaviour
#endif
    {
        [SerializeField] private GameObject lineRendererTemplate;
        [SerializeField] private GameObject debugInfoDisplayTemplate;

        public bool isAvailable = false;

        private bool _shouldAcceptNewDrawCommands = true;
        private readonly List<LineRenderer> _lineRenderers = new List<LineRenderer>();
        private int _totalLinesDrawnLastFrame;
        private int _totalLinesDrawnThisFrame = 0;
        private readonly List<P12DebugInfoDisplay> _textReserve = new List<P12DebugInfoDisplay>();
        private readonly List<H12LingeringText> _textBeingShown = new List<H12LingeringText>();

        private const float BaseWidth = 0.01f;

        private void OnEnable()
        {
            lineRendererTemplate.gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            var index = 0;
            while (index < _textBeingShown.Count)
            {
                var lingeringText = _textBeingShown[index];
                if (Time.time > lingeringText.expirationTimeSeconds)
                {
                    lingeringText.display.gameObject.SetActive(false);
                    _textBeingShown.Remove(lingeringText);
                    _textReserve.Add(lingeringText.display);
                }
                else
                {
                    index++;
                }
            }

            if (_totalLinesDrawnLastFrame > _totalLinesDrawnThisFrame)
            {
                for (var i = _totalLinesDrawnThisFrame; i < _totalLinesDrawnLastFrame; i++)
                {
                    _lineRenderers[i].gameObject.SetActive(false);
                }
            }
            _totalLinesDrawnLastFrame = _totalLinesDrawnThisFrame;
            _totalLinesDrawnThisFrame = 0;
        }

        public void DrawTextLingering(string msg, Vector3 pos, float lingeringSeconds, Color color)
        {
            if (!isAvailable) return;
            if (!_shouldAcceptNewDrawCommands) return;

            P12DebugInfoDisplay current;
            if (_textReserve.Count > 0)
            {
                current = _textReserve[_textReserve.Count - 1];
                _textReserve.Remove(current);
            }
            else
            {
                current = Object.Instantiate(debugInfoDisplayTemplate, transform).GetComponent<P12DebugInfoDisplay>();
            }

            var lingering = new H12LingeringText();
            lingering.display = current;
            lingering.expirationTimeSeconds = Time.time + lingeringSeconds;

            current._SetDesiredPosition(pos);
            current._SetText(msg);
            current._SetColor(color);

            current.gameObject.SetActive(true);
            _textBeingShown.Add(lingering);
        }

#if !HVR_IS_BASIS
        public void _DrawGizmo(VRCPlayerApi.TrackingData hand)
#else
        public void _DrawGizmo(CrossTrackerData hand)
#endif
        {
            if (!isAvailable) return;
            if (!_shouldAcceptNewDrawCommands) return;

            _DrawLine(hand.position, hand.position + hand.rotation * Vector3.right * 0.1f, Color.red, Color.red, 1f);
            _DrawLine(hand.position, hand.position + hand.rotation * Vector3.up * 0.1f, Color.green, Color.green, 1f);
            _DrawLine(hand.position, hand.position + hand.rotation * Vector3.forward * 0.1f, Color.blue, Color.blue, 1f);
        }

        public void _DrawLine(Vector3 from, Vector3 to, Color start, Color end, float relativeWidth)
        {
            if (!isAvailable) return;
            if (!_shouldAcceptNewDrawCommands) return;

            var line = NextLineRenderer();
            line.positionCount = 2;
            line.SetPosition(0, from);
            line.SetPosition(1, to);
            line.startColor = start;
            line.endColor = end;
            line.widthMultiplier = BaseWidth * relativeWidth;
        }

        private LineRenderer NextLineRenderer()
        {
            _totalLinesDrawnThisFrame++;
            if (_lineRenderers.Count < _totalLinesDrawnThisFrame)
            {
                var copy = Instantiate(lineRendererTemplate, transform);
                copy.gameObject.SetActive(true);

                var ourNewLineRenderer = copy.GetComponent<LineRenderer>();
                _lineRenderers.Add(ourNewLineRenderer);

                return ourNewLineRenderer;
            }

            var nextLineRenderer = _lineRenderers[_totalLinesDrawnThisFrame - 1];
            nextLineRenderer.gameObject.SetActive(true);

            return nextLineRenderer;
        }

        public void _ProvideSetting(bool wantsDebugVisuals)
        {
            isAvailable = wantsDebugVisuals;
        }
    }
}

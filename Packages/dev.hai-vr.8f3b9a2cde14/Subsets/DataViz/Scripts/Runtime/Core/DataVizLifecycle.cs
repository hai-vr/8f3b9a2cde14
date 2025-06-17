using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Resilience.Visualize
{
    [DefaultExecutionOrder(30000)]
    public class DataVizLifecycle : MonoBehaviour
    {
        [SerializeField] private Material lineMaterial;
        [SerializeField] private Transform textPrefab;
        public bool accept = true;

        private DataViz _dataViz;

        private Transform _lineRendererHolder;
        private int _totalLinesToDraw;
        private List<LineRenderer> _lineRenderers = new List<LineRenderer>();
        private readonly float baseWidth = 0.005f;

        private Transform _textRendererHolder;
        private int _totalTextToDraw;
        private List<Transform> _textRenderers = new List<Transform>();
        private int _totalTextToBeDrawnThisFrame;
        private bool _isEnabled;
        private bool _rotationIsLocked;
        private bool ShouldAcceptNewDrawCommands => _isEnabled && accept;

        private void OnEnable()
        {
            if (_dataViz == null)
            {
                _dataViz = new DataViz();
                _lineRendererHolder = NewHolder("LineRenderer");
                _textRendererHolder = NewHolder("TextRenderer");
                DataViz.DeclareInstance(_dataViz, this);
            }

            Camera.onPreCull += OnAnyCameraPreRender;
            _isEnabled = true;

            // Try to fix serialization issue on unity reload, so that when unity reloads, this continues to function.
            // This is probably not the proper way to fix it (?)
            _dataViz = new DataViz();
            DataViz.DeclareInstance(_dataViz, this);
        }

        private void OnDisable()
        {
            Camera.onPreCull -= OnAnyCameraPreRender;
            _isEnabled = false;
        }

        public void ApplyVRLock(Quaternion cameraRotation, float visionScale)
        {
            _rotationIsLocked = true;
            RotateAndScaleText(cameraRotation, visionScale);
        }

        public void UnlockVR()
        {
            _rotationIsLocked = false;
        }

        private void OnAnyCameraPreRender(Camera which)
        {
            if (_rotationIsLocked) return;

            RotateAndScaleText(which.transform.rotation, 1f);
        }

        private void RotateAndScaleText(Quaternion cameraRotation, float visionScale)
        {
            var transformRotation = Quaternion.LookRotation(cameraRotation * Vector3.forward);
            for (var i = 0; i < _totalTextToBeDrawnThisFrame; i++)
            {
                _textRenderers[i].rotation = transformRotation;
                _textRenderers[i].localScale = Vector3.one * (visionScale * 0.001f);
            }
        }

        private Transform NewHolder(string what)
        {
            return new GameObject
            {
                transform = { parent = transform },
                name = $"DataViz{what}Holder"
            }.transform;
        }

        private void LateUpdate()
        {
            for (var i = 0; i < _lineRenderers.Count; i++)
            {
                _lineRenderers[i].enabled = i < _totalLinesToDraw;
            }
            for (var i = 0; i < _textRenderers.Count; i++)
            {
                _textRenderers[i].gameObject.SetActive(i < _totalTextToDraw);
            }

            _totalTextToBeDrawnThisFrame = _totalTextToDraw;
            _totalLinesToDraw = 0;
            _totalTextToDraw = 0;
        }

        public void Text(Vector3 position, Color color, string message)
        {
            if (!ShouldAcceptNewDrawCommands) return;

            var text = NextTextRenderer();
            text.position = position;
            var textComp = text.GetComponentInChildren<Text>();
            textComp.text = message;
            textComp.color = color;
        }

        private Transform NextTextRenderer()
        {
            Transform prefab;
            if (_totalTextToDraw == _textRenderers.Count)
            {
                var newTextRenderer = InstantiateNewTextRenderer();
                _textRenderers.Add(newTextRenderer);
                prefab = newTextRenderer;
            }
            else
            {
                prefab = _textRenderers[_totalTextToDraw];
            }
            _totalTextToDraw++;

            return prefab;
        }

        public void Line(Vector3[] positions, Color start, Color end, float relativeWidth)
        {
            if (!ShouldAcceptNewDrawCommands) return; // Prevent component from filling up when DataViz is disabled during debugging

            var line = NextLineRenderer();
            line.positionCount = positions.Length;
            line.SetPositions(positions);
            line.startColor = start;
            line.endColor = end;
            line.widthMultiplier = baseWidth * relativeWidth;
        }

        private LineRenderer NextLineRenderer()
        {
            LineRenderer line;
            if (_totalLinesToDraw == _lineRenderers.Count)
            {
                var newLineRenderer = InstantiateNewLineRenderer();
                _lineRenderers.Add(newLineRenderer);
                line = newLineRenderer;
            }
            else
            {
                line = _lineRenderers[_totalLinesToDraw];
            }
            _totalLinesToDraw++;

            return line;
        }

        private LineRenderer InstantiateNewLineRenderer()
        {
            var o = new GameObject
            {
                transform = { parent = _lineRendererHolder },
                name = "DataVizLineRenderer"
            };
            var lineRenderer = o.AddComponent<LineRenderer>();
            lineRenderer.widthMultiplier = baseWidth;
            lineRenderer.sharedMaterial = lineMaterial;
            return lineRenderer;
        }

        private Transform InstantiateNewTextRenderer()
        {
            var instantiate = Instantiate(textPrefab, _textRendererHolder, true);
            instantiate.name = "DataVizTextRenderer";
            return instantiate;
        }
    }
}

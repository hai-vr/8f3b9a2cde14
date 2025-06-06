using System.Collections.Generic;
using Hai.Project12.HaiSystems.Supporting;
using HVR.Basis.Comms;
using UnityEngine;

namespace Hai.Project12.Vixxy.Runtime
{
    public class P12VixxyAggregator : MonoBehaviour, I12VixxyAggregator
    {
        [SerializeField] private string addressA;
        [SerializeField] private string addressB;
        [SerializeField] private string outputAddress;
        [EarlyInjectable] public P12VixxyOrchestrator orchestrator;
        [LateInjectable] public AcquisitionService acquisitionService;

        private readonly HashSet<P12VixxyAggregator> _transformerResult = new();
        private readonly HashSet<P12VixxyActuatorSampler> _actuatorResult = new();

        private float _activeResult = float.MinValue;
        private bool _hasNeverBeenAggregated = true;

        // FIXME: We need some way to initialize those values. It's probably the job of the orchestrator to do this.
        private float a = 0f;
        private float b = 0f;

        private void Awake()
        {
            acquisitionService = AcquisitionService.SceneInstance;

            if (string.IsNullOrEmpty(addressA) || string.IsNullOrEmpty(addressB) || string.IsNullOrEmpty(outputAddress))
            {
                H12Debug.LogWarning($"{nameof(P12VixxyAggregator)} actuator named \"{name}\" has some missing addresses. It will be disabled.", H12Debug.LogTag.Vixxy);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            orchestrator.RegisterAggregator(addressA, this);
            orchestrator.RegisterAggregator(addressB, this);
            // FIXME: Address registration should be inside the orchestrator
            acquisitionService.RegisterAddresses(new []{ addressA, addressB }, OnAddressUpdated);
        }

        private void OnDisable()
        {
            orchestrator.UnregisterAggregator(addressA, this);
            orchestrator.UnregisterAggregator(addressB, this);
            // FIXME: Address registration should be inside the orchestrator
            acquisitionService.UnregisterAddresses(new []{ addressA, addressB }, OnAddressUpdated);
        }

        private void OnAddressUpdated(string whichAddress, float value)
        {
            if (whichAddress == addressA) a = value;
            else if (whichAddress == addressB) b = value;

            orchestrator.PassAddressUpdated(whichAddress);
        }

        public bool TryAggregate(out IEnumerable<I12VixxyAggregator> aggregators, out IEnumerable<I12VixxyActuator> actuators)
        {
            var result = a * b;

            aggregators = _transformerResult;
            actuators = _actuatorResult;

            if (_hasNeverBeenAggregated || _activeResult != result)
            {
                // First aggregation is always considered a successful aggregation, for initialization purposes.
                _hasNeverBeenAggregated = false;
                _activeResult = result;

                orchestrator.ProvideValue(outputAddress, result);

                return true;
            }

            // Even if an input changes, if the output doesn't change, then it will not result in a change on the actuators.
            return false;
        }
    }
}

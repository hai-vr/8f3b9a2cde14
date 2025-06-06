using System.Collections.Generic;
using Hai.Project12.HaiSystems.Supporting;
using HVR.Basis.Comms;
using UnityEngine;

namespace Hai.Project12.Vixxy.Runtime
{
    public class P12VixxyAggregator : MonoBehaviour, I12VixxyAggregator
    {
        private const string AddressA = "??? A";
        private const string AddressB = "??? B";

        [EarlyInjectable] public P12VixxyOrchestrator orchestrator;
        [LateInjectable] public AcquisitionService acquisitionService;

        private readonly HashSet<P12VixxyAggregator> _transformerResult = new();
        private readonly HashSet<P12VixxyActuatorSampler> _actuatorResult = new();

        private float _activeResult = float.MinValue;
        private bool _hasNeverBeenAggregated = true;

        private void Awake()
        {
            acquisitionService = AcquisitionService.SceneInstance;
        }

        private void OnEnable()
        {
            orchestrator.RegisterAggregator(AddressA, this);
            orchestrator.RegisterAggregator(AddressB, this);
            acquisitionService.RegisterAddresses(new []{ AddressA, AddressB }, OnAddressUpdated);
        }

        private void OnDisable()
        {
            orchestrator.UnregisterAggregator(AddressA, this);
            orchestrator.UnregisterAggregator(AddressB, this);
            acquisitionService.UnregisterAddresses(new []{ AddressA, AddressB }, OnAddressUpdated);
        }

        private void OnAddressUpdated(string whichAddress, float value)
        {
            orchestrator.PassAddressUpdated(whichAddress);
        }

        public bool TryAggregate(out IEnumerable<I12VixxyAggregator> aggregators, out IEnumerable<I12VixxyActuator> actuators)
        {
            var result = 0f;

            aggregators = _transformerResult;
            actuators = _actuatorResult;

            if (_hasNeverBeenAggregated)
            {
                // First aggregation is always considered a successful aggregation, for initialization purposes.
                _hasNeverBeenAggregated = false;
                _activeResult = result;
                return true;
            }

            if (_activeResult != result)
            {
                _activeResult = result;
                return true;
            }

            // Even if an input changes, if the output doesn't change, then it will not result in a change on the actuators.
            return false;
        }
    }
}

using System.Collections.Generic;
using Hai.Project12.HaiSystems.Supporting;
using HVR.Basis.Comms;
using UnityEngine;

namespace Hai.Project12.Vixxy.Runtime
{
    /// There is one instance of this **per avatar** or **per world object**.
    public class P12VixxyOrchestrator : MonoBehaviour
    {
        // TODO:
        // - Collect arriving data.
        // - When data arrives, we mark the transformer consumers and the actuator consumers of that data.
        // - When all data arrived, and we're starting the update cycle, we wake up all transformer consumers of that data.

        [LateInjectable] [SerializeField] private AcquisitionService acquisitionService;
        private readonly HashSet<P12Aggregator> _aggregatorsToUpdateThisTick = new HashSet<P12Aggregator>();
        private readonly HashSet<P12Actuator> _actuatorsToUpdateThisTick = new HashSet<P12Actuator>();
        private bool _anythingNeedsUpdating;

        private readonly HashSet<P12Aggregator> _workAggregators = new HashSet<P12Aggregator>();

        private void Awake()
        {
            if (!acquisitionService) acquisitionService = AcquisitionService.SceneInstance;

            // TODO (CURRENTLY DOING): In AcquisitionService, acquisitors events are raised as soon as the data arrives.
            // We need to deck all arrivals so that we only run the consumer aggregators once.
            var addressIdentifier = "TestInput";
            acquisitionService.RegisterAddresses(new[] { addressIdentifier }, (address, value) =>
            {
                // TODO: Store received addresses and value

                var aggregators = TEMP_FindAggregatorsOf(addressIdentifier);
                var actuators = TEMP_FindActuatorsOf(addressIdentifier);

                _aggregatorsToUpdateThisTick.UnionWith(aggregators);
                _actuatorsToUpdateThisTick.UnionWith(actuators);
                _anythingNeedsUpdating = true;
            });
        }

        private IEnumerable<P12Aggregator> TEMP_FindAggregatorsOf(string addressListener)
        {
            return new List<P12Aggregator>();
        }

        private IEnumerable<P12Actuator> TEMP_FindActuatorsOf(string addressListener)
        {
            return new List<P12Actuator>();
        }

        private void Update()
        {
            // TODO: This update loop must only run after the services that submit to AcquisitionService have run.
            // Execution order may need tweaking.
            if (!_anythingNeedsUpdating) return;

            // Randomness in the number of iteration cycles tries to ensure we don't get implementation-specific behaviour that expects
            // a specific number of cycles to happen.
            var randomIterations = UnityEngine.Random.Range(5, 10);
            while (randomIterations > 0 && _aggregatorsToUpdateThisTick.Count > 0)
            {
                randomIterations--;
                // Starting a new cycle.
                _workAggregators.Clear();
                _workAggregators.UnionWith(_aggregatorsToUpdateThisTick);
                _aggregatorsToUpdateThisTick.Clear();

                foreach (var aggregator in _workAggregators)
                {
                    if (aggregator.TryAggregate(out var newAggregators, out var newActuators))
                    {
                        _aggregatorsToUpdateThisTick.UnionWith(newAggregators);
                        _actuatorsToUpdateThisTick.UnionWith(newActuators);
                    }
                }
            }

            // Deck remaining aggregations for next frame. We already gave it a bunch of chances.
            _anythingNeedsUpdating = _aggregatorsToUpdateThisTick.Count > 0;

            // TODO: It may be possible to do a reverse graph traversal, where we deny listening to addresses
            // or processing aggregators if there are no actuators that listen to that data in the first place.
            if (_actuatorsToUpdateThisTick.Count > 0)
            {
                foreach (var actuator in _actuatorsToUpdateThisTick)
                {
                    actuator.Actuate();
                }

                _actuatorsToUpdateThisTick.Clear();
            }
        }
    }
}

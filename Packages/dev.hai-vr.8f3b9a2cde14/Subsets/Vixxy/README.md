Vixxy
===

Rethinking Vixen for use in Basis.

Data acquisition:
- Data, or impulses (0-bit data) comes in from different sources.
- When the data changes, or when an impulse is received, we take note of which aggregators and actuators we'll need to wake up.
  - We do not want to inform the consumers immediately, so that those consumers have a complete view of the data that arrived.
- Once all data has arrived for this tick, we wake the data aggregators.

Data aggregators:
- After data arrives, some of the consumers of that data may be aggregators. They will combine the data to produce new values.
- Data aggregators can depend on other data aggregators, but it must reach stability. No smoothers or endless loops.
  - Interpolation and/or smoothing will be built-in into the acquisition and aggregators.
- We run the aggregators that are consumers of the data that arrived.
- We check if any of the results of the aggregators has changed.
  - If any has changed, we run the aggregators again, if at least one aggregator is also a consumer of the data that was aggregated.
  - We run the aggregators again as many times as 5 to 10 times, but the number of iterations must be random every time so that
    this doesn't create an implementation-specific dependency.
  - If by any chance this cycle is interrupted between that 5 to 10 times, we remember that for the next tick and treat it as it being a
    result of data arrival.
- (Aggregators are run at least once on the first run for initialization purposes)

Some of this data, including aggregated data, will get transmitted over the network.

Data aggregators provides the backend to improve the function of Vixen flags, as flags that depend on other flags
will be solved using these iteration cycles, within one frame.

Actuators:
- The actuators that consume of any data that has changed (either in arrival or aggregator) get updated.
- (Actuators are run at least once on the first run for initialization purposes)

---

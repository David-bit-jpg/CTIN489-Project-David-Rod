namespace USCG.Core.Telemetry.Internal
{
    using UnityEngine;

    using System;
    using System.Text;

    /// <summary>
    /// This metric is used to acculumate a value over time. The underlying value is stored as a
    /// float, but this can be used to track integer values as well.
    /// </summary>
    [Serializable]
    public class AccumulatedMetric : Metric
    {
        [SerializeField] private float _accumulatedValue = default;

        public float accumulatedValue { get => _accumulatedValue; }

        public void Accumulate(float inValue)
        {
            _accumulatedValue += inValue;
        }

        public override void Reset()
        {
            _accumulatedValue = 0.0f;
        }

        public override string ToCsv(MetricId metricId)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"{metricId.metricName},{_accumulatedValue}");
            return stringBuilder.ToString();
        }
    }
}

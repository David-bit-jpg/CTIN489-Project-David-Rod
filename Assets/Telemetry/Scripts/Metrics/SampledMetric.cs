namespace USCG.Core.Telemetry.Internal
{
    using UnityEngine;

    using System;
    using System.Text;
    using System.Collections.Generic;

    /// <summary>
    /// This metric is used to gather samples of multiple values over time. The underlying type
    /// can be anything, although float, int, and Vector values will be most common.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class TSampledMetric<T> : Metric
    {
        [SerializeField] private List<T> _sampledValues = new List<T>();

        public List<T> sampledValues { get => _sampledValues; }

        public void AddSample(T inSampleValue)
        {
            _sampledValues.Add(inSampleValue);
        }

        public override void Reset()
        {
            _sampledValues.Clear();
        }

        public override string ToCsv(MetricId metricId)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"{metricId.metricName},");
            foreach (T sampledValue in _sampledValues)
            {
                stringBuilder.AppendLine($",{Sanitize(sampledValue.ToString(), ' ')}");
            }
            return stringBuilder.ToString();
        }
    }
}

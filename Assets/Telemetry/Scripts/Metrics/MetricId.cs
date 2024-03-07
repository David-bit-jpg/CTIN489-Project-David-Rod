namespace USCG.Core.Telemetry
{
    using UnityEngine;

    using System;
    using System.Collections.Generic;

    [Serializable]
    public class MetricId : IComparable<MetricId>, IEquatable<MetricId>, IEqualityComparer<MetricId>
    {
        [SerializeField] private string _metricName = string.Empty;

        public string metricName { get => _metricName; }

        public MetricId(string inMetricName)
        {
            _metricName = inMetricName;
        }

        public int CompareTo(MetricId other)
        {
            return _metricName.CompareTo(other._metricName);
        }

        public bool Equals(MetricId other)
        {
            return _metricName.Equals(other._metricName);
        }

        public bool Equals(MetricId x, MetricId y)
        {
            return x._metricName.Equals(y._metricName);
        }

        public int GetHashCode(MetricId obj)
        {
            return obj._metricName.GetHashCode();
        }
    }
}

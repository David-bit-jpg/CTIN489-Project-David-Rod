namespace USCG.Core.Telemetry.Internal
{
    using System;

    [Serializable]
    public abstract class Metric
    {
        /// <summary>
        /// Creates a CSV representation for this metric given the provided MetricId.
        /// </summary>
        /// <param name="metricId"></param>
        /// <returns></returns>
        public abstract string ToCsv(MetricId metricId);

        /// <summary>
        /// Resets this metric by clearing all of the data associated with the MetricId.
        /// </summary>
        public abstract void Reset();

        // Helper method to sanitize string values for metrics. The .ToString() method for
        // Vector3, for example, uses commas, which would make for a rather poor .csv.
        protected static string Sanitize(string inString, char commaReplacement)
        {
            return inString.Replace(',', commaReplacement);
        }
    }
}

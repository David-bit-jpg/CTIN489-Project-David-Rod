namespace USCG.Core.Telemetry
{
    using USCG.Core.Telemetry.Internal;

    using UnityEngine;

    using System;
    using System.IO;
    using System.Text;
    using System.Collections.Generic;

    public class TelemetryManager : MonoBehaviour
    {
        [Header("Telemetry Manager Settings")]

        [Tooltip("Whether or not metrics will be printed when the application quits.")]
        [SerializeField] private bool _printMetricsOnQuit = true;

        [Tooltip("Whether or not to print metrics while running in the editor.")]
        [SerializeField] private bool _printMetricsWhileInEditor = false;

        [Tooltip("Pressing the combination of these keys will write all current metrics values to a file and reset them.")]
        [SerializeField]
        private List<KeyCode> _printAndResetKeyCodes = new()
        {
            KeyCode.LeftCommand,
            KeyCode.LeftShift,
            KeyCode.M
        };

        private Dictionary<MetricId, object> _metrics = new Dictionary<MetricId, object>();
        private bool _bIsPrintingMetrics = false;

        /// <summary>
        /// Access the singleton instance of the TelemetryManager.
        /// </summary>
        public static TelemetryManager instance { get => _instance; }
        private static TelemetryManager _instance = default;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                DestroyImmediate(this);
            }
        }

        private void Update()
        {
            if (ArePrintAndResetKeyCodesPressed())
            {
                PrintMetricsImmediate(true);
            }
            else
            {
                _bIsPrintingMetrics = false;
            }
        }

        private void OnApplicationQuit()
        {
            if (_printMetricsOnQuit)
            {
                PrintMetricsImmediate();
            }
        }

        /// <summary>
        /// Creates a new accumulated metric with the given name. The returned value should be used with
        /// the AccumulateMetric method.
        /// </summary>
        /// <param name="inMetricName"></param>
        /// <returns></returns>
        public MetricId CreateAccumulatedMetric(string inMetricName)
        {
            MetricId metricId = new MetricId(inMetricName);

            if (_metrics.ContainsKey(metricId))
            {
                Debug.LogWarning($"Metric {inMetricName} was not created because a metric with that name already exists.");
                return default;
            }

            _metrics.Add(metricId, new AccumulatedMetric());
            return metricId;
        }

        /// <summary>
        /// Accumulates the specified metric with the given MetricId by the provided amount.
        /// </summary>
        /// <param name="metricId"></param>
        /// <param name="value"></param>
        public void AccumulateMetric(MetricId metricId, float value)
        {
            if (!_metrics.ContainsKey(metricId))
            {
                Debug.LogWarning($"Metric {metricId.metricName} does not exist!");
                return;
            }

            try
            {
                AccumulatedMetric accumulatedMetric = (AccumulatedMetric)_metrics[metricId];
                accumulatedMetric.Accumulate(value);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.Message);
            }
        }

        /// <summary>
        /// Creates a new sampled metric with the given name. The returned value should be used with
        /// the AddMetricSample method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inMetricName"></param>
        /// <returns></returns>
        public MetricId CreateSampledMetric<T>(string inMetricName)
        {
            MetricId metricId = new MetricId(inMetricName);

            if (_metrics.ContainsKey(metricId))
            {
                Debug.LogWarning($"Metric {inMetricName} was not created because a metric with that name already exists.");
                return default;
            }

            _metrics.Add(metricId, new TSampledMetric<T>());
            return metricId;
        }

        /// <summary>
        /// Adds a sample for the specified metric with the given MetricId. The type of value must
        /// match the type used to create the original metric.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="metricId"></param>
        /// <param name="value"></param>
        public void AddMetricSample<T>(MetricId metricId, T value)
        {
            if (!_metrics.ContainsKey(metricId))
            {
                Debug.LogWarning($"Metric {metricId.metricName} does not exist!");
                return;
            }

            try
            {
                TSampledMetric<T> sampledMetric = (TSampledMetric<T>)_metrics[metricId];
                sampledMetric.AddSample(value);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.Message);
            }
        }

        // Prints all metrics to a file. The optional parameter can be used to reset all
        // current metrics, which will clear their values.
        private void PrintMetricsImmediate(bool bResetMetrics = false)
        {
            // Don't print anything if metrics are still printing. This gets reset in Update().
            if (_bIsPrintingMetrics)
            {
                return;
            }

            // Don't print anything while running in editor unless it's explicitly allowed.
            if (Application.isEditor && !_printMetricsWhileInEditor)
            {
                return;
            }

            // Always flip this value to be true. In Update() we check for whether or not the
            // keys have been released and turn this value false again. Without this safeguard,
            // users could "print" multiple files each frame while the keys are held down.
            _bIsPrintingMetrics = true;

            StringBuilder stringBuilder = new StringBuilder();
            foreach (KeyValuePair<MetricId, object> kvp in _metrics)
            {
                Metric metric = (Metric)kvp.Value;
                stringBuilder.AppendLine(metric.ToCsv(kvp.Key));
                if (bResetMetrics)
                {
                    metric.Reset();
                }
            }

            // Sanitize the file name to make sure it doesn't contain bad characters.
            string dateTime = DateTime.Now.ToString();
            dateTime = dateTime.Replace("/", "_");
            dateTime = dateTime.Replace(":", "_");
            dateTime = dateTime.Replace(" ", "___");

            string filePath = $"{Application.persistentDataPath}/{Application.productName}-Metrics-{dateTime}.csv";
            File.WriteAllText(filePath, stringBuilder.ToString());
            Debug.Log($"Metrics written to {filePath}");
        }

        // Helper method to check if the keys are pressed.
        private bool ArePrintAndResetKeyCodesPressed()
        {
            bool bArePrintAndResetKeyCodesPressed = _printAndResetKeyCodes.Count > 0;
            foreach (KeyCode key in _printAndResetKeyCodes)
            {
                bArePrintAndResetKeyCodesPressed &= Input.GetKey(key);
            }
            return bArePrintAndResetKeyCodesPressed;
        }
    }
}

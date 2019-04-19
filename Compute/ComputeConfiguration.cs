using System;
using System.ComponentModel;
using System.Configuration;
using System.Text;

namespace Compute
{
    public sealed class ComputeConfiguration
    {
        private static readonly Lazy<ComputeConfiguration> config = new Lazy<ComputeConfiguration>(() => new ComputeConfiguration());

        private ComputeConfiguration()
        {
            this.NumberOfContainersToStart = Convert<int>("NumberOfContainersToStart");
            this.MinPort = Convert<ushort>("MinPort");
            this.MaxPort = Convert<ushort>("MaxPort");
        }

        /// <summary>
        /// Returns singleton instance of ComputeConfiguration class
        /// </summary>
        public static ComputeConfiguration Instance => config.Value;

        public int NumberOfContainersToStart { get; set; }
        public ushort MinPort { get; set; }
        public ushort MaxPort { get; set; }

        /// <summary>
        /// Attempts to convert configuration value corresponding to the appSettingsConfigKey key
        /// into the given T type
        /// </summary>
        /// <typeparam name="T">Type of value to convert to</typeparam>
        /// <param name="appSettingsConfigKey">
        /// key of element in AppSettings section in config file
        /// </param>
        /// <exception cref="ConfigurationErrorsException"></exception>
        /// <exception cref="NotSupportedException">
        /// Config value could not be converted into an object of type T
        /// </exception>
        /// <returns>Parsed value</returns>
        private static T Convert<T>(string appSettingsConfigKey)
        {
            string configValue = ConfigurationManager.AppSettings.Get(appSettingsConfigKey);
            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter == null)
            {
                throw new NotSupportedException($"Value={configValue} can't be parsed into type {typeof(T)}");
            }

            return (T)converter.ConvertFromString(configValue);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"{nameof(this.NumberOfContainersToStart)} = {this.NumberOfContainersToStart}");
            builder.AppendLine($"{nameof(this.MinPort)} = {this.MinPort}");
            builder.AppendLine($"{nameof(this.MaxPort)} = {this.MaxPort}");
            return builder.ToString();
        }
    }
}
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
            this.NumberOfContainersToStart = this.GetConfigValue<int>(nameof(this.NumberOfContainersToStart));
            this.MinPort = this.GetConfigValue<ushort>(nameof(this.MinPort));
            this.MaxPort = this.GetConfigValue<ushort>(nameof(this.MaxPort));
            this.PackageFilePath = this.GetConfigValue(nameof(this.PackageFilePath));
        }

        /// <summary>
        /// Returns singleton instance of ComputeConfiguration class
        /// </summary>
        public static ComputeConfiguration Instance => config.Value;

        public ushort MaxPort { get; set; }
        public ushort MinPort { get; set; }
        public int NumberOfContainersToStart { get; set; }
        public string PackageFilePath { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"{nameof(this.NumberOfContainersToStart)} = {this.NumberOfContainersToStart}");
            builder.AppendLine($"{nameof(this.MinPort)} = {this.MinPort}");
            builder.AppendLine($"{nameof(this.MaxPort)} = {this.MaxPort}");
            builder.AppendLine($"{nameof(this.PackageFilePath)} = {this.PackageFilePath}");
            return builder.ToString();
        }

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
        private T Convert<T>(string appSettingsConfigKey)
        {
            string configValue = ConfigurationManager.AppSettings.Get(appSettingsConfigKey);
            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter == null)
            {
                throw new NotSupportedException($"Value={configValue} can't be parsed into type {typeof(T)}");
            }

            return (T)converter.ConvertFromString(configValue);
        }

        private T GetConfigValue<T>(string appSettingsConfigKey)
        {
            return this.Convert<T>(appSettingsConfigKey);
        }

        private string GetConfigValue(string appSettingsConfigKey)
        {
            return ConfigurationManager.AppSettings.Get(appSettingsConfigKey);
        }
    }
}
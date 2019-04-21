using System;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

namespace Compute
{
    internal sealed class ComputeConfiguration
    {
        private static readonly Lazy<ComputeConfiguration> config = new Lazy<ComputeConfiguration>(() => new ComputeConfiguration());

        private ComputeConfiguration()
        {
            ConfigurationItem.ContainerRelativeFilePath = this.GetConfigValue(nameof(ConfigurationItem.ContainerRelativeFilePath));
            ConfigurationItem.ContainerFullFilePath = this.GetFullPath(ConfigurationItem.ContainerRelativeFilePath);

            ConfigurationItem.MaxPort = this.GetConfigValue<ushort>(nameof(ConfigurationItem.MaxPort));
            ConfigurationItem.MinPort = this.GetConfigValue<ushort>(nameof(ConfigurationItem.MinPort));

            ConfigurationItem.NumberOfContainersToStart = this.GetConfigValue<int>(nameof(ConfigurationItem.NumberOfContainersToStart));

            ConfigurationItem.PackageConfigFileName = this.GetConfigValue(nameof(ConfigurationItem.PackageConfigFileName));

            ConfigurationItem.PackageRelativeFolderPath = this.GetConfigValue(nameof(ConfigurationItem.PackageRelativeFolderPath));
            ConfigurationItem.PackageFullFolderPath = this.GetFullPath(ConfigurationItem.PackageRelativeFolderPath);

            ConfigurationItem.PackageTempRelativeFolderPath = this.GetConfigValue(nameof(ConfigurationItem.PackageTempRelativeFolderPath));
            ConfigurationItem.PackageTempFullFolderPath = this.GetFullPath(ConfigurationItem.PackageTempRelativeFolderPath);

            ConfigurationItem.PackageAcquisitionIntervalMilliseconds = this.GetConfigValue<int>(nameof(ConfigurationItem.PackageAcquisitionIntervalMilliseconds));
        }

        /// <summary>
        /// Returns singleton instance of ComputeConfiguration class
        /// </summary>
        public static ComputeConfiguration Instance => config.Value;

        /// <summary>
        /// Returns class that holds configuration properties
        /// </summary>
        public ComputeConfigurationItem ConfigurationItem { get; } = new ComputeConfigurationItem();

        public override string ToString()
        {
            var builder = new StringBuilder();

            foreach (var property in this.GetType().GetProperties().Where(prop => prop.PropertyType != this.GetType()))
            {
                builder.Append(property.Name).Append(" = ").Append(property.GetValue(this)).AppendLine();
            }

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

        private string GetFullPath(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath));
        }
    }
}
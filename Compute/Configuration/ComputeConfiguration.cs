using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
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
            this.ConfigurationItem = new ComputeConfigurationItem();

            this.ConfigurationItem.ContainerRelativeFilePath = this.GetConfigValue(nameof(this.ConfigurationItem.ContainerRelativeFilePath));
            this.ConfigurationItem.ContainerFullFilePath = this.GetFullPath(this.ConfigurationItem.ContainerRelativeFilePath);

            this.ConfigurationItem.MaxPort = this.GetConfigValue<ushort>(nameof(this.ConfigurationItem.MaxPort));
            this.ConfigurationItem.MinPort = this.GetConfigValue<ushort>(nameof(this.ConfigurationItem.MinPort));

            this.ConfigurationItem.NumberOfContainersToStart = this.GetConfigValue<int>(nameof(this.ConfigurationItem.NumberOfContainersToStart));

            this.ConfigurationItem.PackageConfigFileName = this.GetConfigValue(nameof(this.ConfigurationItem.PackageConfigFileName));

            this.ConfigurationItem.PackageRelativeFolderPath = this.GetConfigValue(nameof(this.ConfigurationItem.PackageRelativeFolderPath));
            this.ConfigurationItem.PackageFullFolderPath = this.GetFullPath(this.ConfigurationItem.PackageRelativeFolderPath);

            this.ConfigurationItem.PackageTempRelativeFolderPath = this.GetConfigValue(nameof(this.ConfigurationItem.PackageTempRelativeFolderPath));
            this.ConfigurationItem.PackageTempFullFolderPath = this.GetFullPath(this.ConfigurationItem.PackageTempRelativeFolderPath);

            this.ConfigurationItem.PackageAcquisitionIntervalMilliseconds = this.GetConfigValue<int>(nameof(this.ConfigurationItem.PackageAcquisitionIntervalMilliseconds));

            Debug.WriteLine($"Configuration Item Created ({DateTime.Now}):" + Environment.NewLine + this.ConfigurationItem);
        }

        /// <summary>
        /// Returns singleton instance of ComputeConfiguration class
        /// </summary>
        public static ComputeConfiguration Instance => config.Value;

        /// <summary>
        /// Returns class that holds configuration properties
        /// </summary>
        public ComputeConfigurationItem ConfigurationItem { get; }

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
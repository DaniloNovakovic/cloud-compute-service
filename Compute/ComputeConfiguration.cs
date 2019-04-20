using System;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

namespace Compute
{
    public sealed class ComputeConfiguration
    {
        private static readonly Lazy<ComputeConfiguration> config = new Lazy<ComputeConfiguration>(() => new ComputeConfiguration());

        private ComputeConfiguration()
        {
            this.ContainerRelativeFilePath = this.GetConfigValue(nameof(this.ContainerRelativeFilePath));
            this.ContainerFullFilePath = this.GetFullPath(this.ContainerRelativeFilePath);

            this.MaxPort = this.GetConfigValue<ushort>(nameof(this.MaxPort));
            this.MinPort = this.GetConfigValue<ushort>(nameof(this.MinPort));

            this.NumberOfContainersToStart = this.GetConfigValue<int>(nameof(this.NumberOfContainersToStart));

            this.PackageConfigFileName = this.GetConfigValue(nameof(this.PackageConfigFileName));
            this.PackageDllFileName = this.GetConfigValue(nameof(this.PackageDllFileName));

            this.PackageRelativeFolderPath = this.GetConfigValue(nameof(this.PackageRelativeFolderPath));
            this.PackageFullFolderPath = this.GetFullPath(this.PackageRelativeFolderPath);

            this.PackageTempRelativeFolderPath = this.GetConfigValue(nameof(this.PackageTempRelativeFolderPath));
            this.PackageTempFullFolderPath = this.GetFullPath(this.PackageTempRelativeFolderPath);
        }

        /// <summary>
        /// Returns singleton instance of ComputeConfiguration class
        /// </summary>
        public static ComputeConfiguration Instance => config.Value;

        public string ContainerFullFilePath { get; }
        public string ContainerRelativeFilePath { get; }
        public ushort MaxPort { get; }
        public ushort MinPort { get; }
        public int NumberOfContainersToStart { get; }
        public string PackageConfigFileName { get; }
        public string PackageDllFileName { get; }
        public string PackageFullFolderPath { get; }
        public string PackageRelativeFolderPath { get; }
        public string PackageTempRelativeFolderPath { get; }
        public string PackageTempFullFolderPath { get; }

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
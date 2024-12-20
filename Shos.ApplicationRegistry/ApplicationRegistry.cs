using Microsoft.Win32;
using System.Reflection;
using System.Runtime.Versioning;

namespace Shos.ApplicationRegistry
{
    public static class EnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> @this, Action<T> action)
        {
            foreach (var item in @this)
                action(item);
        }
    }

    public class ApplicationRegistry
    {
        readonly string companyName;
        readonly string applicationName;

        public ApplicationRegistry(string companyName, string applicationName)
            => (this.companyName, this.applicationName) = (companyName, applicationName);

        [SupportedOSPlatform("windows")]
        public void Save<TItem>(TItem item)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));

            using var key = GetRegistryKey<TItem>(true);
            if (key is null)
                throw new ApplicationException("Failed to open registry key.");

            item.GetType().GetProperties().ForEach(property => {
                var value = GetValue(property, item);
                if (value is not null)
                    SetValueToKey(key, property.Name, value);
            });
        }

        protected virtual object? GetValue(PropertyInfo property, object item)
            => property.GetValue(item);

        protected virtual void SetValue(PropertyInfo property, object item, object value)
            => property.SetValue(item, value);

        [SupportedOSPlatform("windows")]
        void SetValueToKey(RegistryKey key, string name, object value)
        {
            if (value.GetType().IsEnum)
                key.SetValue(name, (int)value, RegistryValueKind.DWord);
            //if (value is string)
            //    key.SetValue(name, value, RegistryValueKind.String);
            //else if (value is int)
            //    key.SetValue(name, value, RegistryValueKind.DWord);
            //else if (value is byte[])
            //    key.SetValue(name, value, RegistryValueKind.Binary);
            else
                key.SetValue(name, value);
        }

        [SupportedOSPlatform("windows")]
        public TItem? Load<TItem>() where TItem : class
        {
            using var key = GetRegistryKey<TItem>(false);
            if (key is null)
                throw new ApplicationException("Failed to open registry key.");

            var item = Activator.CreateInstance<TItem>();

            typeof(TItem).GetProperties().ForEach(property => {
                var value = key.GetValue(property.Name, property.GetValue(item));
                if (value is not null)
                    SetValue(property, item, value);
            });
            return item;
        }

        [SupportedOSPlatform("windows")]
        RegistryKey? GetRegistryKey<TItem>(bool writable)
            => writable
              ? Registry.CurrentUser.CreateSubKey(GetRegistryKeyPath<TItem>())
              : Registry.CurrentUser.OpenSubKey(GetRegistryKeyPath<TItem>());

        string GetRegistryKeyPath<TItem>()
            => @$"Software\{companyName}\{applicationName}\{GetName<TItem>()}";

        string GetName<TItem>() => typeof(TItem).Name;
    }
}

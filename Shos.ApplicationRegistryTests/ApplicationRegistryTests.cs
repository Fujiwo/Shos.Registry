using System.Reflection;
using System.Runtime.Versioning;

namespace Shos.ApplicationRegistry.Tests
{
    public enum BookType
    {
        Magazine,
        Paperback
    }

    public class Settings
    {
        public BookType BookKind { get; set; } = BookType.Magazine;
        public string BookName { get; set; } = "";
        public int Price { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is not Settings settings)
                return false;
            return (BookKind, BookName, Price) == (settings.BookKind, settings.BookName, settings.Price);
        }
    }


    public class ReactiveProperty<T>
    {
        public string Name { get; set; } = "";
        public T? Value { get; set; }
    }

    public enum SomeMode
    {
        A,
        B
    }

    public class ReactiveSettings
    {
        public ReactiveProperty<BookType> BookKind { get; set; } = new() { Name = nameof(BookKind), Value = BookType.Magazine };
        public ReactiveProperty<string> BookName { get; set; } = new() { Name = nameof(BookName), Value = "" };
        public ReactiveProperty<int> Price { get; set; } = new() { Name = nameof(Price), Value = 0 };

        public int SomeNumber { get; set; } = 0;
        public string SomeText { get; set; } = "";
        public SomeMode SomeMode { get; set; } = SomeMode.A;

        public override bool Equals(object? obj)
        {
            if (obj is not ReactiveSettings settings)
                return false;
            return (BookKind.Value, BookName.Value, Price.Value, SomeNumber, SomeText, SomeMode)
                == (settings.BookKind.Value, settings.BookName.Value, settings.Price.Value, settings.SomeNumber, settings.SomeText, settings.SomeMode);
        }
    }

    [TestClass()]
    public class ApplicationRegistryTests
    {
        const string companyName     = "Consto";
        const string applicationName = nameof(ApplicationRegistryTests);

        [TestMethod()]
        [SupportedOSPlatform("windows")]
        public void Test()
        {
            var settings = new Settings { BookKind = BookType.Paperback, BookName = "The Book", Price = 1000 };

            ApplicationRegistry registry = new(companyName, applicationName);
            registry.Save(settings);

            var loadedSettings = registry.Load<Settings>();
            Assert.AreEqual(loadedSettings, settings);
        }

        class ReactivePropertyRegistry : ApplicationRegistry
        {
            public ReactivePropertyRegistry()
                : base(companyName, applicationName)
            {}

            protected override object? GetValue(PropertyInfo property, object item)
            {
                if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(ReactiveProperty<>)) {
                    var reactiveProperty = property.GetValue(item);
                    return Get(reactiveProperty, nameof(ReactiveProperty<object>.Value));
                }
                return base.GetValue(property, item);
            }

            protected override void SetValue(PropertyInfo property, object item, object value)
            {
                if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(ReactiveProperty<>)) {
                    var reactiveProperty = property.GetValue(item);
                    Set(reactiveProperty, nameof(ReactiveProperty<object>.Value), value);
                } else {
                    base.SetValue(property, item, value);
                }
            }

            object? Get(object? item, string propertyName)
                => item is null ? null
                                : item.GetType().GetProperty(propertyName)?.GetValue(item);

            void Set(object? item, string propertyName, object value)
                => item?.GetType()?.GetProperty(propertyName)?.SetValue(item, value);
        }

        [TestMethod()]
        [SupportedOSPlatform("windows")]
        public void ReactivePropertyTest()
        {
            var settings = new ReactiveSettings();
            settings.BookKind.Value = BookType.Paperback;
            settings.BookName.Value = "WPF入門";
            settings.Price   .Value = 3000;
            settings.SomeNumber = 123;
            settings.SomeText   = "something";
            settings.SomeMode   = SomeMode.B;

            ReactivePropertyRegistry registry = new();
            registry.Save(settings);

            var loadedSettings = registry.Load<ReactiveSettings>();
            Assert.AreEqual(loadedSettings, settings);
        }
    }
}

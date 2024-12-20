using System.Reflection;

Staff staff = new Staff { Name = "John", Age = 30, Position = "Developer" };
Type staffType = staff.GetType();
PropertyInfo[] properties = staffType.GetProperties();

foreach (PropertyInfo property in properties) {
    var value = property.GetValue(staff);
    Console.WriteLine($"{property.Name}: {value}");
}

Console.WriteLine($"Name: {Get(staff, nameof(staff.Name))}");

Set(staff, "Name", "Smith");
Console.WriteLine($"Name: {staff.Name}");

object? Get(object? item, string propertyName)
    => item is null ? null
                    : item.GetType().GetProperty(propertyName)?.GetValue(item);

//void Set(object item, string propertyName, object value)
//{
//    Type type = item.GetType();
//    PropertyInfo[] properties = type.GetProperties();
//    PropertyInfo? theProperty = properties.FirstOrDefault(property => property.Name == propertyName);
//    if (theProperty is not null)
//        theProperty.SetValue(item, value);
//}

void Set(object item, string propertyName, object value)
    => item.GetType().GetProperty(propertyName)?.SetValue(item, value);

class Staff
{
    public string Name { get; set; } = "";
    public int Age { get; set; }
    public string Position { get; set; } = "";
}

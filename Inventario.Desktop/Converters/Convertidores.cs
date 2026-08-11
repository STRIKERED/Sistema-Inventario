using System.Globalization;

namespace Inventario.Desktop.Converters;

/// <summary>true cuando el string no es nulo/vacío; útil para IsVisible de mensajes de error.</summary>
public class StringNoVacioABoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        !string.IsNullOrWhiteSpace(value as string);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>Invierte un bool; útil para IsEnabled="{Binding IsBusy, Converter={StaticResource InversoConverter}}".</summary>
public class BoolInversoConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is bool b && !b;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is bool b && !b;
}

/// <summary>true cuando una colección tiene al menos un elemento; útil para mostrar un estado "vacío".</summary>
public class ColeccionVaciaABoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is System.Collections.ICollection coleccion && coleccion.Count == 0;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>true cuando el valor (cualquier tipo referencia) es distinto de null; p. ej. para mostrar
/// contenido condicionado a que ya haya un CorteDeCajaDto cargado.</summary>
public class NoEsNuloABoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is not null;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>Inverso de <see cref="NoEsNuloABoolConverter"/>: true cuando el valor es null.</summary>
public class EsNuloABoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is null;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

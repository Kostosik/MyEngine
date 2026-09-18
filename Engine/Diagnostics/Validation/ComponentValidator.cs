using System.Reflection;

namespace MyEngine.Diagnostics.Validation;

/// <summary>
/// Валидирует компоненты по атрибутам. Кэширует информацию
/// о полях каждого типа — Reflection только при первом обращении.
///
/// Использование:
///   ComponentValidator.Validate(health);
/// В Debug — бросит, если поле не проходит проверку.
/// В Release — вызов стирается.
/// </summary>
public static class ComponentValidator
{
    private static readonly Dictionary<Type, FieldValidator[]> _cache = new();

    /// <summary>
    /// Проверить компонент. В Release — no-op.
    /// </summary>
    [System.Diagnostics.Conditional("DEBUG")]
    public static void Validate(object component)
    {
        if (component == null) return;

        var type = component.GetType();
        if (!_cache.TryGetValue(type, out var validators))
        {
            validators = BuildValidators(type);
            _cache[type] = validators;
        }

        foreach (var v in validators)
            v.Check(component);
    }

    private static FieldValidator[] BuildValidators(Type type)
    {
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        var validators = new List<FieldValidator>();

        foreach (var field in fields)
        {
            bool isNumeric = IsNumeric(field.FieldType);
            bool isReferenceOrString = !field.FieldType.IsValueType
                || field.FieldType == typeof(string);

            // Range / Positive / NonNegative — только числовые
            if (isNumeric)
            {
                var range = field.GetCustomAttribute<RangeAttribute>();
                if (range != null)
                    validators.Add(new RangeValidator(field, range.Min, range.Max));

                if (field.GetCustomAttribute<PositiveAttribute>() != null)
                    validators.Add(new PositiveValidator(field));

                if (field.GetCustomAttribute<NonNegativeAttribute>() != null)
                    validators.Add(new NonNegativeValidator(field));
            }
            else
            {
                // Если атрибут поставлен на не-числовое поле — предупреждение
                if (field.GetCustomAttribute<RangeAttribute>() != null
                    || field.GetCustomAttribute<PositiveAttribute>() != null
                    || field.GetCustomAttribute<NonNegativeAttribute>() != null)
                {
                    Log.Warn("Validation",
                        $"[Range/Positive/NonNegative] on non-numeric field " +
                        $"{type.Name}.{field.Name} ({field.FieldType.Name}) — ignored");
                }
            }

            // NotNull / NotEmpty — только ссылочные типы и string
            if (isReferenceOrString)
            {
                if (field.GetCustomAttribute<NotNullAttribute>() != null)
                    validators.Add(new NotNullValidator(field));

                if (field.GetCustomAttribute<NotEmptyAttribute>() != null)
                    validators.Add(new NotEmptyValidator(field));
            }

            // NotNull на value-type (кроме Nullable) — предупреждение
            if (!isReferenceOrString && field.GetCustomAttribute<NotNullAttribute>() != null)
            {
                Log.Warn("Validation",
                    $"[NotNull] on value-type field {type.Name}.{field.Name} — ignored");
            }
        }

        return validators.ToArray();
    }

    private static bool IsNumeric(Type type)
    {
        return type == typeof(int) || type == typeof(long)
            || type == typeof(short) || type == typeof(byte)
            || type == typeof(sbyte) || type == typeof(uint)
            || type == typeof(ulong) || type == typeof(ushort)
            || type == typeof(float) || type == typeof(double)
            || type == typeof(decimal);
    }
    // ============================================================
    // Валидаторы
    // ============================================================

    private abstract class FieldValidator
    {
        protected readonly FieldInfo Field;

        protected FieldValidator(FieldInfo field) => Field = field;

        public void Check(object component)
        {
            var value = Field.GetValue(component);
            if (!IsValid(value, out var error))
            {
                var msg = $"{component.GetType().Name}.{Field.Name}: {error}";
                Log.Error("Validation", msg);
                throw new InvalidOperationException(msg);
            }
        }

        protected abstract bool IsValid(object? value, out string error);
    }

    private sealed class RangeValidator : FieldValidator
    {
        private readonly double _min, _max;
        public RangeValidator(FieldInfo field, double min, double max) : base(field)
        {
            _min = min;
            _max = max;
        }

        protected override bool IsValid(object? value, out string error)
        {
            if (value == null)
            {
                error = "is null";
                return false;
            }

            double v = Convert.ToDouble(value);
            if (v < _min || v > _max)
            {
                error = $"must be in [{_min}, {_max}], got {v}";
                return false;
            }
            error = "";
            return true;
        }
    }

    private sealed class PositiveValidator : FieldValidator
    {
        public PositiveValidator(FieldInfo field) : base(field) { }

        protected override bool IsValid(object? value, out string error)
        {
            if (value == null) { error = "is null"; return false; }

            double v = Convert.ToDouble(value);
            if (v <= 0)
            {
                error = $"must be positive, got {v}";
                return false;
            }
            error = "";
            return true;
        }
    }

    private sealed class NonNegativeValidator : FieldValidator
    {
        public NonNegativeValidator(FieldInfo field) : base(field) { }

        protected override bool IsValid(object? value, out string error)
        {
            if (value == null) { error = "is null"; return false; }

            double v = Convert.ToDouble(value);
            if (v < 0)
            {
                error = $"must be non-negative, got {v}";
                return false;
            }
            error = "";
            return true;
        }
    }

    private sealed class NotNullValidator : FieldValidator
    {
        public NotNullValidator(FieldInfo field) : base(field) { }

        protected override bool IsValid(object? value, out string error)
        {
            if (value == null)
            {
                error = "must not be null";
                return false;
            }
            error = "";
            return true;
        }
    }

    private sealed class NotEmptyValidator : FieldValidator
    {
        public NotEmptyValidator(FieldInfo field) : base(field) { }

        protected override bool IsValid(object? value, out string error)
        {
            if (value == null)
            {
                error = "must not be null";
                return false;
            }

            if (value is string s && s.Length == 0)
            {
                error = "must not be empty";
                return false;
            }

            if (value is System.Collections.ICollection c && c.Count == 0)
            {
                error = "must not be empty collection";
                return false;
            }

            error = "";
            return true;
        }
    }
}
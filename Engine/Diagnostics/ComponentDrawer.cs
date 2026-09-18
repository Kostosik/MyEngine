using ImGuiNET;
using System.Numerics;
using System.Reflection;

namespace MyEngine.Diagnostics;

/// <summary>
/// Рисует и редактирует поля компонента через ImGui.
/// Использует рефлексию, чтобы не писать UI для каждого типа вручную.
///
/// Поддерживаемые типы:
///   int, float, bool, string, Vector2, Vector3, Vector4, enum
/// </summary>
public static class ComponentDrawer
{
    public static void Draw(object component)
    {
        var type = component.GetType();
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

        foreach (var field in fields)
        {
            object? value = field.GetValue(component);

            if (!DrawField(field.Name, field.FieldType, value, out object? newValue))
                continue;

            field.SetValue(component, newValue);
        }
    }

    private static bool DrawField(string name, Type type, object? value, out object? newValue)
    {
        newValue = value;

        if (type == typeof(int))
        {
            int v = (int)(value ?? 0);
            if (ImGui.DragInt(name, ref v))
            {
                newValue = v;
                return true;
            }
        }
        else if (type == typeof(float))
        {
            float v = (float)(value ?? 0f);
            if (ImGui.DragFloat(name, ref v, 0.1f))
            {
                newValue = v;
                return true;
            }
        }
        else if (type == typeof(bool))
        {
            bool v = (bool)(value ?? false);
            if (ImGui.Checkbox(name, ref v))
            {
                newValue = v;
                return true;
            }
        }
        else if (type == typeof(string))
        {
            string v = (string)(value ?? "");
            if (ImGui.InputText(name, ref v, 256))
            {
                newValue = v;
                return true;
            }
        }
        else if (type == typeof(Vector2))
        {
            Vector2 v = (Vector2)(value ?? Vector2.Zero);
            if (ImGui.DragFloat2(name, ref v, 0.5f))
            {
                newValue = v;
                return true;
            }
        }
        else if (type == typeof(Vector3))
        {
            Vector3 v = (Vector3)(value ?? Vector3.Zero);
            if (ImGui.DragFloat3(name, ref v, 0.5f))
            {
                newValue = v;
                return true;
            }
        }
        else if (type == typeof(Vector4))
        {
            Vector4 v = (Vector4)(value ?? Vector4.Zero);
            if (ImGui.DragFloat4(name, ref v, 0.01f))
            {
                newValue = v;
                return true;
            }
        }
        else if (type.IsEnum)
        {
            var names = Enum.GetNames(type);
            int idx = System.Array.IndexOf(names, value?.ToString() ?? names[0]);
            if (ImGui.Combo(name, ref idx, names, names.Length))
            {
                newValue = Enum.Parse(type, names[idx]);
                return true;
            }
        }
        else
        {
            // Неподдерживаемый тип — показываем название серым
            ImGui.TextDisabled($"{name}: {type.Name}");
        }

        return false;
    }
}
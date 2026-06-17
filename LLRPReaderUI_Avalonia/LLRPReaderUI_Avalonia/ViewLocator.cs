using Avalonia.Controls;
using Avalonia.Controls.Templates;
using LLRPReaderUI_Avalonia.ViewModels;
using System;
using System.Collections.Concurrent;

namespace LLRPReaderUI_Avalonia;

public class ViewLocator : IDataTemplate
{
    private static readonly ConcurrentDictionary<Type, Control> _cache = new();

    public Control Build(object? data)
    {
        if (data is null)
            return new TextBlock { Text = "Data is null" };

        var vmType = data.GetType();

        // 尝试从缓存获取
        if (_cache.TryGetValue(vmType, out var cachedControl))
        {
            return cachedControl;
        }

        var name = vmType.FullName!.Replace("ViewModel", "View");
        var type = Type.GetType(name);

        if (type != null)
        {
            var control = (Control?)Activator.CreateInstance(type);
            if (control != null)
            {
                _cache.TryAdd(vmType, control);
                return control;
            }
        }

        return new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}

using Avalonia.Controls;
using Avalonia.Controls.Templates;
using LLRPReaderUI_Avalonia.ViewModels;
using System;
using System.Linq;

namespace LLRPReaderUI_Avalonia
{
    public class ViewLocator : IDataTemplate
    {
        public Control? Build(object? param)
        {
            if (param is null)
                return null;

            if (param is not ViewModelBase)
                return new TextBlock { Text = param.ToString() ?? "Unsupported content" };

            if (param is Control control)
                return control;

            if (param is string text)
                return new TextBlock { Text = text };

            var viewModelTypeName = param.GetType().FullName;
            if (string.IsNullOrWhiteSpace(viewModelTypeName)
                || !viewModelTypeName.EndsWith("ViewModel", StringComparison.Ordinal))
            {
                return new TextBlock { Text = param.ToString() ?? "Unsupported content" };
            }

            var viewTypeName = viewModelTypeName.Replace("ViewModel", "View", StringComparison.Ordinal);
            var viewType = AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(assembly => assembly.GetType(viewTypeName))
                .FirstOrDefault(candidate => candidate is not null);

            if (viewType is not null && typeof(Control).IsAssignableFrom(viewType))
            {
                return (Control)Activator.CreateInstance(viewType)!;
            }

            return new TextBlock { Text = "Not Found: " + viewTypeName };
        }

        public bool Match(object? data)
        {
            return data is ViewModelBase;
        }
    }
}

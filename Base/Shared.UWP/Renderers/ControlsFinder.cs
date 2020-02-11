using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Heleus.Apps.Shared.UWP.Renderers
{
    static class ControlsFinder
    {
        public static FrameworkElement FindParent(this DependencyObject root, string name)
        {
            if (root == null)
                return null;

            var e = root as FrameworkElement;
            if (e != null && e.Name == name)
                return e;

            return FindParent(VisualTreeHelper.GetParent(root), name);
        }

        public static void PrintControls(this DependencyObject root, string baseName = "")
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);

                string eName = string.Empty;
                var e = (child as FrameworkElement);
                if (e != null && !string.IsNullOrEmpty(e.Name))
                    eName = " - " + e.Name;

                System.Diagnostics.Debug.WriteLine("Child : " + baseName + "/" + child.GetType().Name + eName);
                PrintControls(child, baseName + "/" + child.GetType().Name + eName);

                if (child.GetType() == typeof(TextBlock))
                {
                    System.Diagnostics.Debug.WriteLine((child as TextBlock).Text);
                }
            }
        }

        public static T FindControlByName<T>(this DependencyObject root, string controlName) where T : FrameworkElement
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);

                string eName = string.Empty;
                var e = (child as FrameworkElement);
                if (e != null && e.Name == controlName)
                    return (T)e;

                var result = FindControlByName<T>(child, controlName);
                if (result != null)
                    return result;
            }
            return null;
        }

        public static TextBlock FindTextBlock(this DependencyObject root, string text)
        {
            int count = VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (child.GetType() == typeof(TextBlock))
                {
                    var tb = child as TextBlock;
                    if (tb.Text == text)
                        return tb;
                }

                var result = FindTextBlock(child, text);
                if (result != null)
                    return result;
            }
            return null;
        }

        public static T FindFirstControl<T>(this DependencyObject root) where T : DependencyObject
        {
            int count = VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (child is T)
                {
                    return child as T;
                }

                var result = FindFirstControl<T>(child);
                if (result != default(T))
                    return result;
            }
            return default(T);
        }


        public static List<T> FindAllControls<T>(this DependencyObject root) where T : DependencyObject
        {
            var result = new List<T>();
            int count = VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (child is T)
                {
                    result.Add((T)child);
                }

                var children = FindAllControls<T>(child);
                foreach (var item in children)
                    result.Add(item);
            }

            return result;
        }
    }
}

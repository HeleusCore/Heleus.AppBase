using System;
using System.Collections.Generic;
using Android.Graphics;
using Android.Views;

namespace Heleus.Apps.Shared.Android
{
    public static class ViewChildren
    {
        /*
            Layout: FrameLayout
            Layout: LinearLayout
            Layout: FrameLayout
            Layout: FitWindowsLinearLayout
            Layout: ContentFrameLayout
            Layout: RelativeLayout
            Layout: PlatformRenderer
            Layout: NavigationPageRenderer
            Layout: Toolbar
            Layout: ActionMenuView
         */
        public static void IterateViewGroupChildren(ViewGroup view, int indent = 0)
        {
            if (view == null)
                return;

            var name = view.GetType().Name;
            Console.WriteLine("Layout: " + name + " (" + indent + " Depth, " + view.ChildCount + " Children)");

            if(name == "PlatformRenderer")
            {
                //view.SetFitsSystemWindows(true);
                //view.SystemUiVisibility = StatusBarVisibility.Hidden;
            }
            /*
            if (view is global::Android.Widget.RelativeLayout || view is Xamarin.Forms.Platform.Android.AppCompat.NavigationPageRenderer)
            {
                view.SetFitsSystemWindows(true);
                //view.SetBackgroundColor(Color.DeepPink);
            }

            if (view is global::Android.Widget.RelativeLayout)
            {
                view.SetBackgroundColor(Color.Blue);
            }

            if (view is Xamarin.Forms.Platform.Android.AppCompat.NavigationPageRenderer)
            {
                //view.SetFitsSystemWindows(true);
                view.SetBackgroundColor(Color.Transparent);
            }
            view.SetBackgroundColor(Color.Transparent);
            */

            for (int i = 0; i < view.ChildCount; i++)
            {
                var c = view.GetChildAt(i);
                if (c is ViewGroup)
                    IterateViewGroupChildren(c as ViewGroup, indent + 1);
            }
        }

        public static List<T> GetChild<T>(ViewGroup view) where T : ViewGroup
        {
            var result = new List<T>();

            if (view == null)
                return result;

            if (view is T)
                result.Add((T)view);

            for (int i = 0; i < view.ChildCount; i++)
            {
                var child = view.GetChildAt(i);
                if (child is ViewGroup viewGroup)
                    result.AddRange(GetChild<T>(viewGroup));
            }

            return result;
        }

        public static List<ViewGroup> GetChild(ViewGroup view, params string[] names)
        {
            var result = new List<ViewGroup>();

            if (view == null)
                return result;

            var viewName = view.GetType().Name;
            foreach (var name in names)
            {
                if (viewName == name)
                    result.Add(view);
            }

            for (int i = 0; i < view.ChildCount; i++)
            {
                var child = view.GetChildAt(i);
                if (child is ViewGroup viewGroup)
                    result.AddRange(GetChild(viewGroup, names));
            }

            return result;
        }
    }
}

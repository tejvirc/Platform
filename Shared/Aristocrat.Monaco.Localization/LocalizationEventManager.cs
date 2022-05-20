namespace Aristocrat.Monaco.Localization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Markup.Primitives;
    using WPFLocalizeExtension.Engine;

    internal static class LocalizationEventManager
    {
        private static readonly List<WeakReference<DependencyObject>> Targets =
            new List<WeakReference<DependencyObject>>();

        private static readonly object TargetsLock = new object();

        public static void AddTarget(DependencyObject obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            lock (TargetsLock)
            {
                var exists = false;

                foreach (var wr in Targets.ToArray())
                {
                    if (!wr.TryGetTarget(out var target))
                    {
                        Targets.Remove(wr);
                    }
                    else if (ReferenceEquals(target, obj))
                    {
                        exists = true;
                    }
                }

                if (!exists)
                {
                    Targets.Add(new WeakReference<DependencyObject>(obj));
                }
            }
        }

        public static void RemoveTarget(DependencyObject obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            lock (TargetsLock)
            {
                foreach (var wr in Targets.ToArray())
                {
                    if (!wr.TryGetTarget(out var target))
                    {
                        Targets.Remove(wr);
                    }
                    else if (ReferenceEquals(target, obj))
                    {
                        Targets.Remove(wr);
                    }
                }
            }
        }

        public static void Start()
        {
            LocalizeDictionary.Instance.DefaultProvider.ProviderChanged += (sender, args) => Invoke();

            LocalizeDictionary.Instance.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(LocalizeDictionary.Culture))
                {
                    Invoke();
                }
            };
        }

        private static void Invoke()
        {
            foreach (var target in EnumerateTargets())
            {
                target.UpdateBindings();
            }
        }

        private static void UpdateBindings(this DependencyObject target)
        {
            var dp = EnumerateProperties(target).ToArray();
            var ap = EnumerateAttachedProperties(target).ToArray();

            foreach (var property in dp.Concat(ap))
            {
                if (BindingOperations.IsDataBound(target, property))
                {
                    var expression = BindingOperations.GetBindingExpression(target, property);
                    expression?.UpdateTarget();
                }
            }
        }

        private static IEnumerable<DependencyProperty> EnumerateProperties(DependencyObject target)
        {
            var fields = target.GetType().GetFields(
                BindingFlags.Public
                | BindingFlags.FlattenHierarchy
                | BindingFlags.Instance
                | BindingFlags.Static).Where(f => f.FieldType == typeof(DependencyProperty));

            foreach (var field in fields)
            {
                yield return (DependencyProperty)field.GetValue(null);
            }
        }

        private static IEnumerable<DependencyProperty> EnumerateAttachedProperties(DependencyObject target)
        {
            var markupObject = MarkupWriter.GetMarkupObjectFor(target);

            foreach (var mp in markupObject.Properties)
            {
                if (mp.IsAttached)
                {
                    yield return mp.DependencyProperty;
                }
            }
        }

        private static IEnumerable<DependencyObject> EnumerateTargets()
        {
            lock (TargetsLock)
            {
                foreach (var wr in Targets.ToArray())
                {
                    if (!wr.TryGetTarget(out var target))
                    {
                        Targets.Remove(wr);
                    }
                    else
                    {
                        yield return target;
                    }
                }
            }
        }
    }
}

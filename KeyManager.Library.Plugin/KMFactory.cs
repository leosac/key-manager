namespace Leosac.KeyManager.Library.Plugin
{
    public abstract class KMFactory<T> where T : KMFactory<T>
    {
        public abstract string Name { get; }

        public abstract Type? GetPropertiesType();

        public static IList<T> RegisteredFactories { get; } = new List<T>();

        public static void Register(T factory)
        {
            lock (RegisteredFactories)
            {
                if (!RegisteredFactories.Contains(factory))
                {
                    RegisteredFactories.Add(factory);
                }
            }
        }

        public static T? GetFactoryFromPropertyType(Type? type)
        {
            if (type == null) return default;
            return GetFactoryFromPropertyType(type.FullName);
        }

        public static T? GetFactoryFromPropertyType(string? typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName)) return default;

            lock (RegisteredFactories)
            {
                foreach (var factory in RegisteredFactories)
                {
                    if (factory == null)
                        continue;
                    try
                    {
                        var t = factory.GetPropertiesType();
                        if (t != null && typeName != null && t.FullName != null && typeName.Equals(t.FullName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            return factory;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.TraceError($"Plugin '{factory?.Name}' failed to load : {ex}");
                    }
                }
            }
            return null;
        }
    }
}

using Leosac.KeyManager.Library.KeyStore;

namespace Leosac.KeyManager.Library.Plugin
{
    public abstract class KeyEntryFactory : KMFactory<KeyEntryFactory>
    {
        public abstract IEnumerable<KeyStore.KeyEntryClass> KClasses { get; }

        public virtual uint Importance => 0;

        public abstract KeyStore.KeyEntry CreateKeyEntry();

        public abstract Type GetKeyEntryType();

        public abstract KeyStore.KeyEntryProperties CreateKeyEntryProperties();

        public abstract KeyStore.KeyEntryProperties? CreateKeyEntryProperties(string serialized);

        public static KeyEntryFactory? GetFactoryFromKeyEntryType(Type? type)
        {
            if (type == null) return default;
            return GetFactoryFromKeyEntryType(type.FullName);
        }

        public static KeyEntryFactory? GetFactoryFromKeyEntryType(string? typeName)
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
                        var t = factory.GetKeyEntryType();
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

        public static KeyEntryFactory GetFactoryFromBestMatch(KeyEntryClass kclass)
        {
            lock (RegisteredFactories)
            {
                var bestMatch = RegisteredFactories.Where(f => f != null && f.KClasses.Contains(kclass)).OrderByDescending(f => f.Importance).FirstOrDefault();
                if (bestMatch == null)
                    throw new Exception($"No factory found for KeyEntryClass '{kclass}'");
                return bestMatch;
            }
        }
    }
}

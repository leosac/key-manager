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
            if (type == null)
            {
                return default;
            }

            lock (RegisteredFactories)
            {
                foreach (var factory in RegisteredFactories)
                {
                    var t = factory.GetPropertiesType();
                    if (t != null)
                    {
                        if (t.FullName == type.FullName)
                        {
                            return factory;
                        }
                    }
                }
            }

            return null;
        }
    }
}

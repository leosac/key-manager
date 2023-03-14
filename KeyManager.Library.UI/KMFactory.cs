using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.UI
{
    public abstract class KMFactory<T>
    {
        public abstract string Name { get; }

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
    }
}

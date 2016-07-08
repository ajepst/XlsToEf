using StructureMap;

namespace XlsToEf.Tests.Infrastructure
{
    public class Bootstrapper
    {
        private static bool _initialized;
        private static readonly object Lock = new object();

        public static void Initialize(IContainer container)
        {
            lock (Lock)
            {
                if (_initialized) return;
                InitializeInternal(container);
                _initialized = true;
            }
        }

        private static void InitializeInternal(IContainer container)
        {
        }
    }
}
using NSpec.Domain;

namespace NSpec.TestAdapter
{
    public class ContextFinder : IContextFinder
    {
        public ContextCollection BuildContextCollection(string binaryPath)
        {
            var reflector = new Reflector(binaryPath);

            var finder = new SpecFinder(reflector);

            var conventions = new DefaultConventions();

            var contextBuilder = new ContextBuilder(finder, conventions);

            var contextCollection = contextBuilder.Contexts();

            contextCollection.Build();

            return contextCollection;
        }
    }

    public interface IContextFinder
    {
        ContextCollection BuildContextCollection(string binaryPath);
    }
}

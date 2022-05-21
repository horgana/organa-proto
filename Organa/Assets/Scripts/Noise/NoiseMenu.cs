using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class NoiseMenu : Attribute
{
    public string Label;

    public NoiseMenu(string displayName)
    {
        Label = displayName;
    }
    
    public static class Source<TIn, TOut> 
        where TIn: unmanaged
        where TOut: unmanaged
    {
        // ReSharper disable once StaticMemberInGenericType
        public static List<Type> NoiseTypes;

        static Source()
        {
            // https://makolyte.com/csharp-get-all-classes-with-a-custom-attribute/
            NoiseTypes = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                from type in assembly.GetTypes()
                where type.IsDefined(typeof(NoiseMenu)) && typeof(Noise.INoiseSource<TIn, TOut>).IsAssignableFrom(type)
                select type).ToList();
        }
    }
}
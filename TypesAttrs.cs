using System;
using System.Collections.Generic;
using System.Linq;

namespace VovaScript
{
    public static partial class Objects
    {
        /*           TYPES           */

        public static IClass IInteger = new IClass("ЯЧисло", new Dictionary<string, object>
        {
            { "числом", new IClass("_числом", new Dictionary<string, object>(), Inting.Cloned()) },
            { "строкой", new IClass("_строкой", new Dictionary<string, object>(), Stringing.Cloned()) },
            { "точкой", new IClass("_точкой", new Dictionary<string, object>(), Doubling.Cloned()) },
        });

        public static IClass IString = new IClass("ЯСтрока", new Dictionary<string, object>
        {
            { "числом", new IClass("_числом", new Dictionary<string, object>(), Inting.Cloned()) },
            { "строкой", new IClass("_строкой", new Dictionary<string, object>(), Stringing.Cloned()) },
            { "точкой", new IClass("_точкой", new Dictionary<string, object>(), Doubling.Cloned()) },
        });

        public static IClass IFloat = new IClass("ЯТочка", new Dictionary<string, object>
        {
            { "числом", new IClass("_числом", new Dictionary<string, object>(), Inting.Cloned()) },
            { "строкой", new IClass("_строкой", new Dictionary<string, object>(), Stringing.Cloned()) },
            { "точкой", new IClass("_точкой", new Dictionary<string, object>(), Doubling.Cloned()) },
        });

        public static IClass IBool = new IClass("ЯПравда", new Dictionary<string, object>
        {
            { "числом", new IClass("_числом", new Dictionary<string, object>(), Inting.Cloned()) },
            { "строкой", new IClass("_строкой", new Dictionary<string, object>(), Stringing.Cloned()) },
            { "точкой", new IClass("_точкой", new Dictionary<string, object>(), Doubling.Cloned()) },
        });

        public static IClass IList = new IClass("ЯЛист", new Dictionary<string, object>
        {
            { "строкой", new IClass("_строкой", new Dictionary<string, object>(), Stringing.Cloned()) },
        });
    }
}
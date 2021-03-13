using System;
using System.Collections;
using System.Linq;

namespace mermaid_gen.generators {
    public static class TypeExtensions {
        public static Type GetEnumeratedType(this Type type) => 
        (type?.GetElementType() ?? (typeof(IEnumerable).IsAssignableFrom(type)
            ? type.GenericTypeArguments.FirstOrDefault()
            : null))!;

        public static bool IsNullable(this Type type) =>
            Nullable.GetUnderlyingType(type) != null;
    }
}
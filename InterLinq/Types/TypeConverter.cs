using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using InterLinq.Types.Anonymous;
using System.Globalization;

namespace InterLinq.Types
{
    /// <summary>
    /// The <see cref="TypeConverter"/> is a helper class providing
    /// several static methods to convert <see cref="AnonymousObject"/> to
    /// C# Anonymous Types and back.
    /// </summary>
    internal class TypeConverter
    {

        #region Convert AnonymousObject To C# Anonymous Type

        /// <summary>
        /// Converts an <see langword="object"/> into a target <see cref="Type"/>.
        /// </summary>
        /// <param name="wantedType">Target <see cref="Type"/>.</param>
        /// <param name="objectToConvert"><see langword="object"/> to convert.</param>
        /// <returns>Returns the converted <see langword="object"/>.</returns>
        public static object ConvertFromSerializable(Type wantedType, object objectToConvert)
        {
            if (objectToConvert == null)
            {
                return null;
            }
            if (wantedType.IsIGrouping() && objectToConvert is InterLinqGroupingBase)
            {
                Type[] genericType = objectToConvert.GetType().GetGenericArguments();
                MethodInfo method = typeof(TypeConverter).GetMethod("ConvertFromInterLinqGrouping", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(genericType);
                return method.Invoke(null, new[] { wantedType, objectToConvert });
            }

            if (wantedType.IsIGroupingArray() && objectToConvert is InterLinqGroupingBase)
            {
                Type[] genericType = objectToConvert.GetType().GetGenericArguments();
                MethodInfo method = typeof(TypeConverter).GetMethod("ConvertFromInterLinqGroupingArray", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(genericType);
                return method.Invoke(null, new[] { wantedType, objectToConvert });
            }

            Type wantedElementType = InterLinqTypeSystem.FindIEnumerable(wantedType);
            if (wantedElementType != null && wantedElementType.GetGenericArguments()[0].IsAnonymous())
            {
                Type typeOfObject = objectToConvert.GetType();
                Type elementType = InterLinqTypeSystem.FindIEnumerable(typeOfObject);
                if (elementType != null && elementType.GetGenericArguments()[0] == typeof(AnonymousObject))
                {
                    MethodInfo method = typeof(TypeConverter).GetMethod("ConvertFromSerializableCollection", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(wantedElementType.GetGenericArguments()[0]);
                    return method.Invoke(null, new[] { objectToConvert });
                }
            }
            if (wantedType.IsAnonymous() && objectToConvert is AnonymousObject)
            {
                AnonymousObject dynamicObject = (AnonymousObject)objectToConvert;
                List<object> properties = new List<object>();
                ConstructorInfo[] constructors = wantedType.GetConstructors();
                int nr = 0;
                if (constructors.Length == 2)
                {
                    var p0 = constructors[0].GetParameters();
                    var p1 = constructors[1].GetParameters();
                    if (p0.Length > 0 && p1.Length > 0)
                        throw new Exception("Usualy, anonymous types have just one constructor.");
                    else
                        nr = p0.Length > 0 ? 1 : 0;
                }
                else if (constructors.Length != 1)
                {
                    throw new Exception("Usualy, anonymous types have just one constructor.");
                }
                ConstructorInfo constructor = constructors[nr];
                foreach (ParameterInfo parameter in constructor.GetParameters())
                {
                    object propertyValue = null;
                    bool propertyHasBeenSet = false;
                    foreach (AnonymousProperty dynProperty in dynamicObject.Properties)
                    {
                        if (dynProperty.Name == parameter.Name)
                        {
                            propertyValue = dynProperty.Value;
                            propertyHasBeenSet = true;
                            break;
                        }
                    }
                    if (!propertyHasBeenSet)
                    {
                        throw new Exception(string.Format("Property {0} could not be found in the dynamic object.", parameter.Name));
                    }
                    properties.Add(ConvertFromSerializable(parameter.ParameterType, propertyValue));
                }
                return constructor.Invoke(properties.ToArray());
            }
            return ConvertValueToTargetType(objectToConvert, wantedType);
        }

        private static object ConvertFromInterLinqGrouping<TKey, TElement>(Type wantedType, InterLinqGrouping<TKey, TElement> grouping)
        {
            Type[] genericArguments = wantedType.GetGenericArguments();
            object key = ConvertFromSerializable(genericArguments[0], grouping.Key);

            MethodInfo method = typeof(TypeConverter).GetMethod("ConvertFromSerializableCollection", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(genericArguments[1]);
            object elements = method.Invoke(null, new object[] { grouping });

            //object elements = ConvertFromSerializableCollection<TElement>( typeof( IEnumerable<> ).MakeGenericType( genericArguments[1] ),  );
            Type elementType = InterLinqTypeSystem.FindIEnumerable(elements.GetType());
            if (elementType == null)
            {
                throw new Exception("ElementType could not be found.");
            }
            Type[] genericTypes = new[] { key.GetType(), elementType.GetGenericArguments()[0] };
            InterLinqGroupingBase newGrouping = (InterLinqGroupingBase)Activator.CreateInstance(typeof(InterLinqGrouping<,>).MakeGenericType(genericTypes));
            newGrouping.SetKey(key);
            newGrouping.SetElements(elements);
            return newGrouping;
        }

        private static object ConvertFromInterLinqGroupingArray<TKey, TElement>(Type wantedType, InterLinqGrouping<TKey, TElement>[] grouping)
        {
            var retVal = new List<InterLinqGroupingBase>();
            var tp = wantedType.GetElementType();
            foreach (var interLinqGrouping in grouping)
            {
                retVal.Add((InterLinqGroupingBase)ConvertFromInterLinqGrouping(tp, interLinqGrouping));

            }
            return retVal.ToArray();
        }

        /// <summary>
        /// Converts each element of an <see cref="IEnumerable"/> 
        /// into a target <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="T">Target <see cref="Type"/>.</typeparam>
        /// <param name="enumerable"><see cref="IEnumerable"/>.</param>
        /// <returns>Returns the converted <see cref="IEnumerable"/>.</returns>
        private static IEnumerable ConvertFromSerializableCollection<T>(IEnumerable enumerable)
        {
            Type enumerableType = typeof(List<>).MakeGenericType(typeof(T));
            IEnumerable newList = (IEnumerable)Activator.CreateInstance(enumerableType);
            MethodInfo addMethod = enumerableType.GetMethod("Add");
            foreach (object item in enumerable)
            {
                addMethod.Invoke(newList, new[] { ConvertFromSerializable(typeof(T), item) });
            }
            return newList;
        }
        #endregion

        #region Convert C# Anonymous Type to AnonymousObject

        /// <summary>
        /// Converts an object to an <see cref="AnonymousObject"/> 
        /// or an <see cref="IEnumerable{AnonymousObject}"/>.
        /// </summary>
        /// <param name="objectToConvert"><see langword="object"/> to convert.</param>
        /// <returns>Returns the converted <see langword="object"/>.</returns>
        public static object ConvertToSerializable(object objectToConvert)
        {
            if (objectToConvert == null)
            {
                return null;
            }

            Type typeOfObject = objectToConvert.GetType();
            Type elementType = InterLinqTypeSystem.FindIEnumerable(typeOfObject);

            // Handle "IGrouping<TKey, TElement>"
            if (typeOfObject.IsIGrouping())
            {
                Type[] genericType = typeOfObject.GetGenericArguments();
                MethodInfo method = typeof(TypeConverter).GetMethod("ConvertToInterLinqGrouping", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(genericType);
                return method.Invoke(null, new[] { objectToConvert });
            }

            // Handle "IGrouping<TKey, TElement>[]"
            if (typeOfObject.IsIGroupingArray())
            {
                Type[] genericType = typeOfObject.GetGenericArguments();
                MethodInfo method = typeof(TypeConverter).GetMethod("ConvertToInterLinqGroupingArray", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(genericType);
                return method.Invoke(null, new[] { objectToConvert });
            }

            // Handle "IEnumerable<AnonymousType>" / "IEnumerator<T>"
            if (elementType != null && elementType.GetGenericArguments()[0].IsAnonymous() || typeOfObject.IsEnumerator())
            {
                MethodInfo method = typeof(TypeConverter).GetMethod("ConvertToSerializableCollection", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(elementType.GetGenericArguments()[0]);
                return method.Invoke(null, new[] { objectToConvert });
            }
            // Handle "AnonymousType"
            if (typeOfObject.IsAnonymous())
            {
                AnonymousObject newObject = new AnonymousObject();
                foreach (PropertyInfo property in typeOfObject.GetProperties(BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.Public))
                {
                    object objectValue = ConvertToSerializable(property.GetValue(objectToConvert, new object[] { }));
                    newObject.Properties.Add(new AnonymousProperty(property.Name, objectValue));
                }
                return newObject;
            }

            return objectToConvert;
        }

        private static object ConvertToInterLinqGrouping<TKey, TElement>(IGrouping<TKey, TElement> grouping)
        {
            object key = ConvertToSerializable(grouping.Key);
            object elements = ConvertToSerializableCollection<TElement>(grouping);
            Type elementType = InterLinqTypeSystem.FindIEnumerable(elements.GetType());
            if (elementType == null)
            {
                throw new Exception("ElementType could not be found.");
            }
            Type[] genericTypes = new Type[] { key.GetType(), elementType.GetGenericArguments()[0] };
            InterLinqGroupingBase newGrouping = (InterLinqGroupingBase)Activator.CreateInstance(typeof(InterLinqGrouping<,>).MakeGenericType(genericTypes));
            newGrouping.SetKey(key);
            newGrouping.SetElements(elements);
            return newGrouping;
        }

        private static object ConvertToInterLinqGroupingArray<TKey, TElement>(IGrouping<TKey, TElement>[] grouping)
        {
            var retVal = new List<InterLinqGrouping<TKey, TElement>>();
            foreach (var g in grouping)
            {
                retVal.Add((InterLinqGrouping<TKey, TElement>)ConvertToInterLinqGrouping(g));
            }

            return retVal.ToArray();
        }

        /// <summary>
        /// Converts each element of an <see cref="IEnumerable"/> to 
        /// an <see cref="IEnumerable{AnonymousObject}"/> 
        /// </summary>
        /// <typeparam name="T">Target <see cref="Type"/>.</typeparam>
        /// <param name="enumerable"><see cref="IEnumerable"/>.</param>
        /// <returns>Returns the converted <see cref="IEnumerable"/>.</returns>
        private static IEnumerable ConvertToSerializableCollection<T>(IEnumerable enumerable)
        {
            Type typeToEnumerate = typeof(T);
            if (typeToEnumerate.IsAnonymous())
            {
                typeToEnumerate = typeof(AnonymousObject);
            }
            Type enumerableType = typeof(List<>).MakeGenericType(typeToEnumerate);
            IEnumerable newList = (IEnumerable)Activator.CreateInstance(enumerableType);
            MethodInfo addMethod = enumerableType.GetMethod("Add");
            foreach (object item in enumerable)
            {
                addMethod.Invoke(newList, new[] { ConvertToSerializable(item) });
            }
            return newList;
        }

        #endregion

        /// <summary>
        /// Helper function to use the correct Types (for example when you use json.net for serialization, it's lost)
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targeType"></param>
        /// <returns></returns>
        public static object ConvertValueToTargetType(object value, Type targeType)
        {

#if !NETFX_CORE
            if (!targeType.IsClass && !targeType.IsInterface)
#else
            if (!targeType.GetTypeInfo().IsClass && !targeType.GetTypeInfo().IsInterface)
#endif
            {

#if !NETFX_CORE
                if (targeType.IsEnum)
#else
                if (targeType.GetTypeInfo().IsEnum)
#endif
                {
                    if (value is string)
                    {
                        return Enum.Parse(targeType, (string)value, true);
                    }
                    return Enum.ToObject(targeType, value);
                }
                else if ((targeType == typeof(Guid) || targeType == typeof(Guid?)) && value is string)
                {
                    return new Guid((string)value);
                }
                else
                {
                    if (value != null)
                    {
                        var underlyingType = Nullable.GetUnderlyingType(targeType);
#if !NETFX_CORE
                        if (underlyingType != null && underlyingType.IsEnum)
#else
                        if (underlyingType != null && underlyingType.GetTypeInfo().IsEnum)
#endif
                        {
                            if (value is string)
                            {
                                return Enum.Parse(underlyingType, (string)value, true);
                            }
                            return Enum.ToObject(underlyingType, value);
                        }
                        else if (underlyingType != null)
                        {
                            return Convert.ChangeType(value, underlyingType, CultureInfo.InvariantCulture);
                        }
                    }
                    return Convert.ChangeType(value, targeType, CultureInfo.InvariantCulture);
                }
            }

            return value;
        }
    }
}

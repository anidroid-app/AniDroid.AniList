﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AniDroid.AniList.Utils.Internal
{
    internal class AniListContractResolver : DefaultContractResolver
    {
        private static AniListContractResolver _instance;

        // Manual Singleton ftw!
        public static AniListContractResolver Instance
            => _instance ?? (_instance = new AniListContractResolver());

        public readonly Dictionary<Type, Type> InterfaceConcreteMap;

        private AniListContractResolver()
        {
            NamingStrategy = new CamelCaseNamingStrategy();
            InterfaceConcreteMap = new Dictionary<Type, Type>
            {
                { typeof(IList<>), typeof(List<>) },
                { typeof(ICollection<>), typeof(List<>) },
            };
        }

        protected override JsonConverter ResolveContractConverter(Type objectType)
        {
            if (!objectType.IsInterface)
            {
                return base.ResolveContractConverter(objectType);
            }

            var isGeneric = objectType.IsGenericType;
            var interfaceType = isGeneric
                ? objectType.GetGenericTypeDefinition()
                : objectType;

            if (!InterfaceConcreteMap.ContainsKey(interfaceType))
            {
                return base.ResolveContractConverter(objectType);
            }

            var actualType = InterfaceConcreteMap[interfaceType];
            var concreteGenericType = actualType.MakeGenericType(isGeneric ? objectType.GetGenericArguments() : new Type[0]);
            var converterType = typeof(AniListJsonConverter<>).MakeGenericType(concreteGenericType);
            return Activator.CreateInstance(converterType) as JsonConverter;

        }
    }
}

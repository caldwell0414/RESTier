﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
#if !NETCOREAPP
using System.Net.Http;
#endif
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Formatter.Serialization;
#if NETCOREAPP
using Microsoft.AspNetCore.Http;
#endif
using Microsoft.OData;
using Microsoft.OData.Edm;


#if NETCOREAPP
namespace Microsoft.Restier.AspNetCore.Formatter
#else
namespace Microsoft.Restier.AspNet.Formatter
#endif
{
    /// <summary>
    /// The default serializer provider.
    /// </summary>
    public class DefaultRestierSerializerProvider : DefaultODataSerializerProvider
    {
        private RestierResourceSetSerializer resourceSetSerializer;
        private RestierPrimitiveSerializer primitiveSerializer;
        private RestierRawSerializer rawSerializer;
        private RestierResourceSerializer resourceSerializer;
        private RestierCollectionSerializer collectionSerializer;
        private RestierEnumSerializer enumSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultRestierSerializerProvider" /> class.
        /// </summary>
        /// <param name="rootContainer">The container to get the service.</param>
        /// <param name="payloadValueConverter">The OData payload value converter to use.</param>
        public DefaultRestierSerializerProvider(IServiceProvider rootContainer, ODataPayloadValueConverter payloadValueConverter)
            : base(rootContainer)
        {
            Ensure.NotNull(rootContainer, nameof(rootContainer));
            Ensure.NotNull(payloadValueConverter, nameof(payloadValueConverter));

            this.resourceSetSerializer = new RestierResourceSetSerializer(this);
            this.primitiveSerializer = new RestierPrimitiveSerializer(payloadValueConverter);
            this.rawSerializer = new RestierRawSerializer(payloadValueConverter);
            this.resourceSerializer = new RestierResourceSerializer(this);
            this.collectionSerializer = new RestierCollectionSerializer(this);
            this.enumSerializer = new RestierEnumSerializer(this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultRestierSerializerProvider" /> class.
        /// </summary>
        /// <param name="rootContainer">The container to get the service.</param>
        public DefaultRestierSerializerProvider(IServiceProvider rootContainer)
            : this(rootContainer, new RestierPayloadValueConverter())
        {
        }

        /// <summary>
        /// Gets the serializer for the given result type.
        /// </summary>
        /// <param name="type">The type of result to serialize.</param>
        /// <param name="request">The HTTP request.</param>
        /// <returns>The serializer instance.</returns>
        public override ODataSerializer GetODataPayloadSerializer(
            Type type,
#if NETCOREAPP
            HttpRequest request)
#else
            HttpRequestMessage request)
#endif
        {
            ODataSerializer serializer = null;
            if (type == typeof(ResourceSetResult))
            {
                serializer = this.resourceSetSerializer;
            }
            else if (type == typeof(PrimitiveResult))
            {
                serializer = this.primitiveSerializer;
            }
            else if (type == typeof(RawResult))
            {
                serializer = this.rawSerializer;
            }
            else if (type == typeof(ComplexResult))
            {
                serializer = this.resourceSerializer;
            }
            else if (type == typeof(NonResourceCollectionResult))
            {
                serializer = this.collectionSerializer;
            }
            else if (type == typeof(EnumResult))
            {
                serializer = this.enumSerializer;
            }
            else
            {
                serializer = base.GetODataPayloadSerializer(type, request);
            }

            return serializer;
        }

        /// <summary>
        /// Gets the serializer for the given EDM type reference.
        /// </summary>
        /// <param name="edmType">The EDM type reference involved in the serializer.</param>
        /// <returns>The serializer instance.</returns>
        public override ODataEdmTypeSerializer GetEdmTypeSerializer(IEdmTypeReference edmType)
        {
            if (edmType.IsComplex())
            {
                return this.resourceSerializer;
            }

            if (edmType.IsPrimitive())
            {
                return this.primitiveSerializer;
            }

            if (edmType.IsEnum())
            {
                return this.enumSerializer;
            }

            if (edmType.IsCollection())
            {
                var collectionType = edmType.AsCollection();
                if (collectionType.Definition.IsDeltaFeed())
                {
                    return base.GetEdmTypeSerializer(edmType);
                }
                else if (collectionType.ElementType().IsEntity() || collectionType.ElementType().IsComplex())
                {
                    return this.resourceSetSerializer;
                }

                return this.collectionSerializer;
            }

            return base.GetEdmTypeSerializer(edmType);
        }
    }
}

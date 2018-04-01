﻿// Copyright (c) 2010-2014 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System.Reflection;
using System.Runtime.Serialization;

namespace SharpGen.Model
{
    [DataContract]
    public class CsMarshalBase : CsBase
    {
        /// <summary>
        ///   Public type used for element.
        /// </summary>
        [DataMember]
        public CsTypeBase PublicType { get; set; }

        /// <summary>
        ///   Internal type used for marshalling to native. If null, then use instead public type.
        /// </summary>
        [DataMember]
        public CsTypeBase MarshalType { get; set; }

        [DataMember]
        public bool HasPointer { get; set; }

        [DataMember]
        public bool IsArray { get; set; }

        [DataMember]
        public int ArrayDimensionValue { get; set; }

        [DataMember]
        public bool IsWideChar { get; set; }

        [DataMember]
        public bool IsBoolToInt { get; set; }
        
        public int Size => MarshalType.Size * ((ArrayDimensionValue > 1) ? ArrayDimensionValue : 1);
        
        public bool IsValueType
        {
            get { return (PublicType is CsStruct csStruct && !csStruct.GenerateAsClass) || PublicType is CsEnum ||
                    (PublicType is CsFundamentalType type && (type.Type.GetTypeInfo().IsValueType || type.Type.GetTypeInfo().IsPrimitive)); }
        }

        public bool IsStructClass
        {
            get { return PublicType is CsStruct csStruct && csStruct.GenerateAsClass; }
        }
    }
}
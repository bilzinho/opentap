﻿//            Copyright Keysight Technologies 2012-2019
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, you can obtain one at http://mozilla.org/MPL/2.0/.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace OpenTap.Plugins
{
    /// <summary> Serializer implementation for default value attributes. </summary>
    public class DefaultValueSerializer : TapSerializerPlugin
    {
        /// <summary> The order of this serializer. </summary>
        public override double Order => 10;

        HashSet<XElement> elems = new HashSet<XElement>();

        /// <summary> Deserialization implementation. </summary>
        public override bool Deserialize(XElement element, ITypeData t, Action<object> setter)
        {
            return false;
        }

        /// <summary> Serialization implementation. </summary>       
        public override bool Serialize(XElement elem, object obj, ITypeData expectedType)
        {
            if (elems.Contains(elem))
                return false;

            try
            {
                elems.Add(elem);
                bool ok = Serializer.Serialize(elem, obj, expectedType);

                var defaultAttr = elem.Attribute(ObjectSerializer.DefaultValue);
                if (defaultAttr != null)
                {
                    if (object.Equals(defaultAttr.Value, obj?.ToString()) && elem.Attributes().Count() == 1)
                    {
                        // To remove this case since default value is same as specified value 
                        // <TimeDelay HasDefaultValue="True">0.1</TimeDelay>.

                        // These cases are NOT to be removed
                        // <TimeDelay Parameter="time" HasDefaultValue="True">0.1</TimeDelay>.
                        // <TimeDelay UserDefinedAttribute="??" HasDefaultValue="True">0.1</TimeDelay>.

                        elem.Remove();
                    }
                    else
                    {
                        // Remove the default value attribute from the element after verification
                        elem.SetAttributeValue(ObjectSerializer.DefaultValue, null);
                    }
                }
               
                return ok;
            }
            finally
            {
                elems.Remove(elem);
            }
        }
    }
}
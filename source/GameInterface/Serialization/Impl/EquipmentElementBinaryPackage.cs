﻿using Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;

namespace GameInterface.Serialization.Impl
{
    [Serializable]
    public class EquipmentElementBinaryPackage : BinaryPackageBase<EquipmentElement>
    {
        public EquipmentElementBinaryPackage(EquipmentElement obj, BinaryPackageFactory binaryPackageFactory) : base(obj, binaryPackageFactory)
        {
        }

        public override void Pack()
        {
            foreach (FieldInfo field in ObjectType.GetAllInstanceFields())
            {
                object obj = field.GetValue(Object);
                StoredFields.Add(field, BinaryPackageFactory.GetBinaryPackage(obj));
            }
        }

        protected override void UnpackInternal()
        {
            TypedReference reference = __makeref(Object);
            foreach (KeyValuePair<FieldInfo, IBinaryPackage> element in StoredFields)
            {
                element.Key.SetValueDirect(reference, element.Value.Unpack());
            }
        }
    }
}

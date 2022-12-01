﻿using Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.Roster;

namespace GameInterface.Serialization.Impl
{
    [Serializable]
    public class ItemRosterBinaryPackage : BinaryPackageBase<ItemRoster>
    {
        public ItemRosterBinaryPackage(ItemRoster obj, BinaryPackageFactory binaryPackageFactory) : base(obj, binaryPackageFactory)
        {
        }

        static readonly HashSet<string> excludes = new HashSet<string>
        {
            "<TotalWeight>k__BackingField",
            "<VersionNo>k__BackingField",
            "<TotalValue>k__BackingField",
            "<TotalFood>k__BackingField",
            "<NumberOfPackAnimals>k__BackingField",
            "<NumberOfMounts>k__BackingField",
            "<NumberOfLivestockAnimals>k__BackingField",
            "<FoodVariety>k__BackingField",
            "_rosterUpdatedEvent"
        };

        public override void Pack()
        {
            foreach (FieldInfo field in ObjectType.GetAllInstanceFields(excludes))
            {
                object obj = field.GetValue(Object);
                StoredFields.Add(field, BinaryPackageFactory.GetBinaryPackage(obj));
            }
        }

        protected override void UnpackInternal()
        {
            TypedReference reference = __makeref(Object);
            foreach (FieldInfo field in StoredFields.Keys)
            {
                field.SetValueDirect(reference, StoredFields[field].Unpack());
            }
        }
    }
}

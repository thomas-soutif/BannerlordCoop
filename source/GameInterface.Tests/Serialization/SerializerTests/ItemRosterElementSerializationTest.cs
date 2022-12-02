﻿using GameInterface.Serialization.Impl;
using GameInterface.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.Roster;
using Xunit;
using TaleWorlds.Core;
using System.Reflection;

namespace GameInterface.Tests.Serialization.SerializerTests
{
    public class ItemRosterElementSerializationTest
    {
        [Fact]
        public void ItemRosterElement_Serialize()
        {
            ItemRosterElement itemRosterElement = new ItemRosterElement();

            BinaryPackageFactory factory = new BinaryPackageFactory();
            ItemRosterElementBinaryPackage package = new ItemRosterElementBinaryPackage(itemRosterElement, factory);

            package.Pack();

            byte[] bytes = BinaryFormatterSerializer.Serialize(package);

            Assert.NotEmpty(bytes);
        }

        [Fact]
        public void ItemRosterElement_Full_Serialization()
        {
            ItemRosterElement itemRosterElement = new ItemRosterElement();
            FieldInfo _amount = typeof(ItemRosterElement).GetField("_amount",BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            PropertyInfo EquipmentElementProperty = typeof(ItemRosterElement).GetProperty("EquipmentElement");
            _amount.SetValue(itemRosterElement, 5);
            EquipmentElementProperty.SetValue(itemRosterElement, new EquipmentElement());
            BinaryPackageFactory factory = new BinaryPackageFactory();
            ItemRosterElementBinaryPackage package = new ItemRosterElementBinaryPackage(itemRosterElement, factory);

            package.Pack();

            byte[] bytes = BinaryFormatterSerializer.Serialize(package);

            Assert.NotEmpty(bytes);

            object obj = BinaryFormatterSerializer.Deserialize(bytes);

            Assert.IsType<ItemRosterElementBinaryPackage>(obj);

            ItemRosterElementBinaryPackage returnedPackage = (ItemRosterElementBinaryPackage)obj;

            ItemRosterElement newRosterElement = returnedPackage.Unpack<ItemRosterElement>();

            //Equals is defined for ItemRosterElement
            Assert.Equal(itemRosterElement,newRosterElement);
        }
    }
}

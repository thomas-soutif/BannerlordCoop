﻿using GameInterface.Serialization.Impl;
using GameInterface.Serialization;
using TaleWorlds.Core;
using Xunit;
using System.Runtime.Serialization;
using Common.Extensions;
using System.Reflection;
using TaleWorlds.CampaignSystem.CharacterDevelopment;

namespace GameInterface.Tests.Serialization.SerializerTests
{
    public class PerkObjectSerializationTest
    {
        [Fact]
        public void PerkObject_Serialize()
        {
            PerkObject testPerkObject = new PerkObject("test");

            BinaryPackageFactory factory = new BinaryPackageFactory();
            PerkObjectBinaryPackage package = new PerkObjectBinaryPackage(testPerkObject, factory);

            package.Pack();

            byte[] bytes = BinaryFormatterSerializer.Serialize(package);

            Assert.NotEmpty(bytes);
        }

        [Fact]
        public void PerkObject_Full_Serialization()
        {
            PerkObject testPerkObject = new PerkObject("test");

            BinaryPackageFactory factory = new BinaryPackageFactory();
            PerkObjectBinaryPackage package = new PerkObjectBinaryPackage(testPerkObject, factory);

            package.Pack();

            byte[] bytes = BinaryFormatterSerializer.Serialize(package);

            Assert.NotEmpty(bytes);

            object obj = BinaryFormatterSerializer.Deserialize(bytes);

            Assert.IsType<PerkObjectBinaryPackage>(obj);

            PerkObjectBinaryPackage returnedPackage = (PerkObjectBinaryPackage)obj;

            Assert.Equal(returnedPackage.StringId, package.StringId);
        }
    }
}

﻿using Xunit;
using System.Runtime.Serialization;
using Common.Extensions;
using System.Reflection;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using GameInterface.Serialization.Impl;
using GameInterface.Serialization;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using System.Diagnostics;

namespace GameInterface.Tests.Serialization.SerializerTests
{
    public class PartyBaseSerializationTest
    {
        [Fact]
        public void PartyBase_Serialize()
        {
            PartyBase testPartyObject = (PartyBase)FormatterServices.GetUninitializedObject(typeof(PartyBase));

            BinaryPackageFactory factory = new BinaryPackageFactory();
            PartyBaseBinaryPackage package = new PartyBaseBinaryPackage(testPartyObject, factory);

            package.Pack();

            byte[] bytes = BinaryFormatterSerializer.Serialize(package);

            Assert.NotEmpty(bytes);
        }

        [Fact]
        public void PartyBase_Full_Serialization()
        {
            PartyBase testPartyObject = (PartyBase)FormatterServices.GetUninitializedObject(typeof(PartyBase));

            testPartyObject.RemainingFoodPercentage = 5;
            testPartyObject.GetType().GetProperty("Index", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).SetValue(testPartyObject, 5);

            BinaryPackageFactory factory = new BinaryPackageFactory();
            PartyBaseBinaryPackage package = new PartyBaseBinaryPackage(testPartyObject, factory);

            package.Pack();

            byte[] bytes = BinaryFormatterSerializer.Serialize(package);

            Assert.NotEmpty(bytes);

            object obj = BinaryFormatterSerializer.Deserialize(bytes);

            Assert.IsType<PartyBaseBinaryPackage>(obj);

            PartyBaseBinaryPackage returnedPackage = (PartyBaseBinaryPackage)obj;

            PartyBase returnedPartyObject = returnedPackage.Unpack<PartyBase>();

            Assert.Equal(testPartyObject.RemainingFoodPercentage, returnedPartyObject.RemainingFoodPercentage);
            Assert.Equal(testPartyObject.Index, returnedPartyObject.Index);
        }
    }
}
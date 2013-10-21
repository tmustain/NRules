﻿using NRules.Core.IntegrationTests.TestAssets;
using NRules.Core.IntegrationTests.TestRules;
using NUnit.Framework;

namespace NRules.Core.IntegrationTests
{
    [TestFixture]
    public class OneFactOneCollectionRuleTest : BaseRuleTestFixture
    {
        [Test]
        public void OneFactOneCollectionRule_TwoMatchingFactsAndOneInvalid_FiresOnceWithTwoFactsInCollection()
        {
            //Arrange
            var fact1 = new FactType1() {TestProperty = "Valid Value"};
            var fact2 = new FactType1() {TestProperty = "Valid Value"};
            var fact3 = new FactType1() {TestProperty = "Invalid Value"};

            Session.Insert(fact1);
            Session.Insert(fact2);
            Session.Insert(fact3);

            //Act
            Session.Fire();

            //Assert
            AssertFiredOnce();
            Assert.AreEqual(2, GetRuleInstance<OneFactOneCollectionRule>().FactCount);
        }

        [Test]
        public void OneFactOneCollectionRule_TwoMatchingFactsInsertedOneRetracted_FiresOnceWithOneFactInCollection()
        {
            //Arrange
            var fact1 = new FactType1() {TestProperty = "Valid Value"};
            var fact2 = new FactType1() {TestProperty = "Valid Value"};

            Session.Insert(fact1);
            Session.Insert(fact2);
            Session.Retract(fact2);

            //Act
            Session.Fire();

            //Assert
            AssertFiredOnce();
            Assert.AreEqual(1, GetRuleInstance<OneFactOneCollectionRule>().FactCount);
        }

        [Test]
        public void OneFactOneCollectionRule_TwoMatchingFactsInsertedTwoRetracted_DoesNotFire()
        {
            //Arrange
            var fact1 = new FactType1() {TestProperty = "Valid Value"};
            var fact2 = new FactType1() {TestProperty = "Valid Value"};

            Session.Insert(fact1);
            Session.Insert(fact2);
            Session.Retract(fact1);
            Session.Retract(fact2);

            //Act
            Session.Fire();

            //Assert
            AssertDidNotFire();
        }

        protected override void SetUpRules()
        {
            SetUpRule<OneFactOneCollectionRule>();
        }
    }
}
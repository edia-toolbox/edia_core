using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;


    public class testSomething
    {
        [Test]
        public void moves_playarea_to_given_location()
        {

        }

        [Test]
        public void sets_damage_to_half_with_50_percent_mitigation () {
            
            // ACT
            int finalDamage = DamageCalculator.CalculateDamage(10,0.5f);

            // ASSERT
            Assert.AreEqual(5, finalDamage);
        }

         [Test]
        public void calculates_2_damage_from_10_with_80_percent_mitigation () {
            
            // ACT
            int finalDamage = DamageCalculator.CalculateDamage(10,0.8f);

            // ASSERT
            Assert.AreEqual(2, finalDamage);
        }


    }

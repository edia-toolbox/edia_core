using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using eDIA.Utilities;

public class UtilityTests
{
    [Test]
    public void _123_As_StringsArray_Returns_123_As_IntsArray()
    {
        string[] testlist = { "1", "2","3" };

        int value = 1;

        int[] intArray = ArrayTools.ConvertStringsIntoInts(testlist);

        Assert.AreEqual(value, intArray[0]);

    }

    [Test]
    public void _123_As_IntsArray_Returns_123_As_StringsArray()
    {
        int[] testlist = { 1, 2, 3 };

        string value = "1";

        string[] stringArray = ArrayTools.ConvertIntsToStrings(testlist);

        Assert.AreEqual(value, stringArray[0]);

    }


}

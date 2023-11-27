using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace UI
{
    public static class Validators
    {
        public static bool CheckIfNameCorrect(string fieldName)
        {
            var regex = new Regex("^[\\s]*$|^.{0,1}$|^.{17,}$");
            return (regex.Matches(fieldName).Count > 0);
        }

        public static bool CheckIfEndsWithNewline(string fieldName)
        {
            var regex = new Regex("^.*\n$");
            return (regex.Matches(fieldName).Count > 0);
        }

        public static bool CheckIfTownPopulationCorrect(string fieldName)
        {
            try
            {
                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                int.Parse(fieldName);
                return true;
            }
            catch (Exception)
            {
                Debug.Log("Can't convert to int");
                return false;
            }
        }

        public static int CheckIfPopulationInRange(int fieldNum)
        {
            fieldNum = fieldNum switch
            {
                < 5 => 5,
                > 99 => 99,
                _ => fieldNum
            };
            return fieldNum;
        }
    }
}
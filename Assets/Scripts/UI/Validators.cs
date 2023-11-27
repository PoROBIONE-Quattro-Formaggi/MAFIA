using System.Text.RegularExpressions;

namespace UI
{
    public static class Validators
    {
        public static bool CheckIfNameCorrect(string fieldName)
        {
            Regex regex = new Regex("^[\\s]*$|^.{0,1}$|^.{17,}$");
            return (regex.matches(fieldName).Length > 0);
        }

        public static bool CheckIfEndsWithNewline(string fieldName)
        {
            Regex regex = new Regex("^.*\n$");
            return (regex.matches(fieldName).Length > 0);
        }
        public static bool CheckIfTownPopulationCorrect(string fieldName)
        {
            try
            {
                maxPlayersInt = int.Parse(fieldName.text);
                return true;
            }
            catch (Exception)
            {
                // Debug.Log("Can't convert to int");
                return false;
            }
        }

        public static int CheckIfPopulationInRange(int fieldNum)
        {
            if (fieldNum < 5)
                fieldNum = 5;
            else if (fieldNum > 99)
                fieldNum = 99;
            return fieldNum;
        }
    }
}
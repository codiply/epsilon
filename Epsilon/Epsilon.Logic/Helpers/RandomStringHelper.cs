using Epsilon.Logic.Wrappers.Interfaces;
using System;
using System.Text;

namespace Epsilon.Logic.Helpers
{
    public static class RandomStringHelper
    {
        public enum CharacterCase
        {
            Lower, Upper, Mixed
        }

        public static string GetString(IRandomWrapper random, int length, CharacterCase characterCase)
        {
            var builder = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                builder.Append(GetCharacter(random, characterCase));
            }

            return builder.ToString();
        }

        public static string GetAlphaNumericString(IRandomWrapper random, int length, CharacterCase characterCase)
        {
            var builder = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                builder.Append(GetAlphaNumericCharacter(random, characterCase));
            }

            return builder.ToString();
        }

        public static string GetDigitString(IRandomWrapper random, int length)
        {
            var builder = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                builder.Append(GetDigit(random));
            }

            return builder.ToString();
        }

        public static char GetDigit(IRandomWrapper random)
        {
            return DigitToChar(random.Next(0, 9));
        }

        public static char GetCharacter(IRandomWrapper random, CharacterCase characterCase)
        {
            switch (characterCase)
            {
                case CharacterCase.Lower:
                    return GetCharacterSingleCase(random, false);
                case CharacterCase.Upper:
                    return GetCharacterSingleCase(random, true);
                case CharacterCase.Mixed:
                    return GetCharacterMixedCase(random);
                default:
                    throw new NotImplementedException();
            }
        }

        public static char GetAlphaNumericCharacter(IRandomWrapper random, CharacterCase characterCase)
        {
            int x;
            switch (characterCase)
            {
                case CharacterCase.Mixed:
                    x = random.Next(0, 62);
                    if (x < 52)
                    {
                        return NumberToCharacterMixedCase(x);
                    }
                    else
                    {
                        return DigitToChar(x - 52);
                    }
                case CharacterCase.Lower:
                case CharacterCase.Upper:
                    x = random.Next(0, 36);
                    if (x < 26)
                    {
                        return NumberToCharacterSingleCase(x, characterCase == CharacterCase.Upper);
                    }
                    else
                    {
                        return DigitToChar(x - 26);
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        private static char DigitToChar(int digit)
        {
            return Convert.ToChar(48 + digit);
        }

        private static char GetCharacterSingleCase(IRandomWrapper random, bool isUpperCase)
        {
            return NumberToCharacterSingleCase(random.Next(0, 26), isUpperCase);
        }

        private static char GetCharacterMixedCase(IRandomWrapper random)
        {
            int x = random.Next(0, 52);
            return NumberToCharacterMixedCase(x);
        }

        private static char NumberToCharacterSingleCase(int zeroBasedNumber, bool isUpperCase)
        {
            if (0 <= zeroBasedNumber && zeroBasedNumber < 26)
            {
                if (isUpperCase)
                {
                    return Convert.ToChar(65 + zeroBasedNumber);
                }
                else
                {
                    return Convert.ToChar(97 + zeroBasedNumber);
                }
            }

            throw new ArgumentException(string.Format("Unexpected argument {0}.", zeroBasedNumber));
        }

        private static char NumberToCharacterMixedCase(int zeroBasedNumber)
        {
            if (0 <= zeroBasedNumber && zeroBasedNumber < 52)
            {
                if (zeroBasedNumber < 26)
                {
                    return Convert.ToChar(65 + zeroBasedNumber);
                }
                else
                {
                    zeroBasedNumber -= 26;
                    return Convert.ToChar(97 + zeroBasedNumber);
                }
            }

            throw new ArgumentException(string.Format("Unexpected argument {0}.", zeroBasedNumber));
        }
    }
}

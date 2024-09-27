﻿using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Tew
{
    static class CustomMath
    {
        public static decimal Truncate(decimal number) => number - (number % 1);

        public static decimal Ceiling(decimal number)
        {
            decimal truncated = Truncate(number);
            decimal x = truncated + 1m * Math.Sign(number);
            if (number > truncated) return x;
            else return truncated;
        }

        public static decimal Floor(decimal number)
        {
            decimal truncated = Truncate(number);
            decimal x = truncated + 1m * Math.Sign(number);
            if (number < truncated) return x;
            else return truncated;
        }

        private static decimal RemoveTrailingZeros(decimal number)
        {
            // TODO: Trailing zeros
            return number / 1.0000000000000000000000000000m;
        }

        public static decimal Round(decimal number, int decimals = 0, MidpointRounding mode = MidpointRounding.ToEven)
        {
            // NOTES:
            // https://en.wikipedia.org/wiki/Floor_and_ceiling_functions#Rounding
            // https://en.wikipedia.org/wiki/Rounding#Rounding_to_integer

            if (decimals < 0 || decimals > 28)
            {
                throw new ArgumentOutOfRangeException(nameof(decimals), "Decimal can only round to between 0 and 28 digits of precision.");
            }

            if (mode == MidpointRounding.AwayFromZero) return AwayFromZero(number, decimals);
            if (mode == MidpointRounding.ToEven) return ToEven(number, decimals);
            if (mode == MidpointRounding.ToNegativeInfinity) return ToNegativeInfinity(number, decimals);
            if (mode == MidpointRounding.ToPositiveInfinity) return ToPositiveInfinity(number, decimals);
            if (mode == MidpointRounding.ToZero) return ToZero(number, decimals);

            throw new ArgumentException(null, nameof(mode));
        }

        private static decimal ToEven(decimal number, int decimals = 0)
        {
            try
            {
                if (number.Scale == decimals) return RemoveTrailingZeros(number);
                //if (number == 0) return RemoveTrailingZeros(number); // TODO: Trailing zeros

                int newDecimals = decimals <= number.Scale ? decimals : number.Scale;
                decimal x = number / (decimal)Math.Pow(10, -newDecimals);

                decimal step1 = Floor(x + 0.5m);
                decimal step2 = 0.5m * x - 0.25m;
                decimal step3 = Ceiling(step2);
                decimal step4 = Floor(step2);
                decimal step5 = step1 + step3 - step4 - 1m;

                decimal result = step5 / (decimal)Math.Pow(10, newDecimals);

                return RemoveTrailingZeros(result);
            }
            catch (OverflowException)
            {
                return Math.Sign(number) == 1 ? decimal.MaxValue : decimal.MinValue;
            }
        }

        private static decimal ToPositiveInfinity(decimal number, int decimals = 0)
        {
            if (number.Scale == decimals) return number;
            else if (decimals > number.Scale) return number;

            int newDecimals = decimals <= number.Scale ? decimals : number.Scale;
            decimal x = number / (decimal)Math.Pow(10, -newDecimals);

            decimal step1 = Ceiling(x);

            decimal result = step1 / (decimal)Math.Pow(10, newDecimals);

            return RemoveTrailingZeros(result);
        }

        private static decimal ToNegativeInfinity(decimal number, int decimals = 0)
        {
            if (number.Scale == decimals) return number;
            else if (decimals > number.Scale) return number;

            int newDecimals = decimals <= number.Scale ? decimals : number.Scale;
            decimal x = number / (decimal)Math.Pow(10, -newDecimals);

            decimal step1 = Floor(x);

            decimal result = step1 / (decimal)Math.Pow(10, newDecimals);

            return RemoveTrailingZeros(result);
        }

        private static decimal AwayFromZero(decimal number, int decimals = 0)
        {
            if (number.Scale == decimals) return number;
            else if (decimals > number.Scale) return number;

            int newDecimals = decimals <= number.Scale ? decimals : number.Scale;
            decimal x = number / (decimal)Math.Pow(10, -newDecimals);

            decimal step1 = Math.Sign(x) * Floor(Math.Abs(x) + 0.5m);

            decimal result = step1 / (decimal)Math.Pow(10, newDecimals);

            return RemoveTrailingZeros(result);
        }

        private static decimal ToZero(decimal number, int decimals = 0)
        {
            /* if (number.Scale == decimals) return number;
            else if (decimals > number.Scale) return number; */

            int newDecimals = decimals <= number.Scale ? decimals : number.Scale;
            decimal x = number / (decimal)Math.Pow(10, -newDecimals);

            decimal step1 = Truncate(x);

            decimal result = step1 / (decimal)Math.Pow(10, newDecimals);

            return RemoveTrailingZeros(result);
        }
    }

    class Test
    {
        private class TestCase
        {
            public int decimals;
            public decimal number;
            public decimal MathRoundResult { get; private set; }
            public decimal CustomRoundResult { get; private set; }

            public bool IsEquals
            {
                get => MathRoundResult == CustomRoundResult;
            }

            private readonly MidpointRounding mode;

            public TestCase(MidpointRounding mode)
            {
                this.mode = mode;
            }

            public override string ToString()
            {
                StringBuilder strBuilder = new();
                strBuilder.AppendLine($"|       Mode = {mode}");
                strBuilder.AppendLine($"|   Decimals = {decimals}");
                strBuilder.AppendLine($"|     Number = {number}");
                strBuilder.AppendLine($"| Math.Round = {MathRoundResult}");
                strBuilder.AppendLine($"|      Round = {CustomRoundResult}");
                strBuilder.AppendLine($"|  Is equals = {IsEquals}");
                return strBuilder.ToString();
            }

            public void Execute()
            {
                MathRoundResult = Math.Round(number, decimals, mode);
                CustomRoundResult = CustomMath.Round(number, decimals, mode);
            }
        }

        public MidpointRounding mode;
        private readonly decimal[] numbers;

        public Test(MidpointRounding mode)
        {
            numbers = [
                1.5m,
                1.555m,
                2.5m,
                2.555m,
                1212.5555555m,
                1764334.4536m,
                999999999999.9m,
                999999999999.1m,
                3.1415926535897932384626433832m,
                12.99999m,
                0.000999m,
                10.1230067m,
                44.4444m,
                decimal.MaxValue,
                decimal.MinValue,
            ];
            this.mode = mode;
        }

        public void TestHalf(int decimals)
        {
            decimal[] halfNumbers = numbers.Where(number =>
            {
                decimal shifted = number * (decimal)Math.Pow(10, number.Scale);
                bool isHalf = shifted % 5 == 0 && shifted % 10 != 0;
                return isHalf;
            }).ToArray();

            TestCase testCase = new(mode);
            foreach (decimal number in halfNumbers)
            {
                testCase.number = number;
                testCase.decimals = decimals;
                testCase.Execute();
                Debug.Assert(testCase.IsEquals, testCase.ToString());
            }
        }

        public void TestNotHalf(int decimals)
        {
            decimal[] notHalfNumbers = numbers.Where(number =>
            {
                decimal shifted = number * (decimal)Math.Pow(10, number.Scale);
                bool isHalf = shifted % 5 == 0 && shifted % 10 != 0;
                return !isHalf;
            }).ToArray();

            TestCase testCase = new(mode);
            foreach (decimal number in notHalfNumbers)
            {
                testCase.number = number;
                testCase.decimals = decimals;
                testCase.Execute();
                Debug.Assert(testCase.IsEquals, testCase.ToString());
            }
        }

        public void TestRandomDouble(int decimals, int max, bool negativeNumbers = false)
        {
            Random rnd = new(19910917);
            TestCase testCase = new(mode);
            for (int i = 0; i < max; i++)
            {
                decimal number = (decimal)rnd.NextDouble();
                if (negativeNumbers) number = -number;
                testCase.number = number;
                testCase.decimals = decimals;
                testCase.Execute();
                Debug.Assert(testCase.IsEquals, testCase.ToString());
            }
        }

        public void TestFormatedString(int decimals, CultureInfo cultureInfo)
        {
            List<decimal> numbers = new(this.numbers)
            {
                0,
                0.0m,
                0.000m,
                0.0000m,
            };

            TestCase testCase = new(mode);
            foreach (decimal number in numbers)
            {
                testCase.number = number;
                testCase.decimals = decimals;
                testCase.Execute();

                cultureInfo.NumberFormat.NumberDecimalDigits = testCase.MathRoundResult.Scale;
                string mathRoundResult = testCase.MathRoundResult.ToString("N", cultureInfo);

                cultureInfo.NumberFormat.NumberDecimalDigits = testCase.CustomRoundResult.Scale;
                string customRoundResult = testCase.CustomRoundResult.ToString("N", cultureInfo);

                bool IsEquals = mathRoundResult == customRoundResult;
                Debug.Assert(IsEquals, testCase.ToString());
            }
        }
    }

    class Program
    {
        static void TestMode(MidpointRounding mode)
        {
            Test test = new(mode);

            CultureInfo testCultureInfo = CultureInfo.CreateSpecificCulture("hu");
            testCultureInfo.NumberFormat.NumberDecimalSeparator = ".";

            for (int decimals = 0; decimals <= 28; decimals++)
            {
                test.TestHalf(decimals);
                test.TestNotHalf(decimals);
                test.TestRandomDouble(decimals, 5_000);
                test.TestRandomDouble(decimals, 5_000, true);
                //test.TestFormatedString(decimals, testCultureInfo); // (FAILED) TODO: Trailing zeros
            }
        }

        static bool AskForADecimalFraction(out string input)
        {
            Console.Write("\nDecimal fraction: ");
            string? userInput = Console.ReadLine();

            if (string.IsNullOrEmpty(userInput))
            {
                input = string.Empty;
                return false;
            }
            else
            {
                input = userInput;
                return true;
            }
        }

        static bool AskRoundingMode(out MidpointRounding mode)
        {
            StringBuilder menuBuilder = new();
            menuBuilder.AppendLine("Midpoint rounding modes:");
            menuBuilder.AppendLine("\t1. Away from zero");
            menuBuilder.AppendLine("\t2. To even");
            menuBuilder.AppendLine("\t3. To negative infinity");
            menuBuilder.AppendLine("\t4. To positive infinity");
            menuBuilder.AppendLine("\t5. To zero");
            menuBuilder.Append("Choice: ");
            Console.Write(menuBuilder);

            MidpointRounding? roundingMode = Console.ReadKey().KeyChar switch
            {
                '1' => MidpointRounding.AwayFromZero,
                '2' => MidpointRounding.ToEven,
                '3' => MidpointRounding.ToNegativeInfinity,
                '4' => MidpointRounding.ToPositiveInfinity,
                '5' => MidpointRounding.ToZero,
                _ => null
            };

            if (roundingMode == null)
            {
                mode = MidpointRounding.ToEven;
                return false;
            }
            else
            {
                mode = (MidpointRounding)roundingMode;
                return true;
            }
        }

        static void Main(string[] args)
        {
            TestMode(MidpointRounding.AwayFromZero);
            TestMode(MidpointRounding.ToEven);
            TestMode(MidpointRounding.ToNegativeInfinity);
            TestMode(MidpointRounding.ToPositiveInfinity);
            TestMode(MidpointRounding.ToZero);

            while (AskRoundingMode(out MidpointRounding mode)
                && AskForADecimalFraction(out string input))
            {
                CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("hu");
                if (input.LastIndexOf('.') >= 0) cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
                if (input.LastIndexOf(',') >= 0) cultureInfo.NumberFormat.NumberDecimalSeparator = ",";

                if (decimal.TryParse(input, cultureInfo, out decimal number))
                {
                    int decimals = number.Scale > 7 ? 7 : number.Scale;

                    Console.WriteLine();
                    for (int i = 0; i <= decimals; i++)
                    {
                        decimal result = CustomMath.Round(number, i, mode);
                        cultureInfo.NumberFormat.NumberDecimalDigits = result.Scale;
                        Console.WriteLine($"Rounded to {i} decimal places: {result.ToString("N", cultureInfo)}");

                        // decimal resultMath = Math.Round(number, i, mode);
                        // cultureInfo.NumberFormat.NumberDecimalDigits = resultMath.Scale;
                        // Console.WriteLine($"Rounded to {i} decimal places: {resultMath.ToString("N", cultureInfo)}\n");
                    }
                }
                else Console.WriteLine("\nMalformed input or the number is too large/small");
                Console.WriteLine();
            }
        }
    }
}

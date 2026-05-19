using System.Collections.Generic;
using UnityEngine;

public static class QuestionGenerator
{
    // Bolum 1-2:  Kolay toplama (tek basamak + tek basamak)
    // Bolum 3:    Kolay cikarma (iki basamak - tek basamak)
    // Bolum 4:    Toplama + Cikarma karisik (iki basamak)
    // Bolum 5:    Kolay carpma (tek basamak x tek basamak)
    // Bolum 6:    Carpma + Toplama karisik
    // Bolum 7:    Kolay bolme (tam bolunen)
    // Bolum 8:    Dort islem karisik (kolay)
    // Bolum 9:    Dort islem karisik (orta)
    // Bolum 10:   Dort islem karisik (zor)

    public static Question Generate(int level)
    {
        switch (level)
        {
            case 1:  return GenerateAddition(1, 9, 1, 9);
            case 2:  return GenerateAddition(1, 9, 10, 50);
            case 3:  return GenerateSubtraction(10, 50, 1, 9);
            case 4:  return GenerateMixed(level, OperationType.Addition, OperationType.Subtraction);
            case 5:  return GenerateMultiplication(2, 9, 2, 9);
            case 6:  return GenerateMixed(level, OperationType.Addition, OperationType.Multiplication);
            case 7:  return GenerateDivision(2, 9, 2, 9);
            case 8:  return GenerateAllMixed(false);
            case 9:  return GenerateAllMixed(true);
            case 10: return GenerateHardMixed();
            default: return GenerateAddition(1, 9, 1, 9);
        }
    }

    static Question GenerateAddition(int min1, int max1, int min2, int max2)
    {
        Question q = new Question();
        q.operation = OperationType.Addition;
        q.number1 = Random.Range(min1, max1 + 1);
        q.number2 = Random.Range(min2, max2 + 1);
        q.correctAnswer = q.number1 + q.number2;
        FillOptions(q);
        return q;
    }

    static Question GenerateSubtraction(int min1, int max1, int min2, int max2)
    {
        Question q = new Question();
        q.operation = OperationType.Subtraction;
        q.number1 = Random.Range(min1, max1 + 1);
        q.number2 = Random.Range(min2, Mathf.Min(max2, q.number1 - 1) + 1);
        q.correctAnswer = q.number1 - q.number2;
        FillOptions(q);
        return q;
    }

    static Question GenerateMultiplication(int min1, int max1, int min2, int max2)
    {
        Question q = new Question();
        q.operation = OperationType.Multiplication;
        q.number1 = Random.Range(min1, max1 + 1);
        q.number2 = Random.Range(min2, max2 + 1);
        q.correctAnswer = q.number1 * q.number2;
        FillOptions(q);
        return q;
    }

    static Question GenerateDivision(int minDivisor, int maxDivisor, int minResult, int maxResult)
    {
        Question q = new Question();
        q.operation = OperationType.Division;
        q.number2 = Random.Range(minDivisor, maxDivisor + 1);
        int result = Random.Range(minResult, maxResult + 1);
        q.number1 = q.number2 * result;
        q.correctAnswer = result;
        FillOptions(q);
        return q;
    }

    static Question GenerateMixed(int level, OperationType op1, OperationType op2)
    {
        OperationType chosen = (Random.value > 0.5f) ? op1 : op2;

        switch (chosen)
        {
            case OperationType.Addition:
                return (level <= 5)
                    ? GenerateAddition(1, 20, 1, 20)
                    : GenerateAddition(10, 99, 10, 99);
            case OperationType.Subtraction:
                return GenerateSubtraction(15, 60, 1, 15);
            case OperationType.Multiplication:
                return GenerateMultiplication(2, 9, 2, 9);
            default:
                return GenerateAddition(1, 9, 1, 9);
        }
    }

    static Question GenerateAllMixed(bool harder)
    {
        int roll = Random.Range(0, 4);
        OperationType op = (OperationType)roll;

        if (harder)
        {
            switch (op)
            {
                case OperationType.Addition:       return GenerateAddition(10, 99, 10, 99);
                case OperationType.Subtraction:    return GenerateSubtraction(20, 99, 5, 30);
                case OperationType.Multiplication: return GenerateMultiplication(3, 12, 3, 12);
                case OperationType.Division:       return GenerateDivision(2, 12, 2, 12);
            }
        }
        else
        {
            switch (op)
            {
                case OperationType.Addition:       return GenerateAddition(5, 50, 5, 50);
                case OperationType.Subtraction:    return GenerateSubtraction(10, 50, 2, 15);
                case OperationType.Multiplication: return GenerateMultiplication(2, 9, 2, 9);
                case OperationType.Division:       return GenerateDivision(2, 9, 2, 9);
            }
        }

        return GenerateAddition(1, 9, 1, 9);
    }

    static Question GenerateHardMixed()
    {
        int roll = Random.Range(0, 4);
        OperationType op = (OperationType)roll;

        switch (op)
        {
            case OperationType.Addition:       return GenerateAddition(50, 999, 50, 999);
            case OperationType.Subtraction:    return GenerateSubtraction(100, 999, 10, 200);
            case OperationType.Multiplication: return GenerateMultiplication(5, 15, 5, 15);
            case OperationType.Division:       return GenerateDivision(3, 15, 3, 15);
            default:                           return GenerateAddition(50, 999, 50, 999);
        }
    }

    static void FillOptions(Question q)
    {
        List<int> wrongs = GenerateWrongAnswers(q.correctAnswer, q.operation);
        q.options = new int[4];
        q.correctOptionIndex = Random.Range(0, 4);

        int wrongIdx = 0;
        for (int i = 0; i < 4; i++)
            q.options[i] = (i == q.correctOptionIndex) ? q.correctAnswer : wrongs[wrongIdx++];
    }

    static List<int> GenerateWrongAnswers(int correct, OperationType op)
    {
        HashSet<int> used = new HashSet<int> { correct };
        List<int> wrongs = new List<int>();

        int maxOffset = op == OperationType.Multiplication || op == OperationType.Division
            ? Mathf.Max(3, correct / 3)
            : Mathf.Max(5, correct / 5);

        int attempts = 0;
        while (wrongs.Count < 3 && attempts < 100)
        {
            int offset = Random.Range(1, maxOffset + 1);
            if (Random.value > 0.5f) offset = -offset;

            int wrong = correct + offset;
            if (wrong >= 0 && used.Add(wrong))
                wrongs.Add(wrong);

            attempts++;
        }

        int fallback = correct + 1;
        while (wrongs.Count < 3)
        {
            if (used.Add(fallback))
                wrongs.Add(fallback);
            fallback++;
        }

        return wrongs;
    }
}

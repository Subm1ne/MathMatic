public enum OperationType
{
    Addition,       // Toplama
    Subtraction,    // Cikarma
    Multiplication, // Carpma
    Division        // Bolme
}

[System.Serializable]
public class Question
{
    public int number1;
    public int number2;
    public int correctAnswer;
    public int[] options;
    public int correctOptionIndex;
    public OperationType operation;

    public string GetOperationSymbol()
    {
        switch (operation)
        {
            case OperationType.Addition:       return "+";
            case OperationType.Subtraction:    return "-";
            case OperationType.Multiplication: return "\u00D7";
            case OperationType.Division:       return "\u00F7";
            default: return "+";
        }
    }
}

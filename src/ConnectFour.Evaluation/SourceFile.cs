namespace ConnectFour.Evaluation
{
    public enum Stage
    {
        Beginning,
        Middle,
        End
    }

    public static class SourceFile
    {
        public static Stage GetStage(string sourceFile)
        {
            switch (sourceFile)
            {
                case ("Test_L1_R1.txt" or "Test_L1_R2.txt" or "Test_L1_R3.txt"): return Stage.Beginning;
                case ("Test_L2_R1.txt" or "Test_L2_R2.txt"): return Stage.Middle;
                case ("Test_L3_R1.txt"): return Stage.End;
                default: throw new ArgumentException($"Unknown source file: {sourceFile}");
            }
        }
    }
}

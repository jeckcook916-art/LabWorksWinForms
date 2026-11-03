namespace LabWorksWinForms
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm()); // Важно: здесь должно быть MainForm, а не Form1
        }
    }
}
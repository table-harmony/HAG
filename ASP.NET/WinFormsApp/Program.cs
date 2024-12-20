using System.Windows.Forms;
using WinFormsApp;

static class Program {
    [STAThread]
    static void Main(string[] args) {
        ApplicationConfiguration.Initialize();
        string? filePath = args.Length > 0 ? args[0] : null;
        Application.Run(new MainForm(filePath));
    }
}
using System.Windows.Forms;
using WinFormsApp;

static class Program {
    [STAThread]
    static void Main(string[] args) {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm(args[0]));
    }
}
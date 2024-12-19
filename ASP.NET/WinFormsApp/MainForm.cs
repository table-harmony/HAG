using ImageProcessing.Serializers;

namespace WinFormsApp; 

public partial class MainForm : Form {
    private readonly PictureBox _pictureBox;
    private readonly Panel _imagePanel;
    private readonly MenuStrip _menuStrip;

    public MainForm(string? filePath = "") {
        InitializeComponent();
        Size = new Size(1560, 780);
        Text = "HAG Viewer";
        BackColor = Color.FromArgb(18, 18, 18);
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        CenterToScreen();

        // Menu Strip with no padding
        _menuStrip = new MenuStrip {
            BackColor = Color.FromArgb(28, 28, 28),
            ForeColor = Color.White,
            RenderMode = ToolStripRenderMode.System,
            Padding = new Padding(0),
            Margin = new Padding(0)
        };

        var fileMenu = new ToolStripMenuItem("File");
        fileMenu.DropDownItems.Add("Open", null, (s, e) => OpenFile());
        fileMenu.DropDownItems.Add("Exit", null, (s, e) => Close());
        _menuStrip.Items.Add(fileMenu);

        Controls.Add(_menuStrip);

        // Image Panel
        _imagePanel = new Panel {
            Dock = DockStyle.Fill,
            Padding = new Padding(0),
            BackColor = Color.FromArgb(28, 28, 28),
            Margin = new Padding(0)
        };

        // PictureBox
        _pictureBox = new PictureBox {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.AutoSize,
            BackColor = Color.FromArgb(28, 28, 28)
        };

        _imagePanel.Controls.Add(_pictureBox);
        Controls.Add(_imagePanel);

        // Center the window
        StartPosition = FormStartPosition.CenterScreen;

        // Enable drag and drop
        AllowDrop = true;
        DragEnter += (s, e) => {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        };
        DragDrop += (s, e) => {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0) LoadHagImage(files[0]);
        };

        if (!string.IsNullOrEmpty(filePath)) {
            LoadHagImage(filePath);
        }
    }

    private void OpenFile() {
        using var dialog = new OpenFileDialog {
            Filter = "HAG Files (*.hag)|*.hag|All Files (*.*)|*.*",
            Title = "Open HAG Image"
        };

        if (dialog.ShowDialog() == DialogResult.OK) {
            LoadHagImage(dialog.FileName);
        }
    }

    private void LoadHagImage(string filePath) {
        try {
            using var fileStream = File.OpenRead(filePath);
            var hagSerializer = new HagSerializer();
            var sif = hagSerializer.Serialize(fileStream);

            var pngSerializer = new PngSerializer();
            using var pngStream = pngSerializer.Deserialize(sif);

            var image = Image.FromStream(pngStream);
            _pictureBox.Image = image;

            Text = $"HAG Viewer - {Path.GetFileName(filePath)}";
        } catch (Exception ex) {
            MessageBox.Show($"Error loading image: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
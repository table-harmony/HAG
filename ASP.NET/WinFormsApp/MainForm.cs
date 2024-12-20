using ImageProcessing.Serializers;
using System.IO;

namespace WinFormsApp; 

public partial class MainForm : Form {
    private readonly PictureBox _pictureBox;
    private readonly Panel _imagePanel;
    private readonly MenuStrip _menuStrip;
    private readonly ToolStrip _toolStrip;
    private readonly StatusStrip _statusStrip;
    private string? _currentFilePath;
    private Point? _lastPoint;
    private Color _currentColor = Color.Black;
    private int _penSize = 2;
    private DrawingTool _currentTool = DrawingTool.Pen;
    private float _zoomFactor = 1.0f;
    private Point _panStartPoint;
    private bool _isPanning = false;

    private enum DrawingTool {
        Pen,
        Rectangle,
        Ellipse,
        Line
    }

    public MainForm(string? filePath = "") {
        InitializeComponent();
        Size = new Size(1560, 780);
        Text = "HAG Viewer";
        BackColor = Color.FromArgb(18, 18, 18);
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        CenterToScreen();

        // Menu Strip
        _menuStrip = new MenuStrip {
            BackColor = Color.FromArgb(28, 28, 28),
            ForeColor = Color.White,
            RenderMode = ToolStripRenderMode.System,
            Padding = new Padding(0),
            Margin = new Padding(0)
        };

        var fileMenu = new ToolStripMenuItem("File");
        fileMenu.DropDownItems.Add("New", null, (s, e) => NewFile());
        fileMenu.DropDownItems.Add("Open", null, (s, e) => OpenFile());
        fileMenu.DropDownItems.Add("Save", null, (s, e) => SaveFile());
        fileMenu.DropDownItems.Add("Save As", null, (s, e) => SaveFileAs());
        fileMenu.DropDownItems.Add("-");
        fileMenu.DropDownItems.Add("Exit", null, (s, e) => Close());
        _menuStrip.Items.Add(fileMenu);

        Controls.Add(_menuStrip);

        // Tool Strip
        _toolStrip = new ToolStrip {
            BackColor = Color.FromArgb(28, 28, 28),
            ForeColor = Color.White
        };

        _toolStrip.Items.Add(CreateToolButton("Pen", DrawingTool.Pen));
        _toolStrip.Items.Add(CreateToolButton("Rectangle", DrawingTool.Rectangle));
        _toolStrip.Items.Add(CreateToolButton("Ellipse", DrawingTool.Ellipse));
        _toolStrip.Items.Add(CreateToolButton("Line", DrawingTool.Line));
        _toolStrip.Items.Add(new ToolStripSeparator());
        _toolStrip.Items.Add("Color", null, (s, e) => SelectColor());
        _toolStrip.Items.Add(CreateSizeComboBox());

        Controls.Add(_toolStrip);

        // Create status strip for zoom info
        _statusStrip = new StatusStrip {
            BackColor = Color.FromArgb(28, 28, 28),
            ForeColor = Color.White
        };
        _statusStrip.Items.Add(new ToolStripLabel("Zoom: 100%"));
        Controls.Add(_statusStrip);

        // Image Panel
        _imagePanel = new Panel {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(28, 28, 28),
            AutoScroll = true
        };

        // PictureBox
        _pictureBox = new PictureBox {
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.White,
            Dock = DockStyle.None,
            Size = new Size(800, 600)
        };

        // Set high quality interpolation mode for the Graphics object
        _pictureBox.Paint += (s, e) => {
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        };

        _pictureBox.MouseDown += PictureBox_MouseDown;
        _pictureBox.MouseMove += PictureBox_MouseMove;
        _pictureBox.MouseUp += PictureBox_MouseUp;

        _imagePanel.Controls.Add(_pictureBox);
        Controls.Add(_imagePanel);

        // Add mouse wheel handler for zoom
        _imagePanel.MouseWheel += ImagePanel_MouseWheel;

        if (!string.IsNullOrEmpty(filePath)) {
            LoadHagImage(filePath);
        } else {
            NewFile();
        }
    }

    private void NewFile() {
        var bitmap = new Bitmap(800, 600);
        using var g = Graphics.FromImage(bitmap);
        g.Clear(Color.White);
        _pictureBox.Image = bitmap;
        _currentFilePath = null;
        Text = "HAG Viewer - Untitled";
        
        _zoomFactor = 1.0f;
        UpdateImageSize();
        CenterImage();
        _statusStrip.Items[0].Text = "Zoom: 100%";
    }

    private ToolStripButton CreateToolButton(string text, DrawingTool tool) {
        var button = new ToolStripButton(text);
        button.Click += (s, e) => _currentTool = tool;
        return button;
    }

    private ToolStripComboBox CreateSizeComboBox() {
        var combo = new ToolStripComboBox();
        combo.Items.AddRange(new object[] { "1px", "2px", "3px", "4px", "5px" });
        combo.SelectedIndex = 1;
        combo.SelectedIndexChanged += (s, e) => {
            _penSize = combo.SelectedIndex + 1;
        };
        return combo;
    }

    private void SelectColor() {
        using var dialog = new ColorDialog();
        if (dialog.ShowDialog() == DialogResult.OK) {
            _currentColor = dialog.Color;
        }
    }

    private void PictureBox_MouseDown(object? sender, MouseEventArgs e) {
        if (_pictureBox.Image == null) NewFile();
        _lastPoint = GetImageCoordinates(Control.MousePosition);
    }

    private void PictureBox_MouseMove(object? sender, MouseEventArgs e) {
        if (e.Button != MouseButtons.Left || _lastPoint == null) return;

        var currentPoint = GetImageCoordinates(Control.MousePosition);
        using var g = Graphics.FromImage(_pictureBox.Image!);
        using var pen = new Pen(_currentColor, _penSize);

        switch (_currentTool) {
            case DrawingTool.Pen:
                g.DrawLine(pen, _lastPoint.Value, currentPoint);
                _lastPoint = currentPoint;
                break;
        }

        _pictureBox.Invalidate();
    }

    private void PictureBox_MouseUp(object? sender, MouseEventArgs e) {
        if (_lastPoint == null) return;

        var currentPoint = GetImageCoordinates(Control.MousePosition);
        using var g = Graphics.FromImage(_pictureBox.Image!);
        using var pen = new Pen(_currentColor, _penSize);

        switch (_currentTool) {
            case DrawingTool.Rectangle:
                var rect = GetRectangle(_lastPoint.Value, currentPoint);
                g.DrawRectangle(pen, rect);
                break;

            case DrawingTool.Ellipse:
                rect = GetRectangle(_lastPoint.Value, currentPoint);
                g.DrawEllipse(pen, rect);
                break;

            case DrawingTool.Line:
                g.DrawLine(pen, _lastPoint.Value, currentPoint);
                break;
        }

        _pictureBox.Invalidate();
        _lastPoint = null;
    }

    private void DrawShape(Graphics g, Pen pen, Action<Graphics, Pen, Point, Point> drawAction) {
        if (_lastPoint.HasValue) {
            drawAction(g, pen, _lastPoint.Value, _pictureBox.PointToClient(MousePosition));
        }
    }

    private Rectangle GetRectangle(Point start, Point end) {
        return new Rectangle(
            Math.Min(start.X, end.X),
            Math.Min(start.Y, end.Y),
            Math.Abs(end.X - start.X),
            Math.Abs(end.Y - start.Y)
        );
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

    private void SaveFileAs() {
        using var dialog = new SaveFileDialog {
            Filter = "HAG Files (*.hag)|*.hag|All Files (*.*)|*.*",
            Title = "Save HAG Image"
        };

        if (dialog.ShowDialog() == DialogResult.OK) {
            _currentFilePath = dialog.FileName;
            SaveHagImage(_currentFilePath);
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
            _currentFilePath = filePath;
            Text = $"HAG Viewer - {Path.GetFileName(filePath)}";
            
            _zoomFactor = 1.0f;
            UpdateImageSize();
            CenterImage();
            _statusStrip.Items[0].Text = "Zoom: 100%";
        } catch (Exception ex) {
            MessageBox.Show($"Error loading image: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SaveFile() {
        if (string.IsNullOrEmpty(_currentFilePath)) {
            SaveFileAs();
            return;
        }

        SaveHagImage(_currentFilePath);
    }

    private void SaveHagImage(string filePath) {
        try {
            if (_pictureBox.Image == null) return;

            // Create a new bitmap with the exact dimensions and copy the image data
            using var saveBitmap = new Bitmap(_pictureBox.Image.Width, _pictureBox.Image.Height);
            using (var g = Graphics.FromImage(saveBitmap)) {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;
                g.DrawImage(_pictureBox.Image, 0, 0, _pictureBox.Image.Width, _pictureBox.Image.Height);
            }

            // Save to PNG first to ensure pixel-perfect conversion
            using var ms = new MemoryStream();
            saveBitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;

            // Convert to HAG format
            var pngSerializer = new PngSerializer();
            var sif = pngSerializer.Serialize(ms);

            var hagSerializer = new HagSerializer();
            using var hagStream = hagSerializer.Deserialize(sif);
            
            // Save using atomic operation
            string tempFile = Path.GetTempFileName();
            using (var fileStream = File.Create(tempFile)) {
                hagStream.CopyTo(fileStream);
            }

            if (File.Exists(filePath)) {
                File.Delete(filePath);
            }
            File.Move(tempFile, filePath);

            _currentFilePath = filePath;
            Text = $"HAG Viewer - {Path.GetFileName(filePath)}";
        } catch (Exception ex) {
            MessageBox.Show($"Error saving image: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ImagePanel_MouseWheel(object? sender, MouseEventArgs e) {
        if (ModifierKeys == Keys.Control && _pictureBox.Image != null) {
            float oldZoom = _zoomFactor;
            
            if (e.Delta > 0) {
                _zoomFactor = Math.Min(_zoomFactor * 1.25f, 5.0f);
            } else {
                _zoomFactor = Math.Max(_zoomFactor * 0.8f, 0.1f);
            }

            _pictureBox.Size = new Size(
                (int)(_pictureBox.Image.Width * _zoomFactor),
                (int)(_pictureBox.Image.Height * _zoomFactor)
            );

            CenterImage();
            _statusStrip.Items[0].Text = $"Zoom: {_zoomFactor * 100:0}%";
            _pictureBox.Invalidate();
        }
    }

    private void UpdateImageSize() {
        if (_pictureBox.Image == null) return;
        
        int newWidth = (int)(_pictureBox.Image.Width * _zoomFactor);
        int newHeight = (int)(_pictureBox.Image.Height * _zoomFactor);
        
        _pictureBox.Size = new Size(newWidth, newHeight);
    }

    private void CenterImage() {
        if (_pictureBox.Image == null) return;

        int x = Math.Max(0, (_imagePanel.ClientSize.Width - _pictureBox.Width) / 2);
        int y = Math.Max(0, (_imagePanel.ClientSize.Height - _pictureBox.Height) / 2);
        
        _pictureBox.Location = new Point(x, y);
    }

    private Point GetImageCoordinates(Point mousePosition) {
        if (_pictureBox.Image == null) return mousePosition;
        
        // Convert screen coordinates to PictureBox client coordinates
        var clientPoint = _pictureBox.PointToClient(mousePosition);
        
        // Calculate the actual image bounds within the PictureBox
        var imageRect = GetImageDisplayRectangle();
        
        // Convert to image coordinates
        float scaleX = (float)_pictureBox.Image.Width / imageRect.Width;
        float scaleY = (float)_pictureBox.Image.Height / imageRect.Height;
        
        int imageX = (int)((clientPoint.X - imageRect.X) * scaleX);
        int imageY = (int)((clientPoint.Y - imageRect.Y) * scaleY);
        
        // Clamp coordinates to image bounds
        return new Point(
            Math.Clamp(imageX, 0, _pictureBox.Image.Width - 1),
            Math.Clamp(imageY, 0, _pictureBox.Image.Height - 1)
        );
    }

    private Rectangle GetImageDisplayRectangle() {
        if (_pictureBox.Image == null) return Rectangle.Empty;

        var imageSize = _pictureBox.Image.Size;
        var containerSize = _pictureBox.ClientSize;

        float imageRatio = (float)imageSize.Width / imageSize.Height;
        float containerRatio = (float)containerSize.Width / containerSize.Height;

        int width, height;
        int x = 0, y = 0;

        if (imageRatio > containerRatio) {
            width = containerSize.Width;
            height = (int)(width / imageRatio);
            y = (containerSize.Height - height) / 2;
        } else {
            height = containerSize.Height;
            width = (int)(height * imageRatio);
            x = (containerSize.Width - width) / 2;
        }

        return new Rectangle(x, y, width, height);
    }
}
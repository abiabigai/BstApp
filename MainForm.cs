namespace BstApp;

public class MainForm : Form
{
    private const int HorizontalSpacing = 50;
    private const int VerticalSpacing = 70;
    private const int NodeRadius = 18;

    private readonly BinarySearchTree _tree = new();

    private readonly TextBox _txtValue = new();
    private readonly Button _btnInsert = new();
    private readonly Button _btnSearch = new();
    private readonly Button _btnDelete = new();
    private readonly Button _btnClear = new();
    private readonly TextBox _txtPreOrder = new();
    private readonly TextBox _txtInOrder = new();
    private readonly TextBox _txtPostOrder = new();
    private readonly Label _lblStatus = new();
    private readonly Panel _canvas = new();

    private readonly Dictionary<TreeNode, Point> _positions = new();
    private int _layoutIndex;
    private int? _highlightedValue;

    public MainForm()
    {
        BuildInterface();
        UpdateView();
    }

    private void BuildInterface()
    {
        Text = "Árbol Binario de Búsqueda (ABB)";
        ClientSize = new Size(900, 640);
        MinimumSize = new Size(760, 560);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 10F);

        // ----- Top bar -----
        var lblValue = new Label { Text = "Valor:", AutoSize = true, Location = new Point(15, 20) };
        _txtValue.Location = new Point(70, 16);
        _txtValue.Width = 100;

        ConfigureButton(_btnInsert, "Insertar", 185);
        ConfigureButton(_btnSearch, "Buscar", 285);
        ConfigureButton(_btnDelete, "Eliminar", 385);
        ConfigureButton(_btnClear, "Limpiar árbol", 485, 120);

        _btnInsert.Click += (_, _) => OnInsert();
        _btnSearch.Click += (_, _) => OnSearch();
        _btnDelete.Click += (_, _) => OnDelete();
        _btnClear.Click += (_, _) => OnClear();
        AcceptButton = _btnInsert;

        _lblStatus.Location = new Point(15, 55);
        _lblStatus.AutoSize = true;
        _lblStatus.Text = "Listo.";

        // ----- Traversals -----
        var grpTraversals = new GroupBox
        {
            Text = "Recorridos en profundidad",
            Location = new Point(15, 85),
            Size = new Size(870, 130),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };
        AddTraversalRow(grpTraversals, "Preorden:", _txtPreOrder, 25);
        AddTraversalRow(grpTraversals, "Inorden:", _txtInOrder, 62);
        AddTraversalRow(grpTraversals, "Postorden:", _txtPostOrder, 99);

        // ----- Canvas -----
        _canvas.Location = new Point(15, 225);
        _canvas.Size = new Size(870, 400);
        _canvas.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        _canvas.BorderStyle = BorderStyle.FixedSingle;
        _canvas.BackColor = Color.White;
        _canvas.AutoScroll = true;
        typeof(Panel).GetProperty("DoubleBuffered",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            ?.SetValue(_canvas, true);
        _canvas.Paint += Canvas_Paint;

        Controls.AddRange(new Control[]
        {
            lblValue, _txtValue, _btnInsert, _btnSearch, _btnDelete, _btnClear,
            _lblStatus, grpTraversals, _canvas
        });
    }

    private static void ConfigureButton(Button button, string text, int x, int width = 90)
    {
        button.Text = text;
        button.Location = new Point(x, 14);
        button.Size = new Size(width, 30);
    }

    private static void AddTraversalRow(GroupBox parent, string title, TextBox box, int y)
    {
        parent.Controls.Add(new Label { Text = title, AutoSize = true, Location = new Point(10, y + 4) });
        box.Location = new Point(95, y);
        box.Width = 755;
        box.ReadOnly = true;
        box.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        parent.Controls.Add(box);
    }

    // ---------------- Event handlers ----------------
    private bool TryReadValue(out int value)
    {
        if (int.TryParse(_txtValue.Text.Trim(), out value))
            return true;

        SetStatus("Ingrese un número entero válido.", Color.Firebrick);
        _txtValue.Focus();
        _txtValue.SelectAll();
        return false;
    }

    private void OnInsert()
    {
        if (!TryReadValue(out int value)) return;

        _highlightedValue = null;
        if (_tree.Insert(value))
            SetStatus($"Se insertó el valor {value}.", Color.DarkGreen);
        else
            SetStatus($"El valor {value} ya existe en el árbol (no se permiten duplicados).", Color.Firebrick);

        FinishOperation();
    }

    private void OnSearch()
    {
        if (!TryReadValue(out int value)) return;

        if (_tree.Search(value) != null)
        {
            _highlightedValue = value;
            SetStatus($"El valor {value} fue encontrado en el árbol.", Color.DarkGreen);
        }
        else
        {
            _highlightedValue = null;
            SetStatus($"El valor {value} no existe en el árbol.", Color.Firebrick);
        }

        _canvas.Invalidate();
        _txtValue.SelectAll();
        _txtValue.Focus();
    }

    private void OnDelete()
    {
        if (!TryReadValue(out int value)) return;

        _highlightedValue = null;
        if (_tree.Delete(value))
            SetStatus($"Se eliminó el valor {value}.", Color.DarkGreen);
        else
            SetStatus($"El valor {value} no existe en el árbol.", Color.Firebrick);

        FinishOperation();
    }

    private void OnClear()
    {
        _tree.Clear();
        _highlightedValue = null;
        SetStatus("El árbol fue vaciado.", Color.DimGray);
        FinishOperation();
    }

    private void FinishOperation()
    {
        UpdateView();
        _txtValue.SelectAll();
        _txtValue.Focus();
    }

    private void SetStatus(string message, Color color)
    {
        _lblStatus.ForeColor = color;
        _lblStatus.Text = message;
    }

    // ---------------- View update ----------------
    private void UpdateView()
    {
        _txtPreOrder.Text = string.Join(", ", _tree.PreOrder());
        _txtInOrder.Text = string.Join(", ", _tree.InOrder());
        _txtPostOrder.Text = string.Join(", ", _tree.PostOrder());

        _canvas.AutoScrollMinSize = new Size(
            _tree.Count * HorizontalSpacing + 80,
            _tree.Height() * VerticalSpacing + 40);
        _canvas.Invalidate();
    }

    // ---------------- Drawing ----------------
    private void ComputeLayout(TreeNode? node, int depth)
    {
        if (node == null) return;

        ComputeLayout(node.Left, depth + 1);
        _positions[node] = new Point(40 + _layoutIndex * HorizontalSpacing, 40 + depth * VerticalSpacing);
        _layoutIndex++;
        ComputeLayout(node.Right, depth + 1);
    }

    private void Canvas_Paint(object? sender, PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.TranslateTransform(_canvas.AutoScrollPosition.X, _canvas.AutoScrollPosition.Y);

        if (_tree.Root == null)
        {
            g.DrawString("El árbol está vacío.", Font, Brushes.Gray, 20, 20);
            return;
        }

        _positions.Clear();
        _layoutIndex = 0;
        ComputeLayout(_tree.Root, 0);

        using var edgePen = new Pen(Color.DimGray, 2);
        foreach (var (node, point) in _positions)
        {
            if (node.Left != null) g.DrawLine(edgePen, point, _positions[node.Left]);
            if (node.Right != null) g.DrawLine(edgePen, point, _positions[node.Right]);
        }

        using var format = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };

        foreach (var (node, point) in _positions)
        {
            var rect = new Rectangle(point.X - NodeRadius, point.Y - NodeRadius, NodeRadius * 2, NodeRadius * 2);
            bool highlighted = _highlightedValue == node.Value;

            using var fill = new SolidBrush(highlighted ? Color.Gold : Color.LightSteelBlue);
            g.FillEllipse(fill, rect);
            g.DrawEllipse(Pens.SteelBlue, rect);
            g.DrawString(node.Value.ToString(), Font, Brushes.Black, rect, format);
        }
    }
}

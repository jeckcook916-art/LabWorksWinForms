using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace LabWorksWinForms
{
    public partial class MainForm : Form
    {
        private TabControl tabs;

        // Переменные для логики 
        private Shape? _selectedShape;
        private readonly List<Shape> _allShapes = new List<Shape>();
        private readonly ShapeGenerator _generator = new ShapeGenerator();
        private readonly FilmRental _rental = new FilmRental();

        // Элементы 1 вкладки 
        private TextBox txtIn = null!, txtOut = null!;
        private Label lblStatus = null!;

        // Элементы 2 вкладки
        private TextBox txtFilm = null!;
        private ComboBox cmbLang = null!;
        private Label lblResult = null!;
        private ListBox lstHistory = null!;

        // Элементы 3 вкладки 
        private ListBox listShapes = null!;
        private Panel canvas = null!;
        private Button btnClone = null!;

        //  Элементы 4 вкладки
        private TreeView treeBuilding = null!;
        private Label lblSingleton = null!, lblCurrent = null!;
        private NumericUpDown numFloor = null!;
        private TextBox txtLog = null!;

        public MainForm()
        {
            InitializeUI();
            InitializeCustomState();
        }

        private void InitializeUI()
        {
            this.Text = "Лабораторная работа №1. Порождающие паттерны";
            this.Size = new Size(950, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            tabs = new TabControl { Dock = DockStyle.Fill };

            var tabBuilder = new TabPage("1. Builder (TXT → JSON)");
            var tabFactory = new TabPage("2. Abstract Factory (Кинопрокат)");
            var tabPrototype = new TabPage("3. Factory Method + Prototype");
            var tabSingleton = new TabPage("4. Singleton (Здание и лифт)");

            tabs.TabPages.AddRange(new[] { tabBuilder, tabFactory, tabPrototype, tabSingleton });
            this.Controls.Add(tabs);

            // ВКЛАДКА 1 
            tabBuilder.Controls.Add(new Label { Text = "Входной TXT:", Location = new Point(10, 10), AutoSize = true });
            txtIn = new TextBox { Location = new Point(10, 30), Size = new Size(900, 220), Multiline = true, ScrollBars = ScrollBars.Vertical, Font = new Font("Consolas", 9F) };
            tabBuilder.Controls.Add(txtIn);

            var btnConvert = new Button { Text = "Конвертировать в JSON", Location = new Point(10, 260), Size = new Size(220, 30) };
            btnConvert.Click += BtnConvert_Click;
            tabBuilder.Controls.Add(btnConvert);

            var btnBadHash = new Button { Text = "Испортить хеш", Location = new Point(240, 260), Size = new Size(150, 30) };
            btnBadHash.Click += BtnBadHash_Click;
            tabBuilder.Controls.Add(btnBadHash);

            lblStatus = new Label { Location = new Point(410, 266), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            tabBuilder.Controls.Add(lblStatus);

            tabBuilder.Controls.Add(new Label { Text = "JSON:", Location = new Point(10, 300), AutoSize = true });
            txtOut = new TextBox { Location = new Point(10, 320), Size = new Size(900, 300), Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both, Font = new Font("Consolas", 9F) };
            tabBuilder.Controls.Add(txtOut);

            //  ВКЛАДКА 2 
            tabFactory.Controls.Add(new Label { Text = "Название фильма:", Location = new Point(20, 30), AutoSize = true });
            txtFilm = new TextBox { Location = new Point(20, 55), Size = new Size(350, 22), Text = "Inception" };
            tabFactory.Controls.Add(txtFilm);

            tabFactory.Controls.Add(new Label { Text = "Язык звуковой дорожки / субтитров:", Location = new Point(20, 100), AutoSize = true });
            cmbLang = new ComboBox { Location = new Point(20, 125), Size = new Size(350, 24), DropDownStyle = ComboBoxStyle.DropDownList };
            tabFactory.Controls.Add(cmbLang);

            var btnOrder = new Button { Text = "Заказать фильм", Location = new Point(20, 170), Size = new Size(180, 35) };
            btnOrder.Click += BtnOrder_Click;
            tabFactory.Controls.Add(btnOrder);

            lblResult = new Label { Location = new Point(20, 230), AutoSize = true, Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.DarkBlue };
            tabFactory.Controls.Add(lblResult);

            tabFactory.Controls.Add(new Label { Text = "История заказов:", Location = new Point(20, 280), AutoSize = true });
            lstHistory = new ListBox { Location = new Point(20, 305), Size = new Size(880, 300), Font = new Font("Consolas", 9F) };
            tabFactory.Controls.Add(lstHistory);

            // ВКЛАДКА 3
            var btnGenerate = new Button { Text = "Сгенерировать случайную фигуру", Location = new Point(20, 20), Size = new Size(300, 35) };
            btnGenerate.Click += BtnGenerate_Click;
            tabPrototype.Controls.Add(btnGenerate);

            btnClone = new Button { Text = "Клонировать выделенную", Location = new Point(330, 20), Size = new Size(220, 35), Enabled = false };
            btnClone.Click += BtnClone_Click;
            tabPrototype.Controls.Add(btnClone);

            var btnClear = new Button { Text = "Очистить", Location = new Point(560, 20), Size = new Size(120, 35) };
            btnClear.Click += BtnClear_Click;
            tabPrototype.Controls.Add(btnClear);

            listShapes = new ListBox { Location = new Point(20, 70), Size = new Size(400, 500), Font = new Font("Consolas", 10F) };
            listShapes.SelectedIndexChanged += ListShapes_SelectedIndexChanged;
            tabPrototype.Controls.Add(listShapes);

            canvas = new Panel { Location = new Point(440, 70), Size = new Size(460, 500), BackColor = Color.WhiteSmoke, BorderStyle = BorderStyle.FixedSingle };
            canvas.Paint += Canvas_Paint;
            tabPrototype.Controls.Add(canvas);

            //  ВКЛАДКА 4 
            var btnBuild = new Button { Text = "Построить здание на 5 этажей", Location = new Point(20, 20), Size = new Size(260, 35) };
            btnBuild.Click += BtnBuild_Click;
            tabSingleton.Controls.Add(btnBuild);

            var btnReset = new Button { Text = "Сбросить", Location = new Point(290, 20), Size = new Size(120, 35) };
            btnReset.Click += BtnReset_Click;
            tabSingleton.Controls.Add(btnReset);

            lblSingleton = new Label { Location = new Point(430, 28), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Italic) };
            tabSingleton.Controls.Add(lblSingleton);

            treeBuilding = new TreeView { Location = new Point(20, 70), Size = new Size(400, 400) };
            tabSingleton.Controls.Add(treeBuilding);

            var grpElevator = new GroupBox { Text = "Управление лифтом", Location = new Point(440, 70), Size = new Size(450, 220) };
            tabSingleton.Controls.Add(grpElevator);

            grpElevator.Controls.Add(new Label { Text = "Целевой этаж:", Location = new Point(15, 30), AutoSize = true });
            numFloor = new NumericUpDown { Location = new Point(140, 28), Size = new Size(80, 22), Minimum = 1, Maximum = 5, Value = 1 };
            grpElevator.Controls.Add(numFloor);

            var btnCall = new Button { Text = "Вызвать лифт", Location = new Point(240, 26), Size = new Size(150, 28) };
            btnCall.Click += BtnCall_Click;
            grpElevator.Controls.Add(btnCall);

            lblCurrent = new Label { Location = new Point(15, 80), AutoSize = true, Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.DarkGreen };
            grpElevator.Controls.Add(lblCurrent);

            txtLog = new TextBox { Location = new Point(15, 120), Size = new Size(415, 80), Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Font = new Font("Consolas", 9F) };
            grpElevator.Controls.Add(txtLog);
        }

        private void InitializeCustomState()
        {
            // Начальные данные для Вкладки 1
            string body = "This is a sample article about design patterns.";
            txtIn.Text = $"Title: Design Patterns\r\nAuthors: Ivanov I., Petrov P.\r\nText: {body}\r\nHash: {HashUtil.ComputeHash(body)}\r\n";

            // Начальные данные для Вкладки 2
            cmbLang.Items.AddRange(new object[] { "Russian", "English", "German" });
            cmbLang.SelectedIndex = 1;

            // Начальные данные для Вкладки 4
            Elevator.Instance.FloorChanged += Elevator_FloorChanged;
            lblCurrent.Text = "Лифт на этаже: " + Elevator.Instance.CurrentFloor;
        }

        //Логика Вкладки 1 
        private void BtnConvert_Click(object? sender, EventArgs e)
        {
            try
            {
                var builder = new TxtArticleBuilder();
                var director = new ArticleDirector(builder);
                var article = director.ConstructFromTxt(txtIn.Text);

                if (ArticleJsonConverter.ValidateHash(article))
                {
                    lblStatus.Text = "✔ Хеш корректен";
                    lblStatus.ForeColor = Color.Green;
                }
                else
                {
                    string correctHash = HashUtil.ComputeHash(article.Text);
                    MessageBox.Show($"Текущий хеш в тексте НЕВЕРНЫЙ.\n\nПравильный хеш для этого текста:\n{correctHash}", "Подсказка", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    lblStatus.Text = "✘ Хеш НЕ совпадает!";
                    lblStatus.ForeColor = Color.Red;
                }
                txtOut.Text = ArticleJsonConverter.ToJson(article);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка"); }
        }

        private void BtnBadHash_Click(object? sender, EventArgs e)
        {
            var lines = txtIn.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Trim().StartsWith("Hash:", StringComparison.OrdinalIgnoreCase))
                    lines[i] = "Hash: DEADBEEFDEADBEEFDEADBEEFDEADBEEF";
            }
            txtIn.Text = string.Join(Environment.NewLine, lines);
        }

        //Логика Вкладки 2 
        private void BtnOrder_Click(object? sender, EventArgs e)
        {
            try
            {
                if (cmbLang.SelectedItem == null) return;
                var film = _rental.OrderFilm(txtFilm.Text, cmbLang.SelectedItem.ToString()!);
                lblResult.Text = film.ToString();
                lstHistory.Items.Add(film.ToString());
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка"); }
        }

        //Логика Вкладки 3 
        private void BtnGenerate_Click(object? sender, EventArgs e)
        {
            var shape = _generator.GenerateRandom();
            _allShapes.Add(shape);
            listShapes.Items.Add($"{_allShapes.Count}. {shape}");
            listShapes.SelectedIndex = listShapes.Items.Count - 1;
        }

        private void BtnClone_Click(object? sender, EventArgs e)
        {
            if (listShapes.SelectedIndex < 0) return;
            var original = _allShapes[listShapes.SelectedIndex];
            var clone = original.Clone();
            _allShapes.Add(clone);
            listShapes.Items.Add($"{_allShapes.Count}. {clone} (копия #{listShapes.SelectedIndex + 1})");
            listShapes.SelectedIndex = listShapes.Items.Count - 1;
        }

        private void BtnClear_Click(object? sender, EventArgs e)
        {
            _allShapes.Clear();
            listShapes.Items.Clear();
            _selectedShape = null;
            canvas.Invalidate();
        }

        private void ListShapes_SelectedIndexChanged(object? sender, EventArgs e)
        {
            btnClone.Enabled = listShapes.SelectedIndex >= 0;
            _selectedShape = listShapes.SelectedIndex >= 0 ? _allShapes[listShapes.SelectedIndex] : null;
            canvas.Invalidate();
        }

        private void Canvas_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(canvas.BackColor);
            if (_selectedShape == null) return;

            const int cell = 30, offsetX = 30, offsetY = 40;
            using (var normalBrush = new SolidBrush(Color.CornflowerBlue))
            using (var superBrush = new SolidBrush(Color.OrangeRed))
            using (var pen = new Pen(Color.Black, 1))
            using (var font = new Font("Segoe UI", 10, FontStyle.Bold))
            {
                var brush = _selectedShape.IsSuper ? superBrush : normalBrush;
                foreach (var c in _selectedShape.Cells)
                {
                    var rect = new Rectangle(offsetX + c.Item1 * cell, offsetY + c.Item2 * cell, cell, cell);
                    g.FillRectangle(brush, rect);
                    g.DrawRectangle(pen, rect);
                }
                g.DrawString(_selectedShape.ToString(), font, Brushes.Black, 10, 10);
            }
        }

        //  Логика Вкладки 4 
        private void Elevator_FloorChanged(int from, int to)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<int, int>(Elevator_FloorChanged), from, to);
                return;
            }
            lblCurrent.Text = "Лифт на этаже: " + Elevator.Instance.CurrentFloor;
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] лифт: {from} → {to}\r\n");
        }

        private void BtnBuild_Click(object? sender, EventArgs e)
        {
            Building.Instance.Reset();
            for (int i = 1; i <= 5; i++)
            {
                var floor = new Floor(i);
                floor.AddRoom(new Room($"Помещение {i}-1"));
                floor.AddRoom(new Room($"Помещение {i}-2"));
                Building.Instance.AddFloor(floor);
            }

            numFloor.Maximum = 5;
            lblCurrent.Text = "Лифт на этаже: " + Elevator.Instance.CurrentFloor;
            RefreshTree();

            bool sameBuilding = ReferenceEquals(Building.Instance, Building.Instance);
            bool sameElevator = ReferenceEquals(Building.Instance.Elevator, Building.Instance.Floors[0].Elevator);
            lblSingleton.Text = $"Building Singleton: {sameBuilding} | Elevator Singleton: {sameElevator}";
        }

        private void BtnReset_Click(object? sender, EventArgs e)
        {
            Building.Instance.Reset();
            treeBuilding.Nodes.Clear();
            txtLog.Clear();
            lblCurrent.Text = "Лифт на этаже: 1";
            lblSingleton.Text = "";
        }

        private void BtnCall_Click(object? sender, EventArgs e)
        {
            if (Building.Instance.Floors.Count == 0)
            {
                MessageBox.Show("Сначала постройте здание.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            Elevator.Instance.MoveTo((int)numFloor.Value);
        }

        private void RefreshTree()
        {
            treeBuilding.BeginUpdate();
            treeBuilding.Nodes.Clear();
            var root = new TreeNode(Building.Instance.Name);
            foreach (var f in Building.Instance.Floors)
            {
                var fNode = new TreeNode("Этаж " + f.Number);
                foreach (var r in f.Rooms) fNode.Nodes.Add(new TreeNode(r.Name));
                root.Nodes.Add(fNode);
            }
            treeBuilding.Nodes.Add(root);
            treeBuilding.ExpandAll();
            treeBuilding.EndUpdate();
        }
    }
}
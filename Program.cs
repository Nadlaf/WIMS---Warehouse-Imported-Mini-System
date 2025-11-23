using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WIMS_Prototype
{
    public enum UserRole { Operator, Manager, Administrator }

    public class User
    {
        public string FullName { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public UserRole Role { get; set; }
    }

    public class InventoryItem
    {
        public string Article { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public string Location { get; set; }
    }

    public static class MockDatabase
    {
        public static List<User> Users = new List<User>()
        {
            new User { FullName = "Іван Петренко", Login = "admin", Password = "123", Role = UserRole.Administrator },
            new User { FullName = "Олена Сидоренко", Login = "manager", Password = "123", Role = UserRole.Manager },
            new User { FullName = "Василь Коваленко", Login = "operator", Password = "123", Role = UserRole.Operator }
        };

        public static List<InventoryItem> Inventory = new List<InventoryItem>()
        {
            new InventoryItem { Article = "100-AB", Name = "Сенсор IoT (A)", Quantity = 150, Location = "Секція A-12" },
            new InventoryItem { Article = "101-BC", Name = "RFID-мітка (B)", Quantity = 5000, Location = "Секція Б-04" },
            new InventoryItem { Article = "203-CD", Name = "Сканер (X)", Quantity = 45, Location = "Секція A-02" },
            new InventoryItem { Article = "305-EF", Name = "Сервер Rack 4U", Quantity = 10, Location = "Секція C-01" }
        };
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LoginForm());
        }
    }

    public class LoginForm : Form
    {
        private TextBox txtLogin;
        private TextBox txtPassword;

        public LoginForm()
        {
            this.Text = "WIMS - Вхід";
            this.Size = new Size(400, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            Label lblTitle = new Label();
            lblTitle.Text = "WIMS";
            lblTitle.Font = new Font("Segoe UI", 36, FontStyle.Bold);
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(110, 50);
            this.Controls.Add(lblTitle);

            Label lblSubtitle = new Label();
            lblSubtitle.Text = "Вхід до системи управління складом";
            lblSubtitle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblSubtitle.AutoSize = true;
            lblSubtitle.Location = new Point(60, 120);
            this.Controls.Add(lblSubtitle);

            Label lblLogin = new Label { Text = "Логін:", Location = new Point(50, 180), Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true };
            txtLogin = new TextBox { Location = new Point(50, 205), Size = new Size(280, 30), Font = new Font("Segoe UI", 10), BackColor = Color.FromArgb(240, 240, 240) };
            this.Controls.Add(lblLogin);
            this.Controls.Add(txtLogin);

            Label lblPass = new Label { Text = "Пароль:", Location = new Point(50, 245), Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true };
            txtPassword = new TextBox { Location = new Point(50, 270), Size = new Size(280, 30), Font = new Font("Segoe UI", 10), PasswordChar = '*', BackColor = Color.FromArgb(240, 240, 240) };
            this.Controls.Add(lblPass);
            this.Controls.Add(txtPassword);

            Button btnLogin = new Button();
            btnLogin.Text = "Увійти";
            btnLogin.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btnLogin.Location = new Point(120, 330);
            btnLogin.Size = new Size(140, 40);
            btnLogin.BackColor = Color.White;
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.Click += BtnLogin_Click;
            this.Controls.Add(btnLogin);

            Label lblForgot = new Label();
            lblForgot.Text = "Забули пароль?";
            lblForgot.Font = new Font("Segoe UI", 9, FontStyle.Bold, GraphicsUnit.Point);
            lblForgot.ForeColor = Color.Gray;
            lblForgot.Location = new Point(135, 390);
            lblForgot.AutoSize = true;
            this.Controls.Add(lblForgot);
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            var user = MockDatabase.Users.FirstOrDefault(u => u.Login == txtLogin.Text && u.Password == txtPassword.Text);

            if (user != null)
            {
                this.Hide();
                Form nextForm = null;

                switch (user.Role)
                {
                    case UserRole.Operator:
                        nextForm = new OperatorForm(user);
                        break;
                    case UserRole.Manager:
                        nextForm = new ManagerForm(user);
                        break;
                    case UserRole.Administrator:
                        nextForm = new AdminUserForm(user);
                        break;
                }

                if (nextForm != null)
                {
                    nextForm.FormClosed += (s, args) => this.Show();
                    nextForm.Show();
                }
            }
            else
            {
                MessageBox.Show("Невірний логін або пароль.\nСпробуйте: admin/123, manager/123, operator/123", "Помилка входу", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    public class OperatorForm : Form
    {
        public OperatorForm(User user)
        {
            this.Text = "WIMS - Панель оператора";
            this.Size = new Size(400, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            Label lblWelcome = new Label();
            lblWelcome.Text = $"Вітаємо, {user.FullName}!";
            lblWelcome.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            lblWelcome.TextAlign = ContentAlignment.MiddleCenter;
            lblWelcome.Dock = DockStyle.Top;
            lblWelcome.Padding = new Padding(0, 20, 0, 0);
            lblWelcome.Height = 60;
            this.Controls.Add(lblWelcome);

            int yPos = 80;
            CreateActionBlock("Приймання товару", "Сканувати нове надходження", ref yPos);
            CreateActionBlock("Відвантаження замовлення", "Сканувати товари для відправки", ref yPos);
            CreateActionBlock("Перевірка залишків", "Сканувати штрих-код для інфо", ref yPos);

            Button btnInternal = new Button { Text = "Внутрішнє переміщення", Location = new Point(50, yPos + 20), Size = new Size(300, 35), BackColor = Color.FromArgb(240, 240, 240), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            this.Controls.Add(btnInternal);

            Button btnExit = new Button { Text = "Вихід", Location = new Point(150, yPos + 70), Size = new Size(100, 35), BackColor = Color.MistyRose, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btnExit.Click += (s, e) => this.Close();
            this.Controls.Add(btnExit);
        }

        private void CreateActionBlock(string title, string btnText, ref int yPos)
        {
            Label lblTitle = new Label { Text = title, Location = new Point(0, yPos), Size = new Size(400, 25), TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 11, FontStyle.Bold) };
            Button btnAction = new Button { Text = btnText, Location = new Point(40, yPos + 30), Size = new Size(320, 40), BackColor = Color.FromArgb(240, 240, 240), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10) };
            
            btnAction.Click += (s, e) => MessageBox.Show($"Запуск модуля: {title}\nСтатус: Очікування сканера штрих-кодів...", "Система WIMS");

            this.Controls.Add(lblTitle);
            this.Controls.Add(btnAction);
            yPos += 90;
        }
    }

    public class ManagerForm : Form
    {
        private DataGridView grid;
        private TextBox txtSearch;

        public ManagerForm(User user)
        {
            this.Text = "WIMS - Панель менеджера";
            this.Size = new Size(600, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            Label lblHeader = new Label { Text = "Панель управління WIMS", Font = new Font("Segoe UI", 16, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true };
            this.Controls.Add(lblHeader);

            Label lblMenu = new Label { Text = "[Меню: Головна | Запаси | Звіти | Інтеграції (CRM/ERP)]", Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(20, 60), AutoSize = true };
            this.Controls.Add(lblMenu);

            Label lblSearchTitle = new Label { Text = "Огляд Запасів", Font = new Font("Segoe UI", 11, FontStyle.Bold), Location = new Point(20, 90), AutoSize = true };
            this.Controls.Add(lblSearchTitle);

            Label lblSearch = new Label { Text = "Пошук:", Location = new Point(20, 123), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            this.Controls.Add(lblSearch);

            txtSearch = new TextBox { Location = new Point(80, 120), Size = new Size(300, 25) };
            txtSearch.TextChanged += TxtSearch_TextChanged;
            this.Controls.Add(txtSearch);

            Label lblFilter = new Label { Text = "[Фільтр]", Location = new Point(390, 123), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            this.Controls.Add(lblFilter);

            grid = new DataGridView();
            grid.Location = new Point(20, 160);
            grid.Size = new Size(540, 300);
            grid.ColumnCount = 4;
            grid.Columns[0].Name = "Артикул";
            grid.Columns[1].Name = "Назва товару";
            grid.Columns[1].Width = 150;
            grid.Columns[2].Name = "К-сть";
            grid.Columns[3].Name = "Розташування";
            grid.AllowUserToAddRows = false;
            grid.ReadOnly = true;
            grid.RowHeadersVisible = false;
            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.Fixed3D;
            this.Controls.Add(grid);

            RefreshGrid();

            Button btnNew = new Button { Text = "+ Новий товар", Location = new Point(20, 480), Size = new Size(150, 40), BackColor = Color.FromArgb(240, 240, 240), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btnNew.Click += (s,e) => MessageBox.Show("Відкриття форми додавання товару...", "WIMS");
            this.Controls.Add(btnNew);

            Button btnReport = new Button { Text = "Сгенерувати звіт", Location = new Point(200, 480), Size = new Size(150, 40), BackColor = Color.FromArgb(240, 240, 240), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btnReport.Click += (s, e) => MessageBox.Show("Звіт сформовано та відправлено до ERP системи.", "Інтеграція");
            this.Controls.Add(btnReport);
        }

        private void RefreshGrid(string filter = "")
        {
            grid.Rows.Clear();
            foreach (var item in MockDatabase.Inventory)
            {
                if (string.IsNullOrEmpty(filter) || 
                    item.Name.ToLower().Contains(filter.ToLower()) || 
                    item.Article.ToLower().Contains(filter.ToLower()))
                {
                    grid.Rows.Add(item.Article, item.Name, item.Quantity, item.Location);
                }
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            RefreshGrid(txtSearch.Text);
        }
    }

    public class AdminUserForm : Form
    {
        private TextBox txtFio;
        private TextBox txtLogin;
        private TextBox txtTempPass;
        private RadioButton rbOperator;
        private RadioButton rbManager;
        private RadioButton rbAdmin;

        public AdminUserForm(User user)
        {
            this.Text = "WIMS - Адміністратор";
            this.Size = new Size(500, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            Label lblHeader = new Label { Text = "Управління користувачами > Створити нового", Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(30, 30), AutoSize = true };
            this.Controls.Add(lblHeader);

            int y = 80;
            AddLabelAndInput("ПІБ:", ref txtFio, y); y += 50;
            AddLabelAndInput("Логін:", ref txtLogin, y); y += 50;
            AddLabelAndInput("Тимчасовий пароль:", ref txtTempPass, y); y += 50;

            Label lblRole = new Label { Text = "Роль у системі:", Location = new Point(30, y), Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true };
            this.Controls.Add(lblRole);

            Panel pnlRoles = new Panel { Location = new Point(180, y), Size = new Size(250, 100) };
            rbOperator = new RadioButton { Text = "Оператор складу", Location = new Point(0, 0), AutoSize = true, Checked = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            rbManager = new RadioButton { Text = "Менеджер запасів", Location = new Point(0, 30), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            rbAdmin = new RadioButton { Text = "Адміністратор", Location = new Point(0, 60), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            
            pnlRoles.Controls.Add(rbOperator);
            pnlRoles.Controls.Add(rbManager);
            pnlRoles.Controls.Add(rbAdmin);
            this.Controls.Add(pnlRoles);

            Button btnCreate = new Button { Text = "Створити користувача", Location = new Point(30, 380), Size = new Size(200, 40), BackColor = Color.FromArgb(240, 240, 240), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btnCreate.Click += BtnCreate_Click;
            this.Controls.Add(btnCreate);

            Button btnCancel = new Button { Text = "Скасувати", Location = new Point(250, 380), Size = new Size(150, 40), BackColor = Color.FromArgb(240, 240, 240), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btnCancel.Click += (s, e) => this.Close();
            this.Controls.Add(btnCancel);
        }

        private void AddLabelAndInput(string text, ref TextBox tb, int y)
        {
            Label lbl = new Label { Text = text, Location = new Point(30, y), Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true };
            tb = new TextBox { Location = new Point(180, y), Size = new Size(250, 25), BackColor = Color.FromArgb(240, 240, 240) };
            this.Controls.Add(lbl);
            this.Controls.Add(tb);
        }

        private void BtnCreate_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrWhiteSpace(txtFio.Text) || string.IsNullOrWhiteSpace(txtLogin.Text))
            {
                MessageBox.Show("Заповніть всі поля!");
                return;
            }

            UserRole role = UserRole.Operator;
            if (rbManager.Checked) role = UserRole.Manager;
            if (rbAdmin.Checked) role = UserRole.Administrator;

            MockDatabase.Users.Add(new User
            {
                FullName = txtFio.Text,
                Login = txtLogin.Text,
                Password = txtTempPass.Text,
                Role = role
            });

            MessageBox.Show($"Користувача {txtLogin.Text} успішно створено!", "Успіх");
            this.Close();
        }
    }
}
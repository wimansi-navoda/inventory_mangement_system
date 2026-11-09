public class LoginForm : Form
{
    TextBox txtUser = new(), txtPass = new() { UseSystemPasswordChar = true };
    public string User;

    public LoginForm()
    {
        Text = "Login"; Size = new Size(340, 240); StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false;

        Controls.Add(new Label { Text = "Username", Left = 20, Top = 15, AutoSize = true });
        txtUser.SetBounds(20, 37, 280, 25); Controls.Add(txtUser);
        Controls.Add(new Label { Text = "Password", Left = 20, Top = 70, AutoSize = true });
        txtPass.SetBounds(20, 92, 280, 25); Controls.Add(txtPass);

        var btn = new Button { Text = "Login", Left = 20, Top = 130, Width = 280, Height = 30 };
        btn.Click += DoLogin; Controls.Add(btn); AcceptButton = btn;

        var link = new LinkLabel { Text = "Forgot password?", Left = 20, Top = 170, AutoSize = true };
        link.Click += (s, e) => new ForgotForm().ShowDialog();
        Controls.Add(link);
    }

    void DoLogin(object s, EventArgs e)
    {
        var dt = Db.Table("SELECT PasswordHash FROM Users WHERE Username=@u", ("@u", txtUser.Text.Trim()));
        if (dt.Rows.Count > 0 && Db.Verify(txtPass.Text, (string)dt.Rows[0][0]))
        {
            User = txtUser.Text.Trim();
            DialogResult = DialogResult.OK;   // opens dashboard
        }
        else MessageBox.Show("Invalid username or password", "Login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}
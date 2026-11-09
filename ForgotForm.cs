using System.Security.Cryptography;

public class ForgotForm : Form
{
    TextBox txtEmail = new(), txtCode = new(), txtNew = new() { UseSystemPasswordChar = true };

    public ForgotForm()
    {
        Text = "Reset password"; Size = new Size(340, 340); StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false; MinimizeBox = false;

        Controls.Add(new Label { Text = "Email address", Left = 20, Top = 15, AutoSize = true });
        txtEmail.SetBounds(20, 37, 280, 25); Controls.Add(txtEmail);
        var send = new Button { Text = "Send reset code", Left = 20, Top = 70, Width = 280 };
        send.Click += Send; Controls.Add(send);

        Controls.Add(new Label { Text = "Reset code", Left = 20, Top = 115, AutoSize = true });
        txtCode.SetBounds(20, 137, 280, 25); Controls.Add(txtCode);
        Controls.Add(new Label { Text = "New password (min 6 chars)", Left = 20, Top = 170, AutoSize = true });
        txtNew.SetBounds(20, 192, 280, 25); Controls.Add(txtNew);
        var reset = new Button { Text = "Reset password", Left = 20, Top = 230, Width = 280 };
        reset.Click += Reset; Controls.Add(reset);
    }

    void Send(object s, EventArgs e)
    {
        var email = txtEmail.Text.Trim();
        if ((int)Db.Scalar("SELECT COUNT(*) FROM Users WHERE Email=@e", ("@e", email)) == 0)
        { MessageBox.Show("Invalid email address"); return; }

        var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        Db.Exec("UPDATE Users SET ResetCode=@c WHERE Email=@e", ("@c", code), ("@e", email));
        // DEMO ONLY: a real system would email this code (SmtpClient / SendGrid).
        MessageBox.Show("DEMO: your reset code is " + code, "Reset code");
    }

    void Reset(object s, EventArgs e)
    {
        if (txtNew.Text.Length < 6) { MessageBox.Show("Password must be at least 6 characters"); return; }
        int n = Db.Exec("UPDATE Users SET PasswordHash=@p, ResetCode=NULL WHERE Email=@e AND ResetCode=@c",
                        ("@p", Db.Hash(txtNew.Text)), ("@e", txtEmail.Text.Trim()), ("@c", txtCode.Text.Trim()));
        if (n == 0) { MessageBox.Show("Wrong code or email"); return; }
        MessageBox.Show("Password updated. Please log in again.");
        DialogResult = DialogResult.OK;
    }
}
static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        try { Db.Init(); }
        catch (Exception ex)
        {
            MessageBox.Show("Database error:\n" + ex.Message, "Startup",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        var login = new LoginForm();
        if (login.ShowDialog() == DialogResult.OK)
            Application.Run(new MainForm(login.User));
    }
}
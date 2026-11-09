using System.Data;
using Microsoft.Data.SqlClient;
//(Products, Sales, Low Stock)
public class MainForm : Form
{
    TextBox txtId = new(), txtName = new(), txtCat = new(), txtPrice = new(), txtQty = new(),
            txtReorder = new() { Text = "5" }, txtSearch = new(), sId = new(), sQty = new();
    DataGridView gridP = Grid(), gridL = Grid(), gridS = Grid();
    Label lblInfo = new() { AutoSize = true, Margin = new Padding(12, 10, 0, 0) },
          lblLow = new() { AutoSize = true, Margin = new Padding(12, 10, 0, 0) };
    TabControl tabs = new() { Dock = DockStyle.Fill };

    static DataGridView Grid() => new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AllowUserToAddRows = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
    };
    static FlowLayoutPanel Bar() => new() { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(8) };
    static Label L(string t) => new() { Text = t, AutoSize = true, Margin = new Padding(8, 8, 2, 0) };
    static Button B(string t, EventHandler h)
    { var b = new Button { Text = t, AutoSize = true, Margin = new Padding(8, 4, 2, 0) }; b.Click += h; return b; }

    public MainForm(string user)
    {
        Text = "Inventory & Sales System - " + user;
        Size = new Size(980, 640); StartPosition = FormStartPosition.CenterScreen;
        var t1 = new TabPage("Products"); var t2 = new TabPage("Sales"); var t3 = new TabPage("Low Stock");
        BuildProducts(t1); BuildSales(t2); BuildLow(t3);
        tabs.TabPages.AddRange(new[] { t1, t2, t3 });
        Controls.Add(tabs);
        tabs.SelectedIndexChanged += (s, e) =>
        {
            if (tabs.SelectedTab == t3) LoadLow();
            if (tabs.SelectedTab == t1) LoadProducts();
        };
        LoadProducts();
    }

    // ================= PRODUCT MANAGEMENT =================
    void BuildProducts(TabPage p)
    {
        var bar = Bar();
        foreach (var (n, c) in new (string, TextBox)[]
            { ("Product ID", txtId), ("Name", txtName), ("Category", txtCat),
              ("Price", txtPrice), ("Quantity", txtQty), ("Reorder level", txtReorder) })
        { c.Width = 90; bar.Controls.Add(L(n)); bar.Controls.Add(c); }
        bar.SetFlowBreak(txtReorder, true);

        bar.Controls.Add(B("Add", OnAdd));
        bar.Controls.Add(B("Update", OnUpdate));
        bar.Controls.Add(B("Delete", OnDelete));
        bar.Controls.Add(B("Clear", (s, e) => ClearFields()));
        bar.Controls.Add(L("Search"));
        txtSearch.Width = 150; bar.Controls.Add(txtSearch);
        bar.Controls.Add(B("Search", (s, e) => LoadProducts(txtSearch.Text.Trim())));
        bar.Controls.Add(B("Show all", (s, e) => { txtSearch.Clear(); LoadProducts(); }));

        gridP.CellClick += (s, e) =>
        {
            if (e.RowIndex < 0) return;
            var r = gridP.Rows[e.RowIndex];
            txtId.Text = r.Cells["ProductID"].Value.ToString();
            txtName.Text = r.Cells["ProductName"].Value.ToString();
            txtCat.Text = r.Cells["Category"].Value.ToString();
            txtPrice.Text = r.Cells["Price"].Value.ToString();
            txtQty.Text = r.Cells["Quantity"].Value.ToString();
            txtReorder.Text = r.Cells["ReorderLevel"].Value.ToString();
        };
        p.Controls.Add(gridP);   // fill control first
        p.Controls.Add(bar);
    }

    void LoadProducts(string filter = null)
    {
        gridP.DataSource = string.IsNullOrEmpty(filter)
            ? Db.Table("SELECT * FROM Products ORDER BY ProductID")
            : Db.Table(@"SELECT * FROM Products WHERE ProductID LIKE @f OR ProductName LIKE @f
                         OR Category LIKE @f ORDER BY ProductID", ("@f", "%" + filter + "%"));
    }

    void ClearFields()
    { txtId.Clear(); txtName.Clear(); txtCat.Clear(); txtPrice.Clear(); txtQty.Clear(); txtReorder.Text = "5"; }

    bool Valid(out decimal price, out int qty, out int reorder)
    {
        price = 0; qty = 0; reorder = 0;
        return txtId.Text.Trim() != "" && txtName.Text.Trim() != ""
            && decimal.TryParse(txtPrice.Text, out price) && price > 0
            && int.TryParse(txtQty.Text, out qty) && qty >= 0
            && int.TryParse(txtReorder.Text, out reorder) && reorder >= 0;
    }

    bool Exists(string id) =>
        (int)Db.Scalar("SELECT COUNT(*) FROM Products WHERE ProductID=@i", ("@i", id)) > 0;

    void OnAdd(object s, EventArgs e)
    {
        if (!Valid(out var price, out var qty, out var re))
        { MessageBox.Show("Error: check details (price > 0, quantity >= 0)."); return; }
        try
        {
            Db.Exec("INSERT INTO Products VALUES(@i,@n,@c,@p,@q,@r)",
                ("@i", txtId.Text.Trim()), ("@n", txtName.Text.Trim()), ("@c", txtCat.Text.Trim()),
                ("@p", price), ("@q", qty), ("@r", re));
            MessageBox.Show("Product added successfully"); LoadProducts(); ClearFields();
        }
        catch (SqlException ex) when (ex.Number == 2627) { MessageBox.Show("Product ID already exists"); }
    }

    void OnUpdate(object s, EventArgs e)
    {
        var id = txtId.Text.Trim();
        if (!Exists(id)) { MessageBox.Show("Product not found"); return; }
        if (!Valid(out var price, out var qty, out var re))
        { MessageBox.Show("Error: check details (price > 0, quantity >= 0)."); return; }
        Db.Exec(@"UPDATE Products SET ProductName=@n,Category=@c,Price=@p,Quantity=@q,ReorderLevel=@r
                  WHERE ProductID=@i",
            ("@i", id), ("@n", txtName.Text.Trim()), ("@c", txtCat.Text.Trim()),
            ("@p", price), ("@q", qty), ("@r", re));
        MessageBox.Show("Product updated successfully"); LoadProducts();
    }

    void OnDelete(object s, EventArgs e)
    {
        var id = txtId.Text.Trim();
        bool sold = (int)Db.Scalar("SELECT COUNT(*) FROM SaleDetails WHERE ProductID=@i", ("@i", id)) > 0;
        if (!Exists(id) || sold) { MessageBox.Show("Product deletion restricted (not found or already sold)"); return; }
        if (MessageBox.Show("Delete this product?", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
        Db.Exec("DELETE FROM Products WHERE ProductID=@i", ("@i", id));
        MessageBox.Show("Product deleted successfully"); LoadProducts(); ClearFields();
    }

    // ================= SALES MANAGEMENT =================
    void BuildSales(TabPage p)
    {
        var bar = Bar();
        sId.Width = 110; sQty.Width = 60;
        bar.Controls.Add(L("Product ID")); bar.Controls.Add(sId);
        bar.Controls.Add(L("Quantity")); bar.Controls.Add(sQty);
        bar.Controls.Add(B("Sell", OnSell));
        bar.Controls.Add(lblInfo);
        p.Controls.Add(gridS);
        p.Controls.Add(bar);
        LoadSales();
    }

    void LoadSales() => gridS.DataSource = Db.Table(
        @"SELECT TOP 50 s.SaleID, s.SaleDate, d.ProductID, d.Qty, d.UnitPrice, s.TotalAmount
          FROM Sales s JOIN SaleDetails d ON d.SaleID=s.SaleID ORDER BY s.SaleID DESC");

    void OnSell(object s, EventArgs e)
    {
        var pid = sId.Text.Trim();
        var dt = Db.Table("SELECT Price,Quantity,ReorderLevel FROM Products WHERE ProductID=@i", ("@i", pid));
        if (dt.Rows.Count == 0 || !int.TryParse(sQty.Text, out var q) || q <= 0)
        { MessageBox.Show("Validation error: product not found or invalid quantity"); return; }

        var r = dt.Rows[0];
        decimal price = (decimal)r["Price"]; int stock = (int)r["Quantity"];
        if (q > stock) { MessageBox.Show($"Insufficient stock. Available: {stock}"); return; }

        decimal total = q * price;
        if (MessageBox.Show($"Total amount: {total:0.00}\n\nConfirm sale?", "Confirm sale",
                            MessageBoxButtons.YesNo) != DialogResult.Yes)
        { lblInfo.Text = "Transaction cancelled"; return; }

        using var c = new SqlConnection(Db.ConnStr); c.Open();
        using var tx = c.BeginTransaction();
        try
        {
            var cmd = new SqlCommand("INSERT INTO Sales(TotalAmount) OUTPUT INSERTED.SaleID VALUES(@t)", c, tx);
            cmd.Parameters.AddWithValue("@t", total);
            int saleId = (int)cmd.ExecuteScalar();

            cmd = new SqlCommand("INSERT INTO SaleDetails(SaleID,ProductID,Qty,UnitPrice) VALUES(@s,@p,@q,@u)", c, tx);
            cmd.Parameters.AddWithValue("@s", saleId); cmd.Parameters.AddWithValue("@p", pid);
            cmd.Parameters.AddWithValue("@q", q); cmd.Parameters.AddWithValue("@u", price);
            cmd.ExecuteNonQuery();

            cmd = new SqlCommand("UPDATE Products SET Quantity=Quantity-@q WHERE ProductID=@p AND Quantity>=@q", c, tx);
            cmd.Parameters.AddWithValue("@q", q); cmd.Parameters.AddWithValue("@p", pid);
            if (cmd.ExecuteNonQuery() == 0) throw new Exception("Stock changed, please try again");
            tx.Commit();
        }
        catch (Exception ex) { tx.Rollback(); MessageBox.Show(ex.Message); return; }

        lblInfo.Text = "Sale completed successfully";
        if (stock - q <= (int)r["ReorderLevel"])
            MessageBox.Show("Low-stock alert: reorder level reached for " + pid);
        sId.Clear(); sQty.Clear(); LoadSales(); LoadProducts();
    }

    // ================= LOW STOCK =================
    void BuildLow(TabPage p)
    {
        var bar = Bar();
        bar.Controls.Add(B("Refresh", (s, e) => LoadLow()));
        bar.Controls.Add(lblLow);
        p.Controls.Add(gridL);
        p.Controls.Add(bar);
    }

    void LoadLow()
    {
        var dt = Db.Table(@"SELECT ProductID,ProductName,Quantity,ReorderLevel
                            FROM Products WHERE Quantity<=ReorderLevel ORDER BY Quantity");
        gridL.DataSource = dt;
        lblLow.Text = dt.Rows.Count > 0
            ? $"Low-stock report: {dt.Rows.Count} product(s) need reorder"
            : "No low-stock products found";
    }
}
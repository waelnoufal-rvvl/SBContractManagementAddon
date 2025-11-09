using System;
using System.Data.Odbc;
using System.Drawing;
using System.Windows.Forms;
using ContractManagementAddon.Utilities;

namespace ContractManagementAddon
{
    /// <summary>
    /// Test form for ODBC connection diagnostics
    /// </summary>
    public class OdbcTestForm : Form
    {
        private TextBox txtOutput;
        private Button btnTest;
        private Button btnListDrivers;
        private Button btnClose;
        private GroupBox grpConfig;
        private Label lblServer;
        private Label lblPort;
        private Label lblDatabase;
        private Label lblUsername;
        private TextBox txtServer;
        private TextBox txtPort;
        private TextBox txtDatabase;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Label lblPassword;

        public OdbcTestForm()
        {
            InitializeComponents();
            LoadConfiguration();
        }

        private void InitializeComponents()
        {
            this.Text = "ODBC Connection Test - Contract Management Add-On";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Configuration Group
            grpConfig = new GroupBox
            {
                Text = "HANA Database Configuration",
                Location = new Point(10, 10),
                Size = new Size(760, 150)
            };

            lblServer = new Label { Text = "Server:", Location = new Point(10, 25), Size = new Size(80, 20) };
            txtServer = new TextBox { Location = new Point(100, 22), Size = new Size(250, 20) };

            lblPort = new Label { Text = "Port:", Location = new Point(370, 25), Size = new Size(50, 20) };
            txtPort = new TextBox { Location = new Point(430, 22), Size = new Size(100, 20) };

            lblDatabase = new Label { Text = "Database:", Location = new Point(10, 55), Size = new Size(80, 20) };
            txtDatabase = new TextBox { Location = new Point(100, 52), Size = new Size(250, 20) };

            lblUsername = new Label { Text = "Username:", Location = new Point(10, 85), Size = new Size(80, 20) };
            txtUsername = new TextBox { Location = new Point(100, 82), Size = new Size(250, 20) };

            lblPassword = new Label { Text = "Password:", Location = new Point(10, 115), Size = new Size(80, 20) };
            txtPassword = new TextBox { Location = new Point(100, 112), Size = new Size(250, 20), UseSystemPasswordChar = true };

            grpConfig.Controls.AddRange(new Control[] {
                lblServer, txtServer, lblPort, txtPort,
                lblDatabase, txtDatabase, lblUsername, txtUsername,
                lblPassword, txtPassword
            });

            // Buttons
            btnListDrivers = new Button
            {
                Text = "List Installed Drivers",
                Location = new Point(10, 170),
                Size = new Size(150, 30)
            };
            btnListDrivers.Click += BtnListDrivers_Click;

            btnTest = new Button
            {
                Text = "Test Connection",
                Location = new Point(170, 170),
                Size = new Size(150, 30)
            };
            btnTest.Click += BtnTest_Click;

            btnClose = new Button
            {
                Text = "Close",
                Location = new Point(640, 170),
                Size = new Size(130, 30)
            };
            btnClose.Click += (s, e) => this.Close();

            // Output TextBox
            txtOutput = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                Location = new Point(10, 210),
                Size = new Size(760, 330),
                Font = new Font("Consolas", 9),
                ReadOnly = true
            };

            // Add controls to form
            this.Controls.AddRange(new Control[] {
                grpConfig, btnListDrivers, btnTest, btnClose, txtOutput
            });
        }

        private void LoadConfiguration()
        {
            try
            {
                txtServer.Text = System.Configuration.ConfigurationManager.AppSettings["HANAServer"] ?? "localhost";
                txtPort.Text = System.Configuration.ConfigurationManager.AppSettings["HANAPort"] ?? "30015";
                txtDatabase.Text = System.Configuration.ConfigurationManager.AppSettings["HANADatabase"] ?? "";
                txtUsername.Text = System.Configuration.ConfigurationManager.AppSettings["HANAUsername"] ?? "";
                txtPassword.Text = System.Configuration.ConfigurationManager.AppSettings["HANAPassword"] ?? "";

                AppendOutput("Configuration loaded from App.config");
                AppendOutput("Ready to test ODBC connection.");
                AppendOutput("");
            }
            catch (Exception ex)
            {
                AppendOutput($"Error loading configuration: {ex.Message}");
            }
        }

        private void BtnListDrivers_Click(object sender, EventArgs e)
        {
            txtOutput.Clear();
            AppendOutput("=== Installed ODBC Drivers ===");
            AppendOutput("");

            try
            {
                using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\ODBC\ODBCINST.INI\ODBC Drivers"))
                {
                    if (key != null)
                    {
                        AppendOutput("32-bit ODBC Drivers:");
                        int count = 0;

                        foreach (var driverName in key.GetValueNames())
                        {
                            var value = key.GetValue(driverName);
                            if (value != null && value.ToString() == "Installed")
                            {
                                count++;
                                if (driverName.ToUpper().Contains("HANA") || driverName.ToUpper().Contains("HDB"))
                                {
                                    AppendOutput($"  ✓ {driverName} [SAP HANA] ← Use this!");
                                }
                                else
                                {
                                    AppendOutput($"    {driverName}");
                                }
                            }
                        }

                        if (count == 0)
                        {
                            AppendOutput("  ⚠ No ODBC drivers found in 32-bit registry");
                        }
                    }
                }

                AppendOutput("");
                AppendOutput("NOTE: This application is compiled as x86 (32-bit).");
                AppendOutput("You need the 32-bit SAP HANA ODBC driver installed.");
                AppendOutput("");
                AppendOutput("To install: Download SAP HANA Client (32-bit) from:");
                AppendOutput("https://tools.hana.ondemand.com/#hanatools");
            }
            catch (Exception ex)
            {
                AppendOutput($"Error listing drivers: {ex.Message}");
            }
        }

        private void BtnTest_Click(object sender, EventArgs e)
        {
            txtOutput.Clear();
            AppendOutput("=== Testing ODBC Connection ===");
            AppendOutput("");

            string server = txtServer.Text;
            string port = txtPort.Text;
            string database = txtDatabase.Text;
            string username = txtUsername.Text;
            string password = txtPassword.Text;

            AppendOutput($"Server: {server}");
            AppendOutput($"Port: {port}");
            AppendOutput($"Database: {database}");
            AppendOutput($"Username: {username}");
            AppendOutput($"Password: {new string('*', password.Length)}");
            AppendOutput("");

            // Try multiple driver names
            string[] driverNames = new string[]
            {
                "HDBODBC",
                "HDBODBC32",
                "SAP HANA ODBC",
                "SAP HANA",
                System.Configuration.ConfigurationManager.AppSettings["ODBCDriver"] ?? "HDBODBC"
            };

            bool success = false;
            string workingDriver = null;

            foreach (string driverName in driverNames)
            {
                try
                {
                    AppendOutput($"Trying driver: {driverName}");

                    string connectionString = $"Driver={{{driverName}}};ServerNode={server}:{port};Database={database};UID={username};PWD={password}";

                    using (OdbcConnection conn = new OdbcConnection(connectionString))
                    {
                        conn.Open();

                        AppendOutput($"✓ Connected successfully with driver: {driverName}!");
                        AppendOutput($"  Data Source: {conn.DataSource}");
                        AppendOutput($"  Database: {conn.Database}");
                        AppendOutput($"  State: {conn.State}");
                        AppendOutput($"  Server Version: {conn.ServerVersion}");
                        AppendOutput("");

                        // Test query
                        AppendOutput("Testing query: SELECT 1 FROM DUMMY");
                        using (OdbcCommand cmd = new OdbcCommand("SELECT 1 FROM DUMMY", conn))
                        {
                            object result = cmd.ExecuteScalar();
                            AppendOutput($"✓ Query executed successfully! Result: {result}");
                        }

                        success = true;
                        workingDriver = driverName;
                        break;
                    }
                }
                catch (OdbcException odbcEx)
                {
                    if (odbcEx.Errors[0].SQLState == "IM002")
                    {
                        AppendOutput($"  ✗ Driver '{driverName}' not found, trying next...");
                    }
                    else
                    {
                        AppendOutput($"✗ Connection failed with driver '{driverName}'!");
                        AppendOutput($"  Error: {odbcEx.Message}");
                        AppendOutput($"  SQLSTATE: {odbcEx.Errors[0].SQLState}");
                        AppendOutput("");

                        if (odbcEx.Errors[0].SQLState != "IM002")
                        {
                            // Non-driver error, stop trying
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    AppendOutput($"Error: {ex.Message}");
                }
            }

            AppendOutput("");
            if (success)
            {
                AppendOutput("=== SUCCESS ===");
                AppendOutput($"Working driver: {workingDriver}");
                AppendOutput("");
                AppendOutput("Update your App.config with:");
                AppendOutput($"  <add key=\"ODBCDriver\" value=\"{workingDriver}\" />");

                MessageBox.Show(
                    $"Connection successful!\n\nWorking driver: {workingDriver}\n\nUpdate your App.config to use this driver.",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            else
            {
                AppendOutput("=== FAILED ===");
                AppendOutput("Could not connect with any known SAP HANA driver.");
                AppendOutput("");
                AppendOutput("Please check:");
                AppendOutput("  1. SAP HANA ODBC driver (32-bit) is installed");
                AppendOutput("  2. Server address and port are correct");
                AppendOutput("  3. Database name is correct");
                AppendOutput("  4. Username and password are correct");
                AppendOutput("  5. Network connectivity to SAP HANA server");

                MessageBox.Show(
                    "Connection failed!\n\nCheck the output window for details.",
                    "Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void AppendOutput(string text)
        {
            txtOutput.AppendText(text + Environment.NewLine);
            txtOutput.SelectionStart = txtOutput.Text.Length;
            txtOutput.ScrollToCaret();
        }
    }
}

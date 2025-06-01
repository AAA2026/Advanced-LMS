using System;
using System.Windows.Forms;
using LibraryManagement.GUI;
using LibraryManagement.Services;

namespace LibraryManagement
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // Initialize database
                DatabaseService.InitializeDatabase();

                while (true)
                {
                    var welcome = new WelcomeForm();
                    if (welcome.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(welcome.SelectedRole))
                    {
                        var mainForm = new MainForm(welcome.SelectedRole, welcome.SelectedMemberId);
                        Application.Run(mainForm);

                        // If MainForm is closed via logout, loop and show WelcomeForm again
                        if (!mainForm.LoggedOut)
                            break; // Exit if not logging out, e.g., user closed the window
                    }
                    else
                    {
                        break; // Exit if user cancels at welcome screen
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing application: {ex.Message}\n\nPlease make sure MySQL server is running and the connection settings are correct.", 
                    "Database Connection Error", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
            }
        }
    }
} 
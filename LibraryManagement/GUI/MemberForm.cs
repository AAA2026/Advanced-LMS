using System;
using System.Windows.Forms;
using LibraryManagement.Models;
using LibraryManagement.Services;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;

namespace LibraryManagement.GUI
{
    public partial class MemberForm : Form
    {
        private readonly Member? _member;
        private TextBox txtName;
        private TextBox txtEmail;
        private TextBox txtAddress;
        private ListBox lstPhones;
        private Button btnAddPhone;
        private Button btnRemovePhone;
        private Button btnSave;
        private Button btnCancel;
        private TextBox txtNewPhone;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Member Member { get; private set; }

        public MemberForm(Member? member = null)
        {
            _member = member;
            Member = member ?? new Member();
            InitializeComponent();
            LoadMemberData();
        }

        private void InitializeComponent()
        {
            this.Text = _member == null ? "Add New Member" : "Edit Member";
            this.Size = new Size(400, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 6,
                Padding = new Padding(10)
            };

            // Add controls to main panel
            mainPanel.Controls.Add(CreateLabel("Name:"), 0, 0);
            txtName = CreateTextBox();
            mainPanel.Controls.Add(txtName, 1, 0);

            mainPanel.Controls.Add(CreateLabel("Email:"), 0, 1);
            txtEmail = CreateTextBox();
            mainPanel.Controls.Add(txtEmail, 1, 1);

            mainPanel.Controls.Add(CreateLabel("Address:"), 0, 2);
            txtAddress = CreateTextBox();
            mainPanel.Controls.Add(txtAddress, 1, 2);

            mainPanel.Controls.Add(CreateLabel("Phones:"), 0, 3);

            var phonePanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true
            };

            lstPhones = new ListBox
            {
                Width = 150,
                Height = 80,
                SelectionMode = SelectionMode.One
            };
            phonePanel.Controls.Add(lstPhones);

            var phoneButtonPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true
            };

            txtNewPhone = new TextBox { Width = 100 };
            btnAddPhone = new Button { Text = "Add Phone" };
            btnRemovePhone = new Button { Text = "Remove Selected" };

            phoneButtonPanel.Controls.Add(txtNewPhone);
            phoneButtonPanel.Controls.Add(btnAddPhone);
            phoneButtonPanel.Controls.Add(btnRemovePhone);

            phonePanel.Controls.Add(phoneButtonPanel);

            mainPanel.Controls.Add(phonePanel, 1, 3);

            // Button panel
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(10),
                AutoSize = true
            };

            btnSave = new Button { Text = "Save" };
            btnCancel = new Button { Text = "Cancel" };

            buttonPanel.Controls.Add(btnCancel);
            buttonPanel.Controls.Add(btnSave);

            this.Controls.Add(mainPanel);
            this.Controls.Add(buttonPanel);

            // Wire up events
            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            btnAddPhone.Click += BtnAddPhone_Click;
            btnRemovePhone.Click += BtnRemovePhone_Click;

            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }

        private Label CreateLabel(string text)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Anchor = AnchorStyles.Left,
                AutoSize = true
            };
        }

        private TextBox CreateTextBox()
        {
            return new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9)
            };
        }

        private void LoadMemberData()
        {
            if (_member != null)
            {
                txtName.Text = _member.Name;
                txtEmail.Text = _member.Email;
                txtAddress.Text = _member.Address;
                if (_member.MemberPhones != null)
                {
                    foreach (var phone in _member.MemberPhones)
                    {
                        lstPhones.Items.Add(phone.Phone);
                    }
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (ValidateInput())
            {
                Member.Name = txtName.Text.Trim();
                Member.Email = txtEmail.Text.Trim();
                Member.Address = txtAddress.Text.Trim();
                Member.MemberPhones = lstPhones.Items.Cast<string>().Select(p => new MemberPhone { MemberID = (int)Member.MemberID, Phone = p }).ToList();

                if (_member == null)
                {
                    DatabaseService.AddMember(Member);
                }
                else
                {
                    DatabaseService.UpdateMember(Member);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void BtnAddPhone_Click(object sender, EventArgs e)
        {
            var newPhone = txtNewPhone.Text.Trim();
            if (!string.IsNullOrWhiteSpace(newPhone) && !lstPhones.Items.Contains(newPhone))
            {
                lstPhones.Items.Add(newPhone);
                txtNewPhone.Clear();
            }
            else if (!string.IsNullOrWhiteSpace(newPhone) && lstPhones.Items.Contains(newPhone))
            {
                 MessageBox.Show("Phone number already exists.", "Duplicate Phone", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnRemovePhone_Click(object sender, EventArgs e)
        {
            if (lstPhones.SelectedItem != null)
            {
                lstPhones.Items.Remove(lstPhones.SelectedItem);
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtName.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Email is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtEmail.Focus();
                return false;
            }
            // Basic email format validation
            try
            {
                var addr = new System.Net.Mail.MailAddress(txtEmail.Text);
                if (addr.Address != txtEmail.Text)
                {
                     MessageBox.Show("Invalid email format.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtEmail.Focus();
                    return false;
                }
            }
            catch
            {
                 MessageBox.Show("Invalid email format.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtEmail.Focus();
                return false;
            }
             if (lstPhones.Items.Count == 0)
            {
                MessageBox.Show("At least one phone number is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtNewPhone.Focus();
                return false;
            }

            // Add duplicate email check
            if (DatabaseService.MemberExistsWithEmail(txtEmail.Text.Trim()))
            {
                // If editing, allow saving if the email belongs to the current member
                if (_member != null && _member.Email.Equals(txtEmail.Text.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    // Allow saving, email is the same as the original
                }
                else
                {
                    MessageBox.Show("A member with this email already exists.", "Duplicate Email", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtEmail.Focus();
                    return false;
                }
            }

            return true;
        }
    }
} 
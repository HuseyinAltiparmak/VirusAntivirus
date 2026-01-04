using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace VisurAntivirus
{
    public partial class SettingsForm : Form
    {
        private VirusScanner scanner;
        private CheckBox chkRealTimeProtection;
        private CheckBox chkHeuristicAnalysis;
        private ListBox lstSignatures;
        private TextBox txtNewSignature;
        private Button btnAddSignature;
        private Button btnRemoveSignature;
        private Button btnClose;
        private Label lblSignatures;
        private Label lblNewSignature;

        public SettingsForm(VirusScanner scanner)
        {
            this.scanner = scanner;
            InitializeComponent();
            LoadSettings();
            LoadSignatures();
        }

        private void InitializeComponent()
        {
            this.Text = "Ayarlar - Visur Antivirüs";
            this.Size = new Size(600, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Gerçek zamanlı koruma
            chkRealTimeProtection = new CheckBox();
            chkRealTimeProtection.Text = "Gerçek Zamanlı Koruma (Demo)";
            chkRealTimeProtection.Location = new Point(20, 20);
            chkRealTimeProtection.Size = new Size(250, 25);
            chkRealTimeProtection.Font = new Font("Microsoft Sans Serif", 10, FontStyle.Regular);
            chkRealTimeProtection.CheckedChanged += ChkRealTimeProtection_CheckedChanged;

            // Sezgisel analiz
            chkHeuristicAnalysis = new CheckBox();
            chkHeuristicAnalysis.Text = "Sezgisel Analiz";
            chkHeuristicAnalysis.Location = new Point(20, 60);
            chkHeuristicAnalysis.Size = new Size(200, 25);
            chkHeuristicAnalysis.Font = new Font("Microsoft Sans Serif", 10, FontStyle.Regular);
            chkHeuristicAnalysis.CheckedChanged += ChkHeuristicAnalysis_CheckedChanged;

            // İmza listesi
            lblSignatures = new Label();
            lblSignatures.Text = "Virüs İmzaları:";
            lblSignatures.Location = new Point(20, 110);
            lblSignatures.Size = new Size(150, 25);
            lblSignatures.Font = new Font("Microsoft Sans Serif", 10, FontStyle.Bold);

            lstSignatures = new ListBox();
            lstSignatures.Location = new Point(20, 140);
            lstSignatures.Size = new Size(550, 150);
            lstSignatures.Font = new Font("Microsoft Sans Serif", 9, FontStyle.Regular);

            // Yeni imza ekleme
            lblNewSignature = new Label();
            lblNewSignature.Text = "Yeni Virüs İmzası:";
            lblNewSignature.Location = new Point(20, 310);
            lblNewSignature.Size = new Size(150, 25);
            lblNewSignature.Font = new Font("Microsoft Sans Serif", 10, FontStyle.Bold);

            txtNewSignature = new TextBox();
            txtNewSignature.Location = new Point(20, 340);
            txtNewSignature.Size = new Size(300, 25);
            txtNewSignature.Font = new Font("Microsoft Sans Serif", 9, FontStyle.Regular);

            btnAddSignature = new Button();
            btnAddSignature.Text = "Ekle";
            btnAddSignature.Location = new Point(330, 340);
            btnAddSignature.Size = new Size(100, 25);
            btnAddSignature.Font = new Font("Microsoft Sans Serif", 9, FontStyle.Regular);
            btnAddSignature.Click += BtnAddSignature_Click;

            btnRemoveSignature = new Button();
            btnRemoveSignature.Text = "Seçileni Kaldır";
            btnRemoveSignature.Location = new Point(20, 380);
            btnRemoveSignature.Size = new Size(150, 30);
            btnRemoveSignature.Font = new Font("Microsoft Sans Serif", 9, FontStyle.Regular);
            btnRemoveSignature.Click += BtnRemoveSignature_Click;

            btnClose = new Button();
            btnClose.Text = "Kapat";
            btnClose.Location = new Point(470, 380);
            btnClose.Size = new Size(100, 30);
            btnClose.Font = new Font("Microsoft Sans Serif", 9, FontStyle.Regular);
            btnClose.Click += BtnClose_Click;

            // Kontrolleri forma ekle
            this.Controls.Add(chkRealTimeProtection);
            this.Controls.Add(chkHeuristicAnalysis);
            this.Controls.Add(lblSignatures);
            this.Controls.Add(lstSignatures);
            this.Controls.Add(lblNewSignature);
            this.Controls.Add(txtNewSignature);
            this.Controls.Add(btnAddSignature);
            this.Controls.Add(btnRemoveSignature);
            this.Controls.Add(btnClose);
        }

        private void LoadSettings()
        {
            chkRealTimeProtection.Checked = scanner.RealTimeProtectionEnabled;
            chkHeuristicAnalysis.Checked = scanner.HeuristicAnalysisEnabled;
        }

        private void LoadSignatures()
        {
            lstSignatures.Items.Clear();
            var signatures = scanner.GetVirusSignatures();
            foreach (var signature in signatures)
            {
                lstSignatures.Items.Add(signature);
            }
        }

        private void ChkRealTimeProtection_CheckedChanged(object sender, EventArgs e)
        {
            scanner.RealTimeProtectionEnabled = chkRealTimeProtection.Checked;
            MessageBox.Show("Gerçek zamanlı koruma " + (chkRealTimeProtection.Checked ? "açıldı" : "kapatıldı"),
                "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ChkHeuristicAnalysis_CheckedChanged(object sender, EventArgs e)
        {
            scanner.HeuristicAnalysisEnabled = chkHeuristicAnalysis.Checked;
        }

        private void BtnAddSignature_Click(object sender, EventArgs e)
        {
            string signature = txtNewSignature.Text.Trim();
            if (!string.IsNullOrEmpty(signature))
            {
                scanner.AddVirusSignature(signature);
                LoadSignatures();
                txtNewSignature.Clear();
                txtNewSignature.Focus();
                MessageBox.Show($"'{signature}' imzası başarıyla eklendi.", "Başarılı",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Lütfen bir virüs imzası girin.", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnRemoveSignature_Click(object sender, EventArgs e)
        {
            if (lstSignatures.SelectedIndex != -1)
            {
                string signature = lstSignatures.SelectedItem.ToString();
                DialogResult result = MessageBox.Show($"'{signature}' imzasını kaldırmak istediğinizden emin misiniz?",
                    "İmza Kaldırma Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    scanner.RemoveVirusSignature(signature);
                    LoadSignatures();
                    MessageBox.Show("İmza başarıyla kaldırıldı.", "Başarılı",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("Lütfen kaldırılacak bir imza seçin.", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
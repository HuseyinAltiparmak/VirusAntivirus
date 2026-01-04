using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VisurAntivirus
{
    public partial class Form1 : Form
    {
        private VirusScanner scanner;
        private QuarantineManager quarantineManager;
        private Button btnQuickScan;
        private Button btnFullScan;
        private Button btnQuarantine;
        private Button btnSettings;
        private ListView lvScanResults;
        private ProgressBar progressBar1;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel lblStatus;

        public Form1()
        {
            InitializeComponent();
            scanner = new VirusScanner();
            quarantineManager = new QuarantineManager();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            // ListView sütunlarýný oluþtur
            lvScanResults.Columns.Add("Dosya", 300);
            lvScanResults.Columns.Add("Durum", 150);
            lvScanResults.Columns.Add("Tehdit", 150);
            lvScanResults.Columns.Add("Boyut", 100);

            // Güncelleme kontrolü
            CheckForUpdates();
        }

        private void InitializeComponent()
        {
            this.btnQuickScan = new System.Windows.Forms.Button();
            this.btnFullScan = new System.Windows.Forms.Button();
            this.btnQuarantine = new System.Windows.Forms.Button();
            this.btnSettings = new System.Windows.Forms.Button();
            this.lvScanResults = new System.Windows.Forms.ListView();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();

            // btnQuickScan
            this.btnQuickScan.Location = new System.Drawing.Point(12, 12);
            this.btnQuickScan.Name = "btnQuickScan";
            this.btnQuickScan.Size = new System.Drawing.Size(120, 40);
            this.btnQuickScan.TabIndex = 0;
            this.btnQuickScan.Text = "Hýzlý Tarama";
            this.btnQuickScan.UseVisualStyleBackColor = true;
            this.btnQuickScan.Click += new System.EventHandler(this.btnQuickScan_Click);

            // btnFullScan
            this.btnFullScan.Location = new System.Drawing.Point(138, 12);
            this.btnFullScan.Name = "btnFullScan";
            this.btnFullScan.Size = new System.Drawing.Size(120, 40);
            this.btnFullScan.TabIndex = 1;
            this.btnFullScan.Text = "Tam Tarama";
            this.btnFullScan.UseVisualStyleBackColor = true;
            this.btnFullScan.Click += new System.EventHandler(this.btnFullScan_Click);

            // btnQuarantine
            this.btnQuarantine.Location = new System.Drawing.Point(264, 12);
            this.btnQuarantine.Name = "btnQuarantine";
            this.btnQuarantine.Size = new System.Drawing.Size(120, 40);
            this.btnQuarantine.TabIndex = 2;
            this.btnQuarantine.Text = "Karantina";
            this.btnQuarantine.UseVisualStyleBackColor = true;
            this.btnQuarantine.Click += new System.EventHandler(this.btnQuarantine_Click);

            // btnSettings
            this.btnSettings.Location = new System.Drawing.Point(390, 12);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(120, 40);
            this.btnSettings.TabIndex = 3;
            this.btnSettings.Text = "Ayarlar";
            this.btnSettings.UseVisualStyleBackColor = true;
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);

            // lvScanResults
            this.lvScanResults.HideSelection = false;
            this.lvScanResults.Location = new System.Drawing.Point(12, 70);
            this.lvScanResults.Name = "lvScanResults";
            this.lvScanResults.Size = new System.Drawing.Size(776, 340);
            this.lvScanResults.TabIndex = 4;
            this.lvScanResults.UseCompatibleStateImageBehavior = false;
            this.lvScanResults.View = System.Windows.Forms.View.Details;

            // progressBar1
            this.progressBar1.Location = new System.Drawing.Point(12, 420);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(776, 23);
            this.progressBar1.TabIndex = 5;

            // statusStrip1
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 450);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(800, 22);
            this.statusStrip1.TabIndex = 6;
            this.statusStrip1.Text = "statusStrip1";

            // lblStatus
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(164, 17);
            this.lblStatus.Text = "Visur Antivirüs - Hazýr";

            // Form1
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 472);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.lvScanResults);
            this.Controls.Add(this.btnSettings);
            this.Controls.Add(this.btnQuarantine);
            this.Controls.Add(this.btnFullScan);
            this.Controls.Add(this.btnQuickScan);
            this.Name = "Form1";
            this.Text = "Visur Antivirüs";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void CheckForUpdates()
        {
            lblStatus.Text = "Veritabaný güncel: 2024.01.15";
        }

        private async void btnQuickScan_Click(object sender, EventArgs e)
        {
            await StartScan(ScanType.Quick);
        }

        private async void btnFullScan_Click(object sender, EventArgs e)
        {
            await StartScan(ScanType.Full);
        }

        private async Task StartScan(ScanType scanType)
        {
            lvScanResults.Items.Clear();
            progressBar1.Value = 0;
            btnQuickScan.Enabled = false;
            btnFullScan.Enabled = false;

            try
            {
                ScanResult result = await Task.Run(() => scanner.PerformScan(scanType));

                // Sonuçlarý listeye ekle
                foreach (var file in result.ScannedFiles)
                {
                    ListViewItem item = new ListViewItem(file.FilePath);
                    item.SubItems.Add(file.IsInfected ? "Enfekte" : "Temiz");
                    item.SubItems.Add(file.ThreatName);
                    item.SubItems.Add(FormatFileSize(file.FileSize));
                    item.ForeColor = file.IsInfected ? Color.Red : Color.Green;
                    lvScanResults.Items.Add(item);
                }

                // Ýstatistikleri göster
                lblStatus.Text = $"Tarama tamamlandý: {result.TotalFiles} dosya taraný, {result.InfectedFiles} tehdit bulundu";

                // Tehdit bulunduysa uyar
                if (result.InfectedFiles > 0)
                {
                    DialogResult dialog = MessageBox.Show(
                        $"{result.InfectedFiles} tehdit bulundu! Karantinaya alýnsýn mý?",
                        "Tehdit Tespit Edildi",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (dialog == DialogResult.Yes)
                    {
                        MoveToQuarantine(result.ScannedFiles.Where(f => f.IsInfected).ToList());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Tarama hatasý: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnQuickScan.Enabled = true;
                btnFullScan.Enabled = true;
                progressBar1.Value = 100;
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private void MoveToQuarantine(List<ScannedFile> infectedFiles)
        {
            int movedCount = 0;
            foreach (var file in infectedFiles)
            {
                if (quarantineManager.AddToQuarantine(file.FilePath, file.ThreatName))
                    movedCount++;
            }
            MessageBox.Show($"{movedCount} dosya karantinaya alýndý.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnQuarantine_Click(object sender, EventArgs e)
        {
            QuarantineForm quarantineForm = new QuarantineForm(quarantineManager);
            quarantineForm.ShowDialog();
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            SettingsForm settingsForm = new SettingsForm(scanner);
            settingsForm.ShowDialog();
        }
    }

    public enum ScanType
    {
        Quick,
        Full
    }
}
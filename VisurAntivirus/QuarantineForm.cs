using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace VisurAntivirus
{
    public partial class QuarantineForm : Form
    {
        private QuarantineManager quarantineManager;
        private ListView lvQuarantine;
        private Button btnRestore;
        private Button btnDelete;
        private Button btnClearAll;
        private Button btnClose;

        public QuarantineForm(QuarantineManager manager)
        {
            quarantineManager = manager;
            InitializeComponent();
            LoadQuarantinedFiles();
        }

        private void InitializeComponent()
        {
            this.Text = "Karantina Yöneticisi - Visur Antivirüs";
            this.Size = new Size(850, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // ListView oluştur
            lvQuarantine = new ListView();
            lvQuarantine.View = View.Details;
            lvQuarantine.FullRowSelect = true;
            lvQuarantine.GridLines = true;
            lvQuarantine.Location = new Point(12, 12);
            lvQuarantine.Size = new Size(810, 350);
            lvQuarantine.MultiSelect = false;

            // Sütunlar
            lvQuarantine.Columns.Add("Orijinal Konum", 300);
            lvQuarantine.Columns.Add("Tehdit Adı", 150);
            lvQuarantine.Columns.Add("Karantina Tarihi", 150);
            lvQuarantine.Columns.Add("Karantina Dosyası", 200);

            // Butonlar
            btnRestore = new Button();
            btnRestore.Text = "Seçileni Geri Yükle";
            btnRestore.Location = new Point(12, 375);
            btnRestore.Size = new Size(150, 35);
            btnRestore.Font = new Font("Microsoft Sans Serif", 9, FontStyle.Regular);
            btnRestore.Click += BtnRestore_Click;

            btnDelete = new Button();
            btnDelete.Text = "Seçileni Sil";
            btnDelete.Location = new Point(172, 375);
            btnDelete.Size = new Size(150, 35);
            btnDelete.Font = new Font("Microsoft Sans Serif", 9, FontStyle.Regular);
            btnDelete.Click += BtnDelete_Click;

            btnClearAll = new Button();
            btnClearAll.Text = "Tümünü Temizle";
            btnClearAll.Location = new Point(332, 375);
            btnClearAll.Size = new Size(150, 35);
            btnClearAll.Font = new Font("Microsoft Sans Serif", 9, FontStyle.Regular);
            btnClearAll.Click += BtnClearAll_Click;

            btnClose = new Button();
            btnClose.Text = "Kapat";
            btnClose.Location = new Point(672, 375);
            btnClose.Size = new Size(150, 35);
            btnClose.Font = new Font("Microsoft Sans Serif", 9, FontStyle.Regular);
            btnClose.Click += BtnClose_Click;

            // Kontrolleri forma ekle
            this.Controls.Add(lvQuarantine);
            this.Controls.Add(btnRestore);
            this.Controls.Add(btnDelete);
            this.Controls.Add(btnClearAll);
            this.Controls.Add(btnClose);
        }

        private void LoadQuarantinedFiles()
        {
            lvQuarantine.Items.Clear();
            var files = quarantineManager.GetQuarantinedFiles();

            foreach (var file in files)
            {
                ListViewItem item = new ListViewItem(file.OriginalPath);
                item.SubItems.Add(file.ThreatName);
                item.SubItems.Add(file.DateQuarantined.ToString("yyyy-MM-dd HH:mm"));
                item.SubItems.Add(Path.GetFileName(file.QuarantinePath));
                item.Tag = file.QuarantinePath;
                lvQuarantine.Items.Add(item);
            }

            if (lvQuarantine.Items.Count == 0)
            {
                ListViewItem item = new ListViewItem("Karantinada dosya bulunmuyor");
                item.ForeColor = Color.Gray;
                lvQuarantine.Items.Add(item);
            }
        }

        private void BtnRestore_Click(object sender, EventArgs e)
        {
            if (lvQuarantine.SelectedItems.Count == 0 || lvQuarantine.Items[0].Text == "Karantinada dosya bulunmuyor")
            {
                MessageBox.Show("Lütfen geri yüklenecek bir dosya seçin.", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string quarantinePath = lvQuarantine.SelectedItems[0].Tag.ToString();
            string fileName = Path.GetFileName(quarantinePath);

            DialogResult result = MessageBox.Show(
                $"'{fileName}' dosyasını geri yüklemek istediğinizden emin misiniz?\nDosya orijinal konumuna taşınacaktır.",
                "Geri Yükleme Onayı",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                if (quarantineManager.RestoreFromQuarantine(quarantinePath))
                {
                    MessageBox.Show("Dosya başarıyla geri yüklendi.", "Başarılı",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadQuarantinedFiles();
                }
                else
                {
                    MessageBox.Show("Dosya geri yüklenirken hata oluştu.", "Hata",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (lvQuarantine.SelectedItems.Count == 0 || lvQuarantine.Items[0].Text == "Karantinada dosya bulunmuyor")
            {
                MessageBox.Show("Lütfen silinecek bir dosya seçin.", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string quarantinePath = lvQuarantine.SelectedItems[0].Tag.ToString();
            string fileName = Path.GetFileName(quarantinePath);

            DialogResult result = MessageBox.Show(
                $"'{fileName}' dosyasını KALICI olarak silmek istediğinizden emin misiniz?\nBu işlem geri alınamaz!",
                "Silme Onayı",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                if (quarantineManager.DeleteFromQuarantine(quarantinePath))
                {
                    MessageBox.Show("Dosya başarıyla silindi.", "Başarılı",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadQuarantinedFiles();
                }
                else
                {
                    MessageBox.Show("Dosya silinirken hata oluştu.", "Hata",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnClearAll_Click(object sender, EventArgs e)
        {
            if (lvQuarantine.Items.Count == 0 || lvQuarantine.Items[0].Text == "Karantinada dosya bulunmuyor")
            {
                return;
            }

            DialogResult result = MessageBox.Show(
                "Tüm karantina dosyalarını KALICI olarak silmek istediğinizden emin misiniz?\nBu işlem geri alınamaz!",
                "Tümünü Temizleme Onayı",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                quarantineManager.ClearQuarantine();
                LoadQuarantinedFiles();
                MessageBox.Show("Karantina temizlendi.", "Başarılı",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
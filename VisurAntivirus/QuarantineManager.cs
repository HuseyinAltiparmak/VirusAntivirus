using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VisurAntivirus
{
    public class QuarantineManager
    {
        private string quarantineFolder;
        private List<QuarantinedFile> quarantinedFiles;

        public QuarantineManager()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            quarantineFolder = Path.Combine(appData, "VisurAntivirus", "Quarantine");
            quarantinedFiles = new List<QuarantinedFile>();

            if (!Directory.Exists(quarantineFolder))
            {
                Directory.CreateDirectory(quarantineFolder);
            }

            LoadQuarantinedFiles();
        }

        private void LoadQuarantinedFiles()
        {
            quarantinedFiles.Clear();
            string infoFile = Path.Combine(quarantineFolder, "quarantine.info");

            if (File.Exists(infoFile))
            {
                try
                {
                    string[] lines = File.ReadAllLines(infoFile);
                    foreach (string line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line))
                            continue;

                        string[] parts = line.Split('|');
                        if (parts.Length >= 4)
                        {
                            quarantinedFiles.Add(new QuarantinedFile
                            {
                                OriginalPath = parts[0],
                                QuarantinePath = parts[1],
                                ThreatName = parts[2],
                                DateQuarantined = DateTime.Parse(parts[3])
                            });
                        }
                    }
                }
                catch
                {
                    // Dosya bozulmuş olabilir
                }
            }
        }

        private void SaveQuarantinedFiles()
        {
            string infoFile = Path.Combine(quarantineFolder, "quarantine.info");
            List<string> lines = new List<string>();

            foreach (var file in quarantinedFiles)
            {
                lines.Add($"{file.OriginalPath}|{file.QuarantinePath}|{file.ThreatName}|{file.DateQuarantined:yyyy-MM-dd HH:mm:ss}");
            }

            File.WriteAllLines(infoFile, lines);
        }

        public bool AddToQuarantine(string filePath, string threatName)
        {
            try
            {
                if (!File.Exists(filePath))
                    return false;

                string fileName = Path.GetFileName(filePath);
                string quarantineFileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString().Substring(0, 8)}_{fileName}";
                string quarantinePath = Path.Combine(quarantineFolder, quarantineFileName);

                // Dosyayı karantinaya taşı
                File.Move(filePath, quarantinePath);

                // Bilgileri kaydet
                quarantinedFiles.Add(new QuarantinedFile
                {
                    OriginalPath = filePath,
                    QuarantinePath = quarantinePath,
                    ThreatName = threatName,
                    DateQuarantined = DateTime.Now
                });

                SaveQuarantinedFiles();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Karantina hatası: {ex.Message}");
                return false;
            }
        }

        public bool RestoreFromQuarantine(string quarantinePath, string newLocation = null)
        {
            try
            {
                var file = quarantinedFiles.FirstOrDefault(f => f.QuarantinePath == quarantinePath);
                if (file == null || !File.Exists(quarantinePath))
                    return false;

                string restorePath = newLocation ?? file.OriginalPath;

                // Hedef klasör yoksa oluştur
                string directory = Path.GetDirectoryName(restorePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Orijinal konuma geri taşı
                File.Move(quarantinePath, restorePath);

                // Listeden kaldır
                quarantinedFiles.Remove(file);
                SaveQuarantinedFiles();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Geri yükleme hatası: {ex.Message}");
                return false;
            }
        }

        public bool DeleteFromQuarantine(string quarantinePath)
        {
            try
            {
                var file = quarantinedFiles.FirstOrDefault(f => f.QuarantinePath == quarantinePath);
                if (file == null)
                    return false;

                // Dosyayı kalıcı olarak sil
                if (File.Exists(quarantinePath))
                {
                    File.Delete(quarantinePath);
                }

                // Listeden kaldır
                quarantinedFiles.Remove(file);
                SaveQuarantinedFiles();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public List<QuarantinedFile> GetQuarantinedFiles()
        {
            return new List<QuarantinedFile>(quarantinedFiles);
        }

        public void ClearQuarantine()
        {
            foreach (var file in quarantinedFiles.ToList())
            {
                try
                {
                    if (File.Exists(file.QuarantinePath))
                    {
                        File.Delete(file.QuarantinePath);
                    }
                }
                catch { }
            }

            quarantinedFiles.Clear();
            SaveQuarantinedFiles();
        }
    }

    public class QuarantinedFile
    {
        public string OriginalPath { get; set; }
        public string QuarantinePath { get; set; }
        public string ThreatName { get; set; }
        public DateTime DateQuarantined { get; set; }
    }
}
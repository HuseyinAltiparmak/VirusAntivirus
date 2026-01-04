using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VisurAntivirus
{
    public class VirusScanner
    {
        // Virüs imzaları
        private List<string> virusSignatures = new List<string>
        {
            "malware.exe", "virus.sys", "trojan.dll", "ransomware",
            "X5O!P%@AP[4\\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*"
        };

        private List<string> suspiciousExtensions = new List<string>
        {
            ".exe", ".dll", ".sys", ".bat", ".cmd", ".vbs", ".js", ".ps1"
        };

        public bool RealTimeProtectionEnabled { get; set; } = true;
        public bool HeuristicAnalysisEnabled { get; set; } = true;

        public ScanResult PerformScan(ScanType scanType)
        {
            ScanResult result = new ScanResult();
            List<string> filesToScan = new List<string>();

            // Hızlı tarama: sistem klasörleri
            if (scanType == ScanType.Quick)
            {
                string systemRoot = Environment.GetFolderPath(Environment.SpecialFolder.System);
                string windowsDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);

                if (Directory.Exists(systemRoot))
                {
                    filesToScan.AddRange(Directory.GetFiles(systemRoot, "*.exe", SearchOption.TopDirectoryOnly).Take(10));
                    filesToScan.AddRange(Directory.GetFiles(systemRoot, "*.dll", SearchOption.TopDirectoryOnly).Take(10));
                }

                // Temp klasöründeki dosyaları da kontrol et
                string tempPath = Path.GetTempPath();
                if (Directory.Exists(tempPath))
                {
                    filesToScan.AddRange(Directory.GetFiles(tempPath, "*.exe", SearchOption.TopDirectoryOnly).Take(5));
                }
            }
            // Tam tarama: daha fazla klasör
            else if (scanType == ScanType.Full)
            {
                // Masaüstü
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                if (Directory.Exists(desktop))
                {
                    filesToScan.AddRange(Directory.GetFiles(desktop, "*.*", SearchOption.TopDirectoryOnly)
                        .Where(f => suspiciousExtensions.Contains(Path.GetExtension(f).ToLower())));
                }

                // Belgelerim
                string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (Directory.Exists(documents))
                {
                    filesToScan.AddRange(Directory.GetFiles(documents, "*.*", SearchOption.TopDirectoryOnly)
                        .Where(f => suspiciousExtensions.Contains(Path.GetExtension(f).ToLower()))
                        .Take(20));
                }

                // Downloads
                string downloads = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                if (Directory.Exists(downloads))
                {
                    filesToScan.AddRange(Directory.GetFiles(downloads, "*.*", SearchOption.TopDirectoryOnly)
                        .Where(f => suspiciousExtensions.Contains(Path.GetExtension(f).ToLower()))
                        .Take(20));
                }
            }

            // Dosyaları tara
            foreach (string filePath in filesToScan)
            {
                try
                {
                    ScannedFile scannedFile = ScanFile(filePath);
                    result.ScannedFiles.Add(scannedFile);

                    if (scannedFile.IsInfected)
                        result.InfectedFiles++;
                }
                catch
                {
                    continue;
                }
            }

            result.TotalFiles = filesToScan.Count;
            return result;
        }

        private ScannedFile ScanFile(string filePath)
        {
            ScannedFile result = new ScannedFile
            {
                FilePath = filePath,
                IsInfected = false,
                ThreatName = "Temiz",
                FileSize = 0
            };

            try
            {
                FileInfo fileInfo = new FileInfo(filePath);
                if (!fileInfo.Exists)
                    return result;

                result.FileSize = fileInfo.Length;

                // 1. Dosya adı kontrolü
                string fileName = Path.GetFileName(filePath).ToLower();
                foreach (string signature in virusSignatures)
                {
                    if (fileName.Contains(signature.ToLower()))
                    {
                        result.IsInfected = true;
                        result.ThreatName = $"Virus.{signature}";
                        return result;
                    }
                }

                // 2. EICAR test dosyası kontrolü
                if (fileInfo.Length < 1024 * 1024) // 1MB'den küçükse
                {
                    try
                    {
                        string content = File.ReadAllText(filePath);
                        if (content.Contains("X5O!P%@AP[4\\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*"))
                        {
                            result.IsInfected = true;
                            result.ThreatName = "EICAR-Test-File";
                            return result;
                        }
                    }
                    catch
                    {
                        // Binary dosya, devam et
                    }
                }

                // 3. Sezgisel analiz
                if (HeuristicAnalysisEnabled)
                {
                    string extension = Path.GetExtension(filePath).ToLower();

                    // Çift uzantı kontrolü
                    if (fileName.Contains("..") || (fileName.Count(c => c == '.') > 1 && extension == ".exe"))
                    {
                        result.IsInfected = true;
                        result.ThreatName = "Heuristic.DoubleExtension";
                        return result;
                    }

                    // Temp klasöründeki executable'lar
                    string tempPath = Path.GetTempPath().ToLower();
                    if (filePath.ToLower().StartsWith(tempPath) && (extension == ".exe" || extension == ".bat" || extension == ".cmd"))
                    {
                        result.IsInfected = true;
                        result.ThreatName = "Heuristic.TempExecutable";
                        return result;
                    }

                    // Çok küçük .exe dosyaları
                    if (extension == ".exe" && fileInfo.Length < 10240) // 10KB'den küçük
                    {
                        result.IsInfected = true;
                        result.ThreatName = "Heuristic.TinyExecutable";
                        return result;
                    }
                }
            }
            catch
            {
                result.IsInfected = true;
                result.ThreatName = "Error.ScanFailed";
            }

            return result;
        }

        public List<string> GetVirusSignatures()
        {
            return new List<string>(virusSignatures);
        }

        public void AddVirusSignature(string signature)
        {
            if (!string.IsNullOrWhiteSpace(signature) && !virusSignatures.Contains(signature))
                virusSignatures.Add(signature);
        }

        public void RemoveVirusSignature(string signature)
        {
            virusSignatures.Remove(signature);
        }
    }

    public class ScanResult
    {
        public List<ScannedFile> ScannedFiles { get; set; } = new List<ScannedFile>();
        public int TotalFiles { get; set; }
        public int InfectedFiles { get; set; }
    }

    public class ScannedFile
    {
        public string FilePath { get; set; }
        public bool IsInfected { get; set; }
        public string ThreatName { get; set; }
        public long FileSize { get; set; }
    }
}
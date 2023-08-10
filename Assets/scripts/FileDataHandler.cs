using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class FileDataHandler {
    private readonly string dataDirPath;
    private readonly string dataFileName;

    public FileDataHandler(string dataDirPath, string dataFileName) {
        this.dataDirPath = dataDirPath;
        this.dataFileName = dataFileName;
    }
    public PanparuData LoadGame() {
        string filePath = Path.Combine(dataDirPath, dataFileName);
        PanparuData panparuData;
        if (File.Exists(filePath))
        {
            try {
                using(FileStream fs = File.Open(filePath, FileMode.Open)) {
                    using(StreamReader reader = new StreamReader(fs)) {
                        string json = reader.ReadToEnd();
                        panparuData = JsonUtility.FromJson<PanparuData>(json);
                    }
                }
            }
            catch (Exception e) {
                Debug.LogError("Error loading game data: " + e.Message);
                panparuData = null;
            }
        }
        else
        {
            panparuData = null;
        }
        return panparuData;
    }
    public void SaveGame(PanparuData panparuData) {
        string filePath = Path.Combine(dataDirPath, dataFileName);
        try {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            string json = JsonUtility.ToJson(panparuData, true);
            using(FileStream fs = File.Open(filePath, FileMode.Create)) {
                using(StreamWriter writer = new StreamWriter(fs)) {
                    writer.Write(json);
                }
            }
        }
        catch (Exception e) {
            Debug.LogError("Error saving game data: " + e.Message);
        }
    }
}

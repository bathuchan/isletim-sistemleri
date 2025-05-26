using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

public class ConstructionVisualizer : MonoBehaviour
{
    public GameObject floorPrefab;  // Kat prefabı
    public GameObject roomPrefab;   // Daire prefabı

    private List<string> logLines = new List<string>();

    private Dictionary<int, GameObject> katlar = new Dictionary<int, GameObject>();
    private Dictionary<(int kat, int daire), GameObject> daireler = new Dictionary<(int, int), GameObject>();

    IEnumerator Start()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "insaat_log.txt");
        if (File.Exists(path))
        {
            logLines.AddRange(File.ReadAllLines(path));
            yield return VisualizeConstruction();
        }
        else
        {
            Debug.LogError("Log dosyası bulunamadı: " + path);
        }
    }

    IEnumerator VisualizeConstruction()
    {
        foreach (string line in logLines)
        {
            yield return ProcessLogLine(line);
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator ProcessLogLine(string logLine)
    {
        if (string.IsNullOrWhiteSpace(logLine))
            yield break;

        // Kat numarasını al
        var katMatch = Regex.Match(logLine, @"\[Kat (\d+)\]");
        if (!katMatch.Success)
        {
            Debug.LogWarning("Kat numarası yok veya yanlış format: " + logLine);
            yield break;
        }
        int kat = int.Parse(katMatch.Groups[1].Value);

        // Daire numarasını al (opsiyonel)
        var daireMatch = Regex.Match(logLine, @"Daire (\d+)");
        int daire = daireMatch.Success ? int.Parse(daireMatch.Groups[1].Value) : -1;

        if (logLine.Contains("Kat inşaatı başladı"))
        {
            CreateKat(kat);
            yield break;
        }

        if (logLine.Contains("Kat inşaatı tamamlandı"))
        {
            // İstersen kat objesine efekt veya renk değişikliği yapabilirsin
            Debug.Log($"Kat {kat} inşaatı tamamlandı.");
            yield break;
        }

        if (daire >= 1)
        {
            if (logLine.Contains("Tesisat işlemi başladı"))
                yield return ShowDaireProcess(kat, daire, Color.yellow, "Tesisat Başladı");
            else if (logLine.Contains("Tesisat işlemi tamamlandı"))
                yield return ShowDaireProcess(kat, daire, Color.green, "Tesisat Tamamlandı");
            else if (logLine.Contains("İç tasarım başladı"))
                yield return ShowDaireProcess(kat, daire, Color.cyan, "İç Tasarım Başladı");
            else if (logLine.Contains("İnşaat tamamlandı"))
                yield return ShowDaireProcess(kat, daire, Color.magenta, "İnşaat Tamamlandı");
        }
    }

    void CreateKat(int kat)
    {
        if (katlar.ContainsKey(kat))
        {
            Debug.Log($"Kat {kat} zaten var.");
            return;
        }

        Vector3 katPos = new Vector3(0, kat * 3f, 0);
        GameObject katObj = Instantiate(floorPrefab, katPos, Quaternion.identity);
        katObj.name = "Kat_" + kat;
        katlar[kat] = katObj;

        Debug.Log($"Kat {kat} oluşturuldu.");

        int totalDaireSayisi = 4;

        BoxCollider box = katObj.GetComponent<BoxCollider>();
        if (box == null)
        {
            Debug.LogWarning("Kat prefabı BoxCollider içermiyor!");
            return;
        }
        else
        {
            Debug.Log("BoxCollider bulundu. Genişlik: " + box.size.x);
        }

        float floorWidth = box.size.x * katObj.transform.localScale.x;

        float daireSpacing = floorWidth / totalDaireSayisi;
        float startX = -floorWidth / 2 + daireSpacing / 2;

        for (int i = 1; i <= totalDaireSayisi; i++)
        {
            Vector3 daireLocalPos = new Vector3(startX + (i - 1) * daireSpacing, 0.5f, 0);
            GameObject daireObj = Instantiate(roomPrefab, katObj.transform);
            daireObj.transform.localPosition = daireLocalPos;
            daireObj.name = $"Kat{kat}_Daire{i}";

            Debug.Log($"Daire oluşturuldu: Kat {kat} Daire {i}");

            daireObj.SetActive(true);

            daireler[(kat, i)] = daireObj;
        }
    }

    IEnumerator ShowDaireProcess(int kat, int daire, Color targetColor, string processName)
    {
        if (!daireler.TryGetValue((kat, daire), out GameObject daireObj))
        {
            Debug.LogWarning($"Daire bulunamadı: Kat {kat}, Daire {daire}");
            yield break;
        }

        Renderer rend = daireObj.GetComponent<Renderer>();
        if (rend == null)
        {
            Debug.LogWarning("Daire objesinde Renderer bulunamadı!");
            yield break;
        }

        Debug.Log($"Kat {kat} Daire {daire} - {processName}");

        Color startColor = rend.material.color;
        float duration = 1f;
        float elapsed = 0f;

        // Renk geçişi (start -> hedef)
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            rend.material.color = Color.Lerp(startColor, targetColor, elapsed / duration);
            yield return null;
        }

        // Hedef renk 1 saniye kalacak
        yield return new WaitForSeconds(1f);

        // Renk geri dönüş (hedef -> başlangıç)
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            rend.material.color = Color.Lerp(targetColor, startColor, elapsed / duration);
            yield return null;
        }
    }
}

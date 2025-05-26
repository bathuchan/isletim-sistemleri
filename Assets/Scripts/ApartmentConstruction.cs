using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApartmentConstruction : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject floorPrefab;
    public GameObject roomPrefab;

    [Header("Ayarlar")]
    public int maxKat = 10;
    public int daireSayisi = 4;

    private Dictionary<int, GameObject> katlar = new();
    private Dictionary<(int kat, int daire), GameObject> daireler = new();
    private Dictionary<(int kat, int daire), bool> daireTamamlandi = new();

    private readonly Color renkHazirlaniyor = Color.gray;
    private readonly Color renkTesisat = Color.yellow;
    private readonly Color renkIceriTasarim = Color.blue;
    private readonly Color renkBitis = Color.green;

    private void Start()
    {
        StartCoroutine(VisualizeConstruction());
    }

    IEnumerator VisualizeConstruction()
    {
        for (int kat = 1; kat <= maxKat; kat++)
        {
            yield return StartCoroutine(CreateKat(kat));

            Debug.Log($"Kat {kat} kaba inşaat başladı.");
            yield return new WaitForSeconds(1f);
            Debug.Log($"Kat {kat} kaba inşaat tamamlandı.");

            StartDaireIslemleri(kat);
        }

        Debug.Log("Tüm katların kaba inşaatı ve daire işlemleri başlatıldı.");
    }

    void StartDaireIslemleri(int kat)
    {
        List<int> daireListesi = new();
        for (int i = 1; i <= daireSayisi; i++) daireListesi.Add(i);
        ShuffleList(daireListesi);

        foreach (int daire in daireListesi)
        {
            StartCoroutine(ProcessDaire(kat, daire));
        }
    }

    void ShuffleList(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    IEnumerator ProcessDaire(int kat, int daire)
    {
        daireTamamlandi[(kat, daire)] = false;

        if (!daireler.TryGetValue((kat, daire), out GameObject daireObj) || daireObj == null)
        {
            Debug.LogWarning($"Daire bulunamadı: Kat {kat}, Daire {daire}");
            yield break;
        }

        Renderer renderer = daireObj.GetComponent<Renderer>();
        SetDaireRengi(renderer, renkHazirlaniyor);

        yield return new WaitForSeconds(Random.Range(0.1f, 0.6f));

        // 🔧 Tesisat işlemi (artık paralel)
        float tesisatSuresi = Random.Range(0.5f, 1.5f);
        Debug.Log($"Kat {kat} Daire {daire} tesisat başladı.");
        SetDaireRengi(renderer, renkTesisat);
        yield return new WaitForSeconds(tesisatSuresi);
        Debug.Log($"Kat {kat} Daire {daire} tesisat tamamlandı.");

        // 🏠 İç tasarım
        Debug.Log($"Kat {kat} Daire {daire}: İç tasarım başladı.");
        SetDaireRengi(renderer, renkIceriTasarim);
        yield return new WaitForSeconds(Random.Range(0.3f, 1.0f));
        Debug.Log($"Kat {kat} Daire {daire}: İç tasarım tamamlandı.");

        SetDaireRengi(renderer, renkBitis);
        Debug.Log($"Kat {kat} Daire {daire} işlemleri tamamlandı.");

        daireTamamlandi[(kat, daire)] = true;
    }

    IEnumerator CreateKat(int kat)
    {
        if (katlar.ContainsKey(kat)) yield break;

        Vector3 katPos = new(0, kat * 3f, 0);
        GameObject katObj = Instantiate(floorPrefab, katPos, Quaternion.identity);
        Renderer rend = katObj.GetComponent<Renderer>();
        SetDaireRengi(rend, renkHazirlaniyor);
        katObj.name = $"Kat_{kat}";
        katlar[kat] = katObj;

        yield return new WaitForSeconds(Random.Range(0.3f, 1.0f));

        float floorWidth = 10f;
        float daireSpacing = floorWidth / daireSayisi;
        float startX = -floorWidth / 2 + daireSpacing / 2;

        for (int i = 1; i <= daireSayisi; i++)
        {
            Vector3 daireLocalPos = new(startX + (i - 1) * daireSpacing, 0.5f, 0);
            GameObject daireObj = Instantiate(roomPrefab, katObj.transform);
            daireObj.transform.localPosition = daireLocalPos;
            daireObj.name = $"Kat{kat}_Daire{i}";

            Renderer renderer = daireObj.GetComponent<Renderer>();
            if (renderer != null)
                renderer.material.color = renkHazirlaniyor;

            daireObj.SetActive(true);
            daireler[(kat, i)] = daireObj;

            yield return new WaitForSeconds(Random.Range(0.3f, 1.0f));
        }
    }

    void SetDaireRengi(Renderer renderer, Color renk)
    {
        if (renderer != null)
            renderer.material.color = renk;
    }

    bool KatDaireleriTamamlandi(int kat)
    {
        for (int daire = 1; daire <= daireSayisi; daire++)
        {
            if (!daireTamamlandi.TryGetValue((kat, daire), out bool tamamlandi) || !tamamlandi)
                return false;
        }
        return true;
    }
}

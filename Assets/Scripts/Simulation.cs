using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

class Daire
{
    public int KatNumarasi;
    public int DaireNumarasi;
    public bool TesisatBasladi = false;
    public bool TesisatTamamlandi = false;
    public bool IçTasarimBasladi = false;
    public bool InsaatTamamlandi = false;

    public async Task GuncelleAsync(string islem)
    {
        Console.WriteLine($"[{KatNumarasi}] Daire {DaireNumarasi}: {islem}");
        // Örnek simülasyon için delay ekleyelim
        await Task.Delay(500);

        switch (islem)
        {
            case "Tesisat işlemi başladı":
                TesisatBasladi = true;
                break;
            case "Tesisat işlemi tamamlandı":
                TesisatTamamlandi = true;
                break;
            case "İç tasarım başladı":
                IçTasarimBasladi = true;
                break;
            case "İnşaat tamamlandı":
                InsaatTamamlandi = true;
                break;
        }
    }
}

class Kat
{
    public int KatNumarasi;
    public Dictionary<int, Daire> Daireler = new Dictionary<int, Daire>();

    public bool KatInsaatBasladi = false;
    public bool KatInsaatTamamlandi = false;

    public void Guncelle(string islem)
    {
        Console.WriteLine($"[Kat {KatNumarasi}]: {islem}");
        if (islem == "Kat inşaatı başladı") KatInsaatBasladi = true;
        else if (islem == "Kat inşaatı tamamlandı") KatInsaatTamamlandi = true;
    }

    public Daire GetDaire(int daireNo)
    {
        if (!Daireler.ContainsKey(daireNo))
        {
            Daireler[daireNo] = new Daire { KatNumarasi = KatNumarasi, DaireNumarasi = daireNo };
        }
        return Daireler[daireNo];
    }
}

class InsaatSimulasyonu
{
    Dictionary<int, Kat> Katlar = new Dictionary<int, Kat>();

    Kat GetKat(int katNo)
    {
        if (!Katlar.ContainsKey(katNo))
            Katlar[katNo] = new Kat { KatNumarasi = katNo };
        return Katlar[katNo];
    }

    public async Task LogdanOkuVeCalistir(string logDosyasi)
    {
        string[] satirlar = File.ReadAllLines(logDosyasi);

        foreach (var satir in satirlar)
        {
            var matchKat = Regex.Match(satir, @"\[Kat (\d+)\]");
            if (!matchKat.Success) continue;

            int katNo = int.Parse(matchKat.Groups[1].Value);
            Kat kat = GetKat(katNo);

            var matchDaire = Regex.Match(satir, @"Daire (\d+):");
            if (matchDaire.Success)
            {
                int daireNo = int.Parse(matchDaire.Groups[1].Value);
                var islemMatch = Regex.Match(satir, @": (.*)\.");
                if (islemMatch.Success)
                {
                    string islem = islemMatch.Groups[1].Value;
                    Daire daire = kat.GetDaire(daireNo);
                    await daire.GuncelleAsync(islem);
                }
            }
            else
            {
                // Daire yok, kat işlemi olabilir
                var islemMatch = Regex.Match(satir, @"]: (.*)\.");
                if (islemMatch.Success)
                {
                    string islem = islemMatch.Groups[1].Value;
                    kat.Guncelle(islem);
                }
            }
        }

        Console.WriteLine("Tüm inşaat tamamlandı.");
    }
}

namespace CanSayNo.Domain.Enums;

/// <summary>
/// Karakterin o cevaptaki ruh hali — modelin her cevapla birlikte seçtiği etiket. Mobil yazma
/// temposunu (ve ileride avatar ifadesini) buna göre ayarlar. Yalnızca duygu bildirir; baskı
/// taktiğini ele vermez (taktikler istemciye asla gitmez).
/// </summary>
public enum CharacterMood
{
    /// <summary>Sakin, rahat; baskı henüz yumuşak.</summary>
    Calm = 1,
    /// <summary>Israrcı, ikna etmeye çalışıyor.</summary>
    Insistent = 2,
    /// <summary>Kırgın, alıngan, sitemli.</summary>
    Hurt = 3,
    /// <summary>Sinirli, sabırsız.</summary>
    Annoyed = 4,
    /// <summary>Geri adım atıyor, kabulleniyor ya da memnun kapatıyor.</summary>
    Yielding = 5
}

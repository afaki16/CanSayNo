
namespace CanSayNo.Domain.Common.Enums;
public enum ErrorCode
{
    InvalidRequest = 1000,
    ValidationFailed = 1001,
    NotFound = 1002,
    AlreadyExists = 1003,
    DatabaseError = 1004,
    ConcurrencyError = 1005,
    SaveError = 1006,
    DeleteError = 1007,
    UpdateError = 1008,
    InternalError = 1009,
    InvalidOperation = 1010,
    Forbidden = 1011,
    Unauthorized = 1012,
    EmailNotConfirmed = 1013,
    DailyLimitReached = 1014,   // Ücretsiz günlük pratik limiti doldu (429)
    PremiumRequired = 1015,     // Premium içerik, aktif abonelik yok (403)

    // Hayırcı'ya özel yeni kodlar 1100'den başlar: Kickstart template'i 1014+ aralığını
    // kendi kodlarına (PasswordChangeRequired, TenantSelectionRequired, ModuleDisabled) veriyor.
    AiUnavailable = 1100,       // Karakter (LLM) şu an cevap veremiyor (503)
    AiResponseRefused = 1101,   // LLM bu mesaja cevap vermeyi reddetti (422); raunt ilerlemez
    SessionNotActive = 1102,    // Oturum tamamlanmış/terk edilmiş, mesaj alınmaz (409)
    SessionTokenLimitReached = 1103, // Oturum token tavanına ulaştı (429)
    SessionNotCompleted = 1104, // Değerlendirme için oturum henüz tamamlanmadı (409)
}


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
}

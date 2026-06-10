using System.Text.Json.Serialization;
using CanSayNo.Domain.Common.Enums;

namespace CanSayNo.Application.Common.Results;

/// <summary>
/// Doğrulama hatası bilgilerini temsil eden sınıf
/// </summary>
public class ValidationError : Domain.Models.Error
{
    [JsonInclude] 
    public string PropertyName { get; }

    public ValidationError(string propertyName, string message) 
        : base(ErrorCode.ValidationFailed, message)
    {
        PropertyName = propertyName;
    }
}

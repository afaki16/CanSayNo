using System.Text.Json.Serialization;
using Kickstart.Domain.Common.Enums;

namespace Kickstart.Application.Common.Results;

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

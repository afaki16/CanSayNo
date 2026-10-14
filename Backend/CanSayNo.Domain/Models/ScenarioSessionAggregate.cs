using System;

namespace CanSayNo.Domain.Models;

/// <summary>
/// Bir kullanıcının tek bir senaryodaki oturum (Session) özetini taşır.
/// Sessions tablosundan tek bir GroupBy sorgusuyla üretilir (N+1 yok),
/// Query handler'larında DTO'ların user-spesifik alanlarını doldurmak için kullanılır.
/// </summary>
public class ScenarioSessionAggregate
{
    public string ScenarioId { get; set; } = string.Empty;
    public int AttemptsCount { get; set; }
    public int? BestScore { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? LastAttemptAt { get; set; }
}

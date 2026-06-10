using System;

namespace Kickstart.Application.Features.Auth.Dtos
{
    public class SessionDto
    {
        public int Id { get; set; }

        /// <summary>Masked fingerprint for UI (not the refresh secret).</summary>
        public string Token { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsActive { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public string DeviceType { get; set; }
        public string Location { get; set; }
        public TimeSpan RemainingTime { get; set; }
        public bool IsCurrentSession { get; set; }
    }
} 

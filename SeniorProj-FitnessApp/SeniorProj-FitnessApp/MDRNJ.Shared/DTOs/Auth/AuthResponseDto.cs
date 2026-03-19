using MDRNJ.Shared.DTOs.User;
using System;
using System.Collections.Generic;
using System.Text;

namespace MDRNJ.Shared.DTOs.Auth
{
    public class AuthResponseDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public UserDto User { get; set; }
    }
}

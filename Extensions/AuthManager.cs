﻿
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Hangman.Models;
using Hangman.Models.RequestModels;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Hangman.Extensions
{
    public class AuthManager : IAuthManager
    {
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly int JWTExpirationTimeInMinutes = 30;
        private readonly int RefreshTokenExpirationTimeInMinutes = 30;
        public AuthManager(IConfiguration configuration,IUserService userService)
        {
                _configuration= configuration;
                _userService = userService;
        }

        public async Task<string> SetToken(User user,HttpResponse Response)
        {
            var Token = CreateToken(user);

            var refreshToken = GenerateRefreshToken();

            SetRefreshToken(refreshToken,Response);

            await _userService.SetTokenToUserAsync(user.Username, refreshToken);

            return Token;
        }

        public async Task<object> VerifyUser(UserLoginRequestDto request)
        {
            var user = await _userService.GetUserAsync(request.Username);

            if (user == null)
            {
                return "1";
            }

            if (!VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return "2";
            }
            return user;
        }


        private bool VerifyPassword(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512(passwordSalt);
            var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(passwordHash);
        }
        private string CreateToken(User user)
        {

            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role == 0 ? "User" : "Admin") //have to get rid of this and user.Role should return string from enum
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value
                ));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddMinutes(JWTExpirationTimeInMinutes),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
        private RefreshToken GenerateRefreshToken()
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.Now.AddMinutes(RefreshTokenExpirationTimeInMinutes),
                Created = DateTime.Now
            };

            return refreshToken;
        }
        private void SetRefreshToken(RefreshToken newRefreshToken,HttpResponse Response)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = newRefreshToken.Expires
            };
            Response.Cookies.Append("refreshToken", newRefreshToken.Token, cookieOptions);


        }


        public async Task<int>? CheckTokenValidation(HttpRequest Request)
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var userName = _userService.GetMyName();

            if (userName.IsNullOrEmpty() || refreshToken.IsNullOrEmpty())
            {
                //return BadRequest("Username or RefreshToken is Empty!");
                return 400;
            }
            var user = await _userService.GetUserAsync(userName);

            if (user == null)
            {
                //return BadRequest("No Such A User!");
                return 404;
            }

            if (!user.RefreshToken.Equals(refreshToken))
            {
                //return Unauthorized("Invalid Refresh Token.");
                return 401;
            }
            else if (user.TokenExpires < DateTime.Now)
            {
                //return Unauthorized("Token expired.");
                return 401;
            }

            return 200;
        }

       
    }
}

﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ReactASPCore.Models;
using ReactASPCore.EmployeesData;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Text;
/*using Newtonsoft.Json;*/

namespace ReactASPCore.Controllers
{
    [ApiController]
    public class LoginController : ControllerBase
    {
        private EmployeeData _authData = new EmployeeData();

        [HttpPost]
        [Route("api/sign_up")]
        public async Task<IActionResult> Registration([FromBody] Employee employee)
        {
            return Ok(await _authData.Registration(employee));
        }

        [HttpPost]
        [Route("api/sign_in")]
        public async Task<IActionResult> Login([FromBody] Employee employee)
        {
            if (await _authData.Login(employee, "") == "Login is successed")
            {
                var identity = SetClaims(employee.Email, employee.Password);
                var jwt = new JwtSecurityToken(issuer: AuthOptions.ISSUER,
                        audience: AuthOptions.AUDIENCE,
                        notBefore: DateTime.UtcNow,
                        claims: identity.Claims,
                        expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
                        signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
                var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
                HttpContext.Request.Headers["Authorization"] = "Bearer " + encodedJwt;
                HttpContext.Request.Headers["Content-Type"] = "application/json;charset=utf-8";
                await _authData.Login(employee, encodedJwt);
                /*var headers = HttpContext.Request.Headers;
                var context = HttpContext.Request.HttpContext;
                var body = HttpContext.Request.Body;
                var request = HttpContext.Request;*/
                string bearer = HttpContext.Request.Headers["Authorization"];
                var token = new
                {
                    bearer = bearer,
                };
                return Ok(token);
            }
            return BadRequest("User isn't found, try again");
        }

        [HttpDelete]
        [Route("api/deleteEmployee/{id}")]
        public async Task<IActionResult> DeleteEmployee(Guid id, [FromHeader] string Authorization)
        {
            return Ok(await _authData.DeleteEmployee(id, Authorization));
        }

        [HttpPost("loginTest")]
        [AllowAnonymous]
        public async Task<IActionResult> Login()
        {
            var username = "Prerak";
            if (username == "Prerak")
            {
                var token = await GenerateJwtToken(username);
                return Ok(new { user = username, token = token });
            }
            else
            {
                return BadRequest("Invalid User");
            }
        }


        private async Task<string> GenerateJwtToken(string username)
        {
            var someSecret = "a very random string which should";
            List<Claim> claims = new List<Claim>() {
        new Claim(ClaimTypes.Name,username),
        new Claim(ClaimTypes.Role,"User"),

    };
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(someSecret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            JwtSecurityToken SecurityToken = new JwtSecurityToken(
                issuer: "myapi.com",
                audience: "myapi.com",
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: credentials
                );

            return await Task.Run<string>(() =>
            {
                return jwtTokenHandler.WriteToken(SecurityToken);
            });
        }

        private ClaimsIdentity SetClaims(string mail, string password)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, mail),
                    new Claim(ClaimsIdentity.DefaultRoleClaimType, password)
                };
                ClaimsIdentity claimsIdentity =
                new ClaimsIdentity(claims);
                return claimsIdentity;
            }
            return null;
        }
    }


}


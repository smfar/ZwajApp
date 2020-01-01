using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ZwajApp.API.Data;
using ZwajApp.API.Dtos;
using ZwajApp.API.Models;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System;
using Microsoft.AspNetCore.Authorization;

namespace ZwajApp.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepositry _repo;
        private readonly IConfiguration _config;

        public AuthController(IAuthRepositry repo,IConfiguration config){
            _repo = repo;
           _config = config;
        }
        [HttpPost("register")]
         [AllowAnonymous]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            //validation
            userForRegisterDto.Username=userForRegisterDto.Username.ToLower();
            if (await _repo.UserExists(userForRegisterDto.Username) )
            return BadRequest("هذا المستخدم مسجل مسبقا");
            var userToCreate=new User{
                Username=userForRegisterDto.Username
            };
             var CreatedUser= await _repo.Register(userToCreate,userForRegisterDto.Password);
             return StatusCode(201);
            
        }
        
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult>Login(UserForLoginDto userForLoginDto){
            var userFromRepo = await _repo.Login(userForLoginDto.username.ToLower(),userForLoginDto.password);
            if(userFromRepo == null) return Unauthorized();
            var claims = new[]{
                new Claim(ClaimTypes.NameIdentifier,userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name,userFromRepo.Username)
            };
            var Key = new SymmetricSecurityKey(Encoding.UTF8. GetBytes(_config.GetSection("AppSettings:Token").Value));
            // GetBytes(_config.GetSection("AppSettings:Token").Value));
            var creds= new SigningCredentials(Key,SecurityAlgorithms.HmacSha512);
            var tokenDescripror = new SecurityTokenDescriptor{
                Subject = new ClaimsIdentity(claims),
                Expires=DateTime.Now.AddDays(1),
                SigningCredentials=creds
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token= tokenHandler.CreateToken(tokenDescripror);
            return Ok(new {
                token=tokenHandler.WriteToken(token)
            });

        }

    }
}
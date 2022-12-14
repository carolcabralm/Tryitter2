using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Tryitter.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Tryitter.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class CadastroController : ControllerBase
  {
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IConfiguration _configuration;

    public CadastroController(UserManager<IdentityUser> userManager,
      SignInManager<IdentityUser> signInManager, IConfiguration configuration)
    {
      _userManager = userManager;
      _signInManager = signInManager;
      _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<ActionResult> RegisterUser([FromBody] Cadastro model)
    {
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState.Values.SelectMany(e => e.Errors));
      }
      //
      
      var user = new IdentityUser
      {
        UserName = model.Email,
        Email = model.Email,
        EmailConfirmed = true
      };

      var result = await _userManager.CreateAsync(user, model.Password);

      if (!result.Succeeded)
      {
        return BadRequest(result.Errors);
      }

      await _signInManager.SignInAsync(user, false);
      return Ok(GeraToken(model));
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] Cadastro userInfo)
    {
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState.Values.SelectMany(e => e.Errors));
      }

      var result = await _signInManager.PasswordSignInAsync(userInfo.Email,
        userInfo.Password, isPersistent: false, lockoutOnFailure: false);

        if (result.Succeeded)
        {
          return Ok(GeraToken(userInfo));
        }
        else
        {
          ModelState.AddModelError(string.Empty, "Login inválido");
          return BadRequest(ModelState);
        }
    }

    private UsuarioToken GeraToken(Cadastro userInfo)
    {
      var claims = new[]
      {
        new Claim(JwtRegisteredClaimNames.UniqueName, userInfo.Email),
        new Claim("meuPet", "pipoca"),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
      };
      var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(_configuration["Jwt:key"])); //Key ou key?????

      var credenciais = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

      var expiracao = _configuration["TokenConfiguration:ExpireHours"];
      var expiration = DateTime.UtcNow.AddHours(double.Parse(expiracao));

      JwtSecurityToken token = new JwtSecurityToken(
        issuer: _configuration["TokenConfiguration:Issuer"],
        audience: _configuration["TokenConfiguration:Audience"],
        claims: claims,
        expires: expiration,
        signingCredentials: credenciais);

      return new UsuarioToken()
        {
          Authenticated = true,
          Token = new JwtSecurityTokenHandler().WriteToken(token),
          Expiration = expiration,
          Message = "Token JWT OK"
        };
    }
  }
}
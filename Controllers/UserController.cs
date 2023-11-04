
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserCalendarAPI.DTOs;
using UserCalendarAPI.Entity;
using UserCalendarAPI.Models;

namespace UserCalendarAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration config;
        private readonly APIEntity Context;

        public UserController(APIEntity _Context,UserManager<ApplicationUser> userManager,IConfiguration config)
        {
            this.Context = _Context;
            this.userManager = userManager;
            this.config = config;
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register(ApplicationUserREGESTERDto userDto)
        {
            if (ModelState.IsValid)
            {
                //save
                ApplicationUser? applicationUserByUName = await userManager.FindByNameAsync(userDto.UserName);
                ApplicationUser? applicationUserByEMail = await userManager.FindByEmailAsync(userDto.Email);
                if (applicationUserByUName ==null && applicationUserByEMail == null)
                {
                    ApplicationUser user = new ApplicationUser();
                    user.UserName = userDto.UserName;
                    user.Email = userDto.Email;
                    user.PhoneNumber = userDto.PhoneNumber;
                    user.FirstName = userDto.FirstName;
                    user.LastName = userDto.LastName;
                    user.Age = userDto.Age;

                    IdentityResult result = await userManager.CreateAsync(user, userDto.Password);
                    if (result.Succeeded)
                    {
                        return Ok("Account Created Success");
                    }
                    return BadRequest(result.Errors.ToList());

                }
                ModelState.AddModelError(" ","username or email already signed in");
                IEnumerable<ModelError> Errors = ModelState.Values.SelectMany(v => v.Errors);
                return BadRequest(Errors.ToList());
            }
            IEnumerable<ModelError> allErrors = ModelState.Values.SelectMany(v => v.Errors);
            return BadRequest(allErrors);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(ApplicationUserLOGINDto userDTO)
        {
            if(ModelState.IsValid) 
            {
                ApplicationUser? applicationUserByUName = await userManager.FindByNameAsync(userDTO.UserNameOrEmail);
                ApplicationUser? applicationUserByEMail = await userManager.FindByEmailAsync(userDTO.UserNameOrEmail);
                if(applicationUserByUName != null || applicationUserByEMail != null)
                {
                    ApplicationUser user = applicationUserByUName != null ? applicationUserByUName : applicationUserByEMail;
                    bool ExistUser=await userManager.CheckPasswordAsync(user,userDTO.Password);
                    if(ExistUser)
                    {
                        //Claims Token
                        var claims = new List<Claim> {
                            new Claim(ClaimTypes.Name, user.UserName),
                            new Claim(ClaimTypes.NameIdentifier, user.Id),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                        };

                        //get roles
                        var roles = await userManager.GetRolesAsync(user);
                        if(roles != null) { 
                        foreach (var itemRole in roles)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, itemRole));
                        }
                        }
                        SecurityKey securityKey =
                            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:Key"]));

                        SigningCredentials signincred =
                            new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
                        //Create token
                        JwtSecurityToken tokenBeforeCompact = new JwtSecurityToken(
                            issuer: config["JWT:ValidIssuer"],
                            audience: config["JWT:ValidAudiance"],
                            claims: claims,
                            expires: DateTime.Now.AddDays(int.Parse(config["JWT:DurationInDays"])),
                            signingCredentials: signincred
                            );
                        return Ok(new {
                            token = new JwtSecurityTokenHandler().WriteToken(tokenBeforeCompact),
                            expiration = tokenBeforeCompact.ValidTo
                        });
                    }
                    return Unauthorized();
                }
                return Unauthorized();
            }
            return Unauthorized();
        }
    }
}

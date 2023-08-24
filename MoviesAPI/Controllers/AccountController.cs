using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MoviesAPI.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MoviesAPI.Controllers
{
    [ApiController]
    [Route("api/account")]
    public class AccountController : CustomBaseController
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IConfiguration configuration,
            ApplicationDbContext context,
            IMapper mapper)
            : base(context, mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _context = context;
        }

        /// <summary>
        /// Method to create a new user in DB
        /// </summary>
        /// <param name="model">Object with the user info</param>
        /// <returns></returns>
        [HttpPost("create", Name = "createUser")]
        public async Task<ActionResult<UserToken>> CreateUser([FromBody] UserInfo model)
        {
            var user = new IdentityUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                return await BuildToken(model);
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

        /// <summary>
        /// MEthod to login an existing user
        /// </summary>
        /// <param name="model">Object with the user info</param>
        /// <returns></returns>
        [HttpPost("login", Name = "loginUser")]
        public async Task<ActionResult<UserToken>> Login([FromBody] UserInfo model)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email,
                model.Password, isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return await BuildToken(model);
            }
            else
            {
                return BadRequest("Invalid login attempt");
            }
        }

        /// <summary>
        /// Method to neww the user's token
        /// </summary>
        /// <returns></returns>
        [HttpPost("renewToken", Name = "renewToken")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<UserToken>> Renew()
        {
            var userInfo = new UserInfo
            {
                Email = HttpContext.User.Identity.Name
            };

            return await BuildToken(userInfo);
        }

        /// <summary>
        /// Method to build a token
        /// </summary>
        /// <param name="userInfo">Object with the user info</param>
        /// <returns></returns>
        private async Task<UserToken> BuildToken(UserInfo userInfo)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, userInfo.Email),
                new Claim(ClaimTypes.Email, userInfo.Email),
            };

            var identityUser = await _userManager.FindByEmailAsync(userInfo.Email);

            claims.Add(new Claim(ClaimTypes.NameIdentifier, identityUser.Id));

            var claimsDB = await _userManager.GetClaimsAsync(identityUser);

            claims.AddRange(claimsDB);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddYears(1);

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: expiration,
                signingCredentials: creds
            );

            return new UserToken()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expiration
            };

        }

        /// <summary>
        /// Method to get all the users in DB
        /// </summary>
        /// <param name="paginationDTO">Object with filters</param>
        /// <returns></returns>
        [HttpGet("users", Name = "getUsers")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<ActionResult<List<UserDTO>>> Get([FromQuery] PaginationDTO paginationDTO)
        {
            var queryable = _context.Users.AsQueryable();
            queryable = queryable.OrderBy(x => x.Email);
            return await Get<IdentityUser, UserDTO>(paginationDTO);
        }

        /// <summary>
        /// Method to get all the roles
        /// </summary>
        /// <returns></returns>
        [HttpGet("roles", Name = "getRoles")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<ActionResult<List<string>>> GetRoles()
        {
            return await _context.Roles.Select(x => x.Name).ToListAsync();
        }

        /// <summary>
        /// Method to assign a role to an user
        /// </summary>
        /// <param name="editRoleDTO">Object with data</param>
        /// <returns></returns>
        [HttpPost("asignRole", Name = "asignRole")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<ActionResult> AsignRole(EditRoleDTO editRoleDTO)
        {
            var user = await _userManager.FindByIdAsync(editRoleDTO.UserId);
            if (user == null)
            {
                return NotFound();
            }

            await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, editRoleDTO.RoleName));
            return NoContent();
        }

        /// <summary>
        /// Method to remove a rol to an user
        /// </summary>
        /// <param name="editRoleDTO">Object with data</param>
        /// <returns></returns>
        [HttpPost("removeRole", Name = "removeRole")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<ActionResult> RemoveRole(EditRoleDTO editRoleDTO)
        {
            var user = await _userManager.FindByIdAsync(editRoleDTO.UserId);
            if (user == null)
            {
                return NotFound();
            }

            await _userManager.RemoveClaimAsync(user, new Claim(ClaimTypes.Role, editRoleDTO.RoleName));
            return NoContent();
        }

    }
}

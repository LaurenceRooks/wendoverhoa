using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WendoverHOA.Domain.Entities;
using WendoverHOA.Domain.Enums;
using WendoverHOA.Domain.Interfaces;
using WendoverHOA.Infrastructure.Persistence;

namespace WendoverHOA.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for user-related operations
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserRepository"/> class
        /// </summary>
        /// <param name="context">The database context</param>
        /// <param name="userManager">The user manager</param>
        public UserRepository(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        /// <inheritdoc/>
        public async Task<ApplicationUser?> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        /// <inheritdoc/>
        public async Task<ApplicationUser?> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == username);
        }

        /// <inheritdoc/>
        public async Task<ApplicationUser?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<ApplicationUser>> GetUsersByRoleAsync(UserRole role)
        {
            var roleName = role.ToString();
            var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
            return usersInRole.ToList();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<ApplicationUser>> GetUsersByPermissionAsync(Permission permission)
        {
            // This implementation assumes that permissions are stored in user claims
            // We need to find all users with the specific permission claim
            var permissionName = permission.ToString();
            var usersWithPermission = await _context.Users
                .Join(
                    _context.UserClaims,
                    user => user.Id,
                    claim => claim.UserId,
                    (user, claim) => new { User = user, Claim = claim })
                .Where(x => x.Claim.ClaimType == "permission" && x.Claim.ClaimValue == permissionName)
                .Select(x => x.User)
                .Distinct()
                .ToListAsync();

            return usersWithPermission;
        }

        /// <inheritdoc/>
        public async Task<bool> IsUsernameTakenAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.UserName == username);
        }

        /// <inheritdoc/>
        public async Task<bool> IsEmailRegisteredAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<ApplicationUser>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<ApplicationUser>> FindAsync(Expression<Func<ApplicationUser, bool>> predicate)
        {
            return await _context.Users
                .Where(predicate)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<ApplicationUser> AddAsync(ApplicationUser entity)
        {
            var result = await _userManager.CreateAsync(entity);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
            return entity;
        }

        /// <inheritdoc/>
        public async Task<ApplicationUser> UpdateAsync(ApplicationUser entity)
        {
            var result = await _userManager.UpdateAsync(entity);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to update user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
            return entity;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(ApplicationUser entity)
        {
            var result = await _userManager.DeleteAsync(entity);
            return result.Succeeded;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteByIdAsync(int id)
        {
            var user = await GetByIdAsync(id);
            if (user == null)
            {
                return false;
            }

            return await DeleteAsync(user);
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsAsync(Expression<Func<ApplicationUser, bool>> predicate)
        {
            return await _context.Users.AnyAsync(predicate);
        }

        /// <inheritdoc/>
        public async Task<int> CountAsync(Expression<Func<ApplicationUser, bool>>? predicate = null)
        {
            if (predicate == null)
            {
                return await _context.Users.CountAsync();
            }

            return await _context.Users.CountAsync(predicate);
        }
    }
}

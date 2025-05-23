using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WendoverHOA.Domain.Entities;
using WendoverHOA.Domain.Enums;
using WendoverHOA.Domain.Interfaces;
using WendoverHOA.Infrastructure.Persistence;

namespace WendoverHOA.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for role permission operations
    /// </summary>
    public class RolePermissionRepository : IRolePermissionRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="RolePermissionRepository"/> class
        /// </summary>
        /// <param name="context">The database context</param>
        public RolePermissionRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc/>
        public async Task<RolePermission?> GetByRoleAsync(UserRole role)
        {
            return await _context.RolePermissions
                .FirstOrDefaultAsync(rp => rp.Role == role);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Permission>> GetPermissionsForRoleAsync(UserRole role)
        {
            var permissions = await _context.RolePermissions
                .Where(rp => rp.Role == role)
                .Select(rp => rp.Permission)
                .ToListAsync();

            return permissions;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<UserRole>> GetRolesWithPermissionAsync(Permission permission)
        {
            var roles = await _context.RolePermissions
                .Where(rp => rp.Permission == permission)
                .Select(rp => rp.Role)
                .ToListAsync();

            return roles;
        }

        /// <inheritdoc/>
        public async Task<RolePermission?> GetByIdAsync(Guid id)
        {
            return await _context.RolePermissions.FindAsync(id);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<RolePermission>> GetAllAsync()
        {
            return await _context.RolePermissions.ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<RolePermission>> FindAsync(Expression<Func<RolePermission, bool>> predicate)
        {
            return await _context.RolePermissions
                .Where(predicate)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<RolePermission> AddAsync(RolePermission entity)
        {
            _context.RolePermissions.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        /// <inheritdoc/>
        public async Task<RolePermission> UpdateAsync(RolePermission entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return entity;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(RolePermission entity)
        {
            _context.RolePermissions.Remove(entity);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteByIdAsync(Guid id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
            {
                return false;
            }

            return await DeleteAsync(entity);
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsAsync(Expression<Func<RolePermission, bool>> predicate)
        {
            return await _context.RolePermissions.AnyAsync(predicate);
        }

        /// <inheritdoc/>
        public async Task<int> CountAsync(Expression<Func<RolePermission, bool>>? predicate = null)
        {
            if (predicate == null)
            {
                return await _context.RolePermissions.CountAsync();
            }

            return await _context.RolePermissions.CountAsync(predicate);
        }
    }
}

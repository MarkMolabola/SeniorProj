using MDRNJ.Api.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services
{
    /// <summary>Block lookups shared by chat, friends and moderation.</summary>
    public static class Blocks
    {
        /// <summary>True if either player has blocked the other.</summary>
        public static Task<bool> IsBlockedEitherWayAsync(this AppDbContext db, Guid a, Guid b) =>
            db.UserBlocks.AnyAsync(x =>
                (x.BlockerId == a && x.BlockedId == b) || (x.BlockerId == b && x.BlockedId == a));

        /// <summary>Players this user has blocked (their messages are hidden from them).</summary>
        public static async Task<HashSet<Guid>> BlockedByAsync(this AppDbContext db, Guid userId) =>
            (await db.UserBlocks.Where(x => x.BlockerId == userId).Select(x => x.BlockedId).ToListAsync()).ToHashSet();
    }
}

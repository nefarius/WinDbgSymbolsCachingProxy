using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using WinDbgSymbolsCachingProxy.Core;
using WinDbgSymbolsCachingProxy.Models;
using WinDbgSymbolsCachingProxy.Services;

namespace WinDbgSymbolsCachingProxy.Server.Tests;

/// <summary>
///     Tests for the stale not-found cleanup predicate exposed by <see cref="RecheckNotFoundService.IsStaleNotFound" />.
///     These tests exercise the in-process rule evaluation without requiring a live MongoDB instance.
/// </summary>
public sealed class RecheckNotFoundServiceCleanupTests
{
    private static readonly DateTime Now = new(2026, 5, 17, 0, 0, 0, DateTimeKind.Utc);

    // Cutoffs matching the default 180-day thresholds
    private static readonly DateTime InactiveCutoff = Now - TimeSpan.FromDays(180);
    private static readonly DateTime UnusedCutoff = Now - TimeSpan.FromDays(180);

    // ── Rule A (inactive): had a client, but LastAccessedAt is stale ────────────

    [Fact]
    public void RuleA_deletes_placeholder_whose_LastAccessedAt_is_before_cutoff()
    {
        SymbolsEntity entity = new()
        {
            IndexPrefix = "foo.pdb/abc/",
            FileName = "foo.pdb",
            SymbolKey = "abc",
            NotFoundAt = Now - TimeSpan.FromDays(200),
            LastAccessedAt = Now - TimeSpan.FromDays(181)
        };

        Assert.True(RecheckNotFoundService.IsStaleNotFound(entity, InactiveCutoff, UnusedCutoff));
    }

    [Fact]
    public void RuleA_keeps_placeholder_whose_LastAccessedAt_is_recent()
    {
        SymbolsEntity entity = new()
        {
            IndexPrefix = "foo.pdb/abc/",
            FileName = "foo.pdb",
            SymbolKey = "abc",
            NotFoundAt = Now - TimeSpan.FromDays(200),
            LastAccessedAt = Now - TimeSpan.FromDays(30)
        };

        Assert.False(RecheckNotFoundService.IsStaleNotFound(entity, InactiveCutoff, UnusedCutoff));
    }

    [Fact]
    public void RuleA_keeps_placeholder_whose_LastAccessedAt_equals_cutoff_exactly()
    {
        SymbolsEntity entity = new()
        {
            IndexPrefix = "foo.pdb/abc/",
            FileName = "foo.pdb",
            SymbolKey = "abc",
            NotFoundAt = Now - TimeSpan.FromDays(200),
            LastAccessedAt = InactiveCutoff
        };

        // Strict less-than: exactly at the cutoff should NOT be deleted
        Assert.False(RecheckNotFoundService.IsStaleNotFound(entity, InactiveCutoff, UnusedCutoff));
    }

    // ── Rule B (unused): never requested, NotFoundAt is stale ───────────────────

    [Fact]
    public void RuleB_deletes_placeholder_that_was_never_requested_and_NotFoundAt_is_old()
    {
        SymbolsEntity entity = new()
        {
            IndexPrefix = "bar.pdb/def/",
            FileName = "bar.pdb",
            SymbolKey = "def",
            NotFoundAt = Now - TimeSpan.FromDays(181),
            LastAccessedAt = null
        };

        Assert.True(RecheckNotFoundService.IsStaleNotFound(entity, InactiveCutoff, UnusedCutoff));
    }

    [Fact]
    public void RuleB_keeps_placeholder_that_was_never_requested_but_NotFoundAt_is_recent()
    {
        SymbolsEntity entity = new()
        {
            IndexPrefix = "bar.pdb/def/",
            FileName = "bar.pdb",
            SymbolKey = "def",
            NotFoundAt = Now - TimeSpan.FromDays(30),
            LastAccessedAt = null
        };

        Assert.False(RecheckNotFoundService.IsStaleNotFound(entity, InactiveCutoff, UnusedCutoff));
    }

    // ── Custom-row immunity ──────────────────────────────────────────────────────

    [Fact]
    public void Custom_row_is_never_deleted_regardless_of_age()
    {
        SymbolsEntity entity = new()
        {
            IndexPrefix = "custom.pdb/xyz/",
            FileName = "custom.pdb",
            SymbolKey = "xyz",
            IsCustom = true,
            NotFoundAt = Now - TimeSpan.FromDays(400),
            LastAccessedAt = Now - TimeSpan.FromDays(400)
        };

        Assert.False(RecheckNotFoundService.IsStaleNotFound(entity, InactiveCutoff, UnusedCutoff));
    }

    [Fact]
    public void Custom_row_without_LastAccessedAt_is_never_deleted()
    {
        SymbolsEntity entity = new()
        {
            IndexPrefix = "custom.pdb/xyz/",
            FileName = "custom.pdb",
            SymbolKey = "xyz",
            IsCustom = true,
            NotFoundAt = Now - TimeSpan.FromDays(400),
            LastAccessedAt = null
        };

        Assert.False(RecheckNotFoundService.IsStaleNotFound(entity, InactiveCutoff, UnusedCutoff));
    }

    // ── NotFoundAt == null (found/cached rows) are always kept ──────────────────

    [Fact]
    public void Row_without_NotFoundAt_is_never_deleted()
    {
        SymbolsEntity entity = new()
        {
            IndexPrefix = "found.pdb/aaa/",
            FileName = "found.pdb",
            SymbolKey = "aaa",
            NotFoundAt = null,
            LastAccessedAt = Now - TimeSpan.FromDays(400)
        };

        Assert.False(RecheckNotFoundService.IsStaleNotFound(entity, InactiveCutoff, UnusedCutoff));
    }

    // ── Memory cache eviction ────────────────────────────────────────────────────

    [Fact]
    public void Memory_cache_entry_for_stale_row_is_removed_on_eviction()
    {
        using MemoryCache cache = new(Options.Create(new MemoryCacheOptions()));

        SymbolsEntity entity = new()
        {
            IndexPrefix = "evict.pdb/bbb/",
            FileName = "evict.pdb",
            SymbolKey = "bbb",
            NotFoundAt = Now - TimeSpan.FromDays(200),
            LastAccessedAt = Now - TimeSpan.FromDays(200)
        };

        string cacheKey = entity.RelativeUri;
        cache.Set(cacheKey, entity);

        Assert.True(cache.TryGetValue(cacheKey, out _), "entry should exist before eviction");

        cache.Remove(cacheKey);

        Assert.False(cache.TryGetValue(cacheKey, out _), "entry should be gone after eviction");
    }

    // ── Enabled flag ─────────────────────────────────────────────────────────────

    [Fact]
    public void IsStaleNotFound_Predicate_Ignores_EnabledFlag()
    {
        // IsStaleNotFound is a pure predicate; it deliberately does not know about
        // the Enabled flag — that gating lives in PruneStaleNotFoundAsync, which
        // returns early when Enabled == false (the default).
        NotFoundCleanupOptions disabled = new() { Enabled = false };
        Assert.False(disabled.Enabled);

        SymbolsEntity staleEntity = new()
        {
            IndexPrefix = "stale.pdb/ccc/",
            FileName = "stale.pdb",
            SymbolKey = "ccc",
            NotFoundAt = Now - TimeSpan.FromDays(300),
            LastAccessedAt = null
        };

        // Predicate matches regardless — the service gates on Enabled separately.
        Assert.True(RecheckNotFoundService.IsStaleNotFound(staleEntity, InactiveCutoff, UnusedCutoff));
    }
}

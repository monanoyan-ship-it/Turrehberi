using ErkanTatilPlani.API.Services;
using ErkanTatilPlani.Core.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;

namespace ErkanTatilPlani.Tests.Services;

public class CacheServiceTests
{
    private readonly ICacheService _cacheService;
    private readonly IMemoryCache _memoryCache;

    public CacheServiceTests()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        var loggerMock = new Mock<ILogger<CacheService>>();
        _cacheService = new CacheService(_memoryCache, loggerMock.Object);
    }

    [Fact]
    public void Set_And_Get_Should_Work()
    {
        // Arrange
        var key = "test-key";
        var value = "test-value";

        // Act
        _cacheService.Set(key, value);
        var result = _cacheService.Get<string>(key);

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void Get_NonExistent_Key_Should_Return_Default()
    {
        // Arrange
        var key = "non-existent-key";

        // Act
        var result = _cacheService.Get<string>(key);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Remove_Should_Delete_Key()
    {
        // Arrange
        var key = "remove-test-key";
        _cacheService.Set(key, "value");

        // Act
        _cacheService.Remove(key);
        var result = _cacheService.Get<string>(key);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Exists_Should_Return_True_For_Existing_Key()
    {
        // Arrange
        var key = "exists-test-key";
        _cacheService.Set(key, "value");

        // Act
        var result = _cacheService.Exists(key);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Exists_Should_Return_False_For_NonExistent_Key()
    {
        // Arrange
        var key = "non-existent-key";

        // Act
        var result = _cacheService.Exists(key);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void RemoveByPrefix_Should_Remove_Matching_Keys()
    {
        // Arrange
        _cacheService.Set("prefix:key1", "value1");
        _cacheService.Set("prefix:key2", "value2");
        _cacheService.Set("other:key3", "value3");

        // Act
        _cacheService.RemoveByPrefix("prefix:");

        // Assert
        Assert.Null(_cacheService.Get<string>("prefix:key1"));
        Assert.Null(_cacheService.Get<string>("prefix:key2"));
        Assert.Equal("value3", _cacheService.Get<string>("other:key3"));
    }

    [Fact]
    public async Task GetOrCreateAsync_Should_Return_Cached_Value()
    {
        // Arrange
        var key = "get-or-create-key";
        var cachedValue = "cached-value";
        _cacheService.Set(key, cachedValue);

        var factoryCallCount = 0;
        Func<Task<string>> factory = () =>
        {
            factoryCallCount++;
            return Task.FromResult("new-value");
        };

        // Act
        var result = await _cacheService.GetOrCreateAsync(key, factory);

        // Assert
        Assert.Equal(cachedValue, result);
        Assert.Equal(0, factoryCallCount); // Factory should not be called
    }

    [Fact]
    public async Task GetOrCreateAsync_Should_Call_Factory_For_New_Key()
    {
        // Arrange
        var key = "new-get-or-create-key";
        var newValue = "new-value";

        Func<Task<string>> factory = () => Task.FromResult(newValue);

        // Act
        var result = await _cacheService.GetOrCreateAsync(key, factory);

        // Assert
        Assert.Equal(newValue, result);
        Assert.Equal(newValue, _cacheService.Get<string>(key)); // Should be cached
    }

    [Fact]
    public void Set_With_Expiration_Should_Work()
    {
        // Arrange
        var key = "expiration-test-key";
        var value = "value";
        var expiration = TimeSpan.FromMinutes(5);

        // Act
        _cacheService.Set(key, value, expiration);
        var result = _cacheService.Get<string>(key);

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void Set_Complex_Object_Should_Work()
    {
        // Arrange
        var key = "complex-object-key";
        var value = new TestObject { Id = 1, Name = "Test" };

        // Act
        _cacheService.Set(key, value);
        var result = _cacheService.Get<TestObject>(key);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Test", result.Name);
    }

    private class TestObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}

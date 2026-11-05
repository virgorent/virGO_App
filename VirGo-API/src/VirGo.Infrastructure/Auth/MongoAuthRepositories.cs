using MongoDB.Bson;
using MongoDB.Driver;
using VirGo.Application.Auth;
using VirGo.Domain.Auth;

namespace VirGo.Infrastructure.Auth;

public sealed class MongoUserRepository(IMongoDatabase database) : IUserRepository
{
    private readonly IMongoCollection<BsonDocument> users = database.GetCollection<BsonDocument>("users");

    public async Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("email", email);
        var document = await users.Find(filter).FirstOrDefaultAsync(cancellationToken);
        return document is null ? null : Map(document);
    }

    public async Task<User?> FindByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
        var document = await users.Find(filter).FirstOrDefaultAsync(cancellationToken);
        return document is null ? null : Map(document);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        var document = new BsonDocument
        {
            ["_id"] = user.Id,
            ["name"] = user.Name,
            ["email"] = user.Email,
            ["phone"] = user.Phone,
            ["passwordHash"] = user.PasswordHash,
            ["role"] = user.Role.ToString().ToLowerInvariant(),
            ["isVerified"] = user.IsVerified,
            ["failedLoginCount"] = user.FailedLoginCount,
            ["unreadNotices"] = user.UnreadNotices,
            ["createdAt"] = new BsonDateTime(user.CreatedAt),
            ["updatedAt"] = new BsonDateTime(user.UpdatedAt)
        };
        try
        {
            await users.InsertOneAsync(document, cancellationToken: cancellationToken);
        }
        catch (MongoWriteException exception) when (exception.WriteError?.Code == 11000)
        {
            throw new DuplicateUserEmailException();
        }
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("_id", user.Id);
        var update = Builders<BsonDocument>.Update
            .Set("passwordHash", user.PasswordHash)
            .Set("updatedAt", new BsonDateTime(user.UpdatedAt));
        await users.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
    }

    public async Task<User?> RecordFailedLoginAsync(
        string userId,
        DateTime now,
        int lockoutThreshold,
        TimeSpan lockoutDuration,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("_id", userId);
        var update = Builders<BsonDocument>.Update
            .Inc("failedLoginCount", 1)
            .Set("updatedAt", new BsonDateTime(now));
        var options = new FindOneAndUpdateOptions<BsonDocument>
        {
            ReturnDocument = ReturnDocument.After
        };
        var document = await users.FindOneAndUpdateAsync(filter, update, options, cancellationToken);
        if (document is null)
        {
            return null;
        }

        if (document.GetValue("failedLoginCount").ToInt32() >= lockoutThreshold)
        {
            var lockoutEndsAt = now.Add(lockoutDuration);
            await users.UpdateOneAsync(
                filter,
                Builders<BsonDocument>.Update.Set("lockoutEndsAt", new BsonDateTime(lockoutEndsAt)),
                cancellationToken: cancellationToken);
            document["lockoutEndsAt"] = new BsonDateTime(lockoutEndsAt);
        }

        return Map(document);
    }

    public Task ResetLoginFailuresAsync(string userId, DateTime now, CancellationToken cancellationToken = default)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("_id", userId);
        var update = Builders<BsonDocument>.Update
            .Set("failedLoginCount", 0)
            .Set("lockoutEndsAt", BsonNull.Value)
            .Set("updatedAt", new BsonDateTime(now));
        return users.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
    }

    private static User Map(BsonDocument document)
    {
        return new User
        {
            Id = document.GetValue("_id").AsString,
            Name = document.GetValue("name").AsString,
            Email = document.GetValue("email").AsString,
            Phone = document.GetValue("phone").AsString,
            PasswordHash = document.GetValue("passwordHash").AsString,
            Role = ParseRole(document.GetValue("role").AsString),
            IsVerified = document.TryGetValue("isVerified", out var isVerified) && isVerified.IsBoolean && isVerified.AsBoolean,
            FailedLoginCount = document.TryGetValue("failedLoginCount", out var failedLoginCount) && failedLoginCount.IsInt32 ? failedLoginCount.AsInt32 : 0,
            LockoutEndsAt = document.TryGetValue("lockoutEndsAt", out var lockoutEndsAt) && lockoutEndsAt.IsBsonDateTime ? lockoutEndsAt.AsBsonDateTime.ToUniversalTime() : null,
            UnreadNotices = document.TryGetValue("unreadNotices", out var unreadNotices) && unreadNotices.IsInt32 ? unreadNotices.AsInt32 : 0,
            CreatedAt = document.GetValue("createdAt").AsBsonDateTime.ToUniversalTime(),
            UpdatedAt = document.GetValue("updatedAt").AsBsonDateTime.ToUniversalTime()
        };
    }

    private static UserRole ParseRole(string role) => role switch
    {
        "client" => UserRole.Client,
        "owner" => UserRole.Owner,
        "provider" => UserRole.Provider,
        _ => throw new InvalidOperationException("The user role is invalid.")
    };
}

public sealed class MongoSessionRepository(IMongoDatabase database) : ISessionRepository
{
    private readonly IMongoCollection<BsonDocument> sessions = database.GetCollection<BsonDocument>("sessions");

    public async Task<Session?> FindAsync(string sessionId, string userId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<BsonDocument>.Filter.And(
            Builders<BsonDocument>.Filter.Eq("_id", sessionId),
            Builders<BsonDocument>.Filter.Eq("userId", userId));
        var document = await sessions.Find(filter).FirstOrDefaultAsync(cancellationToken);
        return document is null ? null : Map(document);
    }

    public Task AddAsync(Session session, CancellationToken cancellationToken = default)
    {
        var document = new BsonDocument
        {
            ["_id"] = session.Id,
            ["userId"] = session.UserId,
            ["tokenHash"] = session.TokenHash,
            ["createdAt"] = new BsonDateTime(session.CreatedAt),
            ["expiresAt"] = new BsonDateTime(session.ExpiresAt),
            ["purgeAt"] = new BsonDateTime(session.PurgeAt)
        };
        return sessions.InsertOneAsync(document, cancellationToken: cancellationToken);
    }

    public async Task<bool> DeleteAsync(string sessionId, string userId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<BsonDocument>.Filter.And(
            Builders<BsonDocument>.Filter.Eq("_id", sessionId),
            Builders<BsonDocument>.Filter.Eq("userId", userId));
        var result = await sessions.DeleteOneAsync(filter, cancellationToken);
        return result.DeletedCount == 1;
    }

    private static Session Map(BsonDocument document)
    {
        return new Session
        {
            Id = document.GetValue("_id").AsString,
            UserId = document.GetValue("userId").AsString,
            TokenHash = document.GetValue("tokenHash").AsString,
            CreatedAt = document.GetValue("createdAt").AsBsonDateTime.ToUniversalTime(),
            ExpiresAt = document.GetValue("expiresAt").AsBsonDateTime.ToUniversalTime(),
            PurgeAt = document.GetValue("purgeAt").AsBsonDateTime.ToUniversalTime()
        };
    }
}

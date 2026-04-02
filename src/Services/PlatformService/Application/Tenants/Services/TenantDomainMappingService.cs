
using DotNetCore.CAP;
using PlatformService.Application.Common.Abstractions;
using PlatformService.Application.Tenants.Abstractions;
using PlatformService.Application.Tenants.Commands;
using PlatformService.Application.Tenants.Dtos;
using PlatformService.Domain.Entities;
using PlatformService.Domain.Enums;
using SharedKernel.Common.Results;
using SharedKernel.Contracts.Platform;
using SharedKernel.Topics;

namespace PlatformService.Application.Tenants.Services;

public class TenantDomainMappingService(
    ITenantDomainMappingRepository repository,
    IUnitOfWork unitOfWork,
    ICapPublisher capPublisher)
    : ITenantDomainMappingService
{
    public async Task<ServiceResult<IReadOnlyList<TenantDomainMappingDto>>> GetByTenantAsync(
        Guid tenantPublicId,
        CancellationToken cancellationToken = default)
    {
        var mappings = await repository.GetByTenantPublicIdAsync(tenantPublicId, cancellationToken);

        var result = mappings.Select(MapToDto).ToList();

        return ServiceResult<IReadOnlyList<TenantDomainMappingDto>>.Ok(
            result,
            "Platform.TenantDomain.GetSuccess",
            "Tenant domain mappings retrieved successfully.");
    }

    public async Task<ServiceResult<TenantDomainMappingDto>> CreateAsync(
        CreateTenantDomainMappingCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<TenantDomainType>(command.DomainType, true, out var domainType))
        {
            return ServiceResult<TenantDomainMappingDto>.Fail(
                "Platform.TenantDomain.InvalidDomainType",
                "Invalid domain type.");
        }

        var normalizedHost = NormalizeHost(command.Host);

        var existingHost = await repository.GetByHostAsync(normalizedHost, cancellationToken);
        if (existingHost is not null)
        {
            return ServiceResult<TenantDomainMappingDto>.Fail(
                "Platform.TenantDomain.HostAlreadyExists",
                "Host already exists.");
        }

        if (command.IsPrimary)
        {
            var existingPrimary = await repository.GetPrimaryByTenantAndTypeAsync(
                command.TenantPublicId,
                domainType,
                cancellationToken);

            if (existingPrimary is not null)
            {
                existingPrimary.IsPrimary = false;
                existingPrimary.UpdatedAt = DateTime.UtcNow;
                repository.Update(existingPrimary);
            }
        }

        var mapping = new TenantDomainMapping
        {
            TenantPublicId = command.TenantPublicId,
            Host = normalizedHost,
            DomainType = domainType,
            IsPrimary = command.IsPrimary,
            IsActive = command.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await repository.AddAsync(mapping, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await capPublisher.PublishAsync(
            PlatformTopics.TenantDomainUpserted,
            new TenantDomainUpsertedMessage
            {
                DomainPublicId = mapping.PublicId,
                TenantPublicId = mapping.TenantPublicId,
                Host = mapping.Host,
                DomainType = mapping.DomainType.ToString(),
                IsPrimary = mapping.IsPrimary,
                IsActive = mapping.IsActive,
                OccurredAtUtc = DateTime.UtcNow
            },
            cancellationToken: cancellationToken);
        
        return ServiceResult<TenantDomainMappingDto>.Ok(
            MapToDto(mapping),
            "Platform.TenantDomain.CreateSuccess",
            "Tenant domain mapping created successfully.");
    }

    public async Task<ServiceResult<TenantDomainMappingDto>> UpdateAsync(
        UpdateTenantDomainMappingCommand command,
        CancellationToken cancellationToken = default)
    {
        var mapping = await repository.GetByPublicIdAsync(command.DomainPublicId, cancellationToken);
        if (mapping is null)
        {
            return ServiceResult<TenantDomainMappingDto>.Fail(
                "Platform.TenantDomain.NotFound",
                "Tenant domain mapping not found.");
        }

        if (!Enum.TryParse<TenantDomainType>(command.DomainType, true, out var domainType))
        {
            return ServiceResult<TenantDomainMappingDto>.Fail(
                "Platform.TenantDomain.InvalidDomainType",
                "Invalid domain type.");
        }

        var normalizedHost = NormalizeHost(command.Host);

        var existingHost = await repository.GetByHostAsync(normalizedHost, cancellationToken);
        if (existingHost is not null && existingHost.PublicId != mapping.PublicId)
        {
            return ServiceResult<TenantDomainMappingDto>.Fail(
                "Platform.TenantDomain.HostAlreadyExists",
                "Host already exists.");
        }

        if (command.IsPrimary)
        {
            var existingPrimary = await repository.GetPrimaryByTenantAndTypeAsync(
                mapping.TenantPublicId,
                domainType,
                cancellationToken);

            if (existingPrimary is not null && existingPrimary.PublicId != mapping.PublicId)
            {
                existingPrimary.IsPrimary = false;
                existingPrimary.UpdatedAt = DateTime.UtcNow;
                repository.Update(existingPrimary);
            }
        }

        mapping.Host = normalizedHost;
        mapping.DomainType = domainType;
        mapping.IsPrimary = command.IsPrimary;
        mapping.IsActive = command.IsActive;
        mapping.UpdatedAt = DateTime.UtcNow;

        repository.Update(mapping);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await capPublisher.PublishAsync(
            PlatformTopics.TenantDomainUpserted,
            new TenantDomainUpsertedMessage
            {
                DomainPublicId = mapping.PublicId,
                TenantPublicId = mapping.TenantPublicId,
                Host = mapping.Host,
                DomainType = mapping.DomainType.ToString(),
                IsPrimary = mapping.IsPrimary,
                IsActive = mapping.IsActive,
                OccurredAtUtc = DateTime.UtcNow
            },
            cancellationToken: cancellationToken);
        return ServiceResult<TenantDomainMappingDto>.Ok(
            MapToDto(mapping),
            "Platform.TenantDomain.UpdateSuccess",
            "Tenant domain mapping updated successfully.");
    }

    public async Task<ServiceResult<bool>> DeleteAsync(
        DeleteTenantDomainMappingCommand command,
        CancellationToken cancellationToken = default)
    {
        var mapping = await repository.GetByPublicIdAsync(command.DomainPublicId, cancellationToken);
        if (mapping is null)
        {
            return ServiceResult<bool>.Fail(
                "Platform.TenantDomain.NotFound",
                "Tenant domain mapping not found.");
        }
        var domainPublicId = mapping.PublicId;

        repository.Remove(mapping);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await capPublisher.PublishAsync(
            PlatformTopics.TenantDomainRemoved,
            new TenantDomainRemovedMessage
            {
                DomainPublicId = domainPublicId,
                OccurredAtUtc = DateTime.UtcNow
            },
            cancellationToken: cancellationToken);
        return ServiceResult<bool>.Ok(
            true,
            "Platform.TenantDomain.DeleteSuccess",
            "Tenant domain mapping deleted successfully.");
    }

    public async Task<ServiceResult<TenantDomainMappingDto>> SetActiveAsync(
        SetTenantDomainMappingActiveCommand command,
        CancellationToken cancellationToken = default)
    {
        var mapping = await repository.GetByPublicIdAsync(command.DomainPublicId, cancellationToken);
        if (mapping is null)
        {
            return ServiceResult<TenantDomainMappingDto>.Fail(
                "Platform.TenantDomain.NotFound",
                "Tenant domain mapping not found.");
        }

        mapping.IsActive = command.IsActive;
        mapping.UpdatedAt = DateTime.UtcNow;

        repository.Update(mapping);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await capPublisher.PublishAsync(
            PlatformTopics.TenantDomainUpserted,
            new TenantDomainUpsertedMessage
            {
                DomainPublicId = mapping.PublicId,
                TenantPublicId = mapping.TenantPublicId,
                Host = mapping.Host,
                DomainType = mapping.DomainType.ToString(),
                IsPrimary = mapping.IsPrimary,
                IsActive = mapping.IsActive,
                OccurredAtUtc = DateTime.UtcNow
            },
            cancellationToken: cancellationToken);
        return ServiceResult<TenantDomainMappingDto>.Ok(
            MapToDto(mapping),
            "Platform.TenantDomain.SetActiveSuccess",
            "Tenant domain mapping status updated successfully.");
    }

    private static string NormalizeHost(string host)
    {
        return host.Trim().ToLowerInvariant();
    }

    private static TenantDomainMappingDto MapToDto(TenantDomainMapping mapping)
    {
        return new TenantDomainMappingDto
        {
            DomainPublicId = mapping.PublicId,
            TenantPublicId = mapping.TenantPublicId,
            Host = mapping.Host,
            DomainType = mapping.DomainType.ToString(),
            IsPrimary = mapping.IsPrimary,
            IsActive = mapping.IsActive,
            CreatedAt = mapping.CreatedAt,
            UpdatedAt = mapping.UpdatedAt
        };
    }
}
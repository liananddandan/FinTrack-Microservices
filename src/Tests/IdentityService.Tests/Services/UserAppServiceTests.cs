using AutoFixture.Xunit2;
using FluentAssertions;
using IdentityService.Application.Common.DTOs;
using IdentityService.Application.Common.Status;
using IdentityService.Application.Services;
using IdentityService.Application.Services.Interfaces;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence.Repositories.Interfaces;
using IdentityService.Tests.Attributes;
using IdentityService.Tests.Extensions;
using Microsoft.AspNetCore.Identity;
using MockQueryable;
using Moq;
using SharedKernel.Common.Constants;
using SharedKernel.Common.DTOs.Auth;
using SharedKernel.Common.Results;
using StackExchange.Redis;

namespace IdentityService.Tests.Services;

public class UserAppServiceTests
{
    
}
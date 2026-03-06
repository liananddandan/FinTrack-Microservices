using AutoFixture.Xunit2;
using FluentAssertions;
using IdentityService.Application.Common.Status;
using IdentityService.Application.Services;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence.Repositories.Interfaces;
using IdentityService.Tests.Attributes;
using Microsoft.AspNetCore.Identity;
using MockQueryable;
using Moq;
using SharedKernel.Common.Constants;
using SharedKernel.Common.Exceptions;
using SharedKernel.Common.Results;
using StackExchange.Redis;

namespace IdentityService.Tests.Services;

public class UserDomainServiceTests
{
    
}
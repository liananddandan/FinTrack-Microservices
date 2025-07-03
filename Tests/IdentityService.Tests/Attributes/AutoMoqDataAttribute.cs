using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using IdentityService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace IdentityService.Tests.Attributes;

public class AutoMoqDataAttribute : AutoDataAttribute
{
    public AutoMoqDataAttribute()
        : base(() =>
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });
            var store = new Mock<IUserStore<ApplicationUser>>();
            var userManager = new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null);
            fixture.Inject(userManager);
            fixture.Inject(userManager.Object);
            // RoleManager mock
            var roleStore = new Mock<IRoleStore<ApplicationRole>>();
            var roleManager = new Mock<RoleManager<ApplicationRole>>(
                roleStore.Object,
                new IRoleValidator<ApplicationRole>[0],
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<ILogger<RoleManager<ApplicationRole>>>().Object);
            fixture.Inject(roleManager);
            fixture.Inject(roleManager.Object);
            return fixture;
        })
    {
    }
}
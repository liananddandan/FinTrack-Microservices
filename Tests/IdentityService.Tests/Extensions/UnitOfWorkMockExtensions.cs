using Moq;
using IdentityService.Repositories.Interfaces;

namespace IdentityService.Tests.Extensions;

public static class UnitOfWorkMockExtensions
{
    public static void SetupExecuteWithTransaction<T>(
        this Mock<IUnitOfWork> unitOfWorkMock)
    {
        unitOfWorkMock.Setup(uow =>
                uow.WithTransactionAsync<T>(
                    It.IsAny<Func<Task<T>>>(),
                    It.IsAny<CancellationToken>()))
            .Returns<Func<Task<T>>, CancellationToken>(async (func, _) => await func());
    }

    public static void SetupExecuteWithTransaction(
        this Mock<IUnitOfWork> unitOfWorkMock)
    {
        unitOfWorkMock.Setup(uow =>
                uow.WithTransactionAsync(
                    It.IsAny<Func<Task>>(),
                    It.IsAny<CancellationToken>()))
            .Returns<Func<Task>, CancellationToken>(async (func, _) => await func());
    }
}
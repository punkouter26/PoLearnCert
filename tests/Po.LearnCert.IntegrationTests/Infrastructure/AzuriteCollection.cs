using Xunit;

namespace Po.LearnCert.IntegrationTests.Infrastructure;

[CollectionDefinition("Azurite collection")]
public class AzuriteCollection : ICollectionFixture<AzuriteFixture>
{
}

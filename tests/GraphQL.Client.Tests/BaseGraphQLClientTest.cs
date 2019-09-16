using System;
using System.Net.Http;

namespace GraphQL.Client.Tests
{
	public abstract class BaseGraphQLClientTest
	{
		protected GraphQLClient GraphQLClient { get; } = new GraphQLClient(new HttpClient { BaseAddress = new Uri("https://swapi.apis.guru/") });
	}
}

using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Client.Exceptions;
using GraphQL.Common.Request;
using GraphQL.Common.Response;
using Newtonsoft.Json;

namespace GraphQL.Client
{

	/// <summary>
	/// A Client to access GraphQL EndPoints
	/// </summary>
	public partial class GraphQLClient
	{
		#region Properties

		/// <summary>
		/// Gets the headers which should be sent with each request.
		/// </summary>
		public HttpRequestHeaders DefaultRequestHeaders =>
			httpClient.DefaultRequestHeaders;

		/// <summary>
		/// The Options	to be used
		/// </summary>
		public GraphQLClientOptions Options { get; set; }

		#endregion

		private readonly HttpClient httpClient;

		#region Constructors

		/// <summary>
		/// Initializes a new instance
		/// </summary>
		/// <param name="httpClient">The HttpClient to be used</param>
		public GraphQLClient(HttpClient httpClient) : this(httpClient, new GraphQLClientOptions())
		{
		}

		/// <summary>
		/// Initializes a new instance
		/// </summary>
		/// <param name="httpClient">The HttpClient to be used</param>
		/// <param name="options">The Options to be used</param>
		public GraphQLClient(HttpClient httpClient, GraphQLClientOptions options)
		{
			this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
			Options = options ?? throw new ArgumentNullException(nameof(options));

			if (Options.JsonSerializerSettings == null) { throw new ArgumentNullException(nameof(Options.JsonSerializerSettings)); }
			if (Options.MediaType == null) { throw new ArgumentNullException(nameof(Options.MediaType)); }
		}

		#endregion

		/// <summary>
		/// Send a query via GET
		/// </summary>
		/// <param name="query">The Request</param>
		/// <returns>The Response</returns>
		public Task<GraphQLResponse> GetQueryAsync(string query)
		{
			return GetQueryAsync(query, CancellationToken.None);
		}

		/// <summary>
		/// Send a query via GET
		/// </summary>
		/// <param name="query">The Request</param>
		/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
		/// <returns>The Response</returns>
		public Task<GraphQLResponse> GetQueryAsync(string query, CancellationToken cancellationToken)
		{
			if (query == null) { throw new ArgumentNullException(nameof(query)); }

			return GetAsync(new GraphQLRequest { Query = query }, cancellationToken);
		}

		/// <summary>
		/// Send a <see cref="GraphQLRequest"/> via GET
		/// </summary>
		/// <param name="request">The Request</param>
		/// <returns>The Response</returns>
		public Task<GraphQLResponse> GetAsync(GraphQLRequest request)
		{
			return GetAsync(request, CancellationToken.None);
		}

		/// <summary>
		/// Send a <see cref="GraphQLRequest"/> via GET
		/// </summary>
		/// <param name="request">The Request</param>
		/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
		/// <returns>The Response</returns>
		public async Task<GraphQLResponse> GetAsync(GraphQLRequest request, CancellationToken cancellationToken)
		{
			if (request == null) { throw new ArgumentNullException(nameof(request)); }
			if (request.Query == null) { throw new ArgumentNullException(nameof(request.Query)); }

			StringBuilder queryParamsBuilder = new StringBuilder($"query={request.Query}", 3);
			if (request.OperationName != null) { queryParamsBuilder.Append("&operationName=").Append(request.OperationName); }
			if (request.Variables != null) { queryParamsBuilder.Append("&variables=").Append(JsonConvert.SerializeObject(request.Variables)); }
			using (HttpResponseMessage httpResponseMessage = await httpClient.GetAsync($"?{queryParamsBuilder}", cancellationToken).ConfigureAwait(false))
			{
				return await ReadHttpResponseMessageAsync(httpResponseMessage).ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Send a query via POST
		/// </summary>
		/// <param name="query">The Request</param>
		/// <returns>The Response</returns>
		public Task<GraphQLResponse> PostQueryAsync(string query)
		{
			return PostQueryAsync(query, CancellationToken.None);
		}

		/// <summary>
		/// Send a query via POST
		/// </summary>
		/// <param name="query">The Request</param>
		/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
		/// <returns>The Response</returns>
		public Task<GraphQLResponse> PostQueryAsync(string query, CancellationToken cancellationToken)
		{
			if (query == null) { throw new ArgumentNullException(nameof(query)); }

			return PostAsync(new GraphQLRequest { Query = query }, cancellationToken);
		}

		/// <summary>
		/// Send a <see cref="GraphQLRequest"/> via POST
		/// </summary>
		/// <param name="request">The Request</param>
		/// <returns>The Response</returns>
		public Task<GraphQLResponse> PostAsync(GraphQLRequest request)
		{
			return PostAsync(request, CancellationToken.None);
		}

		/// <summary>
		/// Send a <see cref="GraphQLRequest"/> via POST
		/// </summary>
		/// <param name="request">The Request</param>
		/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
		/// <returns>The Response</returns>
		public async Task<GraphQLResponse> PostAsync(GraphQLRequest request, CancellationToken cancellationToken)
		{
			if (request == null) { throw new ArgumentNullException(nameof(request)); }
			if (request.Query == null) { throw new ArgumentNullException(nameof(request.Query)); }

			string graphQLString = JsonConvert.SerializeObject(request, Options.JsonSerializerSettings);
			using (StringContent httpContent = new StringContent(graphQLString))
			{
				httpContent.Headers.ContentType = Options.MediaType;
				using (HttpResponseMessage httpResponseMessage = await httpClient.PostAsync("", httpContent, cancellationToken).ConfigureAwait(false))
				{
					return await ReadHttpResponseMessageAsync(httpResponseMessage).ConfigureAwait(false);
				}
			}
		}

		/// <summary>
		/// Reads the <see cref="HttpResponseMessage"/>
		/// </summary>
		/// <param name="httpResponseMessage">The Response</param>
		/// <returns>The GrahQLResponse</returns>
		private async Task<GraphQLResponse> ReadHttpResponseMessageAsync(HttpResponseMessage httpResponseMessage)
		{
			using (Stream stream = await httpResponseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false))
			using (StreamReader streamReader = new StreamReader(stream))
			using (JsonTextReader jsonTextReader = new JsonTextReader(streamReader))
			{
				JsonSerializer jsonSerializer = new JsonSerializer
				{
					ContractResolver = Options.JsonSerializerSettings.ContractResolver
				};
				try
				{
					return jsonSerializer.Deserialize<GraphQLResponse>(jsonTextReader);
				}
				catch (JsonReaderException)
				{
					if (httpResponseMessage.IsSuccessStatusCode)
					{
						throw;
					}
					throw new GraphQLHttpException(httpResponseMessage);
				}
			}
		}
	}
}

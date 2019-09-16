using GraphQL.Common.Exceptions;
using GraphQL.Common.Request;

namespace GraphQL.Common.Response
{

	/// <summary>
	/// Represent the response of a <see cref="GraphQLRequest"/>
	/// For more information <see href="http://graphql.org/learn/serving-over-http/#response"/>
	/// </summary>
	public class GraphQLResponse
	{

		/// <summary>
		/// The data of the response
		/// </summary>
		public dynamic Data { get; set; }

		/// <summary>
		/// The Errors if ocurred
		/// </summary>
		public GraphQLError[] Errors { get; set; }

		/// <summary>
		/// Get a field of <see cref="Data"/> as Type
		/// </summary>
		/// <typeparam name="Type">The expected type</typeparam>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>The field of data as an object</returns>
		public Type GetDataFieldAs<Type>(string fieldName)
		{
			dynamic value = this.Data.GetValue(fieldName);
			return value.ToObject<Type>();
		}

		/// <summary>
		/// Indicates if this response is successful.
		/// </summary>
		/// <returns>True if the response is successful, false otherwise.</returns>
		public bool IsSuccess()
		{
			return Errors == null || Errors.Length == 0;
		}

		/// <summary>
		/// Throws an <see cref="GraphQLException"/> if this response contains any errors.
		/// </summary>
		/// <exception cref="GraphQLException">Thrown if this response contains any errors.</exception>
		public void EnsureNoErrors()
		{
			if (Errors?.Length > 0)
			{
				throw new GraphQLException(Errors[0]);
			}
		}
	}
}

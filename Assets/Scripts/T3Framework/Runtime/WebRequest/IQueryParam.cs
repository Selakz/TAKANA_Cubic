namespace T3Framework.Runtime.WebRequest
{
	public interface IQueryParam
	{
		/// <summary> The query string used for sending GET request. </summary>
		public string Query { get; }
	}
}
#nullable enable

using System;
using App.AutoUpdate.Model;
using Newtonsoft.Json;
using T3Framework.Runtime.WebRequest;

namespace App.AutoUpdate.Schema
{
	public struct VersionsQuery : IQueryParam
	{
		public enum SortOrder
		{
			Descending,
			Ascending,
		}

		public int Offset { get; set; }

		public int Limit { get; set; }

		public SortOrder SortBy { get; set; }

		public string Query
		{
			get
			{
				var sort = SortBy switch
				{
					SortOrder.Descending => "desc",
					SortOrder.Ascending => "asc",
					_ => throw new ArgumentOutOfRangeException()
				};
				return $"offset={Offset}&limit={Limit}&sort={sort}";
			}
		}
	}

	public struct VersionsResponse
	{
		[JsonProperty("versions")]
		public VersionDescriptor[] Versions { get; set; }

		[JsonProperty("total")]
		public int Total { get; set; }
	}
}
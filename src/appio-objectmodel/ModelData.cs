﻿using Newtonsoft.Json;
using System.Collections.Generic;

namespace Appio.ObjectModel
{
	public class ModelData : IModelData
	{
		public ModelData()
		{
		}

		public ModelData(string name, string uri, string types, string namespaceVariable, List<string> requiredModelUris)
		{
			Name = name;
			Uri = uri;
			Types = types;
			NamespaceVariable = namespaceVariable;
			RequiredModelUris = requiredModelUris;
		}

		[JsonProperty("name")]
		public string Name { get; set; } = string.Empty;

		[JsonProperty("uri")]
		public string Uri { get; set; } = string.Empty;

		[JsonProperty("types")]
		public string Types { get; set; } = string.Empty;

		[JsonProperty("namespaceVariable")]
		public string NamespaceVariable { get; set; } = string.Empty;

		[JsonProperty("requiredModelUris")]
		public List<string> RequiredModelUris { get; set; } = new List<string>();
	}
}
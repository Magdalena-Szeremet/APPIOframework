﻿using System.Collections.Generic;

namespace Oppo.ObjectModel
{
    public interface IModelData
    {
		string Name { get; set; }
		string Uri { get; set; }
		string Types { get; set; }
		string NamespaceVariable { get; set; }
		List<string> RequiredModelUris { get; set; }
    }
}
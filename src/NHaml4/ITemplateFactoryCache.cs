using System;
using NHaml4.TemplateResolution;

namespace NHaml4
{
	public interface ITemplateFactoryCache
	{
	  TemplateFactory GetOrAdd(IViewSource viewSource, ITemplateFactoryFactory factoryFactory);
	}
}


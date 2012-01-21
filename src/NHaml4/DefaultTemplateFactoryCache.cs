using System;
using System.Collections.Generic;
using NHaml4.TemplateResolution;

namespace NHaml4
{
  public class DefaultTemplateFactoryCache : ITemplateFactoryCache
	{
	  private IDictionary<string, TemplateFactory> _compiledTemplateCache;
	  
		public DefaultTemplateFactoryCache ()
		{
		  _compiledTemplateCache = new Dictionary<string, TemplateFactory>();
		}

	  public TemplateFactory GetOrAdd(IViewSource viewSource,
					  ITemplateFactoryFactory factoryFactory)
	  {
	    TemplateFactory compiledTemplate;
	    var className = viewSource.GetClassName();

            lock( _compiledTemplateCache )
            {
                if (!_compiledTemplateCache.TryGetValue(className, out compiledTemplate))
                {
                    compiledTemplate = factoryFactory.CompileTemplateFactory(className, viewSource);
                    _compiledTemplateCache.Add(className, compiledTemplate);
                    return compiledTemplate;
                }
            }

	    return compiledTemplate;

	  }
	}
}


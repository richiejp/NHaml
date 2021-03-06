using System;
using System.Collections.Generic;
using System.IO;
using NHaml.Core.Utils;
using System.Web;

namespace NHaml.Core.TemplateResolution
{
    public class FileTemplateContentProvider : ITemplateContentProvider
    {
        public FileTemplateContentProvider()
        {
            PathSources = new List<string> ();
            AddPathSource("Views");
        }

        public IViewSource GetViewSource(string templateName)
        {
            return GetViewSource(templateName, new List<string>());
        }

        public IViewSource GetViewSource(string templateName, IList<string> templatePaths)
        {
            Invariant.ArgumentNotEmpty(templateName, "templateName");
            Invariant.ArgumentNotNull(templatePaths, "parentViewSourceList");
            templateName = SuffixWithHaml(templateName);
            var fileInfo = CreateFileInfo(templateName);
            if (fileInfo != null && fileInfo.Exists)
            {
                return new FileViewSource(fileInfo);
            }
            foreach (string path in templatePaths)
            {
                var combine = Path.Combine(path, templateName);
                if (File.Exists(combine))
                {
                    return new FileViewSource(new FileInfo(combine));
                }
            }
            throw new FileNotFoundException(string.Format("Could not find template '{0}'.", templateName));
        }

        protected virtual FileInfo CreateFileInfo(string templateName)
        {

            foreach (var pathSource in PathSources)
            {
                var fileInfo = CreateFileInfo(pathSource, templateName);
                if (fileInfo.Exists)
                {
                    return fileInfo;
                }
            }
            return null;
        }

        private static string SuffixWithHaml(string templateName)
        {
            if (templateName.EndsWith(".haml"))
            {
                return templateName;
            }
            return templateName + ".haml";
        }


        public IList<string> PathSources { get; set; }

        /// <remarks>The path is assumed to be relative to the AppDoamin BaseDirectory.</remarks>
        public void AddPathSource(string pathSource)
        {
            PathSources.Add(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, pathSource));
        }
   
//TODO:Not sure if this is useful
        //public bool HasSource(string sourceName)
        //{
        //    foreach (var pathSource in PathSources)
        //    {
        //        var fileInfo = CreateFileInfo(pathSource, sourceName);
        //        if (fileInfo.Exists)
        //        {
        //            return true;
        //        }
        //    }

        //    return false;
        //}

        protected static FileInfo CreateFileInfo(string viewRoot, string templateName)
        {
            //TODO: not sure what the purpose of this is. came from castle
            //if (Path.IsPathRooted(templateName))
            //{
            //    templateName = templateName.Substring(Path.GetPathRoot(templateName).Length);
            //}
            var info = new FileInfo(templateName);
            if (!info.Exists)
            {
                info = new FileInfo(Path.Combine(viewRoot, templateName));
            }

            return info;
        }

    }
}
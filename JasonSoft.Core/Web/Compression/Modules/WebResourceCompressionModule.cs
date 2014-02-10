//===========================================================================================
// MbCompression, Complete compression library for ASP.NET 2.0-3.5 (VS2005-VS2008)
// Developed by: Miron Abramson. http://blog.mironabramson.com
// Project site: http://www.codeplex.com/MbCompression
// File last update: 18-07-08
//===========================================================================================

#region Using

using System;
using System.Web.Handlers;
using System.Web;
using System.Net;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Specialized;
using System.Web.UI;
using System.Globalization;

#endregion

namespace JasonSoft.Web.Compression
{
    /// <summary>
    /// A Module that replace the System.Web.Handlers.AssemblyResourceLoader handler for better performance and 
    /// supporting minifing compression (gzip & deflate).
    /// This module handle ONLY WebResource files (with any content)
    /// </summary>
    public sealed class WebResourceCompressionModule : IHttpModule
    {
        private static readonly IDictionary _webResourceCache = Hashtable.Synchronized(new Hashtable());

        #region IHttpModule Members

        /// <summary>
        /// Release resources used by the module (Nothing realy in our case)
        /// </summary>
        void IHttpModule.Dispose()
        {
            // Nothing to dispose in our case;
        }

        /// <summary>
        /// Initializes a module and prepares it to handle requests.
        /// </summary>
        /// <param name="context"></param>
        void IHttpModule.Init(HttpApplication context)
        {
            context.PreRequestHandlerExecute += OnPreRequestHandlerExecute;
        }

        #endregion


        #region // The PreRequestHandlerExecute event

        private void OnPreRequestHandlerExecute(object sender, EventArgs e)
        {
            HttpApplication app = sender as HttpApplication;

            // Check if the current request is Webresource.axd
            if (app.Context.CurrentHandler is AssemblyResourceLoader)
            {
                HttpResponse response = app.Context.Response;
                int queryHash = (app.Context.Request.QueryString.ToString()).GetHashCode();

                // Check if the ETag is match. If so, we don't send nothing to the client, and stop here.
                CheckETag(app, queryHash);

                try
                {
                    // Parse the QueryString into parts
                    Quadruplet<Char, String, String, String> urlInfo = GetDataFromQuery(app.Context.Request.QueryString);

                    // Load the assembly
                    Assembly assembly = GetAssembly(urlInfo.First, urlInfo.Second);

                    if (assembly == null)
                        ThrowHttpException(404, SR.WebResourceCompressionModule_AssemblyNotFound, urlInfo.Forth);

                    // Get the resource info from assembly.
                    Quadruplet<bool, bool, string, bool> resourceInfo = GetResourceInfo(assembly, urlInfo.Third);

                    if (!resourceInfo.First)
                        ThrowHttpException(404, SR.WebResourceCompressionModule_AssemblyNotFound, urlInfo.Forth);

                    // If the WebResource needs to perform substitution (WebResource inside WebResource), we leave it to the original AssemblyResourceLoader handler ;-)
                    if (resourceInfo.Second)
                        return;

                    response.Clear();

                    // Set the response cache headers
                    SetCachingHeadersForWebResource(response.Cache, queryHash);

                    // Set the response content type
                    response.ContentType = resourceInfo.Third;

                    EncodingManager encodingMgr = new EncodingManager(app.Context);

                    // Write content with compression
                    if (resourceInfo.Forth && (Settings.Instance.CompressWebResource || Settings.Instance.MinifyContent))
                    {
                        using (StreamReader resourceStream = new StreamReader(assembly.GetManifestResourceStream(urlInfo.Third), true))
                        {
                            ProcessAndWriteToStream(resourceStream, app.Context, encodingMgr);
                        }
                    }
                    // Write content without compression or minify
                    else
                    {
                        using (Stream resourceStream = assembly.GetManifestResourceStream(urlInfo.Third))
                        {
                            WriteToStream(resourceStream, response.OutputStream);
                        }
                    }
                    response.OutputStream.Flush();
                    app.CompleteRequest();
                }
                catch (ArgumentNullException)
                {
                    return;
                }
                catch (TargetInvocationException)
                {
                    return;
                }
                catch (System.Security.Cryptography.CryptographicException)
                {
                    return;
                }
            }
        }

        #endregion


        #region // Private methods

        /// <summary>
        /// Process & Write a StreamReader to an output stream
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="context"></param>
        /// <param name="encodingMgr"></param>
        private static void ProcessAndWriteToStream(StreamReader reader, HttpContext context, EncodingManager encodingMgr)
        {
            string content = SR.GetString(SR.CREDIT_STRING);
            if (Settings.Instance.MinifyContent)
            {
                if (context.Response.ContentType.Contains("javascript"))
                {
                    JavaScriptMinifier _Minifier = new JavaScriptMinifier();
                    content += _Minifier.Minify(reader);
                }
                else if (context.Response.ContentType.Contains("css"))
                {
                    CssMinifier _Minifier = new CssMinifier();
                    content += _Minifier.Minify(reader);
                }
                else
                {
                    content += reader.ReadToEnd();
                }
            }
            else
            {
                content += reader.ReadToEnd();
            }
            if (encodingMgr.IsEncodingEnabled && Settings.Instance.CompressWebResource)
            {
                encodingMgr.SetResponseEncodingType();
                byte[] compressed = encodingMgr.CompressString(content);
                context.Response.OutputStream.Write(compressed, 0, compressed.Length);
            }
            else
            {
                context.Response.Write(content);
            }
        }


        /// <summary>
        /// Write a StreamReader to an output stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="outputStream"></param>
        private static void WriteToStream(Stream stream, Stream outputStream)
        {
            Util.StreamCopy(stream, outputStream);
        }


        /// <summary>
        /// Get the info about the resource that in the assembly
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        private static Quadruplet<bool, bool, string, bool> GetResourceInfo(Assembly assembly, string resourceName)
        {
            // Create a unique cache key
            int cacheKey = Util.CombineHashCodes(assembly.GetHashCode(), resourceName.GetHashCode());

            Quadruplet<bool, bool, string, bool> resourceInfo = _webResourceCache[cacheKey] as Quadruplet<bool, bool, string, bool>;

            // Assembly info was not in the cache
            if (resourceInfo == null)
            {
                bool first = false;
                bool second = false;
                string third = string.Empty;
                bool forth = false;

                object[] customAttributes = assembly.GetCustomAttributes(false);
                for (int j = 0; j < customAttributes.Length; j++)
                {
                    WebResourceAttribute attribute = customAttributes[j] as WebResourceAttribute;
                    if ((attribute != null) && string.Equals(attribute.WebResource, resourceName, StringComparison.Ordinal))
                    {
                        first = true;
                        second = attribute.PerformSubstitution;
                        third = attribute.ContentType;
                        forth = Util.IsContentTypeCompressible(attribute.ContentType);
                        break;
                    }
                }
                resourceInfo = new Quadruplet<bool, bool, string, bool>(first, second, third, forth);
                _webResourceCache[cacheKey] = resourceInfo;
            }
            return resourceInfo;
        }



        /// <summary>
        /// Load the specifid assembly curresponding to the given signal.
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        private static Assembly GetAssembly(char signal, string assemblyName)
        {
            Assembly assembly = null;
            switch (signal)
            {
                case 's':
                    assembly = typeof(AssemblyResourceLoader).Assembly;
                    break;
                case 'p':
                    assembly = Assembly.Load(assemblyName);
                    break;
                case 'f':
                    {
                        string[] strArray = assemblyName.Split(new[] { ',' });
                        if (strArray.Length != 4)
                        {
                            ThrowHttpException(400, SR.WebResourceCompressionModule_InvalidRequest);
                        }
                        AssemblyName assemblyRef = new AssemblyName();
                        assemblyRef.Name = strArray[0];
                        assemblyRef.Version = new Version(strArray[1]);
                        string name = strArray[2];
                        if (name.Length > 0)
                        {
                            assemblyRef.CultureInfo = new CultureInfo(name);
                        }
                        else
                        {
                            assemblyRef.CultureInfo = CultureInfo.InvariantCulture;
                        }
                        string tokens = strArray[3];
                        byte[] publicKeyToken = new byte[tokens.Length / 2];
                        for (int i = 0; i < publicKeyToken.Length; i++)
                        {
                            publicKeyToken[i] = byte.Parse(tokens.Substring(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                        }
                        assemblyRef.SetPublicKeyToken(publicKeyToken);
                        assembly = Assembly.Load(assemblyRef);
                        break;
                    }
                default:
                    ThrowHttpException(400, SR.WebResourceCompressionModule_InvalidRequest);
                    break;
            }
            return assembly;
        }



        /// <summary>
        /// Collect the necessary data from the query string
        /// </summary>
        /// <param name="queryString"></param>
        /// <returns></returns>
        private static Quadruplet<Char, String, String, String> GetDataFromQuery(NameValueCollection queryString)
        {
            string queryParam = queryString["d"];
            if (string.IsNullOrEmpty(queryParam))
            {
                ThrowHttpException(400, SR.WebResourceCompressionModule_InvalidRequest);
            }
            string decryptedParam = string.Empty;
            try
            {
                decryptedParam = Util.DecryptString(queryParam);
            }
            catch (System.Configuration.Provider.ProviderException)
            {
                ThrowHttpException(403, SR.WebResourceCompressionModule_MachineKeyMissing);
            }
            catch (Exception ex)
            {
                ThrowHttpException(400, SR.WebResourceCompressionModule_InvalidRequest, ex);
            }

            int pipeIndex = decryptedParam.IndexOf('|');

            if (pipeIndex < 1 || pipeIndex > (decryptedParam.Length - 2))
            {
                ThrowHttpException(404, SR.WebResourceCompressionModule_AssemblyNotFound, decryptedParam);
            }
            if (pipeIndex > (decryptedParam.Length - 2))
            {
                ThrowHttpException(404, SR.WebResourceCompressionModule_ResourceNotFound, decryptedParam);
            }
            string assemblyName = decryptedParam.Substring(1, pipeIndex - 1);
            string resourceName = decryptedParam.Substring(pipeIndex + 1);
            return new Quadruplet<Char, String, String, String>(decryptedParam[0], assemblyName, resourceName, decryptedParam);
        }



        /// <summary>
        /// Check if the ETag that sent from the client is match to the current ETag.
        /// If so, set the status code to 'Not Modified' and stop the response.
        /// </summary>
        private static void CheckETag(HttpApplication app, int etagCode)
        {
            string etag = "\"" + etagCode + "\"";

            if (etag.Equals(app.Context.Request.Headers["If-None-Match"], StringComparison.Ordinal))
            {
                app.Context.Response.Cache.SetETag(etag);
                app.Context.Response.AppendHeader("Content-Length", "0");
                app.Context.Response.StatusCode = (int)HttpStatusCode.NotModified;
                app.Context.Response.SuppressContent = true;
                app.Context.Response.End();
            }
        }

        /// <summary>
        /// Set the response cache headers for WebResource
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="etag"></param>
        private static void SetCachingHeadersForWebResource(HttpCachePolicy cache, int etag)
        {
            cache.VaryByParams["d"] = true;
            cache.VaryByHeaders["Accept-Encoding"] = true;
            cache.SetOmitVaryStar(true);

            // Keep in the client cache for the time configured in the Web.Config
            cache.SetExpires(DateTime.UtcNow.AddDays(Settings.Instance.DaysInCache));
            cache.SetMaxAge(TimeSpan.FromDays(Settings.Instance.DaysInCache));
            cache.SetValidUntilExpires(true);
            cache.SetLastModified(DateTime.UtcNow);

            cache.SetCacheability(HttpCacheability.Public);

            cache.SetETag(string.Concat("\"", etag, "\""));
        }

        #endregion


        #region // Throw HttpException
        private static void ThrowHttpException(int num, string SRName)
        {
            throw new HttpException(num, SR.GetString(SRName));
        }

        private static void ThrowHttpException(int num, string SRName, string param1)
        {
            throw new HttpException(num, SR.GetString(SRName, param1));
        }
        private static void ThrowHttpException(int num, string SRName, Exception innerException)
        {
            throw new HttpException(num, SR.GetString(SRName), innerException);
        }

        #endregion
    }
}


using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SimpleCacheForTests
{
    /// <summary>
    /// Class for Testing Objects with <see cref="Microsoft.QualityTools.Testing.Fakes"></see>
    /// </summary>
    public static class ObjectCache
    {
        /// <summary>
        /// (Awaitable) Get object from Entity Framework Query or Json Query Cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query">the original query</param>
        /// <param name="cacheFileName">the cache file name</param>
        /// <param name="reloadFromSource">if true reloads data from datasource</param>
        /// <returns>object</returns>
        public static async Task<T> FromQueryCacheAsync<T>(Func<T> query, string cacheFileName, bool reloadFromSource = false) where T : class
        {
            // test if cachefile exists or if clear is set
            if (!File.Exists(cacheFileName) || reloadFromSource)
            {
                // load data direct from ef query
                var data = query();
                // save to json file
                await SaveToJsonFile(cacheFileName, reloadFromSource, data);
                return data;
            }

            using (var sr = new StreamReader(cacheFileName))
            {
                return JsonConvert.DeserializeObject<T>(
                    await sr.ReadToEndAsync()
                    );
            }
        }

        /// <summary>
        /// Get object from Entity Framework Query or Json Query Cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query">NOTE: this Action must be called with
        ///ShimsContext.ExecuteWithoutShims(() => query()) </param>
        /// <param name="cacheFileName">the cache file name</param>
        /// <param name="reloadFromSource">if true reloads data from datasource</param>
        /// <returns>object</returns>
        public static T FromQueryCache<T>(Func<T> query, string cacheFileName, bool reloadFromSource = false) where T : class
        {
            // test if cachefile exists or if clear is set
            if (!File.Exists(cacheFileName) || reloadFromSource)
            {
                // load data direct from ef query
                var data = query();
                // save to json file
                SaveToJsonFile(cacheFileName, reloadFromSource, data).GetAwaiter().GetResult();

                return data;
            }

            // load data from cache file / json
            using (var sr = new StreamReader(cacheFileName))
            {
                // return data
                return JsonConvert.DeserializeObject<T>(
                    sr.ReadToEndAsync().GetAwaiter().GetResult()
                    );
            }
        }


        /// <summary>
        /// save object to JSON File
        /// </summary>
        /// <typeparam name="T">type parameter</typeparam>
        /// <param name="data">the single object</param>
        /// <param name="cacheFileName">the JSON filename</param>
        /// <param name="clear">if yes clear the cache and create new one</param>
        /// <returns></returns>
        private static async Task SaveToJsonFile<T>(string cacheFileName, bool clear, T data) where T : class
        {
            // create writer for serialization to JSON
            using (var sw = new StreamWriter(cacheFileName, !clear))
            {
                /*
                 serialize the object list
                 Json Settings ReferenceLoopHandling and PreserveReferencesHandling set for allowing the JSON to store object references and not to run into reference loop errors.
                 */
                await sw.WriteAsync(JsonConvert.SerializeObject(data, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects
                }));
            }
        }
    }
}

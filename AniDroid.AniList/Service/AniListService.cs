﻿using System;
using System.Collections.Generic;
using System.Text;
using RestSharp;
using System.Threading.Tasks;
using System.Threading;
using AniDroid.AniList.Models;
using System.IO;
using Newtonsoft.Json;
using RestSharp.Serializers;
using Newtonsoft.Json.Serialization;
using AniDroid.AniList.Interfaces;
using RestSharp.Deserializers;
using AniDroid.AniList.Queries;
using Newtonsoft.Json.Linq;
using System.Net;
using AniDroid.AniList.GraphQL;
using AniDroid.AniList.Utils;

namespace AniDroid.AniList.Service
{
    public class AniListService : IAniListService
    {
        public IAniListServiceConfig Config { get; }
        public IAuthCodeResolver AuthCodeResolver { get; }

        private AniListService() { }

        public AniListService(IAniListServiceConfig config, IAuthCodeResolver auth)
        {
            Config = config;
            AuthCodeResolver = auth;
        }

        public static async Task<IRestResponse<AniListAuthorizationResponse>> AuthenticateUser(IAniListServiceConfig config, string code, CancellationToken cToken = default(CancellationToken))
        {
            var authReq = new RestRequest(config.AuthUrl, Method.POST);
            authReq.AddParameter("client_id", config.ClientId);
            authReq.AddParameter("client_secret", config.ClientSecret);
            authReq.AddParameter("grant_type", "refresh_token");
            authReq.AddParameter("redirect_uri", config.RedirectUrl);
            authReq.AddParameter("code", code);

            var client = new RestClient();
            return await client.ExecuteTaskAsync<AniListAuthorizationResponse>(authReq, cToken).ConfigureAwait(false);
        }

        #region Media

        public async Task<IAniListServiceResponse<Media>> GetMedia(int id, Media.MediaType type, CancellationToken cToken = default(CancellationToken))
        {
            var query = new GraphQLQuery
            {
                Query = QueryStore.GetMediaByIdAndType,
                Variables = JsonConvert.SerializeObject(new { id, type = type.Value })
            };
            var req = CreateRequest(query);
            return await ExecuteRequest<Media>(req, cToken);
        }

        public async Task<IAniListServiceResponse<AniListObject.PagedData<List<Media>>>> SearchMedia(string queryText, int page, int count, Media.MediaType type = null, CancellationToken cToken = default(CancellationToken))
        {
            var variableObj = JObject.FromObject(new { queryText, page, count }, new JsonNetSerializer().Serializer);

            if (type != null)
            {
                variableObj.Add("type", type.Value);
            }

            var query = new GraphQLQuery
            {
                Query = QueryStore.SearchMedia,
                Variables = variableObj.ToString()
            };
            var req = CreateRequest(query);
            return await ExecuteRequest<AniListObject.PagedData<List<Media>>>(req, cToken);
        }

        public IAsyncEnumerable<AniListObject.PagedData<ICollection<Media>>> SearchMediaPaging(string queryText,
            Media.MediaType type = null, int perPage = 20)
        {
            async Task<AniListObject.PagedData<ICollection<Media>>> GetPageAsync(PagingInfo info, CancellationToken ct)
            {
                // TODO: Standardize between all GetPageAsync methods
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Include,
                };
                var arguments = new
                {
                    queryText,
                    page = info.Page,
                    count = info.PageSize,
                    type = type?.Value,
                };

                var req = CreateRequest(Method.POST, new GraphQLQuery
                {
                    Query = QueryStore.SearchMedia,
                    Variables = JsonConvert.SerializeObject(arguments, settings),
                });

                return (await ExecuteRequest<AniListObject.PagedData<ICollection<Media>>>(req, ct)
                    .ConfigureAwait(false)).Data;
            }
            return new PagedAsyncEnumerable<ICollection<Media>>(perPage, GetPageAsync, HasNextPage);
        }

        #endregion

        #region User

        public async Task<IAniListServiceResponse<User>> GetUser(string name, CancellationToken cToken = default(CancellationToken))
        {
            var query = new GraphQLQuery
            {
                Query = QueryStore.GetUserByName,
                Variables = JsonConvert.SerializeObject(new { name })
            };
            var req = CreateRequest(query);
            return await ExecuteRequest<User>(req, cToken);
        }

        public async Task<IAniListServiceResponse<Media.MediaListCollection>> GetUserMediaList(string userName, Media.MediaType type, CancellationToken cToken = default(CancellationToken))
        {
            var query = new GraphQLQuery
            {
                Query = QueryStore.GetMediaListsByUserNameAndType,
                Variables = JsonConvert.SerializeObject(new { name = userName, type = type.Value })
            };
            var req = CreateRequest(query);
            return await ExecuteRequest<Media.MediaListCollection>(req, cToken);
        }

        public async Task<IAniListServiceResponse<AniListObject.PagedData<List<User>>>> SearchUsers(string queryText, int page, int count, CancellationToken cToken = default(CancellationToken))
        {
            var query = new GraphQLQuery
            {
                Query = QueryStore.SearchUsers,
                Variables = JsonConvert.SerializeObject(new { queryText, page, count })
            };
            var req = CreateRequest(query);
            return await ExecuteRequest<AniListObject.PagedData<List<User>>>(req, cToken);
        }

        #endregion

        #region Activity

        public async Task<IAniListServiceResponse<List<User>>> ToggleLike(int id, AniListObject.LikeableType type, CancellationToken cToken = default(CancellationToken))
        {
            var query = new GraphQLQuery
            {
                Query = QueryStore.ToggleLike,
                Variables = JsonConvert.SerializeObject(new { id, type = type.Value })
            };
            var req = CreateRequest(query);
            return await ExecuteRequest<List<User>>(req, cToken);
        }

        public async Task<IAniListServiceResponse<AniListObject.PagedData<List<AniListActivity>>>> GetAniListActivity(int page, int count, CancellationToken cToken = default(CancellationToken))
        {
            var query = new GraphQLQuery
            {
                Query = QueryStore.GetUserActivity,
                Variables = JsonConvert.SerializeObject(new { page, count })
            };
            var req = CreateRequest(query);
            return await ExecuteRequest<AniListObject.PagedData<List<AniListActivity>>>(req, cToken);
        }

        public async Task<IAniListServiceResponse<AniListActivity>> PostTextActivity(string text, CancellationToken cToken = default(CancellationToken))
        {
            var query = new GraphQLQuery
            {
                Query = QueryStore.PostTextActivity,
                Variables = JsonConvert.SerializeObject(new { text })
            };
            var req = CreateRequest(query);
            return await ExecuteRequest<AniListActivity>(req, cToken);
        }

        public async Task<IAniListServiceResponse<AniListActivity.ActivityReply>> PostActivityReply(int activityId, string text, CancellationToken cToken = default(CancellationToken))
        {
            var query = new GraphQLQuery
            {
                Query = QueryStore.PostActivityReply,
                Variables = JsonConvert.SerializeObject(new { activityId, text })
            };
            var req = CreateRequest(query);
            return await ExecuteRequest<AniListActivity.ActivityReply>(req, cToken);
        }

        public async Task<IAniListServiceResponse<AniListActivity>> GetAniListActivityById(int id, CancellationToken cToken = default(CancellationToken))
        {
            var query = new GraphQLQuery
            {
                Query = QueryStore.GetAniListActivityById,
                Variables = JsonConvert.SerializeObject(new { id })
            };
            var req = CreateRequest(query);
            return await ExecuteRequest<AniListActivity>(req, cToken);
        }

        public async Task<IAniListServiceResponse<AniListObject.PagedData<List<AniListNotification>>>> GetAniListNotifications(int page, int count, CancellationToken cToken = default(CancellationToken))
        {
            var query = new GraphQLQuery
            {
                Query = QueryStore.GetUserNotifications,
                Variables = JsonConvert.SerializeObject(new { page, count })
            };
            var req = CreateRequest(query);
            return await ExecuteRequest<AniListObject.PagedData<List<AniListNotification>>>(req, cToken);
        }

        #endregion

        #region Character

        public async Task<IAniListServiceResponse<AniListObject.PagedData<List<Character>>>> SearchCharacters(string queryText, int page, int count, CancellationToken cToken = default(CancellationToken))
        {
            var query = new GraphQLQuery
            {
                Query = QueryStore.SearchCharacters,
                Variables = JsonConvert.SerializeObject(new { queryText, page, count })
            };
            var req = CreateRequest(query);
            return await ExecuteRequest<AniListObject.PagedData<List<Character>>>(req, cToken);
        }

        public async Task<IAniListServiceResponse<Character>> GetCharacterById(int id, CancellationToken cToken = default(CancellationToken))
        {
            var query = new GraphQLQuery
            {
                Query = QueryStore.GetCharacterById,
                Variables = JsonConvert.SerializeObject(new { id })
            };
            var req = CreateRequest(query);
            return await ExecuteRequest<Character>(req, cToken);
        }

        #endregion

        #region Staff

        public async Task<IAniListServiceResponse<AniListObject.PagedData<List<Staff>>>> SearchStaff(string queryText, int page, int count, CancellationToken cToken = default(CancellationToken))
        {
            var query = new GraphQLQuery
            {
                Query = QueryStore.SearchStaff,
                Variables = JsonConvert.SerializeObject(new { queryText, page, count })
            };
            var req = CreateRequest(query);
            return await ExecuteRequest<AniListObject.PagedData<List<Staff>>>(req, cToken);
        }

        #endregion

        #region Studio

        public async Task<IAniListServiceResponse<AniListObject.PagedData<List<Studio>>>> SearchStudios(string queryText, int page, int count, CancellationToken cToken = default(CancellationToken))
        {
            var query = new GraphQLQuery
            {
                Query = QueryStore.SearchStudios,
                Variables = JsonConvert.SerializeObject(new { queryText, page, count })
            };
            var req = CreateRequest(query);
            return await ExecuteRequest<AniListObject.PagedData<List<Studio>>>(req, cToken);
        }

        #endregion

        #region ForumThread

        public async Task<IAniListServiceResponse<AniListObject.PagedData<List<ForumThread>>>> SearchForumThreads(string queryText, int page, int count, CancellationToken cToken = default(CancellationToken))
        {
            var query = new GraphQLQuery
            {
                Query = QueryStore.SearchForumThreads,
                Variables = JsonConvert.SerializeObject(new { queryText, page, count })
            };
            var req = CreateRequest(query);
            return await ExecuteRequest<AniListObject.PagedData<List<ForumThread>>>(req, cToken);
        }

        #endregion

        #region Internal

        private IRestClient CreateClient()
        {
            var client = new RestClient(Config.BaseUrl);
            client.ClearHandlers();
            client.AddHandler("*", JsonNetSerializer.Default);

            if (AuthCodeResolver.IsAuthorized)
            {
                client.AddDefaultHeader("Authorization", $"Bearer {AuthCodeResolver.AuthCode}");
            }

            return client;
        }

        private IRestRequest CreateRequest(GraphQLQuery query)
        {
            var req = new RestRequest(Method.POST)
            {
                JsonSerializer = JsonNetSerializer.Default
            };
            req.AddJsonBody(query);
            return req;
        }

        private async Task<IAniListServiceResponse<T>> ExecuteRequest<T>(IRestRequest req, CancellationToken cToken) where T : class
        {
            return AniListServiceResponse<T>.CreateResponse(await CreateClient().ExecuteTaskAsync<GraphQLResponse<T>>(req, cToken));
        }

        private bool HasNextPage<T>(PagingInfo info, AniListObject.PagedData<T> data)
            => data.PageInfo.HasNextPage;

        private interface IJsonSerializer : ISerializer, IDeserializer
        {
        }

        public class JsonNetSerializer : IJsonSerializer
        {
            public string DateFormat { get; set; }
            public string RootElement { get; set; }
            public string Namespace { get; set; }
            public string ContentType { get; set; }
            public Newtonsoft.Json.JsonSerializer Serializer { get; }

            public JsonNetSerializer()
            {
                Serializer = new Newtonsoft.Json.JsonSerializer
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Include
                };
                ContentType = "application/json";
            }

            public JsonNetSerializer(Newtonsoft.Json.JsonSerializer serializer)
            {
                Serializer = serializer;
                ContentType = "application/json";
            }

            public string Serialize(object obj)
            {
                using (var stringWriter = new StringWriter())
                {
                    using (var jsonTextWriter = new JsonTextWriter(stringWriter))
                    {
                        jsonTextWriter.Formatting = Formatting.Indented;
                        jsonTextWriter.QuoteChar = '"';
                        Serializer.ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() };

                        Serializer.Serialize(jsonTextWriter, obj);

                        var result = stringWriter.ToString();
                        return result;
                    }
                }
            }

            public T Deserialize<T>(IRestResponse response)
            {
                var content = response.Content;

                using (var stringReader = new StringReader(content))
                {
                    using (var jsonTextReader = new JsonTextReader(stringReader))
                    {
                        return Serializer.Deserialize<T>(jsonTextReader);
                    }
                }
            }

            public static JsonNetSerializer Default
            {
                get
                {
                    return new JsonNetSerializer();
                }
            }
        }

        #endregion
    }
}
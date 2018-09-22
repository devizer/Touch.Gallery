using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Gallery.MVC.Utils;
using Google.Cloud.Datastore.V1;

namespace Gallery.Logic.DataAccess
{
    public class PhotosRepository
    {
        DatastoreDb Db = DatastoreDb.Create("touch-galleries");

        static PhotosRepository()
        {
            var creds = Environment.GetEnvironmentVariable("TOUCH_GALLERIES_CREDENTIALS");
            if (!string.IsNullOrEmpty(creds))
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", creds);
        }

        // Key - idContent
        // Value - Totals
        public async Task<IDictionary<string, Content>> GetPhotoTotalsByTopic(string topic)
        {
            // select All UserPhoto, where 
            var keyTopic = new Key().WithElement("TopicTotals", topic);
            Query query = new Query("ContentTotals")
            {
                Filter = Filter.HasAncestor(keyTopic),
            };

            DatastoreQueryResults results = await Db.RunQueryAsync(query, ReadOptions.Types.ReadConsistency.Strong);
            var userPhotos = results.Entities.Select(x => x.ToContent());
            return userPhotos.ToDictionary(x => x.IdContent, x => x);
        }

        // Key - idContent
        // Value - My flags
        public async Task<IDictionary<string, UserPhoto>> GetUserPhotosByTopic(string topic, string idUser)
        {
            // select All UserPhoto, where 
            var keyTopic = new Key().WithElement("Topic", topic);
            var keyTopicAndUser = new KeyFactory(keyTopic, "User").CreateKey(idUser);
            Query query = new Query("UserPhoto")
            {
                Filter = Filter.HasAncestor(keyTopicAndUser),
            };

            DatastoreQueryResults results = await Db.RunQueryAsync(query, ReadOptions.Types.ReadConsistency.Strong);
            var userPhotos = results.Entities.Select(x => x.ToUserPhoto());
            return userPhotos.ToDictionary(x => x.IdContent, x => x);
        }

        [Obsolete("Topic is not used in a storage")]
        public void CreateTopics(IEnumerable<string> topics)
        {
            string kind = "Topic";
            var keyFactory = Db.CreateKeyFactory(kind);
            List<Entity> list = new List<Entity>();
            foreach (var topic in topics)
            {
                list.Add(new Entity()
                {
                    Key = keyFactory.CreateKey(topic),
                    ["Stars"] = 0,
                    ["Likes"] = 0,
                    ["Dislikes"] = 0,
                    ["Shares"] = 0,
                });
            }

            Db.Upsert(list);

            var keys = list.Select(x => x.Key.Path[0].Name);
            Console.WriteLine($"Saved Topics: {string.Join(", ", keys)}");
        }

        [Obsolete("Useless method", true)]
        public void CreateContent(IEnumerable<string> idContentList)
        {
            var list = idContentList
                .Select(x => new Content() {IdContent = x})
                .Select(x => x.ToEntity());

            Db.Upsert(list);
        }

        public UserPhoto GetUserPhoto(string topic, string idUser, string idContent)
        {
            var userPhoto = Db.Lookup(DataModelExtensions.ToUserPhotoKey(topic, idUser, idContent)).ToUserPhoto();
            if (userPhoto == null)
                userPhoto = new UserPhoto() { IdUser = idUser, IdContent = idContent };

            return userPhoto;
        }

        public Content GetContent(string topic, string idContent)
        {
            var content = Db.Lookup(DataModelExtensions.ToContentKey(topic, idContent)).ToContent();
            if (content == null)
                content = new Content() { Topic = topic, IdContent = idContent };

            return content;
        }


        public void AddUserAction(string topic, string idUser, string idContent, UserAction action)
        {
            // User 'Tester' on [Liked-content] topic 'One Topic'
/*
            if (Debugger.IsAttached && topic == "One Topic" && idUser == "Tester" && idContent == "Liked-content")
                Debugger.Break();
*/

            Stopwatch sw = Stopwatch.StartNew();
            String debug = $"{action,-7} by User '{idUser}' on [{idContent}] topic '{topic}'";
            try
            {
/*
                var userPhotoKey = DataModelExtensions.ToUserPhotoKey(topic, idContent);
                var contentIs = Filter.Equal("__key__", userPhotoKey);
                var userIs = Filter.Equal("IdUser", idUser);
                Query querySingleUserPhoto = new Query("UserPhoto")
                {
                    Filter = Filter.And(contentIs, userIs)
                };
                var userPhoto = Db.RunQuery(querySingleUserPhoto).Entities.FirstOrDefault().ToUserPhoto();
*/
                var userPhoto = Db.Lookup(DataModelExtensions.ToUserPhotoKey(topic, idUser, idContent)).ToUserPhoto();
                bool isUserPhotoNew = userPhoto == null;

                if (userPhoto == null)
                    userPhoto = new UserPhoto() {Topic = topic, IdUser = idUser, IdContent = idContent};

                var contentKey = DataModelExtensions.ToContentKey(topic, idContent);
                var content = Db.Lookup(contentKey).ToContent();
                if (content == null)
                    content = new Content() {Topic = topic, IdContent = idContent};

                if (!ApplyAction(content, userPhoto, action))
                {
                    Console.WriteLine($"None: {debug} in {sw.Elapsed}");
                    return;
                }

                Entity entityContent = content.ToEntity();
                Entity entityUserPhoto = userPhoto.ToEntity();
                Db.Upsert(entityContent, entityUserPhoto);
                Console.WriteLine($"DONE: {debug} in {sw.Elapsed}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FAIL: {debug} in {sw.Elapsed}:" + Environment.NewLine + "      " + ex.GetExceptionDigest());
                throw new Exception("AddAction failed " + debug, ex);
            }
        }


        // false means Storage does not need updates
        private bool ApplyAction(Content content, UserPhoto userPhoto, UserAction action)
        {
            int deltaStars = 0, deltaLike = 0, deltaDislike = 0, deltaShare = 0;
            switch (action)
            {
                case UserAction.Star:
                    if (userPhoto.Stars)
                        return false;

                    deltaStars = 1;
                    userPhoto.Stars = true;
                    break;

                case UserAction.Like:
                    if (userPhoto.Likes)
                        return false;

                    else if (userPhoto.Dislikes)
                    {
                        deltaDislike = -1;
                        userPhoto.Dislikes = false;
                    }

                    deltaLike = 1;
                    userPhoto.Likes = true;
                    break;

                case UserAction.Dislike:
                    if (userPhoto.Dislikes)
                        return false;

                    else if (userPhoto.Likes)
                    {
                        deltaLike = -1;
                        userPhoto.Likes = false;
                    }

                    deltaDislike = 1;
                    userPhoto.Dislikes = true;
                    break;

                case UserAction.Share:
                    deltaShare = 1;
                    userPhoto.Shares = true;
                    break;

                default:
                    throw new ArgumentException($"Action {action} is not supported");
            }

            content.Dislikes += deltaDislike;
            content.Likes += deltaLike;
            content.Shares += deltaShare;
            content.Stars += deltaStars;

            return true;
        }
    }
}

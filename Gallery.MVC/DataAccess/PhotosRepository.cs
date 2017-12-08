using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Gallery.MVC.Utils;
using Google.Cloud.Datastore.V1;
using Microsoft.Extensions.FileProviders;

namespace Gallery.MVC.DataAccess
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

        public void CreateContent(IEnumerable<string> idContentList)
        {
            var list = idContentList
                .Select(x => new Content() {IdContent = x})
                .Select(x => x.ToEntity());

            Db.Upsert(list);
        }

        public UserPhoto GetUserPhoto(string idUser, string idContent)
        {
            var userPhoto = Db.Lookup(DataModelExtensions.ToUserPhotoKey(idUser, idContent)).ToUserPhoto();
            if (userPhoto == null)
                userPhoto = new UserPhoto() { IdUser = idUser, IdContent = idContent };

            return userPhoto;
        }

        public Content GetContent(string idContent)
        {
            var content = Db.Lookup(DataModelExtensions.ToContentKey(idContent)).ToContent();
            if (content == null)
                content = new Content() { IdContent = idContent };

            return content;
        }


        public void AddUserAction(string idUser, string idContent, UserAction action)
        {
            Stopwatch sw = Stopwatch.StartNew();
            String debug = $"Action {action} by User '{idUser}' on [{idContent}]";
            try
            {
                var userPhoto = Db.Lookup(DataModelExtensions.ToUserPhotoKey(idUser, idContent)).ToUserPhoto();
                if (userPhoto == null)
                    userPhoto = new UserPhoto() {IdUser = idUser, IdContent = idContent};

                var content = Db.Lookup(DataModelExtensions.ToContentKey(idContent)).ToContent();
                if (content == null)
                    content = new Content() {IdContent = idContent};

                if (!ApplyAction(content, userPhoto, action))
                {
                    Console.WriteLine($"NONE: {debug} in {sw.Elapsed}");
                    return;
                }

                Db.Upsert(content.ToEntity(), userPhoto.ToEntity());
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
                    deltaShare++;
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

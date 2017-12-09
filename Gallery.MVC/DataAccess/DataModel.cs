using System.Text;
using Google.Cloud.Datastore.V1;

namespace Gallery.MVC.DataAccess
{
    public class UserPhoto
    {
        public string Topic { get; set; }
        public string IdContent { get; set; }
        public string IdUser { get; set; }
        public bool Stars { get; set; }
        public bool Likes { get; set; }
        public bool Dislikes { get; set; }
        public bool Shares { get; set; }

        public override string ToString()
        {
            return $"{nameof(Topic)}: {Topic}, {nameof(IdContent)}: {IdContent}, {nameof(IdUser)}: {IdUser}, {nameof(Stars)}: {Stars}, {nameof(Likes)}: {Likes}, {nameof(Dislikes)}: {Dislikes}, {nameof(Shares)}: {Shares}";
        }
    }

    public class Content
    {
        public string Topic { get; set; }
        public string IdContent { get; set; }
        public long Stars { get; set; }
        public long Likes { get; set; }
        public long Dislikes { get; set; }
        public long Shares { get; set; }
    }

    public static class DataModelExtensions
    {
        public static Key ToContentKey(string topic, string idContent)
        {
            var keyContent = new Key().WithElement("Topic", topic);
            var key = new KeyFactory(keyContent, "Content").CreateKey(idContent);
            return key;
        }

        public static Entity ToEntity(this Content content)
        {
            return new Entity
            {
                Key = ToContentKey(content.Topic, content.IdContent),
                ["Stars"] = content.Stars,
                ["Likes"] = content.Likes,
                ["Dislikes"] = content.Dislikes,
                ["Shares"] = content.Shares,
            };
        }

        public static Content ToContent(this Entity entity)
        {
            if (entity == null) return null;
            var path = entity.Key.Path;
            return new Content()
            {
                Topic = path[0].Name,
                IdContent = path[1].Name,
                Stars = (long) entity["Stars"],
                Likes = (long) entity["Likes"],
                Dislikes = (long) entity["Dislikes"],
                Shares = (long) entity["Shares"],
            };
        }


        public static Key ToUserPhotoKey(string topic, string idUser, string idContent)
        {
            var keyTopic = new Key().WithElement("Topic", topic);
            var keyUser = new KeyFactory(keyTopic, "User").CreateKey(idUser);
            var key = new KeyFactory(keyUser, "UserPhoto").CreateKey(idContent);
            return key;
        }

        public static Entity ToEntity(this UserPhoto userPhoto)
        {
            return new Entity
            {
                Key = ToUserPhotoKey(userPhoto.Topic, userPhoto.IdUser, userPhoto.IdContent),
                ["Stars"] = userPhoto.Stars,
                ["Likes"] = userPhoto.Likes,
                ["Dislikes"] = userPhoto.Dislikes,
                ["Shares"] = userPhoto.Shares,
            };
        }

        public static string ToDebugString(this UserPhoto arg)
        {
            var key = string.Format("Key: {{[{0}] '{1}' {2}}}", arg.Topic, arg.IdUser, arg.IdContent);
            StringBuilder flags = new StringBuilder();
            int n = 0;
            if (arg.Likes) flags.Append(n++ == 0 ? "Like" : ", Like");
            if (arg.Dislikes) flags.Append(n++ == 0 ? "Dislike" : ", Dislike");
            if (arg.Stars) flags.Append(n++ == 0 ? "Starred" : ", Starred");
            if (arg.Shares) flags.Append(n++ == 0 ? "Shared" : ", Shared");

            return key + ": " + flags;
        }

        public static UserPhoto ToUserPhoto(this Entity userPhoto)
        {
            if (userPhoto == null) return null;
            var path = userPhoto.Key.Path;
            return new UserPhoto()
            {
                Topic = path[0].Name,
                IdUser = path[1].Name,
                IdContent = path[2].Name,
                Stars = (bool) userPhoto["Stars"],
                Likes = (bool) userPhoto["Likes"],
                Dislikes = (bool) userPhoto["Dislikes"],
                Shares = (bool) userPhoto["Shares"],
            };
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Datastore.V1;

namespace Gallery.MVC.DataAccess
{
    public class UserPhoto
    {
        public string IdUser { get; set; }
        public string IdContent { get; set; }
        public bool Stars { get; set; }
        public bool Likes { get; set; }
        public bool Dislikes { get; set; }
        public bool Shares { get; set; }
    }

    public class Content
    {
        public string IdContent { get; set; }
        public long Stars { get; set; }
        public long Likes { get; set; }
        public long Dislikes { get; set; }
        public long Shares { get; set; }
    }

    public static class DataModelExtensions
    {
        public static Key ToContentKey(string idContent)
        {
            return  new Key().WithElement("Content", idContent);
        }

        public static Entity ToEntity(this Content content)
        {
            return new Entity
            {
                Key = ToContentKey(content.IdContent),
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
                IdContent = path[0].Name,
                Stars = (long) entity["Stars"],
                Likes = (long) entity["Likes"],
                Dislikes = (long) entity["Dislikes"],
                Shares = (long) entity["Shares"],
            };
        }


        public static Key ToUserPhotoKey(string idUser, string idContent)
        {
            var keyContent = new Key().WithElement("Content", idContent);
            var key = new KeyFactory(keyContent, "UserPhoto").CreateKey(idUser);
            return key;
        }

        public static Entity ToEntity(this UserPhoto userPhoto)
        {
            return new Entity
            {
                Key = ToUserPhotoKey(userPhoto.IdUser, userPhoto.IdContent),
                ["Stars"] = userPhoto.Stars,
                ["Likes"] = userPhoto.Likes,
                ["Dislikes"] = userPhoto.Dislikes,
                ["Shares"] = userPhoto.Shares,
            };
        }

        public static UserPhoto ToUserPhoto(this Entity userPhoto)
        {
            if (userPhoto == null) return null;
            var path = userPhoto.Key.Path;
            return new UserPhoto()
            {
                IdContent = path[0].Name,
                IdUser = path[1].Name,
                Stars = (bool) userPhoto["Stars"],
                Likes = (bool) userPhoto["Likes"],
                Dislikes = (bool) userPhoto["Dislikes"],
                Shares = (bool) userPhoto["Shares"],
            };
        }

    }
}

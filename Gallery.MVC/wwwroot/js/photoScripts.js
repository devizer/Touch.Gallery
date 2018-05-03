/* USER ACTIONS
        Star,
        Like,
        Dislike,
        Share,
*/

theAppContext.GetPhotoById = function(idContent) {
    var ret = null, i=0;
    var l = galleryModel.Photos.length;
    for (i = 0; i < l; i++)
        if (galleryModel.Photos[i].Id == idContent)
            return galleryModel.Photos[i];

    return null;
}

theAppContext.ApplyAction = function (button) {
    var photoContainer = $(button).parents("#photo_with_buttons");
    var idContent = $(photoContainer).attr("id-content");
    var photoInfo = theAppContext.GetPhotoById(idContent);
    var action = $(button).attr("id-action");
    console.log("[" + action + "] ID: " + idContent + "\r\n" + JSON.stringify(photoInfo));

}
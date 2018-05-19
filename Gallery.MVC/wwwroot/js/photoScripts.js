theAppContext.GetPhotoById = function(idContent) {
    var ret = null, i=0;
    var l = galleryModel.Photos.length;
    for (i = 0; i < l; i++)
        if (galleryModel.Photos[i].Id == idContent)
            return galleryModel.Photos[i];

    return null;
}

theAppContext.BindPhotoInfo = function(idContent, photoInfo) {
    var photoContainer = $("#photo_with_buttons[id-content='" + idContent + "']");
    console.log("BIND to: " + photoContainer.length + " containers");

    var el;
    el = photoContainer.find("*[id-action='Star']");
    if (photoInfo.MyStars)
        el.removeClass("fa-star-o").addClass("fa-star");
    else
        el.removeClass("fa-star").addClass("fa-star-o");

    el = photoContainer.find("*[id-action='Like']");
    if (photoInfo.MyLikes)
        el.removeClass("fa-thumbs-o-up").addClass("fa-thumbs-up");
    else
        el.removeClass("fa-thumbs-up").addClass("fa-thumbs-o-up");

    el = photoContainer.find("*[id-action='Dislike']");
    if (photoInfo.MyDislikes)
        el.removeClass("fa-thumbs-o-down").addClass("fa-thumbs-down");
    else
        el.removeClass("fa-thumbs-down").addClass("fa-thumbs-o-down");

    var asCount = function(a) { return a === undefined || a == null || a <= 0 ? "" : theAppContext.FormatCounter(a); };
    photoContainer.find("#count-Star").text(asCount(photoInfo.TotalStars));
    photoContainer.find("#count-Like").text(asCount(photoInfo.TotalLikes));
    photoContainer.find("#count-Dislike").text(asCount(photoInfo.TotalDislikes));
    photoContainer.find("#count-Share").text(asCount(photoInfo.TotalShares));

}

// USER ACTIONS: Star | Like | Dislike | Share
theAppContext.ApplyAction = function(button) {
    var photoContainer = $(button).parents("#photo_with_buttons");
    var idContent = $(photoContainer).attr("id-content");
    var photoInfo = theAppContext.GetPhotoById(idContent);
    var action = $(button).attr("id-action");
    console.log("[" + action + "] ID: " + idContent + "\r\n" + JSON.stringify(photoInfo));
    var asNum = function(a) { return a === undefined || a === null ? 0 : a };
    if (action === "Star") {
        if (!photoInfo.MyStars) {
            photoInfo.MyStars = true;
            photoInfo.TotalStars = asNum(photoInfo.TotalStars) + 1;
        }
    } else if (action === "Like") {
        if (!photoInfo.MyLikes) {
            if (photoInfo.MyDislikes) {
                photoInfo.MyDislikes = false;
                photoInfo.TotalDislikes = asNum(photoInfo.TotalDislikes) - 1;
            }

            photoInfo.MyLikes = true;
            photoInfo.TotalLikes = asNum(photoInfo.TotalLikes) + 1;
        }
    } else if (action === "Dislike") {
        if (!photoInfo.MyDislikes) {
            if (photoInfo.MyLikes) {
                photoInfo.MyLikes = false;
                photoInfo.TotalLikes = asNum(photoInfo.TotalLikes) - 1;
            }

            if (photoInfo.MyStars) {
                photoInfo.MyStars = false;
                photoInfo.TotalStars = asNum(photoInfo.TotalStars) - 1;
            }

            photoInfo.MyDislikes = true;
            photoInfo.TotalDislikes = asNum(photoInfo.TotalDislikes) + 1;
        }
    } else if (action === "Share") {
        photoInfo.MyShares = true;
        photoInfo.TotalShares = asNum(photoInfo.TotalShares) + 1;
    }

    console.log("AFTER [" + action + "] ID: " + idContent + "\r\n" + JSON.stringify(photoInfo));

    theAppContext.BindPhotoInfo(idContent, photoInfo);
}

theAppContext.FormatCounter = function(arg) {

    if (arg < 1000)
        return '' + arg;

    else if (arg < 1000000)
        return (arg / 1000.).toFixed(1) + " K";

    return (arg / 10000000.).toFixed(1) + " M";
}
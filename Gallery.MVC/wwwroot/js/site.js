// Write your JavaScript code.


$(document).ready(function() {
    //initialize swiper when document ready
/*
    var mySwiper = new Swiper('.swiper-container',
        {
            // Optional parameters
            direction: 'vertical',
            loop: true,

            // If we need pagination
            pagination: {
                el: '.swiper-pagination',
            },

            // Navigation arrows
            navigation: {
                nextEl: '.swiper-button-next',
                prevEl: '.swiper-button-prev',
            },

            // And if we need scrollbar
            scrollbar: {
                el: '.swiper-scrollbar',
            }
        });
*/
});

function GetDeviceInfo() {
    
}

$(document).ready(function() {

    $(window).resize(function() {
        // This will execute whenever the window is resized
        $("#debug-info").html("Window: " + $(window).width() + "x" + $(window).height() + ", Ratio: " + window.devicePixelRatio);
        $(window).height(); // New height
        $(window).width(); // New width
    });

    $("#debug-info").html("Window: " + $(window).width() + "x" + $(window).height() + ", Ratio: " + window.devicePixelRatio);

    
});

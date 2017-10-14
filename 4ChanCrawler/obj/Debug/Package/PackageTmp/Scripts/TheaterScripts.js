//---------------------THIS SECTION IS USED FOR Styling rating buttons ---------------------
function ColorButton(Rating) {
    if (Rating == 0) {
        $('#Dislike').addClass("btn-danger")
    } else if (Rating == 10)
    {
        $('#Like').addClass("btn-success")
    } else if (Rating == 20) {
        $('#Favorite').addClass("btn-warning")
    }

}
function SetRating(Likes, Dislikes) {
    $('#Dislike b').text(Dislikes)
    $('#Like b').text(Likes)
}
function RemoveColorOnButtons() {
    $('#Dislike').removeClass("btn-danger")
    $('#Like').removeClass("btn-success")
    $('#Favorite').removeClass("btn-warning")
}
//---------------------THIS SECTION WAS USED FOR Styling rating buttons ---------------------
//---------------------THIS SECTION IS USED FOR REMEMBERING VIDEO VOLUME LEVEL ---------------------
function SaveVolumeLevel(e) {
    $.cookie("Volume", $(e).prop("volume"));
}
function LoadVolumeLevel() {
    $('video').prop("volume", $.cookie("Volume")) 
}


//---------------------THIS SECTION WAS USED FOR REMEMBERING VIDEO VOLUME LEVEL ---------------------
//---------------------THIS SECTION IS USED FOR KEYPRESSES ---------------------
$("body").keydown(function (e) {
    if (e.keyCode == 37 || e.keyCode == 65 ) { // left
        GetPost(2)
    }
    else if (e.keyCode == 39 || e.keyCode == 68) { // right
        GetPost(1)
    }
    else if (e.keyCode == 32) {
        RatePost(20)
    }
});
//---------------------THIS SECTION WAS USED FOR KEYPRESSES ---------------------


$('video').on('click', function (e) {
        this.play()
    });
        $('video').on('play', function (e) {
        $('video').not(this).each(function () {
            this.pause();
            this.currentTime = 0;
        });
    });




//---------------------THIS SECTION IS USED FOR KEYPRESSES ---------------------
$("body").keydown(function (e) {
    if (e.keyCode == 37) { // left
        GetNextPost()
    }
    else if (e.keyCode == 39) { // right
        GetNextPost()
    }
    else if (e.keyCode == 38) {
        //TODO
    }
});
//---------------------THIS SECTION WAS USED FOR KEYPRESSES ---------------------


$('video').on('click', function (e) {
        this.play()
    });
        $('video').on('ended', function (e) {


        $('video')[$('video').index(e.currentTarget) + 1].play()
    });

        $('video').on('play', function (e) {
        $('video').not(this).each(function () {
            this.pause();
            this.currentTime = 0;
        });
    });


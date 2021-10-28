export const name = 'spectacle';


export function loadDropZone() {

    let myDropZone = new Dropzone("div#photo-upload", {
        url: "/upload",
        maxFilesize: 5000000, //5 megs
        acceptedFiles: "image/*",
        createImageThumbnails: false,
    });
    myDropZone.on("addedfile", function (file) {
        console.log("A file has been added");

        document.getElementById('photo-upload').style.display = 'none';


        //show it
        document.getElementById('photo-canvas').style.display = 'block';
    });
    myDropZone.on('complete', function (file) {
        //draw the rest of the things. 
        var canvas = document.getElementById("photo-canvas");
        canvas.onmouseover = function onMouseover(e) {
            //check which 
        };
        var ctx = canvas.getContext("2d");
        canvas.width = window.innerWidth;
        canvas.height = window.innerHeight - 60.0;

        var img = new Image();
        img.onload = function () {
            var hRatio = canvas.width / img.width;
            var vRatio = canvas.height / img.height;
            var ratio = Math.min(hRatio, vRatio);
            var centerShift_x = (canvas.width - img.width * ratio) / 2;
            var centerShift_y = (canvas.height - img.height * ratio) / 2;
            ctx.clearRect(0, 0, canvas.width, canvas.height);
            ctx.clearRect(0, 0, canvas.width, canvas.height);
            ctx.drawImage(img, 0, 0, img.width, img.height,
                centerShift_x, centerShift_y, img.width * ratio, img.height * ratio);

            var readResults = JSON.parse(file.xhr.response);

            var readResults = JSON.parse(file.xhr.response);
            readResults.forEach(page => {
                page.lines.forEach(line => {
                    var boundingBox = line.boundingBox;
                    drawBoundingBox(ctx, ratio, centerShift_x, centerShift_y, boundingBox, 'red');

                    //line.words.forEach(word => {
                    //    drawBoundingBox(ctx, ratio, centerShift_x, centerShift_y, word.boundingBox, 'blue');
                    //});
                });
            });

        };
        img.src = URL.createObjectURL(file);

        
    });
}

function drawBoundingBox(ctx,scaleRatio,scaledShiftX, scaledShiftY, boundingBox, strokeStyle) {
    //the bounding box is that of an unscaled image.  So we need to move the points
    //respective to the scale. 
    ctx.beginPath();
    ctx.lineWidth = "2";
    ctx.strokeStyle = strokeStyle;

    var firstScaledX = 0;
    var firstScaledY = 0;
    for (var i = 0; i < boundingBox.length; i += 2) {
        var x = boundingBox[i];
        var y = boundingBox[i + 1];

        var scaledX = x * scaleRatio + scaledShiftX;
        var scaledY = y * scaleRatio + scaledShiftY;

        if (i < 2) {
            ctx.moveTo(scaledX, scaledY);
            firstScaledX = scaledX;
            firstScaledY = scaledY;
        } else {
            ctx.lineTo(scaledX, scaledY);
        }
    }
    ctx.lineTo(firstScaledX, firstScaledY);

    ctx.stroke();
}


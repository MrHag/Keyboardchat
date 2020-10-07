var socket = io.connect("http://127.0.0.1:4000");
var lastwrite = {is:false};
function writing()
{
   
   socket.emit('keypress', {
       name: $('#Name').val(),
    });
	
}

function send(mess) {
	if (mess != "") {
		socket.emit('chat', {
			message: mess,
		});
	}
}

function send_message()
{
	var inputData = $("#myInput").val();

	send(inputData);

	$('#myInput').val('');

	const messageBody = document.getElementById("chat");
	messageBody.scrollTop = messageBody.scrollHeight - messageBody.clientHeight;
}

$(document).ready(function(){
  $('.addBtn').click(() => {
	 send_message();
});


// TRY TO CONNECT

//socket.emit("JoinRoom", "David/Vitala");

// TRY TO CONNECT



$("body").keypress(function(e) {
          if (e.which == 13) {
            send_message();  
          }
     });
});


function refresh_write_text(clear)
{
	
lastwrite=clear;
clear.is = true;

	setTimeout(function() {	
		if(clear.is)
		$('#writing').css("opacity",0);

	}, 1000);
}


socket.on('chat', (data) => {

	console.log(data);

    today = new Date();
	//$(window).scrollTop($(document).height());

  
  if(today.getHours() < 10)
    var hours = '0' + today.getHours();
  else
    hours = today.getHours();

  if(today.getMinutes() < 10)
    var minuts = '0' + today.getMinutes();
  else
    minuts = today.getMinutes();
    $('#chat').append('<div class="container"><h4 class = Name>' + data.name + '</h4><img src="'+data.avatar+'" alt="Avatar" style="width:100%;">' +"<p id = " + 'dataText' + "> "  + data.message + '</p>' +'<span class="time-right">' + hours+':'+minuts + '</span></div>');

  });

  


/*socket.on('keypress', (data) => {
	
	lastwrite.is = false;
	var clear = {is:false};
	
	$('#writing').css("opacity",100);
    $('#writingText').html(data.name + " is writing...");
	
	refresh_write_text(clear);


});*/
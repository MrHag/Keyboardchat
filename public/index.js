var socket = io.connect("http://127.0.0.1:4000");

$(document).ready(function () {

	$('.addBtn').click(() => {
		console.log($("#Name").val());
		socket.emit('auth', {
			name: $("#Name").val(),
		});
	});

	socket.on("response", async res => {
		console.log(res);
	});
});
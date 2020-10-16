import io from 'socket.io-client';
const Socket = io.connect("http://127.0.0.1:4000"); //Нужно автоматизировать это как-то
export default Socket;


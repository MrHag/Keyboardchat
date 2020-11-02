import io from 'socket.io-client';
const Socket = io.connect('http://localhost:5000'); // Нужно автоматизировать это как-то
export default Socket;

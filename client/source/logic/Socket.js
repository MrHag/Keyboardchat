import io from 'socket.io-client';

const Socket = io.connect('http://localhost:4001'); // Нужно автоматизировать это как-то
export default Socket;

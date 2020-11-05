import io from 'socket.io-client';
import UserData, { User, Message, Room } from './UserData';

const Socket = io.connect('http://localhost:4001');

export default Socket;

const SocketManager = {
  callbacks: new Map([
    ['auth', []],
    ['deAuth', []],
    ['getUsers', []],
    ['registration', []],
    ['getRooms', []],
    ['joinRoom', []],
    ['createRoom', []],
    ['onNewMsg', []],
    ['roomListChange', []],
  ]),
  socket: Socket,

  addCallback(event, callback) {
    if (this.callbacks.has(event)) {
      this.callbacks.get(event).push(callback);
      console.log("SocketManager: AddCallback finished!", this.callbacks);
    } else {
      console.warn(`SocketManager: you're trying to add callback to not registered event. Event ${event} isn't registered in SocketManager!`);
    }
  },

  removeCallback(event, callback) {
    if (this.callbacks.has(event)) {
      this.callbacks.get();
    }
  },

  emitResult(event, result) {
    this.callbacks.get(event).forEach((callback) => {
      callback(result);
    });
  }
};

class Result {
  constructor(data = null, error = null) {
    this.data = data;
    this.error = error;
  }
}

(function Subscribe() {
  function warnCannotHandleError(event, error) {
    console.log(`SocketManager: Seems SocketManager can't handle error '${error}' of '${event}'`);
  }

  Socket.on('auth', (data) => {
    console.log(`SocketManager: 'auth' response handling...`);
    let result = new Result();
    if (data.error === null) {
      result.data = data;
    } else {
      switch (data.error) {
        case 'wrongNamePass':
          result.error = 'Wrong password or username!'
          break;
        default:
          warnCannotHandleError('auth', data.error);
          result.error = data.error;
          break;
      }
    }

    SocketManager.emitResult('auth', result);
  });

  Socket.on('registration', (data) => {
    console.log('SocketManager: registration response!');
    console.log('Response data = ', data);

    let result = new Result();
    if (data.error === null) {
      ;
    } else {
      switch (data.error) {
        case 'nameExists':
          result.error = 'This name is already taken';
          break;
        default:
          warnCannotHandleError('registration', data.error);
          result.error = data.error;
          break;
      }
    }

    SocketManager.emitResult('registration', result);
  })
})();

export { SocketManager };
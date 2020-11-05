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
    ['leaveRoom', []],
  ]),
  socket: Socket,

  addCallback(event, callback) {
    if (this.callbacks.has(event)) {
      this.callbacks.get(event).push(callback);
    } else {
      console.warn(`SocketManager: you're trying to add callback to not registered event. Event ${event} isn't registered in SocketManager!`);
    }
  },

  removeCallback(event, callback) {
    if (this.callbacks.has(event)) {
      const callbacks = this.callbacks.get(event);
      const index = callbacks.indexOf(callback);
      if (index !== -1) {
        callbacks.splice(index, 1);
      }
    }
  },

  emitResult(event, result) {
    console.log('SocketManager emitting result: event = ', event);
    this.callbacks.get(event).forEach((callback) => {
      callback(result);
    });
  },
};

class Result {
  constructor(data = null, error = null) {
    this.data = data;
    this.error = error;
  }
}

function Subscribe() {
  function warnCannotHandleError(event, error) {
    console.warn(`SocketManager: Seems SocketManager can't handle error '${error}' of '${event}'`);
  }

  Socket.on('auth', (data) => {
    console.log('SocketManager: "auth" response handling...');
    const result = new Result();
    if (data.error !== null) {
      switch (data.error) {
        case 'wrongNamePass':
          result.error = 'Wrong password or username!';
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

    const result = new Result();
    if (data.error !== null) {
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
  });

  Socket.on('getUsers', (data) => {
    if (data.error) throw data.error; // Here must not be error in the client app
    const result = new Result(data.data);
    SocketManager.emitResult('getUsers', result);
  });

  Socket.on('onNewMsg', (d) => {
    const { data } = d;
    const result = new Result(
      new Message(
        {
          id: data.userid,
          userName: data.userName,
          avatar: null,
        },
        {
          text: data.message,
        },
        data.roomid,
        new Date(),
      ),
    );
    SocketManager.emitResult('onNewMsg', result);
  });

  Socket.on('joinRoom', (data) => {
    console.log('SocketManager: joinRoom: ', data);
    const result = new Result();
    if (data.error !== null) {
      if (data.error === 'invalidPass') {
        result.error = 'Invalid password';
      } else {
        throw Error('SocketManager: joinRoom returns error! This is critical error!!!');
      }
    } else {
      result.data = data.data;
    }
    SocketManager.emitResult('joinRoom', result);
  });

  Socket.on('roomListChange', (data) => {
    console.log('SocketManager: roomListChange: ', data.data);
    const result = new Result(data.data);
    SocketManager.emitResult('roomListChange', result);
  });

  Socket.on('getRooms', (data) => {
    if (data.error !== null) throw Error('SocketManager: getRooms critical error! Server returns unhandled error!');
    console.log('SocketManager: getRooms: ', data);
    const result = new Result(data.data);
    SocketManager.emitResult('getRooms', result);
  });

  Socket.on('leaveRoom', (data) => {
    const result = new Result(data);
    SocketManager.emitResult('leaveRoom', result);
  });

  Socket.on('createRoom', (data) => {
    console.log('Socket create room = ', data);
    const result = new Result(data.data);
    if (data.error !== null) {
      switch (data.error) {
        case 'roomExists':
          result.error = 'Room with this name already exist!';
          break;
        default:
          warnCannotHandleError('createRoom', data.error);
          break;
      }
    }
    SocketManager.emitResult('createRoom', result);
  });
}
Subscribe();

export { SocketManager };
